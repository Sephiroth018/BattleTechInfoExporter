using System.Text.RegularExpressions;
using BattleTech;
using BattleTech.UI.Tooltips;

namespace BattleTechInfoExporter.Export;

/// <summary>Turns the game's display texts into the plain text the export holds.</summary>
internal static class GameText
{
    // TextMeshPro's formatting tags, e.g. <i> or <color=#F04228>, which the game's texts carry for its UI.
    private static readonly Regex FormattingTag = new("</?[a-zA-Z][^>]*>");

    internal static string ToPlainText(string text) => FormattingTag.Replace(text, string.Empty);

    /// <summary>
    ///     A stat result's text with its tooltip links resolved, as LocalizableText shows it with links disabled.
    /// </summary>
    internal static string ToPlainText(ResultDescriptionEntry description)
    {
        var parser = new TextTooltipParser();
        parser.Parse(description.Text);
        parser.SetFormattingEnabled(false);
        return ToPlainText(parser.ToTMP(description.Context, null).ToString()).Trim();
    }
}
