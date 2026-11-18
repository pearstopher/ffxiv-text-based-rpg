using System;
using System.IO;
using CheapLoc;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Follows Dalamud's UI language: loads <c>Localization\&lt;code&gt;.json</c> next to the plugin DLL with Dalamud's CheapLoc
/// and hands it to <see cref="Strings"/>. English (embedded) is used for a language without a file and for any missing key.
/// </summary>
internal sealed class Language : IDisposable
{
    /// <summary>Folder (next to the plugin DLL) that holds one file per language, named by Dalamud's language code (en, ja, de, fr…).</summary>
    internal const string Folder = "Localization";

    private const string Missing = "\u0000missing";

    internal Language()
    {
        this.Apply(Service.PluginInterface.UiLanguage);
        Service.PluginInterface.LanguageChanged += this.Apply;
    }

    /// <summary>The language code in use.</summary>
    internal string Code { get; private set; } = "en";

    private void Apply(string code)
    {
        this.Code = code;
        try
        {
            var dll = Service.PluginInterface.AssemblyLocation.DirectoryName ?? string.Empty;
            var file = Path.Combine(dll, Folder, code + ".json");
            if (code == "en" || !File.Exists(file))
            {
                Strings.Current.Translation = null; // English only
                return;
            }

            Loc.Setup(File.ReadAllText(file));
            Strings.Current.Translation = key =>
            {
                var text = Loc.Localize(key, Missing);
                return text == Missing ? null : text;
            };
        }
        catch (Exception ex)
        {
            Strings.Current.Translation = null;
            Service.Log.Error(ex, $"Could not load the '{code}' language file; using English");
        }
    }

    public void Dispose()
    {
        Service.PluginInterface.LanguageChanged -= this.Apply;
        Strings.Current.Translation = null;
    }
}
