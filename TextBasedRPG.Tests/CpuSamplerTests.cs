using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class CpuSamplerTests
{
    private static TimeSpan Ms(double ms) => TimeSpan.FromMilliseconds(ms);

    [Fact]
    public void No_value_until_two_samples()
    {
        var sampler = new CpuSampler(4);
        Assert.Null(sampler.Current);
        sampler.Add(Ms(0), Ms(0));
        Assert.Null(sampler.Current);
        Assert.Null(sampler.Average);
    }

    [Fact]
    public void Percent_is_share_of_all_cores()
    {
        var sampler = new CpuSampler(4);
        sampler.Add(Ms(0), Ms(0));
        sampler.Add(Ms(1000), Ms(1000)); // one full core of four for a second
        Assert.Equal(25, sampler.Current!.Value, 6);
    }

    [Fact]
    public void Average_rolls_over_the_window()
    {
        var sampler = new CpuSampler(1, windowSize: 2);
        sampler.Add(Ms(0), Ms(0));
        sampler.Add(Ms(100), Ms(1000));   // 10%
        sampler.Add(Ms(400), Ms(2000));   // 30%
        Assert.Equal(20, sampler.Average!.Value, 6);
        sampler.Add(Ms(900), Ms(3000));   // 50%, drops the 10%
        Assert.Equal(40, sampler.Average!.Value, 6);
    }

    [Fact]
    public void Clamps_to_0_to_100()
    {
        var sampler = new CpuSampler(1);
        sampler.Add(Ms(0), Ms(0));
        sampler.Add(Ms(5000), Ms(1000));
        Assert.Equal(100, sampler.Current!.Value);
        sampler.Add(Ms(4000), Ms(2000)); // processor time went backwards
        Assert.Equal(0, sampler.Current!.Value);
    }

    [Fact]
    public void Ignores_samples_with_no_elapsed_time()
    {
        var sampler = new CpuSampler(1);
        sampler.Add(Ms(0), Ms(0));
        sampler.Add(Ms(500), Ms(0));
        Assert.Null(sampler.Current);
        sampler.Add(Ms(500), Ms(1000)); // measured from the first sample
        Assert.Equal(50, sampler.Current!.Value, 6);
    }

    [Fact]
    public void Reset_clears_the_average_but_keeps_measuring()
    {
        var sampler = new CpuSampler(1);
        sampler.Add(Ms(0), Ms(0));
        sampler.Add(Ms(900), Ms(1000));
        sampler.ResetAverage();
        Assert.Null(sampler.Average);
        sampler.Add(Ms(1000), Ms(2000));
        Assert.Equal(10, sampler.Average!.Value, 6);
    }

    [Fact]
    public void Rejects_bad_arguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CpuSampler(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CpuSampler(1, 0));
    }
}
