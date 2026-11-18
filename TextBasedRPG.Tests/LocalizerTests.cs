using System.Text.RegularExpressions;
using TextBasedRPG.Core;
using TextBasedRPG.Tests.Support;

namespace TextBasedRPG.Tests;

public partial class LocalizerTests
{
    private static Dictionary<string, string> English() => Localizer.LoadEnglish();

    [Fact]
    public void English_is_embedded_and_matches_the_file()
        => Assert.Equal(Localizer.Parse(Repo.Read("TextBasedRPG", "Localization", "en.json")), English());

    [Fact]
    public void Fills_in_placeholders()
        => Assert.Equal("a-b", new Localizer(new Dictionary<string, string> { ["k"] = "{0}-{1}" }).Get("k", "a", "b"));

    [Fact]
    public void Unknown_key_shows_the_key()
        => Assert.Equal("nope", new Localizer(English()).Get("nope"));

    [Fact]
    public void A_translation_is_used_when_it_has_the_key()
    {
        var loc = new Localizer(English()) { Translation = k => k == "mode.off" ? "Au revoir" : null };
        Assert.Equal("Au revoir", loc.Get("mode.off"));
    }

    [Fact]
    public void A_missing_translation_key_falls_back_to_english()
    {
        var loc = new Localizer(English()) { Translation = _ => null };
        Assert.Equal(English()["mode.off"], loc.Get("mode.off"));
    }

    [Fact]
    public void Parse_reads_the_message_of_each_entry()
        => Assert.Equal("x", Localizer.Parse("""{ "a": { "message": "x", "description": "d" } }""")["a"]);

    [Fact]
    public void Placeholders_lists_the_numbers()
        => Assert.Equal(new[] { 0, 1 }, Localizer.Placeholders("{1} then {0} then {1}"));

    [Fact]
    public void Every_key_used_in_code_exists_in_en_json()
    {
        var english = English();
        var used = new List<string>();
        foreach (var file in Directory.EnumerateFiles(Repo.Root, "*.cs", SearchOption.AllDirectories)
                     .Where(f => !f.Contains($"{System.IO.Path.DirectorySeparatorChar}obj{System.IO.Path.DirectorySeparatorChar}")
                                 && !f.Contains($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}")
                                 && !f.EndsWith("Tests.cs", StringComparison.Ordinal)))
        {
            // Every key-shaped literal on a line that calls Strings.Get (a key may sit in a ternary).
            used.AddRange(File.ReadLines(file).Where(l => l.Contains("Strings.Get(", StringComparison.Ordinal))
                .SelectMany(l => KeyPattern().Matches(l).Select(m => m.Groups[1].Value)));
        }

        Assert.NotEmpty(used);
        Assert.Empty(used.Where(k => !english.ContainsKey(k)).Distinct());
    }

    [Fact]
    public void Every_other_language_file_uses_known_keys_and_the_same_placeholders()
    {
        var english = English();
        var files = Directory.EnumerateFiles(Repo.Path("TextBasedRPG", "Localization"), "*.json")
            .Where(f => System.IO.Path.GetFileNameWithoutExtension(f) != "en");
        foreach (var file in files)
        {
            foreach (var (key, message) in Localizer.Parse(File.ReadAllText(file)))
            {
                Assert.True(english.ContainsKey(key), $"{file}: unknown key {key}");
                Assert.Equal(Localizer.Placeholders(english[key]), Localizer.Placeholders(message));
            }
        }
    }

    [GeneratedRegex(@"""([a-z][A-Za-z0-9]*(?:\.[A-Za-z0-9]+)+)""")]
    private static partial Regex KeyPattern();
}
