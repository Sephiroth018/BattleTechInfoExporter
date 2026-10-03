# CLAUDE.md — BattleTechInfoExporter

Three parts: **General** applies to any project, **C#** to any C# project, **Project** to this one
only; where it overrides a General or C# rule, it wins. Instructions never go in HTML comments;
they're stripped before this file is loaded.

# General

## Working agreement

- **Plan architecture before building it.** Anything that shapes structure (new projects or layers,
  dependencies, data formats, cross-cutting patterns) gets a written plan first, presented in chat,
  and waits for the user's sign-off. Work inside an agreed design doesn't need one.
- **Stop when the plan breaks.** If a problem mid-task invalidates the agreed approach, stop and ask
  how to proceed, with concrete options and a recommendation. Never switch approach silently.
- **Say so when there's a better way.** Raise it before implementing, with the trade-off, then do
  what the user decides.
- **Ask before widening scope.** Mention unrelated problems you notice and ask; don't fix them in
  passing.
- **English everywhere:** code, comments, docs, commits, issues and PRs.
- **Mark everything you write on GitHub** (issues, comments, PRs, review replies, releases) by
  ending it with `🤖 Generated with [Claude Code](https://claude.com/claude-code)`. Commits carry a
  `Co-Authored-By: Claude` trailer.
- **Keep chat reports short.** A line or two on what was done. Explain at length only what's
  non-standard, what went wrong, or what needs a decision.
- **Standing authorization** covers the source control steps below, except merging. Other
  destructive or history-rewriting operations (`reset --hard`, deleting unmerged branches, any
  force push not covered there) always need asking.
- **Required project sections:** "Verification" and "Versioning" under "Project" must each say how
  it's done, or that the project has none. If one is missing or not yet defined, tell the user
  before relying on it.
- **The General and C# parts are reused across projects.** When a change touches them, say so, so
  the user can carry it over to other projects.

## New repository setup

Once per repository, before the rest of the workflow applies:

1. **First commit, directly on `main` and without an issue** (`chore: set up repository`): this
   file, the stack's standard `.gitignore`, a `.gitattributes` fixing line endings to LF on every
   machine (`* text=auto eol=lf`, CRLF only for `.cmd`/`.bat`), a README with the project goal and
   the AI disclaimer, the LICENSE (MIT unless
   the project says otherwise) and the initial solution.
2. **Create the GitHub repository** (name and visibility decided with the user) and push.
3. **Repository settings:** merge commits only (no squash or rebase merging), default merge message
   from the PR title and description, head branches deleted automatically after merging.
4. **Ruleset on `main`:** changes only through PRs, signed commits required, no force pushes or
   deletion, and the CI checks required once CI exists.
5. **Labels:** delete `good first issue`, `help wanted`, `accessibility` and `invalid`; add `chore`
   (build, tooling, dependencies), `refactor` (restructuring without behavior change) and
   `research` (finding out how something works before building on it). Work that is only about
   tests gets `enhancement` or `chore`.
6. **CI** is a GitHub Actions workflow, `.github/workflows/ci.yml`, running on PRs and pushes to
   `main`.
7. **No Dependabot or other scheduled workflows**: they use up Actions minutes even on an abandoned
   repository.

## Source control

### Issues and projects

- **Every change starts from a GitHub issue**, small ones included. Search for an existing one
  before creating it.
- **The issue records every decision about it.** Signed-off plans and later decisions go into the
  description while work hasn't started, and into a new comment once it has.
- **Labels:** apply the repo's existing labels that fit; ask before creating a new one. They
  follow the commit types: `feat` → `enhancement`, `fix` → `bug`, `refactor` → `refactor`, `docs` →
  `documentation`, `chore` → `chore`; `research` has no commit type of its own.
- **Help manage the GitHub Project** for planned features: break features into issues, add them to
  the project, and keep their status current as work starts and lands.

### Branches and commits

- **Never commit to `main`.** Branch off an up-to-date `main` as `<issue>-<short-slug>`
  (`12-export-weapon-stats`). If the session assigned a branch (a desktop worktree), use that one.
- **Commit messages:** `<type>(#<issue>): <imperative summary>`, with type `feat`, `fix`,
  `refactor`, `docs`, `test` or `chore` (build, dependencies, CI, tooling).
