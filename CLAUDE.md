# CLAUDE.md — BattleTechInfoExporter

Three parts: **General** applies to any project, **C#** to any C# project, **Project** to this one
only; where it overrides a General or C# rule, it wins. Instructions never go in HTML comments;
they're stripped before this file is loaded.

# General

## Working agreement

- **Plan architecture before building it.** Anything that shapes structure (new projects or layers,
  dependencies, data formats, cross-cutting patterns) gets a written plan first, presented in chat,
  and waits for the user's sign-off. Work inside an agreed design doesn't need one. A signed-off
  parent plan covers what to build, not how; each issue's implementation approach gets its own
  short plan unless it's trivial.
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
   the AI disclaimer, the LICENSE (MIT unless the project says otherwise) and the initial solution.
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
  describe what the issue is, independent of the commit types its work uses: a `bug` issue can
  still have `refactor` commits that came up in review.
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
- **Before merging, check what `main` gained since the branch was cut**
  (`git log --first-parent --oneline <merge-base>..origin/main`). If another PR landed, merge
  `main` into the branch and redo what depends on `main`'s state, such as the build and inspections
  when the merge touched code.
- **Merge only after the user's go-ahead in chat, given after the latest commit.** A later commit
  that changes the PR's diff needs a fresh go-ahead; a conflict-free update from `main` doesn't.
  Merge with a merge commit, and never delete the remote branch yourself (no `--delete-branch`, no
  `git push --delete`): the repository deletes it after the merge, and only then does GitHub
  retarget the PRs stacked on it to `main`; a branch deleted by a client closes them instead.
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
2. **`/code-review` at medium.** Fix the findings, or explain in the PR why one isn't a problem.

Later commits that change behavior get both again, `/simplify` first; trivial follow-ups need
neither.

**Merging needs green CI.** If the project's "Verification" section defines manual checks (game
mods do: the game can't run in CI), they also become a checklist in the PR description, ticked by
the user before the go-ahead.

## Engineering principles

Apply these to every change and check them in every review.

### Design

- **Simplest thing that works.** No abstractions, options or extension points for needs that don't
  exist yet. Expressing a concept the code already has isn't a future need.
- **One source of truth.** No duplicated logic, constants or data. Search for an existing helper or
  type before writing a new one.
- **Types say what things are.** A concept several types share is a shared type: a base record for
  the common fields, an interface for a role unrelated types play. Judge by concept, not by whether
  code uses the shared part today. Two types that merely look alike stay separate.
- **A thing in several states is one base per thing and one derived type per state**, with an
  interface per state so every entry of a state's list carries what the state needs: a stored
  component and a component for sale share the component; everything stored has a count.
- **A choice is structure, not nullable fields.** "Only the fields its type uses are set" means one
  derived type per case. Fields that are null together become one nullable nested type.
- **Generics where only a member's type varies**, never its name or meaning. Where members are
  added, use a base type.
- **A type owns what's about itself**: its shape, its comparison, its header. Code that handles
  many types knows only their common base.
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
- **Refer to other code by symbol name, never by file path,** ours and external code's alike (e.g. a
  game's members). In XML docs use `<see cref="..."/>` so renames follow; in `//` comments, where
  no cref can be written, the plain name, as `(Type.Member)` where it's a side reference.
- **Change a comment in the same edit as its code**, or delete it.
- **No issue links**, except to explain code that deliberately looks like a bug when the reason is
  too long to state inline.
- **Nothing machine-local:** no paths or notes that exist on one machine only; cite the original
  source.
- **XML doc comments only where name and signature aren't enough.** A project that ships a library
  for others may require them in its own section.

### Documentation

- **The README** says what the project is, how to install and use it, and how to build it, and
  carries an AI disclaimer stating how extensively AI is used in the project. It never refers to
  issues: it describes the project as it is, not how it got there.
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
  (`dotnet tool restore`). A new solution takes `Directory.Build.props` and `.editorconfig` from an
  existing one, since they hold the rules the build enforces.
- **Solution format:** keep a repository's existing one; new repositories use `.slnx`.
- **On old target frameworks, PolySharp** supplies the compile-time polyfills newer language
  features need.
- **`.editorconfig` is the single source of style, formatting and naming rules**, ReSharper's
  included (`resharper_*` keys). Inspection severities it can't express go in the team-shared
  ReSharper settings file `<solution name>.sln.DotSettings` (named so even for `.slnx`), created
  when the first one is needed. Never loosen either to make a change pass. Its `end_of_line = lf`
  matches `.gitattributes`, so formatting checks see the same line endings on every machine.
