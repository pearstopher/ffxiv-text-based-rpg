using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class SettingLimitsTests
{
    [Theory]
    [InlineData(-3, 1)]
    [InlineData(0, 1)]
    [InlineData(10, 10)]
    [InlineData(30, 30)]
    [InlineData(144, 30)]
    public void Idle_fps_is_clamped(int value, int expected)
    {
        Assert.Equal(expected, SettingLimits.ClampIdleFps(value));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(30, 30)]
    [InlineData(240, 60)]
    public void Typing_fps_is_clamped(int value, int expected)
    {
        Assert.Equal(expected, SettingLimits.ClampTypingFps(value));
    }

    [Fact]
    public void Defaults_are_inside_the_ranges()
    {
        Assert.Equal(FpsTarget.DefaultIdleFps, SettingLimits.ClampIdleFps(FpsTarget.DefaultIdleFps));
        Assert.Equal(FpsTarget.DefaultTypingFps, SettingLimits.ClampTypingFps(FpsTarget.DefaultTypingFps));
    }

    [Fact]
    public void Ranges_fit_inside_what_the_frame_pacer_accepts()
    {
        Assert.True(SettingLimits.IdleFpsMin >= FramePacer.MinFps && SettingLimits.TypingFpsMax <= FramePacer.MaxFps);
    }
}