- **Every commit lands on `main`** (PRs merge with merge commits), so each one is a single coherent
  step that builds.
- **Moves and renames get their own commit**, made with `git mv`, containing only the move and the
  minimal edits it forces (namespaces, references), so git keeps following the file's history.
- **Rewrite a branch only before its PR is opened** (amend, rebase, `--force-with-lease`) to tidy
  its commits. Once the PR is open, changes are new commits so they can be reviewed on their own,
  in the normal message format (usually `refactor` or `chore`; no `fixup!` commits). `main` is
  never rewritten.
- **Commits are signed through 1Password's SSH agent.** If a commit or push fails because the agent
  is locked, stop, tell the user and wait. Don't retry in a loop, and never bypass signing.

### Pull requests

- **Open a PR into `main`** once the change is done (see "Definition of done"). Its title follows
  the commit message format; it becomes the merge commit's message. The description starts with
  `Closes #<issue>` and explains what the code can't: why, rejected alternatives, risks, how it was
  verified, what deserves a close look. No file-by-file list.
- **Watch the PR** after opening it: turn on the desktop app's monitoring (auto-fix CI, address
  comments). Fix CI failures at the cause; never skip or weaken a check.
- **The user's own review comments fire no events** (a solo author can only leave "comment"
  reviews). When the user says they've reviewed, read all three surfaces: PR comments, inline diff
  comments and review summaries. Answer each comment on the PR, inline ones in their own thread.
- **Merge only after the user's go-ahead in chat, given after the latest commit.** A later commit
  that changes the PR's diff needs a fresh go-ahead; a conflict-free update from `main` doesn't.
  Merge with a merge commit.
- **After merging:** switch to `main`, `git pull --prune`, delete the local branch.

## Definition of done

A change is done when all of these hold, checked in this order before every commit that's pushed:

1. **Tests cover the new or changed behavior** (see "Testing"). A gap means the change isn't done.
2. **Formatted:** `jb cleanupcode` on the changed files.
3. **Builds** with no errors or warnings.
4. **All tests pass.**
5. **Inspections are clean:** `jb inspectcode` reports nothing at warning level or above.
6. **Nothing is left stale:** comments next to changed code, the README and this file (commands,
   architecture, glossary) still match.
7. **The diff is self-reviewed** against this file: scope, naming, leftover debug code.

Steps 2-5 are what CI runs; running them first keeps CI from being where problems are found.

Before opening the PR, with the branch as target (its whole diff against `main`, not just
uncommitted changes):

1. **`/simplify`**, then steps 2-7 again for what it changed. A suggested fix that changes design or
   scope is asked about, not applied.
2. **`/code-review` at medium.** Fix the findings, or explain in the PR why one isn't a problem. Run
   it again for later commits that change behavior; trivial follow-ups don't need it.

**Merging needs green CI.** If the project's "Verification" section defines manual checks (game
mods do: the game can't run in CI), they also become a checklist in the PR description, ticked by
the user before the go-ahead.

## Engineering principles

Apply these to every change and check them in every review.

### Design

- **Simplest thing that works.** No abstractions, options or extension points for needs that don't
  exist yet.
- **One source of truth.** No duplicated logic, constants or data. Search for an existing helper or
  type before writing a new one.
- **Pure logic in the middle, side effects at the edges.** I/O, external systems, time and
  randomness stay at the boundaries, so the core is testable without them.
- **Validate at boundaries, trust inside.** Input is checked where it enters; inner code doesn't
  re-check it.
- **Persisted formats and public interfaces change only by decision.** Changing save data, exported
  files, config or a public API needs a plan covering migration or compatibility.

### Errors

- **Fail loud.** Never swallow an error silently. Catch only what you can handle; exceptions aren't
  control flow.
- **Don't silence the compiler or type system** without a narrow, stated reason (see "C#
  conventions").

### Dependencies

- **Prefer an established package over hand-rolled logic**, but adding one is an architecture
  decision (see "Working agreement"). It must be mainstream, in a stable release, under a compatible
  license, and either actively maintained or feature-complete with no known open security issues.
- **Latest stable version only**, never a preview, RC or pre-release. Pin explicitly if a range
  would resolve to a pre-release.

