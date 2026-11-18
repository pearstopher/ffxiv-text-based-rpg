using System.Globalization;

namespace TextBasedRPG.Core;

/// <summary>
/// Keeps separate average CPU use with the mode off and on (this session, in memory only), to show the player how
/// much the mode saves on their PC. The first few samples after each switch are skipped while things settle.
/// </summary>
public sealed class CpuComparison
{
    /// <summary>Samples ignored after each switch.</summary>
    public const int SettleSamples = 3;

    private double offSum, onSum;
    private int offCount, onCount;
    private bool modeOn;
    private int toSkip;

    /// <summary>Average CPU % with the mode off, or null without data.</summary>
    public double? NormalAverage => this.offCount == 0 ? null : this.offSum / this.offCount;

    /// <summary>Average CPU % with the mode on, or null without data.</summary>
    public double? ModeAverage => this.onCount == 0 ? null : this.onSum / this.onCount;

    /// <summary>Tells the comparison the mode switched.</summary>
    public void SetMode(bool on)
    {
        if (on == this.modeOn)
            return;
        this.modeOn = on;
        this.toSkip = SettleSamples;
    }

    /// <summary>Adds one CPU % sample (once per second) to the current mode's average.</summary>
    public void Add(double cpuPercent)
    {
        if (this.toSkip > 0)
        {
            this.toSkip--;
            return;
        }

        if (this.modeOn)
        {
            this.onSum += cpuPercent;
            this.onCount++;
        }
        else
        {
            this.offSum += cpuPercent;
            this.offCount++;
        }
    }

    /// <summary>E.g. "Normal: 12.0% · Text mode: 3.0% (75% less)", or null until both have data.</summary>
    public string? Describe()
    {
        if (this.NormalAverage is not { } normal || this.ModeAverage is not { } mode)
            return null;

        var text = Strings.Get("cpu.compare", normal, mode);
        if (normal <= 0)
            return text;

        var change = (normal - mode) / normal * 100;
        return change >= 0
            ? Strings.Get("cpu.compare.less", text, change)
            : Strings.Get("cpu.compare.more", text, -change);
    }
}
