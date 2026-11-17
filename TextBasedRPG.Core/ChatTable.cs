using System.Globalization;
using System.Text;

namespace TextBasedRPG.Core;

/// <summary>
/// Small tables for the game's chat, laid out in plain ASCII. Chat text is proportional, so the plugin draws each
/// table character with a game glyph of one fixed width (TextBasedRPG.ChatGlyphs): boxed letters and numbers for
/// A–Z and 0–9, the full-width space for a space, and '%' as is.
/// </summary>
/// <remarks>
/// Owner-checked in game: the boxed glyphs and the full-width space are all one width; '%' is slightly different.
/// So columns are padded by counting only the non-'%' characters, and a table must have the same number of '%'
/// before each column start on every row (the savings table has exactly one '%' per column). Full-width ASCII forms,
/// the full-width full stop, the middle dot and the minus are other widths again, so a row may only contain
/// <see cref="IsTableChar"/> characters. There's no decimal point, so tables show whole numbers.
/// </remarks>
public static class ChatTable
{
    /// <summary>True for the characters a table row may contain: A–Z, 0–9, '%' and space.</summary>
    public static bool IsTableChar(char c) => c is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '%' or ' ';

    /// <summary>
    /// One row in uppercase: every cell but the last padded with spaces to its width, counted in characters other
    /// than '%' (see remarks), with at least one space after it. Characters a table can't show are left out.
    /// </summary>
    public static string Row(IReadOnlyList<int> widths, params string[] cells)
    {
        var row = new StringBuilder();
        for (var i = 0; i < cells.Length; i++)
        {
            var cell = Clean(cells[i]);
            row.Append(cell);
            if (i < cells.Length - 1 && i < widths.Count)
                row.Append(' ', Math.Max(1, widths[i] - GlyphCount(cell)));
        }

        return row.ToString();
    }

    /// <summary>Characters in <paramref name="text"/> other than '%' (the ones of the common width).</summary>
    public static int GlyphCount(string text)
    {
        var count = 0;
        foreach (var c in text)
        {
            if (c != '%')
                count++;
        }

        return count;
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
    // Label column ("CPU" + gap), then Before and After (in non-'%' characters); Diff is last and unpadded.
    private static readonly int[] Widths = [4, 7, 6];

    /// <summary>
    /// CPU and GPU rows, each with exactly one '%' per column (as the header), so the columns line up. "Before" is
    /// normal play, "After" is with the measured change, "Diff" is how much lower After is than Before (from the
    /// unrounded values), with "MORE" added if it's higher. GPU is left out if it wasn't measured. E.g.:
    /// <code>
    ///     BEFORE% AFTER% DIFF%
    /// CPU 4%      2%     64%
    /// GPU 11%     1%     91%
    /// </code>
    /// </summary>
    /// <param name="title">Plain-text title, saying what the change is, e.g. "Text-only mode".</param>
    /// <param name="before">The period before the change (normal play).</param>
    /// <param name="after">The period with the change.</param>
    public static SavingsTableText Create(string title, PhaseResult before, PhaseResult after)
    {
        var rows = new List<string>
        {
            ChatTable.Row(Widths, string.Empty, "Before%", "After%", "Diff%"),
            Row("CPU", before.CpuPercent, after.CpuPercent),
        };
        if (before.GpuPercent is { } gpuBefore && after.GpuPercent is { } gpuAfter)
            rows.Add(Row("GPU", gpuBefore, gpuAfter));
        return new SavingsTableText(title, rows);
    }

    private static string Row(string label, double before, double after)
        => ChatTable.Row(Widths, label, Percent(before), Percent(after), Difference(before, after));

    private static string Percent(double value) => Math.Round(value).ToString("0", CultureInfo.InvariantCulture) + "%";

    /// <summary>"64%" when lower, "20% MORE" when higher, "0%" when the same (rounded) or with a zero baseline.</summary>
    private static string Difference(double before, double after)
    {
        var percent = before > 0 ? Math.Round((after - before) / before * 100) : 0;
        var text = Math.Abs(percent).ToString("0", CultureInfo.InvariantCulture) + "%";
        return percent > 0 ? text + " more" : text;
    }
}