### Hygiene

- **Change files only with the Edit and Write tools**, never with shell commands or scripts, so
  every edit shows as a reviewable diff. Exceptions: `git mv`, formatters (`jb cleanupcode`), the
  CLI's own project, solution and package commands (`dotnet new`, `dotnet sln add`,
  `dotnet add package`), and output of the project's own generators.
- **Clean up and modernize the code you touch.** Small fixes and current idioms within the lines
  being changed are part of the change; anything beyond them is asked about first.
- **No dead code.** Delete it rather than comment it out; git keeps the history.
- **No `TODO` without an issue number.**
- **Never commit secrets**: no keys, tokens or personal paths in code, config or logs.

## Naming

Casing and other mechanics are in `.editorconfig` and "C# conventions"; the project's settled terms
are in its glossary. Check every new or changed name against these rules, and check them in every
review.

- **Reuse the existing term.** Search for the concept first and use the word the code already uses;
  never add a synonym.
- **One term, one concept.** Don't stretch an existing word over a second meaning; pick a distinct
  one.
- **Say what it is or returns, all of it.** A name true of only part of the value misleads.
- **Be specific.** No vague names (`data`, `info`, `details`, `item`, `value`, `result`, `obj`,
  `temp`). A role suffix (`Handler`, `Manager`, `Helper`, `Processor`, `Utils`) is fine only when
  the rest of the name says exactly what it handles: `HeaderClickHandler`, not `Handler` or
  `HeaderHandler`.
- **Domain words, not invented ones.** Use the problem domain's terms; no project jargon, made-up
  metaphors or cute names.
- **Grammar matches role.** Methods are verb phrases, and one that changes state says so in its
  verb. Properties are nouns. Booleans are positive and read as a question (`isVisible`, `hasAmmo`,
  `canFire`, not `notHidden`).
- **Write words out.** Only widely known abbreviations (`id`, `url`, `json`, `min`, `max`).
- **Short scopes are the exception:** loop variables, short lambdas and comparers may use single
  letters or vague names.
- **External names stay in their own world.** An external system's spelling (a game's ids or data
  keys) appears only in its values and in names that refer to one of them. Our code is named by what
  it does; when it mirrors an external function, cite that function's name in a comment.
- **A rename is finished only when everything follows:** comments, tests and their names, docs,
  config keys and the glossary. Renaming something persisted falls under "Persisted formats".
- **New or settled domain terms** are proposed to the user for the glossary, not added unasked.

## Comments and documentation

### Comments

Write a comment only for what the code can't show: why, an invariant or ordering constraint, a
deliberate workaround, or an external fact with its source (e.g. the game method it comes from).

- **Short:** one or two lines.
- **No history or discussion narration**; that belongs in the commit message.
- **Don't restate** the code or a nearby comment.
- **Refer to other code by symbol name, never by file path.** In XML docs use `<see cref="..."/>`
  so renames follow.
- **Change a comment in the same edit as its code**, or delete it.
- **No issue links**, except to explain code that deliberately looks like a bug when the reason is
  too long to state inline.
- **Nothing machine-local:** no paths or notes that exist on one machine only; cite the original
  source.
- **XML doc comments only where name and signature aren't enough.** A project that ships a library
  for others may require them in its own section.

### Documentation

- **The README** says what the project is, how to install and use it, and how to build it, and
  carries an AI disclaimer stating how extensively AI is used in the project.
