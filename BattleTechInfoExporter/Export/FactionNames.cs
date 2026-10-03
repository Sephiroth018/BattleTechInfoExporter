using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Localize;

namespace BattleTechInfoExporter.Export;

/// <summary>Turns a faction's names from the game data into a title-case display name.</summary>
internal static class FactionNames
{
    private const string LeadingArticle = "the ";

    // English title case keeps these lowercase unless they start the name ("The Magistracy of Canopus").
    private static readonly HashSet<string> MinorWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "as", "at", "but", "by", "for", "in", "nor", "of", "on", "or", "the", "to"
    };

    /// <summary>
    ///     The full name with the short name in brackets, e.g. "The Federated Suns (Davion)". The game's
    ///     names are written for use mid-sentence ("the local pirate organization"), so both get title case.
    ///     Some factions (e.g. placeholders) have no <see cref="FactionDef" />; the enumeration's name stands in.
    /// </summary>
    internal static string Format(FactionValue faction)
    {
        var definition = faction.FactionDef;
        if (definition is null)
        {
            return faction.FriendlyName;
        }

        var name = ToTitleCase(Strings.T(definition.Name));
        var shortName = ToTitleCase(WithoutLeadingArticle(Strings.T(definition.ShortName)));
        return shortName.Length == 0 ||
               WithoutLeadingArticle(name).Equals(shortName, StringComparison.OrdinalIgnoreCase)
            ? name
            : $"{name} ({shortName})";
    }

    private static string WithoutLeadingArticle(string text) =>
        text.StartsWith(LeadingArticle, StringComparison.OrdinalIgnoreCase)
            ? text.Substring(LeadingArticle.Length)
            : text;

    // Only first letters change, so names like "ComStar" or "LLC" keep their casing.
    private static string ToTitleCase(string text) =>
        string.Join(
            " ",
            text.Split(' ')
                .Select((word, index) =>
                    index > 0 && MinorWords.Contains(word) ? word.ToLowerInvariant() : Capitalize(word)));

    private static string Capitalize(string word) =>
        word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word.Substring(1);
}
