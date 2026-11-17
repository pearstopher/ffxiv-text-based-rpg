// Adapted from ECommons (https://github.com/NightmareXIV/ECommons), file ECommons/GameFunctions/RenderDisableManager.cs
// at commit 40591c783ead9f9e63324685d362c0535b12503f, and the unique-id code from ECommons/ECommonsMain.cs.
// Copyright (c) 2023 NightmareXIV. MIT License (see THIRD_PARTY_NOTICES.md).
// The maintainer says the class is self-contained and may be copy-pasted (NightmareXIV/ECommons#175).
//
// Changes from the original: uses this plugin's Service instead of ECommons' Svc/PluginLog; generates the instance id
// itself (same shared dictionary as ECommons, so ids never collide with ECommons plugins); Dispose is called by our
// Plugin. The shared data names and types are unchanged, so requests are shared with every ECommons plugin
// (AutoRetainer, Mini, ...) and the plugins never fight over the flag.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace TextBasedRPG;

/// <summary>
/// RenderDisableManager provides cross-plugin synchronization for disabling world rendering.
/// <br></br>How to use:
/// <br></br>- Call PlaceRequest to indicate that you wish to stop rendering from happening
/// <br></br>- Call RemoveRequest to resume rendering
/// <br></br>Rendering will be disabled for as long as any plugin has a request.
/// </summary>
internal static unsafe class RenderDisableManager
{
    private static bool Initialized = false;
    private static uint* FrameCounter;
    private static byte* RenderDisabled;
    private static HashSet<uint> RenderDisableRequests = null!;
    private static uint[] RenderDisableProcessingFramecount = null!;
    private static ConcurrentDictionary<uint, string>? UniqueIdConcurrentDictionary;
    private static bool IsSubscribed = false;

    /// <summary>Set to true to output verbose logs.</summary>
    public static bool Debug { get; set; }

    private static readonly string Name_RenderDisableRequests = "ECommons.RenderDisableRequests";
    private static readonly string Name_RenderDisableProcessingFramecount = "ECommons.RenderDisableProcessingFramecount";
    private static readonly string Name_UniqueIdConcurrentDictionary = "ECommons.UniqueIdConcurrentDictionary";

    /// <summary>This plugin load's id in the shared request set (ECommons' InstanceUniqueId).</summary>
    internal static uint InstanceUniqueId { get; private set; }

    /// <summary>True while this plugin has a request in the shared set.</summary>
    internal static bool HasOwnRequest => Initialized && RenderDisableRequests.Contains(InstanceUniqueId);

    private static void InitializeInternal()
    {
        if (Initialized)
        {
            Service.Log.Error("RenderDisableManager is already initialized and subsequent initialize call was ignored");
            return;
        }
        Initialized = true;
        GenerateInstanceUniqueId();
        FrameCounter = &Framework.Instance()->FrameCounter;
        RenderDisabled = (byte*)&Manager.Instance()->Is3DRenderingDisabled;
        RenderDisableRequests = Service.PluginInterface.GetOrCreateData<HashSet<uint>>(Name_RenderDisableRequests, () => []);
        RenderDisableProcessingFramecount = Service.PluginInterface.GetOrCreateData<uint[]>(Name_RenderDisableProcessingFramecount, () => [0]);
        Service.Log.Information($"Initialized RenderDisableManager");
    }

    // From ECommonsMain.Init: a random id, unique across every plugin using the shared dictionary.
    private static void GenerateInstanceUniqueId()
    {
        try
        {
            UniqueIdConcurrentDictionary = Service.PluginInterface.GetOrCreateData<ConcurrentDictionary<uint, string>>(Name_UniqueIdConcurrentDictionary, () => []);
            do
            {
                byte[] buffer = new byte[4];
                Random.Shared.NextBytes(buffer);
                InstanceUniqueId = BitConverter.ToUInt32(buffer, 0);
            }
            while (!UniqueIdConcurrentDictionary.TryAdd(InstanceUniqueId, $"{Service.PluginInterface.Manifest.InternalName} v{Service.PluginInterface.Manifest.AssemblyVersion} @{Service.PluginInterface.LoadTime}"));
        }
        catch (Exception e)
        {
            Service.Log.Error(e, "Error generating unique plugin identifier properly. Fallback: random number has been generated without uniqueness verification.");
            byte[] buffer = new byte[4];
            Random.Shared.NextBytes(buffer);
            InstanceUniqueId = BitConverter.ToUInt32(buffer, 0);
        }
    }

