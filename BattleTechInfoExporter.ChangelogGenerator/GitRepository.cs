using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BattleTechInfoExporter.ChangelogGenerator;

/// <summary>What the generator asks git about the repository: its release tags and where fragments were added.</summary>
internal sealed partial class GitRepository(string rootDirectory)
{
    private const string ChangelogFolder = "changelog";

    internal string ChangelogDirectory { get; } = Path.Combine(rootDirectory, ChangelogFolder);

    /// <summary>Every release tag's version (<c>v&lt;Major.Minor.Patch&gt;</c>), reachable from HEAD or not.</summary>
    internal IReadOnlyList<Version> ReleaseVersions() => ParseReleaseTags(Run("tag", "--list", "v*"));

    /// <summary>
    ///     The commit that first added each file in the changelog folder, in the order git lists the commits,
    ///     oldest first; a file added again later keeps its first commit.
    /// </summary>
    internal IReadOnlyList<(string FileName, string Commit)> FragmentAdditions()
    {
        var additions = new List<(string FileName, string Commit)>();
        var commit = string.Empty;
        // Rename detection would hide a renamed file's addition; without it, the file is added under its new name.
        foreach (var line in Run(
                     "log", "--format=%H", "--name-only", "--diff-filter=A", "--no-renames", "--reverse", "--",
                     ChangelogFolder))
        {
            if (CommitHash().IsMatch(line))
            {
                commit = line;
            }
            else if (line.Length > 0)
            {
                var fileName = Path.GetFileName(line);
                if (additions.All(addition => addition.FileName != fileName))
                {
                    additions.Add((fileName, commit));
                }
            }
        }

        return additions;
    }

    /// <summary>The release a commit was first in: the lowest release tag containing it, or null before any.</summary>
    internal Version? ReleaseContaining(string commit) =>
        ParseReleaseTags(Run("tag", "--list", "v*", "--contains", commit)).Min();

    private static List<Version> ParseReleaseTags(IEnumerable<string> tags) =>
        tags.Where(tag => tag.Length > 0).Select(tag => Version.Parse(tag[1..])).ToList();

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

    [GeneratedRegex("^[0-9a-f]{40}$")]
    private static partial Regex CommitHash();
}