- **ReSharper Command Line Tools** (`JetBrains.ReSharper.GlobalTools`, a local tool) apply the
  IDE's rules outside it: `jb cleanupcode` formats, `jb inspectcode` reports inspections. They run
  only on the shared settings layers, so their results don't depend on anyone's IDE settings.
- **Tests use xUnit v3.**
- **CI (GitHub Actions) is strict:** the build passes with warnings as errors, all tests pass,
  `jb cleanupcode` leaves no diff, and `jb inspectcode` reports nothing at warning level or above.
  Fix the cause; never weaken a check to get green.

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

# Project

A BattleTech mod that exports the career state as JSON for tools to read (see README.md).

## Game and runtime constraints

- **BattleTech 1.9.1 on Unity 2018.4 and Mono with the .NET Framework 4.7.2 profile**, so the
  project targets `net472`. It is loaded by ModTek v4.5.1 or later, which calls every public static
  `Init` method.
- **Only the story campaign is supported** (`SimGameState.IsCampaign`); `CampaignExport` skips
  career mode with a log line.
- **Game assemblies come from the install** (`BattleTechGameDir` in the git-ignored
  `Directory.Build.user.props`) and are never copied into the build output or the repository.
- **Libraries the game or ModTek ship are used in their version**, not the latest one; this
  overrides "Latest stable version only" for them. Newtonsoft.Json is the game's 10.0.3. HarmonyX is
  a compile-only package in the version ModTek ships (2.16.0 with ModTek 4.5.1, in
  `Mods/ModTek/lib`). The JetBrains annotations (`[UsedImplicitly]`) come from the game's
  `UnityEngine.CoreModule`, which embeds them on purpose; a `JetBrains.Annotations` package would
  clash with them. The schema generator alone uses Newtonsoft.Json 13, which NJsonSchema brings:
  it runs at build time only, and the game never loads it.
- **Private game members are accessed through Krafs.Publicizer**, never reflection. This is the
  one allowed escape hatch, and only for game types.
- **Hook into the game with Harmony patches**, as ModTek's guide does; the game's `MessageCenter`
  doesn't exist yet when `Init` runs. A patch class is `internal static` and marked
  `[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]`, because ReSharper doesn't recognize
  Harmony's attributes. Harmony's injected arguments (`__instance`, `__state`) are taken through
  `[HarmonyArgument]` under a name that follows our naming rules.
- **An exception escaping a patch breaks the game's own code.** Every exporter catches and logs
  every exception (`CampaignExport`), so a patch that only calls one needs no handling of its own;
  a patch doing anything else catches its own.
- **Mutable static state** is allowed in patches, limited to what a patch needs, and in
  `ExportFile`'s cache of the content it last wrote per file, which lives as long as the game runs.
- **Logging** goes through `ModLog.Logger`, the game's `HBS.Logging` logger under the name
  `BattleTechInfoExporter`, and ends up in ModTek's log.

## Structure

Two projects in `BattleTechInfoExporter.slnx`:

- **The mod,** `BattleTechInfoExporter/`. Its build computes the version `Major.Minor.Patch.Build`
  from git (the `ComputeBuildVersion` target with the inline task `ComputeVersionFromGit`, in
  `Directory.Build.targets`; see "Versioning"), stamps it into the DLL, generates `mod.json` from it
  and the description, author, repository URL and supported game version in
  `Directory.Build.props`, and runs the schema generator. The mod folder holds the DLL, `mod.json`,
  `CHANGELOG.md` and the schemas.
- **The schema generator,** `BattleTechInfoExporter.SchemaGenerator/` (net9.0, never shipped),
  which the mod's build builds and runs on the built mod, in three roles. It writes a JSON Schema
  per export file into `schemas/` with the mod's own serializer settings (`ExportSchemaGenerator`,
  with `ExportFileModels` checking that the mod's `ExportFiles` lists every file). It fails the
  build when a definition, an exported property or a game enum has no description
  (`ModelDocumentation` turns the models' doc comments into descriptions, rendering a cref as where
  the JSON holds its target, `ExportFilePaths`; `GameEnums` describes the game's enums). And it
  validates export files against the schemas (see "Commands"). The mod grants it
  `InternalsVisibleTo` for the serializer settings, the one exception to "its own test project
  only".

In the mod, `ModEntryPoint` is the only public type; it deletes the files in `exports/` and
`schemas/` that `ExportFiles` doesn't list (e.g. an earlier version's) and applies all Harmony
patches in the assembly. Folders:

