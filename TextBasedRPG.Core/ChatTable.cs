using System.Globalization;
using System.Text;

namespace TextBasedRPG.Core;

/// <summary>
/// Small tables for the game's chat. Chat text is proportional, but the game font draws full-width characters
/// (U+FF01–FF5E, and the ideographic space U+3000) all at the same width, so text made of them lines up in columns.
/// </summary>
public static class ChatTable
{
    /// <summary>Full-width space, used for padding.</summary>
    public const char Space = '\u3000';

    /// <summary>Converts printable ASCII to its full-width form (spaces to <see cref="Space"/>); other characters are kept.</summary>
    public static string FullWidth(string text)
    {
        var result = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            result.Append(c switch
            {
                ' ' => Space,
                >= '!' and <= '~' => (char)(c - '!' + '\uFF01'),
                _ => c,
            });
        }

        return result.ToString();
    }

    /// <summary>
    /// One row: each cell converted to full width; every cell but the last padded with <see cref="Space"/> to its
    /// width (in characters). Cells longer than their width are kept whole.
    /// </summary>
    public static string Row(IReadOnlyList<int> widths, params string[] cells)
    {
        var row = new StringBuilder();
        for (var i = 0; i < cells.Length; i++)
        {
            var cell = FullWidth(cells[i]);
            row.Append(cell);
            if (i < cells.Length - 1 && i < widths.Count)
                row.Append(Space, Math.Max(1, widths[i] - cell.Length));
        }

        return row.ToString();
    }
}

/// <summary>The measured-savings table the self-tests print in chat.</summary>
public static class SavingsTable
{
    // Label column ("CPU" + gap), then the two value columns; the change column is last and unpadded.
    private static readonly int[] Widths = [4, 7, 7];

    /// <summary>
    /// A title line, a header and CPU/GPU/FPS rows, e.g. (shown here in ASCII):
    /// <code>
    /// Text-only mode
    ///     Normal Mode   Change
    /// CPU 4.4%   1.6%   -64%
    /// GPU 11%    1%     -91%
    /// FPS 30     10     -67%
    /// </code>
    /// </summary>
    /// <param name="title">The table's title, e.g. "Text-only mode".</param>
    /// <param name="column">Header of the changed period's column, e.g. "Mode".</param>
    public static IReadOnlyList<string> Lines(string title, string column, PhaseResult before, PhaseResult after) =>
    [
        ChatTable.FullWidth(title),
        ChatTable.Row(Widths, " ", "Normal", column, "Change"),
        ChatTable.Row(Widths, "CPU", Percent(before.CpuPercent, "0.0"), Percent(after.CpuPercent, "0.0"), Change(before.CpuPercent, after.CpuPercent)),
        ChatTable.Row(Widths, "GPU", Percent(before.GpuPercent, "0"), Percent(after.GpuPercent, "0"), Change(before.GpuPercent, after.GpuPercent)),
        ChatTable.Row(Widths, "FPS", Number(before.Fps), Number(after.Fps), Change(before.Fps, after.Fps)),
    ];

    private static string Percent(double? value, string format)
        => value is { } v ? v.ToString(format, CultureInfo.InvariantCulture) + "%" : "n/a";

    private static string Number(double value) => value.ToString("0", CultureInfo.InvariantCulture);

    /// <summary>Relative change, e.g. "-64%" or "+20%"; "n/a" without both values or with a zero baseline.</summary>
    private static string Change(double? before, double? after)
    {
        if (before is not { } b || after is not { } a || b <= 0)
            return "n/a";
        var percent = (a - b) / b * 100;
        return (percent < 0 ? "-" : "+") + Math.Abs(percent).ToString("0", CultureInfo.InvariantCulture) + "%";
    }
}
