using System.Globalization;
using System.Text;

namespace TextBasedRPG.Core;

/// <summary>
/// Small tables for the game's chat, laid out in plain ASCII. Chat text is proportional, so the plugin draws each
/// table character with a game glyph of one fixed width (TextBasedRPG.ChatGlyphs): boxed letters and numbers for
/// A–Z and 0–9, the full-width space for a space, and '%' as is.
/// </summary>
/// <remarks>
/// Owner-checked in game: those glyphs, the full-width space and '%' are all the same width. Full-width ASCII forms,
/// the full-width full stop, the middle dot and the minus are not, so a row may only contain
/// <see cref="IsTableChar"/> characters. There's no decimal point, so tables show whole numbers.
/// </remarks>
public static class ChatTable
{
    /// <summary>True for the characters a table row may contain: A–Z, 0–9, '%' and space.</summary>
    public static bool IsTableChar(char c) => c is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '%' or ' ';

    /// <summary>
    /// One row in uppercase: every cell but the last padded with spaces to its width (in characters), with at least
    /// one space after it. Characters a table can't show are left out.
    /// </summary>
    public static string Row(IReadOnlyList<int> widths, params string[] cells)
    {
        var row = new StringBuilder();
        for (var i = 0; i < cells.Length; i++)
        {
            var cell = Clean(cells[i]);
            row.Append(cell);
            if (i < cells.Length - 1 && i < widths.Count)
                row.Append(' ', Math.Max(1, widths[i] - cell.Length));
        }

        return row.ToString();
    }

    private static string Clean(string cell)
    {
        var result = new StringBuilder(cell.Length);
        foreach (var c in cell.ToUpperInvariant())
        {
            if (IsTableChar(c))
                result.Append(c);
        }

        return result.ToString();
    }
}

/// <summary>A measurement's chat table: a plain-text title, then rows made only of <see cref="ChatTable.IsTableChar"/> characters.</summary>
public sealed record SavingsTableText(string Title, IReadOnlyList<string> Rows);

/// <summary>The measured-savings table the self-tests print in chat.</summary>
public static class SavingsTable
{
    // Label column ("CPU" + gap), then Before and After; Difference is last and unpadded.
    private static readonly int[] Widths = [4, 7, 6];

    /// <summary>
    /// CPU/GPU/FPS rows. "Before" is normal play, "After" is with the measured change, "Difference" is After compared
    /// with Before (from the unrounded values). E.g.:
    /// <code>
    ///     BEFORE AFTER DIFFERENCE
    /// CPU 4%     2%    64% LESS
    /// GPU 11%    1%    91% LESS
    /// FPS 30     10    67% LESS
    /// </code>
    /// </summary>
    /// <param name="title">Plain-text title, saying what the change is, e.g. "Text-only mode".</param>
    /// <param name="before">The period before the change (normal play).</param>
    /// <param name="after">The period with the change.</param>
    public static SavingsTableText Create(string title, PhaseResult before, PhaseResult after) => new(
        title,
        [
            ChatTable.Row(Widths, string.Empty, "Before", "After", "Difference"),
            ChatTable.Row(Widths, "CPU", Percent(before.CpuPercent), Percent(after.CpuPercent), Difference(before.CpuPercent, after.CpuPercent)),
            ChatTable.Row(Widths, "GPU", Percent(before.GpuPercent), Percent(after.GpuPercent), Difference(before.GpuPercent, after.GpuPercent)),
            ChatTable.Row(Widths, "FPS", Whole(before.Fps), Whole(after.Fps), Difference(before.Fps, after.Fps)),
        ]);

    private static string Percent(double? value) => value is { } v ? Whole(v) + "%" : "NA";

    private static string Whole(double value) => Math.Round(value).ToString("0", CultureInfo.InvariantCulture);

    /// <summary>"64% less", "20% more" or "same" (rounded); "NA" without both values or with a zero baseline.</summary>
    private static string Difference(double? before, double? after)
    {
        if (before is not { } b || after is not { } a || b <= 0)
            return "NA";
        var percent = Math.Round((a - b) / b * 100);
        if (percent == 0)
            return "same";
        return $"{Math.Abs(percent).ToString("0", CultureInfo.InvariantCulture)}% {(percent < 0 ? "less" : "more")}";
    }
}