- `Triggers/`: the patches that decide when to export, and those that make the game's own contract
  generation complete in one go.
- `Export/`: the exporters, the only entry points the triggers call: `GameStateExporter` for the
  career state, `MissionExporter` for the mission file, `CombatExporter` for the combat map and
  state, which it also deletes, and `CatalogExporter` for the catalog. The career export first
  generates the current star system's contracts where the game hasn't yet, as the contract screen
  does when it opens (`SystemContractGenerator`). Below the exporters, one
  `<Section>Reader` per section of a file builds its models from the game, and cross-cutting
  helpers make the references, the plain game texts, the rounding and the comparisons; a reference
  with a naming rule of its own is made by the reader that owns it. `ExportFileWriter` replaces,
  deletes and reads the header of the files in `exports/`, by file name and text, knowing no model.
- `Models/`: immutable records, one per JSON object, and the mod's own enums. The records are
  marked `[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]` because only the serializer reads
  them. Every exported property has a doc comment, overriding "XML doc comments only where name and
  signature aren't enough": a `<param>` on its record, or a `<summary>` on a body property, on the
  record that declares it, or once on the interface property it implements where every implementing
  record means the same by it. It's the schema's description, so it's short (a fragment is fine)
  and says what the value is, its unit or scale and what `null` means. `ExportFile`, the root of
  every file, is more than data, by decision: it owns the header, the JSON conventions, the content
  compared before a write and the writing itself, through `ExportFileWriter` and the clock.

Planned features are issues on the GitHub Project "Road to 1.0", whose status follows the work (see
"Issues and projects").

## Export format

What each file holds is in its schema and the README. This section holds what a change to the
format must keep.

Conventions a new field follows:

- **References are `{ "id", "name" }`**, both always set; component references also carry `type`,
  and faction names get title case with the short name in brackets (`FactionNames`).
- **Definitions live only in the catalog,** keyed by the game's ids and grouped by type, one entry
  shape per group with no null sections; the other files refer to them by id and carry only their
  own state. A reference whose definition is missing has no entry.
- **Lists of entries are sorted by their references' comparison** where the game keeps no
  meaningful order; a mech's components keep the game's.
- **Points in time are day numbers** on the `daysPassed` scale, never countdowns; durations stay
  durations.
- **Phases and initiative are numbered as on the HUD** (`HudInitiative`), from 5 down to 1.
- **Game texts are plain** (`GameText`), without the UI's formatting tags and tooltip links.
- **Game enums keep the game's values;** our own enums are camelCase.
- **Floats the mod computes are rounded to a tenth** (`Rounding`); the game's constants keep theirs.
- **A choice is a derived shape** with a switch field, the enum or boolean that tells them apart.
- **Every file has a JSON Schema** (draft-04, `schemas/<file>.schema.json`, committed and
  shipped): every property required, derived shapes pinning their switch value, enums listing only
  the values the mod writes. A format change shows in the schemas' diff.
- **Files are cut for round trips, not size:** the consumer copies each changed file whole, so data
  that changes together shares a file and what a step needs is in the file that step reads.
- **JSON is camelCase and compact,** `null` written explicitly, through the game's Newtonsoft.Json.

Layout decisions a change would otherwise undo:

- **The career state is three files, read together on every career export** (`game-state.json`,
  `rules.json`, `star-systems.json`): the rules don't change between exports, so the game state
  refers to them by name instead of repeating thresholds.
- **Star systems live in `star-systems.json`, not the catalog,** keyed by their active
  definition's id (the story swaps definitions through the `StarSystem_SetActiveDef` result action),
  and repeated in full wherever a step needs one: their state changes during a career. The inactive
  definitions give a consumer nothing and aren't exported.
- **The latest mission is one file written twice,** when the mission ends and once the salvage is
  final, the second write adding only `salvage.received`: the game has no id that identifies every
  contract to link two files by, and by then the battle the outcome reads from is torn down.
- **The combat files read only the battle,** never the career files or the catalog, and the combat
  state only what the HUD shows: it's written after every activation, so its reads stay cached
  lookups and statistics, apart from the movement and destroyed buildings' cells.
