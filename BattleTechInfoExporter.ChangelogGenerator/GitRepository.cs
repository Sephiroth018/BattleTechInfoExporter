using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BattleTechInfoExporter.ChangelogGenerator;

/// <summary>A file's first addition on the first-parent chain from HEAD.</summary>
/// <param name="FileName">The file's name.</param>
/// <param name="Release">The release tagged on the commit that added it, or null when it carries none.</param>
internal sealed record FileAddition(string FileName, Version? Release);

/// <summary>
///     What the generator asks git about the repository: its release tags and where fragments were added. Only
///     tags reachable from HEAD count, the same view the build computes the version from.
/// </summary>
internal sealed partial class GitRepository(string rootDirectory)
{
    private const string ChangelogFolder = "changelog";

    internal string ChangelogDirectory { get; } = Path.Combine(rootDirectory, ChangelogFolder);

    /// <summary>Every release's version (tag <c>v&lt;Major.Minor.Patch&gt;</c>).</summary>
    internal IReadOnlyList<Version> ReleaseVersions() => ReleaseTags(Run("tag", "--list", "v*", "--merged", "HEAD"));

    /// <summary>
    ///     Each file in the changelog folder by the commit that first added it on the first-parent chain, oldest
    ///     first, with the release tagged on that commit. Merges count what they bring in as added, so on main a
    ///     fragment is added by its PR's merge commit, the one its release tags.
    /// </summary>
    internal IReadOnlyList<FileAddition> FragmentAdditions()
    {
        var additions = new List<FileAddition>();
        Version? release = null;
        // Rename detection would hide a renamed file's addition; without it, the file is added under its new name.
        foreach (var line in Run(
                     "log", "--first-parent", "--diff-merges=first-parent", "--reverse", "--format=%x01%D",
                     "--name-only", "--diff-filter=A", "--no-renames", "--", ChangelogFolder))
        {
            if (line.StartsWith('\x01'))
            {
                release = ReleaseTags(line[1..].Split(", ")
                        .Select(decoration => decoration.Replace("tag: ", string.Empty, StringComparison.Ordinal)))
                    .Min();
            }
            else if (line.Length > 0)
            {
                additions.Add(new FileAddition(Path.GetFileName(line), release));
            }
        }

        return additions.DistinctBy(addition => addition.FileName).ToList();
    }

    /// <summary>
    ///     The release that first shipped a file in the changelog folder: the lowest release tag containing the
    ///     commit that first added it, anywhere in history, or null while none does.
    /// </summary>
    internal Version? ReleaseFirstShipping(string fileName)
    {
        var addingCommit = Run(
                "log", "--format=%H", "--diff-filter=A", "--no-renames", "--reverse", "--",
                $"{ChangelogFolder}/{fileName}")
            .First(line => line.Length > 0);
        return ReleaseTags(Run("tag", "--list", "v*", "--merged", "HEAD", "--contains", addingCommit)).Min();
    }

    private static List<Version> ReleaseTags(IEnumerable<string> tags) =>
        tags.Select(tag => ReleaseTag().Match(tag))
            .Where(match => match.Success)
            .Select(match => Version.Parse(match.Groups[1].Value))
            .ToList();

    private string[] Run(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(rootDirectory);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("git didn't start");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0
            ? output.Split('\n', StringSplitOptions.TrimEntries)
            : throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed: {error.Trim()}");
    }

    // The same rule as the build's: any other tag isn't a release.
    [GeneratedRegex(@"^v(\d+\.\d+\.\d+)$")]
    private static partial Regex ReleaseTag();
}
