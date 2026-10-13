using System.Text.RegularExpressions;
using BattleTech;
using BattleTech.UI.Tooltips;

namespace BattleTechInfoExporter.Export;

/// <summary>Turns the game's display texts into the plain text the export holds.</summary>
internal static class GameText
{
    // TextMeshPro's formatting tags, e.g. <i> or <color=#F04228>, which the game's texts carry for its UI.
    private static readonly Regex FormattingTag = new("</?[a-zA-Z][^>]*>");

    // An event text's switch on a pilot's gender, e.g. {TGT_MW.Gender?NonBinary:are|Default:is}, which the game's
    // interpolation resolves to one branch (Interpolator); the export keeps the default one.
    private static readonly Regex GenderSwitch = new(@"\{[A-Za-z_]+\.Gender\?[^}]*Default:([^}|]*)[^}]*\}");

    /// <summary>The text without formatting, with a gender switch resolved to its default; other placeholders stay.</summary>
    internal static string ToPlainText(string text) =>
        GenderSwitch.Replace(FormattingTag.Replace(text, string.Empty), "$1");

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
