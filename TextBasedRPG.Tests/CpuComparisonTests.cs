using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class CpuComparisonTests
{
    private static void AddMany(CpuComparison c, double value, int count)
    {
        for (var i = 0; i < count; i++)
            c.Add(value);
    }

    [Fact]
    public void Nothing_to_show_until_both_modes_have_data()
    {
        var c = new CpuComparison();
        AddMany(c, 12, 5);
        Assert.Null(c.Describe());
        Assert.Equal(12, c.NormalAverage);
        Assert.Null(c.ModeAverage);
    }

    [Fact]
    public void Shows_both_averages_and_the_saving()
    {
        var c = new CpuComparison();
        AddMany(c, 12, 5);
        c.SetMode(true);
        AddMany(c, 99, CpuComparison.SettleSamples); // settling: ignored
        AddMany(c, 3, 5);
        Assert.Equal("Normal: 12.0% · Text mode: 3.0% (75% less)", c.Describe());
    }

    [Fact]
    public void Settle_samples_are_skipped_after_switching_back()
    {
        var c = new CpuComparison();
        c.SetMode(true);
        AddMany(c, 50, CpuComparison.SettleSamples);
        c.Add(4);
        c.SetMode(false);
        AddMany(c, 50, CpuComparison.SettleSamples);
        c.Add(10);
        Assert.Equal(10, c.NormalAverage);
        Assert.Equal(4, c.ModeAverage);
    }

    [Fact]
    public void Setting_the_same_mode_again_does_not_skip_samples()
    {
        var c = new CpuComparison();
        c.SetMode(false);
        c.Add(7);
        Assert.Equal(7, c.NormalAverage);
    }

    [Fact]
    public void Reports_an_increase_honestly()
    {
        var c = new CpuComparison();
        c.Add(4);
        c.SetMode(true);
        AddMany(c, 0, CpuComparison.SettleSamples);
        c.Add(5);
        Assert.EndsWith("(25% more)", c.Describe());
    }
}