    private static void SubscribeIfNeeded()
    {
        if (!IsSubscribed)
        {
            Service.Framework.Update += Framework_Update;
            IsSubscribed = true;
        }
    }

    private static void UnsubscribeIfNeeded()
    {
        if (IsSubscribed)
        {
            Service.Framework.Update -= Framework_Update;
            IsSubscribed = false;
        }
    }

    /// <summary>
    /// Places request indicating that your plugin wants to disable rendering. If not initialized, initializes RenderDisablerManager. Must only be called inside framework update thread. You can call it every frame, subsequent requests will not be piled up.
    /// </summary>
    public static void PlaceRequest()
    {
        if (!Initialized) InitializeInternal();
        if (!Service.Framework.IsInFrameworkUpdateThread)
        {
            Service.Log.Error($"{nameof(RenderDisableManager)}.{nameof(PlaceRequest)} can only be used in Framework Update thread.");
            return;
        }
        RenderDisableRequests.Add(InstanceUniqueId);
        SubscribeIfNeeded();
    }

    /// <summary>
    /// Places request indicating that your plugin no longer wants to disable rendering. If not initialized, does not initializes RenderDisablerManager. Must only be called inside framework update thread. You can call it every frame, subsequent requests when there are no requests from your plugin will not do anything.
    /// </summary>
    public static void RemoveRequest()
    {
        if (!Initialized) return;
        if (!Service.Framework.IsInFrameworkUpdateThread)
        {
            Service.Log.Error($"{nameof(RenderDisableManager)}.{nameof(RemoveRequest)} can only be used in Framework Update thread.");
            return;
        }
        RenderDisableRequests.Remove(InstanceUniqueId);
    }

    /// <summary>Removes this plugin's request, applies the result once, and releases the shared data. Called on unload.</summary>
    internal static void Dispose()
    {
        if (Initialized)
        {
            Service.Framework.RunOnFrameworkThread(() =>
            {
                RemoveRequest();
                Framework_Update(null);
            });
            Service.Framework.Update -= Framework_Update;
            IsSubscribed = false;
            Service.PluginInterface.RelinquishData(Name_RenderDisableRequests);
            Service.PluginInterface.RelinquishData(Name_RenderDisableProcessingFramecount);
            if (UniqueIdConcurrentDictionary != null)
                Service.PluginInterface.RelinquishData(Name_UniqueIdConcurrentDictionary);

            // Like ECommons, Initialized stays true: if Dispose runs off the framework thread, the queued
            // RemoveRequest above still needs it (the static state is discarded with the plugin's load context).
        }
    }

    private static void Framework_Update(IFramework? framework)
    {
        if (RenderDisableProcessingFramecount[0] == *FrameCounter)
        {
            if (Debug) Service.Log.Verbose($"[RenderDisableManager] Frame {*FrameCounter} was already processed by different instance");
        }
        else
        {
            if (RenderDisableRequests.Count == 0)
            {
                if (*RenderDisabled != 0)
                {
                    if (Debug) Service.Log.Verbose($"[RenderDisableManager] Enabling render because there are no requests");
                    *RenderDisabled = 0;
                }
                UnsubscribeIfNeeded();
            }
            else
            {
                if (*RenderDisabled == 0)
                {
                    if (Debug) Service.Log.Verbose($"[RenderDisableManager] Disabling render because there are requests");
                    *RenderDisabled = 1;
                }
            }
            RenderDisableProcessingFramecount[0] = *FrameCounter;
        }
    }
}
