# CLAUDE.md — BattleTechInfoExporter

Three parts: **General** applies to any project, **C#** to any C# project, **Project** to this one
only; where it overrides a General or C# rule, it wins. General and C# are the shared files
imported below. Instructions never go in HTML comments; they're stripped before this file is
loaded.

@.claude/shared/general.md

@.claude/shared/csharp.md

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
  build when a definition, an exported property, a value of the mod's enums or a game enum has no
  description
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
  them. Every exported property has a doc comment, since the schema generator turns it into the
  property's description in the schema: a `<param>` on its record, or a `<summary>` on a body
  property, on the record that declares it, or once on the interface property it implements where
  every implementing record means the same by it. So it's short (a fragment is fine) and says
  what the value is, its unit or scale and what `null` means. Every member of the mod's
  enums has a `<summary>` too, its value's description in the schema. `ExportFile`, the root of
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
  the values the mod writes, the mod's own enums with a described `oneOf` branch per value. A format
  change shows in the schemas' diff.
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
  lookups and statistics, apart from the movement and the zones' and destroyed buildings' cells.
- **The combat map isn't in the catalog:** buildings exist only once the battle's map has loaded.
- **Hexes are one object each,** never rows of characters behind a legend or a mask, and places on
  the map are hexes with an elevation, never world coordinates: the consumer parses whole files
  with code, so size costs it nothing, and every encoding or conversion is a place its parser goes
  wrong.
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

followed by the PR's own feature checks. A PR that only updates `.claude/shared/` has no checklist:
it changes nothing the mod is built from.

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
