using FFXIVClientStructs.FFXIV.Client.UI;

namespace TextBasedRPG;

/// <summary>Whether the player is typing (FFXIVClientStructs <c>AtkModule.IsTextInputActive</c>). Framework thread only.</summary>
internal static unsafe class ChatInput
{
    /// <summary>True while a text input (normally the chat box) has focus.</summary>
    internal static bool IsTyping
    {
        get
        {
            var module = RaptureAtkModule.Instance();
            return module != null && module->IsTextInputActive();
        }
    }
}
