# C#

## Toolchain and enforcement

Tools enforce every rule they can; a rule a tool could check belongs in its configuration, not in
this file.

- **SDK-style projects, built and tested with `dotnet`**, also for .NET Framework targets. Shared
  settings live in `Directory.Build.props`, package versions in `Directory.Packages.props` (central
  package management), the SDK version in `global.json`, and tools in `.config/dotnet-tools.json`
  (`dotnet tool restore`). A new solution takes `Directory.Build.props` and `.editorconfig` from an
  existing one, since they hold the rules the build enforces.
- **The CLI's project, solution and package commands** (`dotnet new`, `dotnet sln add`,
  `dotnet add package`) are the ones "Hygiene" lets change files.
- **Solution format:** keep a repository's existing one; new repositories use `.slnx`.
- **On old target frameworks, PolySharp** supplies the compile-time polyfills newer language
  features need.
- **`.editorconfig` is the single source of style, formatting and naming rules**, ReSharper's
  included (`resharper_*` keys). Inspection severities it can't express go in the team-shared
  ReSharper settings file `<solution name>.sln.DotSettings` (named so even for `.slnx`), created
  when the first one is needed. Never loosen either to make a change pass. Its `end_of_line = lf`
  matches `.gitattributes`, so formatting checks see the same line endings on every machine.
- **ReSharper Command Line Tools** (`JetBrains.ReSharper.GlobalTools`, a local tool) apply the
  IDE's rules outside it: `jb cleanupcode` is the formatter, `jb inspectcode` reports the
  inspections. They run only on the shared settings layers, so their results don't depend on
  anyone's IDE settings.
- **CI (GitHub Actions) is strict:** the build passes with warnings as errors, all tests pass,
  `jb cleanupcode` leaves no diff, and `jb inspectcode` reports nothing at warning level or above.
  Fix the cause; never weaken a check to get green.

## C# testing

- **Tests use xUnit v3**; parameterized tests are `[Theory]`.
- **Each production project grants `InternalsVisibleTo` to its own test project only**; never reach
  private members through reflection.
- **All test doubles use NSubstitute**, with `NSubstitute.Analyzers.CSharp` in every test project.
- **Names:** `MethodName_Scenario_ExpectedResult` (`Parse_EmptyInput_ReturnsNoWeapons`), in a class
  named after the unit under test.
- **Layout:** one `<Project>.Tests` project per production project, folders mirroring it.
  Integration tests spanning several projects go in `<Solution>.IntegrationTests`.

## C# conventions

What the tools enforce lives in `.editorconfig` and the analyzer settings; this section holds what
needs judgment.

- **`internal` by default**; a type is `public` only when another assembly needs it. Public classes
  stay open.
- **Immutable by default:** records for data, `init` and `readonly` where possible, and read-only
  collection interfaces (`IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`) in signatures.
- **No escape hatches** (null-forgiving `!`, `#nullable disable`, `dynamic`, reflection) without a
  narrow, stated reason. Where a project needs one (e.g. reflection for a game's private members),
  its "Project" part says where.
- **No mutable static state** unless a host framework requires it, documented under "Project".
- **Enums stored by number have explicit values**, so reordering members can't change stored data.
  Enums stored by name don't need them.
- **`string.Empty`, not `""`**, except where C# requires a constant (patterns, `const`, attribute
  arguments). No analyzer enforces it.
- **Async all the way:** no `.Result` or `.Wait()`, no `async void` outside event handlers.
- **Extension methods only for types we don't own**, never to avoid putting a method in the right
  class.
- **XML docs link symbols with `<see cref="..."/>`** so renames follow; `//` comments, where no
  cref can be written, use the plain name, as `(Type.Member)` where it's a side reference.
