using System;
using System.IO;
using BattleTechInfoExporter.ChangelogGenerator;

// Usage: <repository root> <version Major.Minor.Patch being built> <output path>.
// Errors are written in MSBuild's format, so the build that runs the generator reports them.
if (args.Length != 3)
{
    Console.Error.WriteLine(
        "error : usage: BattleTechInfoExporter.ChangelogGenerator <repository root> <version> <output path>");
    return 2;
}

var repository = new GitRepository(args[0]);
var (fragments, errors) = ChangelogFragments.Read(repository);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"error : {error}");
    }

    return 1;
}

string content;
try
{
    content = ChangelogAssembler.Assemble(
        File.ReadAllText(Path.Combine(repository.ChangelogDirectory, "preamble.md")),
        Version.Parse(args[1]),
        fragments,
        repository.ReleaseVersions(),
        File.ReadAllText(Path.Combine(repository.ChangelogDirectory, "history.md")));
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine($"error : {exception.Message}");
    return 1;
}

// Only when changed, so an unchanged changelog keeps its timestamp and the deployment skips copying it.
var outputPath = args[2];
if (!File.Exists(outputPath) || File.ReadAllText(outputPath) != content)
{
    File.WriteAllText(outputPath, content);
    Console.WriteLine($"Wrote {outputPath}");
}

return 0;
