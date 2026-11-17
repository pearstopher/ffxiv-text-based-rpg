using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ChatAddonsTests
{
    [Theory]
    [InlineData("ChatLog", true)]
    [InlineData("ChatLogPanel_0", true)]
    [InlineData("ChatLogPanel_3", true)]
    [InlineData("ChatLogPanel_4", false)]
    [InlineData("chatlog", false)]
    [InlineData("_ActionBar", false)]
    [InlineData("", false)]
    public void Recognises_chat_addons(string name, bool isChat)
    {
        Assert.Equal(isChat, ChatAddons.IsChat(name));
    }

    [Fact]
    public void Always_shown_chat_addons_are_chat_addons()
    {
        Assert.All(ChatAddons.AlwaysShown, name => Assert.True(ChatAddons.IsChat(name)));
    }
}
