using System;
using System.IO;
using System.Linq;
using BattleTechInfoExporter.SchemaGenerator;
using NJsonSchema;

// Usage: <game directory> <schema directory>, or <game directory> --validate <exports directory>, which checks the
// export files against freshly generated schemas without writing any.
// Errors are written in MSBuild's format, so the build that runs the generator reports them.
if (args.Length != 2 && (args.Length != 3 || args[1] != "--validate"))
{
    Console.Error.WriteLine(
        "error : usage: BattleTechInfoExporter.SchemaGenerator <game directory> "
        + "(<schema directory> | --validate <exports directory>)");
    return 2;
}

GameAssemblyResolver.Register(args[0]);
var generator = new ExportSchemaGenerator();
var schemas = ExportFileModels.All.Select(file => (file.FileName, Schema: generator.Generate(file.Model))).ToList();

if (generator.UndescribedProperties.Count > 0)
{
    foreach (var property in generator.UndescribedProperties)
    {
        Console.Error.WriteLine($"error : {property} has no description; add one to its model's XML doc comment");
    }

    return 1;
}

if (args.Length == 3)
{
    var exportsDirectory = args[2];
    var errorCount = 0;
    foreach (var (fileName, schema) in schemas)
    {
        var exportPath = Path.Combine(exportsDirectory, fileName);
        if (!File.Exists(exportPath))
        {
            Console.WriteLine($"{fileName}: not exported");
            continue;
        }

        var errors = schema.Validate(File.ReadAllText(exportPath));
        Console.WriteLine($"{fileName}: {(errors.Count == 0 ? "valid" : $"{errors.Count} errors")}");
        foreach (var error in errors)
        {
            Console.Error.WriteLine($"error : {fileName}: {error}");
        }

        errorCount += errors.Count;
    }

    return errorCount == 0 ? 0 : 1;
}

var schemaDirectory = args[1];
Directory.CreateDirectory(schemaDirectory);
var schemaFiles = schemas
    .Select(schema => (
        Path: Path.Combine(schemaDirectory, SchemaFileName(schema.FileName)),
        Content: SchemaFileContent(schema.Schema)))
    .ToList();
foreach (var (path, content) in schemaFiles)
{
    // Only when changed, so an unchanged schema keeps its timestamp and the deployment skips copying it.
    if (!File.Exists(path) || File.ReadAllText(path) != content)
    {
        File.WriteAllText(path, content);
        Console.WriteLine($"Wrote {path}");
    }
}

foreach (var stalePath in Directory.GetFiles(schemaDirectory, SchemaFileName("*"))
             .Except(schemaFiles.Select(file => file.Path)))
{
    File.Delete(stalePath);
    Console.WriteLine($"Deleted {stalePath}, whose export file no longer exists");
}

return 0;

static string SchemaFileName(string exportFileName) => Path.ChangeExtension(exportFileName, ".schema.json");

// LF and a final newline, as .gitattributes and .editorconfig want, so the committed files never differ by platform.
static string SchemaFileContent(JsonSchema schema) => schema.ToJson().ReplaceLineEndings("\n") + "\n";
