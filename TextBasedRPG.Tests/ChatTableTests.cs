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
        Assert.Equal("CPU 4%  X Y", ChatTable.Row([4, 4], "cpu", "4.%", "x y"));
    }

    [Fact]
    public void Row_keeps_a_gap_after_a_cell_wider_than_its_column()
    {
        Assert.Equal("ABCDE X", ChatTable.Row([3], "ABCDE", "x"));
    }

    [Fact]
    public void Savings_table_shows_before_after_and_difference()
    {
        var table = SavingsTable.Create("Text-only mode", before: new PhaseResult(4.4, 30, 11), after: new PhaseResult(1.6, 10, 1));
        Assert.Equal("Text-only mode", table.Title);
        Assert.Equal(
            [
                "    BEFORE AFTER DIFFERENCE",
                "CPU 4%     2%    64% LESS",
                "GPU 11%    1%    91% LESS",
                "FPS 30     10    67% LESS",
            ],
            table.Rows);
    }

    [Fact]
    public void Savings_table_columns_line_up_and_use_only_table_characters()
    {
        var table = SavingsTable.Create("x", before: new PhaseResult(100, 144, 100), after: new PhaseResult(9.5, 60));
        Assert.All(table.Rows, row => Assert.All(row, c => Assert.True(ChatTable.IsTableChar(c), $"'{c}' in \"{row}\"")));

        // The Difference column starts at the same position in every row (4 + 7 + 6).
        Assert.All(table.Rows, row => Assert.True(row[16] == ' ' && row[17] != ' ', row));
    }

    [Fact]
    public void Savings_table_shows_more_same_and_na()
    {
        var table = SavingsTable.Create("x", before: new PhaseResult(0, 30), after: new PhaseResult(1, 30, 2));
        Assert.EndsWith(" NA", table.Rows[1]);          // zero CPU baseline: no relative change
        Assert.StartsWith("GPU NA     2%", table.Rows[2]); // no GPU value before
        Assert.EndsWith(" SAME", table.Rows[3]);

        var more = SavingsTable.Create("x", before: new PhaseResult(5, 30), after: new PhaseResult(6, 30));
        Assert.EndsWith(" 20% MORE", more.Rows[1]);
    }
}
