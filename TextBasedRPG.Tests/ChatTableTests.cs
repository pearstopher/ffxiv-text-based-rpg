using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ChatTableTests
{
    [Fact]
    public void Table_chars_are_only_capitals_digits_percent_and_space()
    {
        Assert.All("AZ09% ", c => Assert.True(ChatTable.IsTableChar(c)));
        Assert.All("az.-/,", c => Assert.False(ChatTable.IsTableChar(c)));
    }

    [Fact]
    public void Row_uppercases_pads_and_drops_other_characters()
    {
        Assert.Equal("CPU 4%   X Y", ChatTable.Row([4, 4], "cpu", "4.%", "x y")); // "4%" is one glyph wide
    }

    [Fact]
    public void Row_keeps_a_gap_after_a_cell_wider_than_its_column()
    {
        Assert.Equal("ABCDE X", ChatTable.Row([3], "ABCDE", "x"));
    }

    [Fact]
    public void Row_pads_by_characters_other_than_percent()
    {
        Assert.Equal("AB%  C", ChatTable.Row([4], "ab%", "c"));
        Assert.Equal(2, ChatTable.GlyphCount("4%%2"));
    }

    [Fact]
    public void Savings_table_shows_before_after_and_difference()
    {
        var table = SavingsTable.Create("Text-only mode", before: new PhaseResult(4.4, 30, 11), after: new PhaseResult(1.6, 10, 1));
        Assert.Equal("Text-only mode", table.Title);
        Assert.Equal(
            [
                "    BEFORE% AFTER% DIFF%",
                "CPU 4%      2%     64%",
                "GPU 11%     1%     91%",
            ],
            table.Rows);
    }

    [Fact]
    public void Savings_table_has_one_percent_per_column_and_lines_up()
    {
        var table = SavingsTable.Create("x", before: new PhaseResult(100, 144, 100), after: new PhaseResult(9.5, 60, 0));
        foreach (var row in table.Rows)
        {
            Assert.All(row, c => Assert.True(ChatTable.IsTableChar(c), $"'{c}' in \"{row}\""));
            Assert.Equal(3, row.Count(c => c == '%'));

            // The Diff column starts after the same number of glyphs (4 + 7 + 6) and the same number of '%' (2).
            var diffStart = row.TrimEnd().LastIndexOf(' ', row.IndexOf('%', row.IndexOf('%', row.IndexOf('%') + 1) + 1)) + 1;
            Assert.Equal(17, ChatTable.GlyphCount(row[..diffStart]));
            Assert.Equal(2, row[..diffStart].Count(c => c == '%'));
        }
    }

    [Fact]
    public void Savings_table_says_more_only_for_an_increase_and_skips_missing_gpu()
    {
        var more = SavingsTable.Create("x", before: new PhaseResult(5, 30), after: new PhaseResult(6, 30));
        Assert.Equal(2, more.Rows.Count); // no GPU row without GPU values
        Assert.Equal("CPU 5%      6%     20% MORE", more.Rows[1]);

        var same = SavingsTable.Create("x", before: new PhaseResult(0, 30, 3), after: new PhaseResult(1, 30, 3));
        Assert.EndsWith(" 0%", same.Rows[1]);  // zero CPU baseline
        Assert.EndsWith(" 0%", same.Rows[2]);  // unchanged GPU
    }
}
