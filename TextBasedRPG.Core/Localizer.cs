using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TextBasedRPG.Core;

/// <summary>
/// Looks up user-facing text by key. English (<c>en.json</c>, embedded) is always available and is the fallback for
/// a missing language or a missing key; a translation is an optional lookup supplied by the plugin (Dalamud's CheapLoc).
/// </summary>
public sealed partial class Localizer
{
    private readonly IReadOnlyDictionary<string, string> english;

    /// <summary>Creates a localizer over the given English text.</summary>
    public Localizer(IReadOnlyDictionary<string, string> english) => this.english = english;

    /// <summary>The translated text for a key, or null when there is none (then English is used).</summary>
    public Func<string, string?>? Translation { get; set; }

    /// <summary>The text for <paramref name="key"/> in the current language, filled with <paramref name="args"/> (<c>{0}</c>, <c>{1}</c>…).</summary>
    public string Get(string key, params object[] args)
    {
        var text = this.Translation?.Invoke(key);
        if (string.IsNullOrEmpty(text) && !this.english.TryGetValue(key, out text))
            return key; // unknown key: show it, so the mistake is visible

        return args.Length == 0 ? text : string.Format(System.Globalization.CultureInfo.InvariantCulture, text, args);
    }

    /// <summary>Reads a language file in CheapLoc's format (<c>{ "key": { "message": "…", "description": "…" } }</c>) into key → message.</summary>
    public static Dictionary<string, string> Parse(string json)
    {
        var result = new Dictionary<string, string>();
        using var doc = JsonDocument.Parse(json);
        foreach (var entry in doc.RootElement.EnumerateObject())
        {
            if (entry.Value.TryGetProperty("message", out var message) && message.GetString() is { } text)
                result[entry.Name] = text;
        }

        return result;
    }

    /// <summary>The placeholder numbers used in a message, e.g. "{0} and {1}" gives 0 and 1.</summary>
    public static SortedSet<int> Placeholders(string message)
        => [.. PlaceholderPattern().Matches(message).Select(m => int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))];

    /// <summary>The English text embedded in this assembly (from <c>TextBasedRPG/Localization/en.json</c>).</summary>
    public static Dictionary<string, string> LoadEnglish()
    {
        using var stream = typeof(Localizer).Assembly.GetManifestResourceStream("en.json")
            ?? throw new InvalidOperationException("en.json is not embedded");
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    [GeneratedRegex(@"\{(\d+)\}")]
    private static partial Regex PlaceholderPattern();
}

/// <summary>The text used by the plugin: <see cref="Get"/> looks a key up in the current language.</summary>
public static class Strings
{
    /// <summary>The localizer the plugin uses; the plugin sets <see cref="Localizer.Translation"/> from the language.</summary>
    public static Localizer Current { get; } = new(Localizer.LoadEnglish());

    /// <summary>Text for a key in the current language (English when missing).</summary>
    public static string Get(string key, params object[] args) => Current.Get(key, args);
}