- **Decisions live in their GitHub issue**, not in separate decision records (see "Issues and
  projects").

## Testing

- **A bug fix starts with a failing test** that reproduces it.
- **Test behavior through the non-private API**, internals included, not implementation details.
  Each production project grants `InternalsVisibleTo` to its own test project only; never reach
  private members through reflection.
- **Real collaborators.** A test double only at a boundary (file system, clock, randomness,
  external systems such as a game) or when the interaction itself is what's tested. All test
  doubles use NSubstitute, with `NSubstitute.Analyzers.CSharp` in every test project; no
  hand-written fakes.
- **One behavior per test**, as Arrange-Act-Assert, with no loops or conditionals. Table-driven
  cases use `[Theory]`.
- **Deterministic:** no real time, randomness, network or sleeps, and no dependence on test order or
  shared mutable state.
- **Complex test data comes from typed builders** in a shared test-support folder.
- **Don't test what a library or the framework already guarantees.**
- **A failing or flaky test is a bug.** Never skip or disable one without an issue number.
- **Names:** `MethodName_Scenario_ExpectedResult` (`Parse_EmptyInput_ReturnsNoWeapons`), in a class
  named after the unit under test.
- **Layout:** one `<Project>.Tests` project per production project, folders mirroring it.
  Integration tests spanning several projects go in `<Solution>.IntegrationTests`.
- **Coverage thresholds are enforced in CI and only go up**: raise them when a change adds coverage.
  A project may set a different policy under "Project".

# C#

## Toolchain and enforcement

Tools enforce every rule they can; a rule a tool could check belongs in its configuration, not in
this file.

- **SDK-style projects, built and tested with `dotnet`**, also for .NET Framework targets. Shared
  settings live in `Directory.Build.props`, package versions in `Directory.Packages.props` (central
  package management), the SDK version in `global.json`, and tools in `.config/dotnet-tools.json`
  (`dotnet tool restore`).
- **Solution format:** keep a repository's existing one; new repositories use `.slnx`.
- **Newest C# language version, nullable reference types enabled.** On old target frameworks,
  PolySharp supplies the compile-time polyfills newer language features need.
- **Warnings are errors** (`TreatWarningsAsErrors`), with the built-in .NET analyzers at
  `latest-recommended` and `EnforceCodeStyleInBuild`. Suppress a diagnostic only at the narrowest
  scope, with a justification.
- **`.editorconfig` is the single source of style, formatting and naming rules**, ReSharper's
  included (`resharper_*` keys). Inspection severities it can't express live in the team-shared
  ReSharper settings file `<solution name>.sln.DotSettings` (named so even for `.slnx`). Never
  loosen either to make a change pass. Its `end_of_line = lf` matches `.gitattributes`, so
  formatting checks see the same line endings on every machine.
- **ReSharper Command Line Tools** (`JetBrains.ReSharper.GlobalTools`, a local tool) apply the
  IDE's rules outside it: `jb cleanupcode` formats, `jb inspectcode` reports inspections.
- **Tests use xUnit v3.**
- **ReSharper runs only on shared settings** (`--disable-settings-layers` for the global and
  personal layers), so its results don't depend on anyone's IDE settings.
- **CI (GitHub Actions) is strict:** the build passes with warnings as errors, all tests pass,
  `jb cleanupcode` leaves no diff, and `jb inspectcode` reports nothing at warning level or above.
  Fix the cause; never weaken a check to get green.

## C# conventions

Everything enforceable lives in `.editorconfig` and the analyzer settings (style, file-scoped
namespaces, namespaces matching folders, `var` everywhere, culture-safe formatting and parsing).
This section holds what needs judgment.

- **`internal` by default**; a type is `public` only when another assembly needs it. Internal
  classes are `sealed` unless designed for inheritance (CA1852 enforces it); public ones stay open.
- **Expression bodies (`=>`) for properties and for value-returning methods that are a single
  expression**, even when it wraps. Constructors and `void` methods use a block body. ReSharper's
  cleanup enforces this (`resharper_use_heuristics_for_body_style`).
- **Immutable by default:** records for data, `init`/`readonly` where possible, and read-only
  collection interfaces (`IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`) in signatures.
- **Classic constructors with explicit fields**, no primary constructors on classes.
- **No escape hatches** (null-forgiving `!`, `#nullable disable`, `dynamic`, reflection) without a
  narrow, stated reason. Where a project needs one (e.g. reflection for a game's private members),
  its "Project" part says where.
- **No mutable static state** unless a host framework requires it, documented under "Project".
- **Persisted enums have explicit values**, so reordering members can't change stored data.
- **Async all the way:** no `.Result` or `.Wait()`, no `async void` outside event handlers.
- **Extension methods only for types we don't own**, never to avoid putting a method in the right
  class.

# Project

A BattleTech mod that exports the career state as JSON for tools to read (see README.md).

## Game and runtime constraints

- **BattleTech 1.9.1 on Mono with the .NET Framework 4.7.2 profile**, so the project targets
  `net472`. It is loaded by ModTek v4.5.1 or later, which calls every public static `Init` method.
- **Game assemblies come from the install** (`BattleTechGameDir` in the git-ignored
  `Directory.Build.user.props`) and are never copied into the build output or the repository.
- **Libraries the game ships are used in the game's version**, not the latest one; this overrides
  "Latest stable version only" for them. Newtonsoft.Json is the game's 10.0.3. HarmonyX is a
  compile-only package because ModTek provides it at runtime.
- **Private game members are accessed through Krafs.Publicizer**, never reflection. This is the
  one allowed escape hatch, and only for game types.
- **Harmony patches are static by nature**; that is the allowed exception to "No mutable static
  state", limited to what a patch needs.
- **Logging** goes through the game's `HBS.Logging.Logger` under the name `BattleTechInfoExporter`
  and ends up in ModTek's log.

## Structure

One project, `BattleTechInfoExporter/`, in `BattleTechInfoExporter.slnx`. `ModEntryPoint` is the
only public type. The version lives only in `<Version>` in `Directory.Build.props`; the build stamps
it into the DLL and generates `mod.json` from it.

## Commands

- **Build and deploy:** `dotnet build` copies the DLL and `mod.json` into
  `<game>/Mods/BattleTechInfoExporter/`.
- **Package:** `dotnet build -c Release` also writes `artifacts/BattleTechInfoExporter-<version>.zip`.
- **Format:** `dotnet jb cleanupcode BattleTechInfoExporter.slnx --profile="Built-in: Full Cleanup"
  --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
  --no-build`, with `--include=<changed files>` for a single change.
- **Inspect:** `dotnet jb inspectcode BattleTechInfoExporter.slnx
  --output=artifacts/inspectcode.sarif --severity=WARNING
  --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
  --no-build`; the SARIF report must have no results.

## Releases

From the first PR after the setup on, every merged PR that changes the mod gets a GitHub release,
after the merge, from the merged
`main`: a Release build, then `gh release create v<version>` with the zip attached and release
notes summarizing the PR (marked as generated with Claude Code).

## Testing and CI

- **No automated tests and no CI.** The mod only reads and exports game state, which can't run
  outside the game, and everything references the game's assemblies. This overrides "Testing",
  Definition of done steps 1 and 4, and the CI rule in "Toolchain and enforcement".
- **Formatting, build and inspections still apply** (Definition of done steps 2, 3 and 5), run
  locally before every pushed commit.
- **A bug fix starts with reproduction steps in its issue** instead of a failing test.
- **Merging needs the "Verification" checklist ticked** instead of green CI.

## Knowledge sources

The game is BattleTech 1.9.1 (Unity 2018.4, Mono, .NET Framework 4.7.2). In order of authority:

1. **The decompiled game code** in the sibling folder `../BattleTechDecompiled` (`Assembly-CSharp`,
   `BattleTech.Common`) is the source of truth for game behavior. It is never copied into this
   repository; comments cite the game's class and member names instead. If the folder is missing,
   ask the user; regenerating it needs a decompiler run on a thread with a large stack, because
   `ilspycmd -p` overflows its stack on `Assembly-CSharp`.
2. **The game's data files** (`BattleTech_Data/StreamingAssets/data` in the install) for
   definitions and ids.
3. **ModTek's documentation** (github.com/BattletechModders/ModTek, `doc/`) for mod loader
   behavior: `mod.json`, DLL entry points, HarmonyX, logging.
4. **Other mods' source and community answers** are hints only, confirmed in the game code before
   relying on them.

## Verification

In the game, by the user. Every PR's description lists what to check in the game; the user ticks it
before the go-ahead. Every checklist starts with the standard checks:

1. The game starts and the main menu shows `/W MODTEK`.
2. ModTek's log (`Mods/.modtek/battletech_log.txt`) shows the mod loaded with the PR's version,
   without errors or exceptions from it.

followed by the PR's own feature checks.

## Versioning

Semantic versioning, bumped in every PR as part of its changes: major for breaking changes to the
exported JSON (its consumers must adapt), minor for new data, files or triggers, patch for fixes.
The version is defined once (see "Structure") and flows into both the DLL and `mod.json`.