- **The combat map isn't in the catalog:** buildings exist only once the battle's map has loaded.
- **The catalog is rebuilt on a career load only when it's missing or stale,** written by another
  mod version or with another `sourceFingerprint` (the game's manifest entries it reads): a rebuild
  loads what the career doesn't. A DLL mod patching definitions in memory isn't detected.
- **Values the game decides are read through its own code,** never re-derived: routes from the
  starmap, a star system's maps from the contract generator's query, blocked steps from the path
  checks, so they match what the game does.
- **The rules export only constants the game's code reads,** never one read only for presentation
  or the AI, so a consumer never plans around a number that does nothing. Hardcoded values a tool
  computes with sit next to them; hardcoded rules go in the description of the value they affect.
- **What the game doesn't show and only its random rolls use is left out** (e.g. the daily event
  chance): a consumer can't act on it.
- **Files are replaced atomically and only when their content apart from `exportedAt` and
  `trigger` changed,** so a tool never reads a half-written file and one copying the changed files
  skips the others. The game state's day counts as content.

## Glossary

- **Component:** anything mounted in a mech's slots (weapon, ammunition box, heat sink, jump jet,
  upgrade), the game's `MechComponentDef`. Never "equipment".
- **Chassis:** a mech's frame without its removable components (`ChassisDef`). Storage keeps
  chassis, not mechs.
- **Mech:** a complete mech (`MechDef`): in the mech bay, or sold whole by a store.
- **Stock mech:** the mech the game treats as a chassis' stock loadout, the one whose id is the
  chassis id with `chassisdef` replaced by `mechdef`.
- **Vehicle:** a ground unit with fixed armor and loadout (`VehicleDef`), only met as an enemy or
  ally in combat.
