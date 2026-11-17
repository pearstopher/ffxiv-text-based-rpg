using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ChatTableTests
{
    // Characters seen in game to be a different width from full-width letters and digits.
    private static readonly char[] UnevenWidth = [(char)0x3000, (char)0x30FB, (char)0xFF0D];

    private static string Fw(string ascii) => ChatTable.FullWidth(ascii);

    private static string Pad(int count) => new(ChatTable.Padding, count);

    [Fact]
    public void Full_width_converts_ascii_and_spaces_and_keeps_the_rest()
    {
        Assert.Equal(
            string.Concat("ABC", ((char)0x3000).ToString(), "4.4%").Length,
            Fw("ABC 4.4%").Length);
        Assert.Equal((char)0xFF21, Fw("A")[0]);
        Assert.Equal((char)0xFF10, Fw("0")[0]);
        Assert.Equal((char)0x3000, Fw(" ")[0]);
        Assert.Equal("é→", Fw("é→"));
    }

    [Fact]
    public void Row_pads_every_cell_but_the_last_to_its_width()
    {
        Assert.Equal(Fw("CPU") + Pad(1) + Fw("4%") + Pad(2) + Fw("x y"), ChatTable.Row([4, 4], "CPU", "4%", "x y"));
    }

    [Fact]
    public void Row_keeps_a_gap_after_a_cell_wider_than_its_column()
    {
        Assert.Equal(Fw("ABCDE") + Pad(1) + Fw("x"), ChatTable.Row([3], "ABCDE", "x"));
    }

    [Fact]
    public void Row_turns_spaces_inside_padded_cells_into_padding()
    {
        Assert.Equal(Fw("AB") + Pad(1) + Fw("CD") + Pad(1) + Fw("x"), ChatTable.Row([6], "AB CD", "x"));
    }

    [Fact]
    public void Savings_table_shows_on_off_and_difference()
    {
        var lines = SavingsTable.Lines("Text-only mode", off: new PhaseResult(4.4, 30, 11), on: new PhaseResult(1.6, 10, 1));
        Assert.Equal(
            [
                "Text-only mode",
                Pad(4) + Fw("On") + Pad(5) + Fw("Off") + Pad(4) + Fw("Difference"),
                Fw("CPU") + Pad(1) + Fw("1.6%") + Pad(3) + Fw("4.4%") + Pad(3) + Fw("64% less"),
                Fw("GPU") + Pad(1) + Fw("1%") + Pad(5) + Fw("11%") + Pad(4) + Fw("91% less"),
                Fw("FPS") + Pad(1) + Fw("10") + Pad(5) + Fw("30") + Pad(5) + Fw("67% less"),
            ],
            lines);
    }

    [Fact]
    public void Savings_table_columns_line_up_and_use_only_even_width_characters()
    {
        var lines = SavingsTable.Lines("x", off: new PhaseResult(9.5, 30, 100), on: new PhaseResult(12.25, 60));
        foreach (var line in lines.Skip(1))
        {
            // Everything before the last column (which starts at character 4 + 7 + 7) must be even-width.
            Assert.True(line.Length >= 18, line);
            Assert.DoesNotContain(line[..18], c => UnevenWidth.Contains(c));
        }
    }

    [Fact]
    public void Savings_table_shows_more_same_and_na()
    {
        var lines = SavingsTable.Lines("x", off: new PhaseResult(0, 30), on: new PhaseResult(1, 30, 2));
        Assert.EndsWith(Fw("NA"), lines[2]);   // zero CPU baseline: no relative change
        Assert.Contains(Fw("NA"), lines[3]);  // no GPU value when off
        Assert.EndsWith(Fw("same"), lines[4]);

        var more = SavingsTable.Lines("x", off: new PhaseResult(5, 30), on: new PhaseResult(6, 30));
        Assert.EndsWith(Fw("20% more"), more[2]);
    }
}
