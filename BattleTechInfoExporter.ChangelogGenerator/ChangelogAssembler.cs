using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BattleTechInfoExporter.ChangelogGenerator;

/// <summary>
///     Builds the shipped changelog: the preamble, an entry per version newer than the history's, newest first, each
///     from the fragments that version first shipped, then the history, the entries written before fragments existed.
/// </summary>
internal static partial class ChangelogAssembler
{
    private const string NoChange = "No export file changed.";

    internal static string Assemble(
        string preamble,
        Version versionBeingBuilt,
        IReadOnlyList<ChangelogFragment> fragments,
        IReadOnlyList<Version> releaseVersions,
        string history)
    {
        var historyVersion = Version.Parse(HistoryHeading().Match(history).Groups[1].Value);
        var historical = fragments.Where(fragment => fragment.Release is not null && fragment.Release <= historyVersion)
            .Select(fragment => fragment.FileName)
            .ToList();
        if (historical.Count > 0)
        {
            throw new InvalidOperationException(
                $"Fragments first released in a version the history already lists: {string.Join(", ", historical)}");
        }

        if (fragments.Any(fragment => fragment.Release is null)
            && (releaseVersions.Contains(versionBeingBuilt) || versionBeingBuilt <= historyVersion))
        {
            throw new InvalidOperationException(
                $"Unreleased fragments exist, but the version being built, {versionBeingBuilt}, isn't new: a changelog "
                + "entry needs a commit whose type bumps the version, computed from the newest release tag the "
                + "branch can see");
        }

        var versions = releaseVersions.Append(versionBeingBuilt).Distinct().Where(version => version > historyVersion);
        var entries = versions.OrderByDescending(version => version)
            .Select(version => Entry(version,
                fragments.Where(fragment => (fragment.Release ?? versionBeingBuilt) == version)));
        return string.Join("\n\n", entries.Prepend(preamble.Trim()).Append(history.Trim())) + "\n";
    }

    private static string Entry(Version version, IEnumerable<ChangelogFragment> fragments) =>
        string.Join(
            "\n\n",
            fragments.Select(fragment => fragment.Body).DefaultIfEmpty(NoChange).Prepend($"## {version}"));

    // The first version heading in the history, which every generated entry is newer than.
    [GeneratedRegex(@"^## (\d+\.\d+\.\d+)$", RegexOptions.Multiline)]
    private static partial Regex HistoryHeading();
}
