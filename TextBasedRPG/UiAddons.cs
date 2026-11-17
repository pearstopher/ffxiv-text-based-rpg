using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>Read-only access to the game's loaded UI addons (FFXIVClientStructs). Framework thread only.</summary>
internal static unsafe class UiAddons
{
    /// <summary>The game's list of every loaded addon, or null before the UI exists.</summary>
    internal static AtkUnitList* AllLoaded
    {
        get
        {
            var stage = AtkStage.Instance();
            if (stage == null || stage->RaptureAtkUnitManager == null)
                return null;
            return &stage->RaptureAtkUnitManager->AtkUnitManager.AllLoadedUnitsList;
        }
    }

    /// <summary>Snapshot of every loaded addon (allocates; for debug dumps and self-tests, not per-frame work).</summary>
    internal static List<AddonInfo> Snapshot()
    {
        var result = new List<AddonInfo>();
        var list = AllLoaded;
        if (list == null)
            return result;

        foreach (var entry in list->Entries[..list->Count])
        {
            var addon = entry.Value;
            if (addon == null)
                continue;
            result.Add(Describe(addon));
        }

        return result;
    }

    /// <summary>Name and visibility of one addon.</summary>
    internal static AddonInfo Describe(AtkUnitBase* addon)
        => new(
            addon->NameString,
            addon->IsVisible,
            addon->RootNode != null && addon->RootNode->IsVisible(),
            addon->DepthLayer);
}
