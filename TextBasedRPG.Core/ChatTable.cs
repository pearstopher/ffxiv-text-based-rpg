using System.Globalization;
using System.Text;

namespace TextBasedRPG.Core;

/// <summary>
/// Small tables for the game's chat. Chat text is proportional, but the game font draws the full-width forms of
/// ASCII letters and digits (U+FF10–FF5A) at one width, so cells made of them line up in columns.
/// </summary>
/// <remarks>
/// Seen in game: the full-width space (U+3000), the katakana middle dot (U+30FB) and the full-width minus (U+FF0D)
/// are each a different width from the letters and digits, so none of them may appear inside a table column. Cells
/// are padded with <see cref="Padding"/>, the full-width full stop (U+FF0E), being checked in game. Spaces inside a
/// cell become padding too. The last cell of a row has nothing after it to line up, so it may contain anything.
/// </remarks>
public static class ChatTable
{
    /// <summary>Full-width (ideographic) space, used between words where nothing has to line up.</summary>
    public const char Space = (char)0x3000;

    /// <summary>Full-width full stop (U+FF0E), used to pad table cells.</summary>
    public const char Padding = (char)0xFF0E;

    /// <summary>Converts printable ASCII to its full-width form (spaces to <see cref="Space"/>); other characters are kept.</summary>
    public static string FullWidth(string text)
    {
        var result = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            result.Append(c switch
            {
                ' ' => Space,
                >= '!' and <= '~' => (char)(c - '!' + 0xFF01),
                _ => c,
            });
        }

        return result.ToString();
    }

    /// <summary>
    /// One row: each cell converted to full width; every cell but the last padded with <see cref="Padding"/> to its
    /// width (in characters), with any spaces in it turned into padding. Cells longer than their width are kept whole.
    /// </summary>
    public static string Row(IReadOnlyList<int> widths, params string[] cells)
    {
        var row = new StringBuilder();
        for (var i = 0; i < cells.Length; i++)
        {
            var cell = FullWidth(cells[i]);
            if (i == cells.Length - 1 || i >= widths.Count)
            {
                row.Append(cell);
                continue;
            }

            cell = cell.Replace(Space, Padding);
            row.Append(cell);
            row.Append(Padding, Math.Max(1, widths[i] - cell.Length));
        }

        return row.ToString();
    }
}

/// <summary>The measured-savings table the self-tests print in chat.</summary>
public static class SavingsTable
{
    // Label column ("CPU" + gap), then the On and Off columns; Difference is last and unpadded.
    private static readonly int[] Widths = [4, 7, 7];

    /// <summary>
    /// A plain title line, a header and CPU/GPU/FPS rows. "On" is the measured change applied, "Off" is normal play,
    /// "Difference" is On relative to Off. E.g. (shown here in ASCII):
    /// <code>
    /// Text-only mode
    /// ....On.....Off....Difference
    /// CPU.1.6%...4.4%...64% less
    /// GPU.1%.....11%....91% less
    /// FPS.10.....30.....67% less
    /// </code>
    /// </summary>
    /// <param name="title">Plain-text title, e.g. "Text-only mode".</param>
    /// <param name="off">The period with the change off (normal play).</param>
    /// <param name="on">The period with the change on.</param>
    public static IReadOnlyList<string> Lines(string title, PhaseResult off, PhaseResult on) =>
    [
        title,
        ChatTable.Row(Widths, string.Empty, "On", "Off", "Difference"),
        ChatTable.Row(Widths, "CPU", Percent(on.CpuPercent, "0.0"), Percent(off.CpuPercent, "0.0"), Difference(off.CpuPercent, on.CpuPercent)),
        ChatTable.Row(Widths, "GPU", Percent(on.GpuPercent, "0"), Percent(off.GpuPercent, "0"), Difference(off.GpuPercent, on.GpuPercent)),
        ChatTable.Row(Widths, "FPS", Number(on.Fps), Number(off.Fps), Difference(off.Fps, on.Fps)),
    ];

    private static string Percent(double? value, string format)
        => value is { } v ? v.ToString(format, CultureInfo.InvariantCulture) + "%" : "NA";

    private static string Number(double value) => value.ToString("0", CultureInfo.InvariantCulture);

    /// <summary>"64% less", "20% more" or "same" (rounded); "NA" without both values or with a zero baseline.</summary>
    private static string Difference(double? off, double? on)
    {
        if (off is not { } before || on is not { } after || before <= 0)
            return "NA";
        var percent = Math.Round((after - before) / before * 100);
        if (percent == 0)
            return "same";
        var amount = Math.Abs(percent).ToString("0", CultureInfo.InvariantCulture);
        return percent < 0 ? $"{amount}% less" : $"{amount}% more";
    }
}
