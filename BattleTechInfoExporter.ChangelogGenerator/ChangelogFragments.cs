using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BattleTechInfoExporter.ChangelogGenerator;

/// <summary>One PR's changelog entry, a file <c>changelog/&lt;issue&gt;-&lt;slug&gt;.md</c> holding the entry's body.</summary>
/// <param name="FileName">The fragment's file name.</param>
/// <param name="Body">The entry without its version heading.</param>
/// <param name="Release">The release that first shipped it, or null while it's unreleased.</param>
internal sealed record ChangelogFragment(string FileName, string Body, Version? Release);

/// <summary>Reads the fragments in the changelog folder, in the order their commits added them, uncommitted ones last.</summary>
internal static partial class ChangelogFragments
{
    private const string RequiredStart = "Changed: ";

    /// <exception cref="InvalidOperationException">A fragment's body doesn't start as an entry must.</exception>
    internal static IReadOnlyList<ChangelogFragment> Read(GitRepository repository)
    {
        var additions = repository.FragmentAdditions();
        var fragments = Directory.GetFiles(repository.ChangelogDirectory, "*.md")
            .Select(path => Path.GetFileName(path))
            .Where(fileName => FragmentFileName().IsMatch(fileName))
            .Select(fileName => (
                FileName: fileName,
                Body: File.ReadAllText(Path.Combine(repository.ChangelogDirectory, fileName)).Trim(),
                Order: additions.ToList().FindIndex(addition => addition.FileName == fileName)))
            .OrderBy(fragment => fragment.Order >= 0 ? fragment.Order : additions.Count)
            .ThenBy(fragment => fragment.FileName, StringComparer.Ordinal)
            .ToList();
        var malformed = fragments.Where(fragment => !fragment.Body.StartsWith(RequiredStart, StringComparison.Ordinal))
            .Select(fragment => fragment.FileName)
            .ToList();
        if (malformed.Count > 0)
        {
            throw new InvalidOperationException(
                $"Fragments that don't start with \"{RequiredStart}\" and the files whose content changed: {
                    string.Join(", ", malformed)}");
        }

        // A fragment added by a tagged commit needs no lookup; one added by an untagged commit (unreleased, or a
        // merge from main into a branch) asks git which release first shipped it.
        return fragments
            .Select(fragment => new ChangelogFragment(
                fragment.FileName,
                fragment.Body,
                fragment.Order < 0
                    ? null
                    : additions[fragment.Order].Release ?? repository.ReleaseFirstShipping(fragment.FileName)))
            .ToList();
    }

    [GeneratedRegex(@"^\d+-[a-z0-9-]+\.md$")]
    private static partial Regex FragmentFileName();
}
