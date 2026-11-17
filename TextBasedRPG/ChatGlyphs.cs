using System.Text;
using Dalamud.Game.Text;
using TextBasedRPG.Core;

namespace TextBasedRPG;

/// <summary>
/// Draws a <see cref="ChatTable"/> row with game glyphs that are all the same width in the chat font (owner-checked
/// in game): Dalamud's <see cref="SeIconChar"/> boxed letters and numbers for A–Z and 0–9, the full-width space
/// (U+3000) for a space, and '%' as is.
/// </summary>
internal static class ChatGlyphs
{
    private const char FullWidthSpace = (char)0x3000;

    /// <summary>The row with each table character replaced by its game glyph.</summary>
    internal static string ToGame(string row)
    {
        var result = new StringBuilder(row.Length);
        foreach (var c in row)
        {
            result.Append(c switch
            {
                >= 'A' and <= 'Z' => (char)((int)SeIconChar.BoxedLetterA + (c - 'A')),
                >= '0' and <= '9' => (char)((int)SeIconChar.BoxedNumber0 + (c - '0')),
                ' ' => FullWidthSpace,
                _ => c,
            });
        }

        return result.ToString();
    }
}
