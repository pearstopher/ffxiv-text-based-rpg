# Localization

User-facing text lives in language files, not in the code. English (`en.json`) is the first language and the fallback for everything.

## Where things are

| What | Where |
|---|---|
| Language files | `TextBasedRPG/Localization/<code>.json`, copied next to the plugin DLL (`Localization\`) and into the release zip |
| English | `TextBasedRPG/Localization/en.json`, also embedded in `TextBasedRPG.Core` so Core and its tests always have English |
| Expected codes | Dalamud's UI language codes: `en`, `ja`, `de`, `fr` (a file for another code is loaded the same way) |
| Lookup | `Strings.Get("key", args)` (Core). Placeholders are `{0}`, `{1}`… |
| Loading | `Language` (plugin) follows Dalamud's `UiLanguage` and `LanguageChanged`, and loads the file with Dalamud's CheapLoc (`Loc.Setup`, `Loc.Localize`) |

## File format

Dalamud's CheapLoc format. `description` is for translators; only `message` is shown:

```json
{
  "mode.off": {
    "message": "Text-only mode off. Welcome back!",
    "description": "Chat message when the mode turns off."
  }
}
```

## Fallback

- No file for the current language (or `en`): English is used.
- A key missing from a language file: that one string is English, the rest stay translated.
- A key missing from `en.json` too: the key itself is shown (and the tests fail before that can ship).

## Adding a language

1. Copy `en.json` to `<code>.json` in the same folder and translate each `message` (keep keys and `{0}` placeholders).
2. Run the tests: they check that every key exists in `en.json` and that the placeholders match.
3. Add the language to this document.

## Adding text

Add the key to `en.json` (with a `description`), then use `Strings.Get`. Tests fail if code uses a key that `en.json` lacks. Log messages stay English and are not localized. Don't look strings up per frame; look up when you need to show them.
