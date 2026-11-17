using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class HudVisibilityPolicyTests
{
    [Theory]
    [InlineData("ChatLog")]
    [InlineData("ChatLogPanel_0")]
    [InlineData("ChatLogPanel_1")]
    [InlineData("ChatLogPanel_2")]
    [InlineData("ChatLogPanel_3")]
    public void Chat_stays(string name)
    {
        Assert.False(HudVisibilityPolicy.ShouldHide(name));
    }

    [Theory]
    [InlineData("_ActionBar")]
    [InlineData("_ActionBar01")]
    [InlineData("_NaviMap")]
    [InlineData("_PartyList")]
    [InlineData("_TargetInfo")]
    [InlineData("_ParameterWidget")]
    [InlineData("_DTR")]
    [InlineData("_Money")]
    [InlineData("_MainCommand")]
    [InlineData("_ToDoList")]
    [InlineData("NamePlate")]
    [InlineData("SelectYesno")]
    [InlineData("ContextMenu")]
    [InlineData("Character")]
    public void Everything_else_is_hidden(string name)
    {
        Assert.True(HudVisibilityPolicy.ShouldHide(name));
    }

    [Fact]
    public void Nameless_addons_are_left_alone()
    {
        Assert.False(HudVisibilityPolicy.ShouldHide(""));
    }
}
