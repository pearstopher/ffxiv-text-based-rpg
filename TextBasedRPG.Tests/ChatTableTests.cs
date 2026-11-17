using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class ChatTableTests
{
    [Fact]
    public void Full_width_converts_ascii_and_spaces_and_keeps_the_rest()
    {
        Assert.Equal("ＣＰＵ　４．４％　－６４％　ｎ／ａ", ChatTable.FullWidth("CPU 4.4% -64% n/a"));
        Assert.Equal("é→", ChatTable.FullWidth("é→"));
    }

    [Fact]
    public void Row_pads_every_cell_but_the_last_to_its_width()
    {
        Assert.Equal("ＣＰＵ　４％　　ｘ", ChatTable.Row([4, 4], "CPU", "4%", "x"));
    }

    [Fact]
    public void Row_keeps_a_gap_after_a_cell_wider_than_its_column()
    {
        Assert.Equal("ＡＢＣＤＥ　ｘ", ChatTable.Row([3], "ABCDE", "x"));
    }

    [Fact]
    public void Savings_table_lines_up_and_shows_every_change()
    {
        var lines = SavingsTable.Lines("Text-only mode", "Mode", new PhaseResult(4.4, 30, 11), new PhaseResult(1.6, 10, 1));
        Assert.Equal(
            [
                "Ｔｅｘｔ－ｏｎｌｙ　ｍｏｄｅ",
                "　　　　Ｎｏｒｍａｌ　Ｍｏｄｅ　　　Ｃｈａｎｇｅ",
                "ＣＰＵ　４．４％　　　１．６％　　　－６４％",
                "ＧＰＵ　１１％　　　　１％　　　　　－９１％",
                "ＦＰＳ　３０　　　　　１０　　　　　－６７％",
            ],
            lines);

        // Every row's change column starts at the same character position (all characters are full width).
        var columns = lines.Skip(1).Select(l => l.Length - l.Split('\u3000', StringSplitOptions.RemoveEmptyEntries)[^1].Length).Distinct();
        Assert.Single(columns);
    }

    [Fact]
    public void Savings_table_shows_na_without_gpu_or_baseline()
    {
        var lines = SavingsTable.Lines("x", "3D off", new PhaseResult(0, 30), new PhaseResult(1, 30, 2));
        Assert.Equal("ＧＰＵ　ｎ／ａ　　　　２％　　　　　ｎ／ａ", lines[3]);
        Assert.EndsWith("ｎ／ａ", lines[2]); // zero CPU baseline: no relative change
    }
}