- **Turret:** a fixed emplacement with a single location (`TurretDef`).
- **Unit:** a mech, vehicle or turret in combat (the game's `AbstractActor`).
- **Location:** a body location of a unit: a mech's head, torsos, arms and legs, a vehicle's front,
  sides, rear and turret, a turret's single one. Never a star system.
- **Loadout:** the components mounted in a unit's locations; a mech's can change, a vehicle's and a
  turret's are fixed.
- **Catalog:** the export of every chassis, mech, vehicle, turret, component, terrain, biome, map,
  event, item collection and faction definition the game has loaded (`catalog.json`).
- **Item collection:** a weighted list the stores and rewards draw items from (`ItemCollectionDef`):
  components, whole mechs, mech parts or other collections, drawn in turn.
- **Star system:** a system on the starmap (`StarSystem`). Never "location" (a mech's body
  location) or "planet".
- **Star system definition:** a version of a star system (`StarSystemDef`) with its name, owner,
  tags and biomes. The story swaps a star system's definition, e.g. when its owner changes, so a
  star system can have several; one is active at a time.
- **Ship upgrade:** an upgrade of the Argo (`ShipModuleUpgrade`), bought in its engineering
  screen. Never just "upgrade", which components also have.
- **Work order:** a job the company waits for (the game's `WorkOrderEntry`): mech lab work, healing
  in the med bay, the trip, the financial report, a ship upgrade. **Work queue:** the timeline's
  list of them, by the day they finish (`workQueue`).
- **Terrain:** a design mask applied to a map cell (`DesignMaskDef`): forest, water, rough ground,
  roads and the like. A cell has one terrain at most.
- **Biome:** a map's map-wide design mask, keyed by the game's `Biome.BIOMESKIN`, applied on top of
  the terrain.
- **Map:** a combat map with fixed terrain (`Map_MDD`, `MapMetaData`); a star system's contracts pick
  among the maps its biomes and map tags allow.
- **Hex:** a cell of the game's 24 m movement grid (`HexGrid`) in axial coordinates (`q`, `r`). A unit
  stands on a hex's center, and gets its height and terrain from the map cell there.
- **Building:** a destructible structure on a combat map, the game's combatant `Building`; units
  can fire at it, stand on its roof and be blocked by it. Crates, trees and fences aren't buildings.
- **Lance:** the up to four units the player deploys on a mission. **Last lance:** the mechs and
  pilots last sent on a mission, which the lance configuration fills a contract's open slots with.
- **Round:** one pass through every unit's activation, counted from 1. **Phase:** the part of a
  round in which the units of one initiative act, numbered as on the HUD from 5 down to 1.
- **Activation:** one unit's turn within a phase: it moves, attacks or braces, and is done for the
  round. Reserving a unit postpones its activation to a later phase.
- **Visibility:** how much of a unit the player's HUD shows (the game's `VisibilityLevel`): full
  view, a blip, or the position it was last detected at.
- **Blip:** an enemy known only from sensors (the game's `VisibilityLevel` from `Blip0Minimum` to
  `Blip4Maximum`), shown with as much detail as the player's side's sensors reveal. An enemy in
  sight but hidden by ECM (`BlipGhost`) isn't a blip: the export treats it as in full view.
- **Evasion pips:** what a unit earns by moving, each making ranged attacks against it harder (the
  game's `EvasivePipsCurrent`).
- **Resolve:** the points the player's lance gains in combat and spends on Precision Strike and
  Vigilance (the game's `Team.Morale`).
- **Statistic change:** a change an ability or component makes to a statistic (the game's
  `EffectData` of type `StatisticEffect`), or an event's result makes (`SimGameStat`).
- **Event:** something the game shows between missions, about the company or a pilot, offering
  options (`SimGameEventDef`). A daily roll draws it, or another event's result schedules it.
- **Option:** what the player can pick in an event (`SimGameEventOption`). Never "choice".
- **Outcome:** one of an option's weighted sets of results, one of which picking it leads to
  (`SimGameEventResultSet`).
- **Result:** one change an outcome makes to one scope: tags, statistics, stored components,
  actions or scheduled events (`SimGameEventResult`).
- **Reference:** what the export refers to by its id instead of repeating it, with the name the UI
  shows (`Reference`): a definition, a mech in the mech bay, a component or a pilot.
- **Entry:** a thing in one of its states, a member of a list: a component stored, for sale,
  mounted, damaged and so on (`ComponentEntry`), mech parts stored or for sale (`MechPartsEntry`).
- **Contract identity:** what a contract is in every state, offered, accepted or fought
  (`ContractIdentity`). **Contract briefing:** what the contract details show before the mission,
  on top of its identity (`ContractBriefing`).
- **Trigger:** what caused an export (`ExportTrigger`), in every file's header. A statistic change's
  `trigger` is the game's own, when the change is made (`EffectTriggerType`).

## Commands

- **Build:** `dotnet build`.
- **Validate exports:** `dotnet
  BattleTechInfoExporter.SchemaGenerator/bin/Debug/net9.0/BattleTechInfoExporter.SchemaGenerator.dll
  <game> --validate <game>/Mods/BattleTechInfoExporter/exports` checks the game's current export
  files against schemas it generates in memory from the last build's DLL, writing none.
- **Deploy:** `dotnet build -p:DeployToGame=true` also copies the DLL, `mod.json`, `CHANGELOG.md`
  and the schemas into `<game>/Mods/BattleTechInfoExporter/`, for testing a branch in the game.
- **Package:** `dotnet build -c Release` also writes `artifacts/BattleTechInfoExporter-<version>.zip`.
- **Format:** `dotnet jb cleanupcode BattleTechInfoExporter.slnx --profile="Built-in: Full Cleanup"
  --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
  --no-build`, with `--include=<changed files>` for a single change.
- **Inspect:** `dotnet jb inspectcode BattleTechInfoExporter.slnx
  --output=artifacts/inspectcode.sarif --severity=WARNING
  --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
  --no-build`; the SARIF report must have no results.

## Releases

Every merged PR that changes the mod gets a GitHub release, after the merge, from the merged
`main`: a Release build, then
`gh release create v<Major.Minor.Patch>` on the merge commit, with the zip attached and release
notes summarizing the PR (marked as generated with Claude Code). For the export format's changes,
the notes point to the version's entry in `CHANGELOG.md` instead of restating them. The tag has no
build number: it marks the semantic release, and the next build derives its version from it. A PR
whose type doesn't bump (see "Versioning") gets no release and rolls into the next one. Before
tagging, check that the merge commit carries no tag yet (`git describe --exact-match` fails); a tag
means the release exists.

## Changelog

`CHANGELOG.md` at the repository root, shipped in the mod folder like the schemas, tells the
consumer, a chat holding copies of the export files and code parsing them, what each version
changed in those files: keyed by `modVersion`, newest first, with a preamble on how to apply it.
Every PR that bumps the version adds its entry under the version its branch's build computes (see
"Versioning"), as part of its changes, checked under step 6 of the Definition of done; the build
fails when the newest heading is older than the version it computes, so a missing entry, or a
number another PR released first, is caught, and after merging `main` the heading moves on to the
new number. An entry starts with the files whose content changed (every version rewrites every
file's header, so a file not listed needn't be re-copied), then per file, with `(breaking)` on a
file whose reading code must change, bullets of JSON paths with one verb each: added, removed,
renamed, moved, meaning changed, values corrected, type changed. A value keeping its shape with a
new meaning gets its own entry, since the schemas don't show it; a fix that corrects exported
numbers is an entry like any other, saying what to re-derive. Nothing internal: no reader, type or
game class names, refactors, build or schema generator changes. Like the README, it never refers
to issues or PRs: its reader can't use them, and no dates either.

## Testing and CI

- **No automated tests and no CI.** The mod only reads and exports game state, which can't run
  outside the game, and everything references the game's assemblies. This overrides "Testing",
  Definition of done steps 1 and 4, "Merging needs green CI" and the CI rule in "Toolchain and
  enforcement"; the "Verification" checklist gates the merge instead.
- **Formatting, build and inspections still apply** (Definition of done steps 2, 3 and 5), run
  locally before every pushed commit.
- **A bug fix starts with reproduction steps in its issue** instead of a failing test.

## Knowledge sources

In order of authority:

1. **The decompiled game code** in the sibling folder `../BattleTechDecompiled` (`Assembly-CSharp`,
   `BattleTech.Common`) is the source of truth for game behavior. It is never copied into this
   repository; comments cite the game's class and member names instead. If the folder is missing,
   ask the user; regenerating it needs a decompiler run on a thread with a large stack, because
   `ilspycmd -p` overflows its stack on `Assembly-CSharp`.
2. **The game's data files** (`BattleTech_Data/StreamingAssets/data` in the install) for
   definitions and ids. The DLC's definitions aren't there but in the asset bundles under
   `data/assetbundles` (`flashpoint`, `heavymetal`, `urbanwarfare`, `shadowhawkdlc`), extracted
   into the sibling folder `../BattleTechDlcData/<bundle>/<id>.json` by the `extract_dlc.py` kept
   there (Python with UnityPy; re-run after a game update). A search over definitions covers both
   folders. Like the decompiled code, the extracted files are never copied into this repository;
   if the folder is missing, ask the user.
3. **ModTek's documentation** (github.com/BattletechModders/ModTek, `doc/`) for mod loader
   behavior: `mod.json`, DLL entry points, HarmonyX, logging.
4. **Other mods' source and community answers** are hints only, confirmed in the game code before
   relying on them.

## Verification

In the game, by the user, as the checklist in the PR's description (see "Definition of done"). Every
checklist starts with the standard checks:

1. The game starts and the main menu shows `/W MODTEK`.
2. ModTek's log (`Mods/.modtek/battletech_log.txt`) shows ModTek loading the mod with the PR's
   four-part version (`"BattleTechInfoExporter" <version>` and `Loaded assembly BattleTechInfoExporter
   (v<version>)`), without errors or exceptions from it.

followed by the PR's own feature checks.

Checks 1 and 2 are Claude's, not the user's: when the user says to merge or that they've verified
the other checks, the game has run with the PR's build, so Claude reads the log then and ticks
both checks, check 1 because the mod ran, or stops and reports instead of merging if it fails.

## Versioning

Semantic versioning on the first three parts, never set by hand: the build derives the version
from the newest release tag reachable from HEAD and the conventional type of the first-parent
commits since it (see "Structure"). A `!` after the type and issue (`feat(#12)!:`) is a breaking change to
the exported JSON, whose consumers must adapt: major. `feat`, new data, files or triggers: minor.
`fix` and `refactor`: patch. `docs`, `chore` and `test` don't bump. The highest bump among a PR's
commits wins. On `main` the only commit since the last release is a PR's merge commit, so the PR
title's type is the bump, and the PR carries no version number at all: two parallel PRs never
collide, and the second to merge gets the next number from the first's tag; only its changelog
heading moves on (see "Changelog"). A branch's build shows the number its PR would get from the
tags it can see; one that hasn't merged `main` since a release computes from an older tag. A PR
stacked on other open PRs computes as if they were released first, bottom up: their remote
branches, those whose tips HEAD contains and `main` doesn't, split the commits into one bump per
PR. Until the stacked branch merges a lower PR's newest commits, or merges `main` once that PR is
released and its branch deleted, it computes the two as one release. The build number counts the
first-parent commits since the newest release on HEAD's first-parent chain: with a bump from 0, so
a PR's merge commit on `main` is build 0; without one the tagged commit is build 0, so a commit
after a release never shares its version. Every commit on a branch raises its version, also once a
lower PR is released, so a deployed build that isn't the branch's latest shows a lower one. While
the export format is still being worked out, versions stay at 0.x and a breaking format change
bumps only the minor version; 1.0.0 marks the format as settled. Issues name the part their change
bumps (major, minor or patch) and the PR title's type applies it.
