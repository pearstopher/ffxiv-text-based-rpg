using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class FramePacerTests
{
    [Theory]
    [InlineData(10, 0, 100)]
    [InlineData(10, 15, 85)]
    [InlineData(10, 99.5, 0)]   // under a millisecond left: don't sleep
    [InlineData(10, 250, 0)]    // frame already too long
    [InlineData(30, 3, 30)]     // 33.3 - 3 = 30.3 → 30
    [InlineData(60, 0, 16)]
    public void Sleeps_for_the_rest_of_the_frame(int fps, double elapsed, int expected)
    {
        Assert.Equal(expected, FramePacer.SleepMs(fps, elapsed));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void No_cap_means_no_sleep(int fps)
    {
        Assert.Equal(0, FramePacer.SleepMs(fps, 0));
    }

    [Fact]
    public void Clamps_the_target_frame_rate()
    {
        Assert.Equal(FramePacer.SleepMs(60, 0), FramePacer.SleepMs(500, 0));
        Assert.Equal(1000, FramePacer.SleepMs(1, 0));
    }

    [Fact]
    public void Bad_elapsed_values_are_safe()
    {
        Assert.Equal(0, FramePacer.SleepMs(10, double.NaN));
        Assert.Equal(100, FramePacer.SleepMs(10, -20)); // clock went backwards: treat as no work done
    }
}
