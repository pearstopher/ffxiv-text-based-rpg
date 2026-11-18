using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ModePartsTests
{
    [Fact]
    public void Default_setting_gives_full_mode()
        => Assert.Equal(new ModeParts(FrameCap: true, HideHud: true), ModeParts.For(worldOffOnly: false));

    [Fact]
    public void World_only_leaves_the_hud_and_frame_rate_alone()
        => Assert.Equal(new ModeParts(FrameCap: false, HideHud: false), ModeParts.For(worldOffOnly: true));
}
