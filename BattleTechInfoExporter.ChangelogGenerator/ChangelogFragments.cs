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

    internal static (IReadOnlyList<ChangelogFragment> Fragments, IReadOnlyList<string> Errors) Read(
        GitRepository repository)
    {
        var additions = repository.FragmentAdditions()
            .Select((addition, order) => (addition.FileName, addition.Commit, Order: order))
            .ToDictionary(addition => addition.FileName);
        var fragments = new List<ChangelogFragment>();
        var errors = new List<string>();
        var fileNames = Directory.GetFiles(repository.ChangelogDirectory, "*.md")
            .Select(path => Path.GetFileName(path))
            .Where(fileName => FragmentFileName().IsMatch(fileName))
            .OrderBy(fileName => additions.TryGetValue(fileName, out var addition) ? addition.Order : int.MaxValue)
            .ThenBy(fileName => fileName, StringComparer.Ordinal);
        foreach (var fileName in fileNames)
        {
            var body = File.ReadAllText(Path.Combine(repository.ChangelogDirectory, fileName)).Trim();
            if (!body.StartsWith(RequiredStart, StringComparison.Ordinal))
            {
                errors.Add(
                    $"changelog/{fileName} must start with \"{RequiredStart}\" and the files whose content changed");
            }

            fragments.Add(new ChangelogFragment(
                fileName,
                body,
                additions.TryGetValue(fileName, out var addition)
                    ? repository.ReleaseContaining(addition.Commit)
                    : null));
        }

        return (fragments, errors);
    }

    [GeneratedRegex(@"^\d+-[a-z0-9-]+\.md$")]
    private static partial Regex FragmentFileName();
}
