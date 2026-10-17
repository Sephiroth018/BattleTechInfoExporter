# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state of the story
campaign as JSON at key moments, for tools to read: for example a Claude session that plans mech
and pilot upgrades. In career mode it exports nothing.

## Export files

The files are in `Mods/BattleTechInfoExporter/exports/`. On every game start the mod deletes any
other file there, e.g. one an earlier version wrote, so a tool never reads a file that's no longer
kept current.

- `game-state.json`: the career state, from the company, pilots, mechs and storage to the stores,
  contracts, events and the next financial report. Never deleted.
- `rules.json`: the game's tables and constants for this career, which the other files refer to
  by name. Written with the game state, never deleted.
- `star-systems.json`: every star system with its owner, tags, maps, stores, employers and the
  route there. Written with the game state, never deleted.
- `mission-outcome.json`: the latest mission's outcome, pay, lance and salvage, written when the
  mission ends and again once the salvage is final. The next mission replaces it.
- `combat-map.json`: the battle's ground on the game's hex grid, written once when the battle
  begins.
- `combat-state.json`: the running battle as the player's HUD shows it, written when a battle is
  loaded, when every phase begins and after every unit's activation. Both combat files are deleted
  when the game tears the battle down (the after-action report is left, or the battle is quit,
  restarted or left by loading a save) and when a career is loaded, so they exist only while their
  battle runs.
- `catalog.json`: every definition the game has loaded, DLC included: chassis, mechs, vehicles,
  turrets, components, terrains, biomes, maps, events, item collections and factions. Rebuilt on
  a career load only when it's missing or stale: written by another mod version, or from other
  game data than now (its `sourceFingerprint`). Delete it to force a rebuild.

Every file starts with `modVersion`, `exportedAt` and the `trigger` of the export that last wrote
it. A file is replaced in one step, never half-written, and only when its content apart from
`exportedAt` and `trigger` changed, plus once after every game start, so a tool can skip copying a
file whose `exportedAt` hasn't changed. The other files refer to the catalog's definitions by id;
they belong together when their `modVersion` matches.

Each file's `trigger` is one of these, and each trigger writes the files marked. The career files
are `game-state.json`, `rules.json` and `star-systems.json`, always written together.

| `trigger`                                                     | When                                                                                                                             | Career files | Mission | Combat map | Combat state | Catalog |
|---------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|:------------:|:-------:|:----------:|:------------:|:-------:|
| `CareerLoaded`                                                | A career is started or loaded, also on the return from a mission; the career files only when no mission's results are pending. |      ✓       |         |            |              |    ✓    |
| `ManualSave`                                                  | The player saves the career outside combat.                                                                                      |      ✓       |         |            |              |         |
| `Autosave`                                                    | The game saves the career outside combat, e.g. once a contract's results are applied.                                           |      ✓       |         |            |              |         |
| `ContractCompleted`                                           | A contract's results are applied, where the game doesn't autosave after it, e.g. a flashpoint's.                                |      ✓       |         |            |              |         |
| `FinancialReportShown`                                        | The monthly financial report is shown.                                                                                           |      ✓       |         |            |              |         |
| `MechLabOrderCompleted`, `PilotHealed`, `ShipUpgradeCompleted` | Work orders complete, at the end of a day or when the mech lab queue is updated; named after the last one.                      |      ✓       |         |            |              |         |
| `ContractsGenerated`                                          | The current system's contracts are generated: when the contract screen first opens there, or after a contract.                  |      ✓       |         |            |              |         |
| `Jumped`                                                      | At the end of the day the ship jumps into its destination system, work orders of that day included.                             |      ✓       |         |            |              |         |
| `Arrived`                                                     | The ship arrives at the planet after a trip.                                                                                     |      ✓       |         |            |              |         |
| `PurchaseCompleted`, `SaleCompleted`                          | A purchase or sale in a store, once per action, a stack sold at once included.                                                   |      ✓       |         |            |              |         |
| `StoreClosed`                                                 | A store is closed, from the main screen or the mech lab.                                                                         |      ✓       |         |            |              |         |
| `MechBayChanged`                                              | An order is queued, cancelled or reordered, a mech is readied, stored or renamed, or a mech, chassis or mech parts are scrapped. |      ✓       |         |            |              |         |
| `ShipUpgradeStarted`                                          | An Argo upgrade is bought.                                                                                                       |      ✓       |         |            |              |         |
| `ExperienceSpent`                                             | A pilot's training is confirmed in the barracks.                                                                                 |      ✓       |         |            |              |         |
| `MechPlacementShown`                                          | The game asks where to put a new mech because every mech bay is full.                                                            |      ✓       |         |            |              |         |
| `MissionCompleted`                                            | A mission ends, still in combat, before the salvage is chosen.                                                                   |              |    ✓    |            |              |         |
| `SalvageChosen`                                               | The salvage is final: the priority salvage is confirmed, or there is nothing to choose.                                          |              |    ✓    |            |              |         |
| `CombatStarted`                                               | A new battle begins, when the briefing is dismissed.                                                                             |              |         |     ✓      |              |         |
| `CombatLoaded`                                                | A battle is loaded from a save.                                                                                                  |              |         |     ✓      |      ✓       |         |
| `PhaseStarted`                                                | A phase begins, before its first unit acts, also one reached by reserving units.                                                 |              |         |            |      ✓       |         |
| `ActivationCompleted`                                         | A unit's activation is done, its attacks resolved; out of contact, once the player's units have all moved.                     |              |         |            |      ✓       |         |

Every file has a JSON Schema (draft-04) in `Mods/BattleTechInfoExporter/schemas/`, e.g.
`game-state.schema.json`, generated from the models the mod writes the files from. It describes
every property: its type, whether it can be `null`, the values of enums and what it means. What
each version changed in the files is in the [changelog](CHANGELOG.md), written for the tool reading
them and shipped as `Mods/BattleTechInfoExporter/CHANGELOG.md`.

## Installing

Requires BattleTech 1.9.1 with [ModTek](https://github.com/BattletechModders/ModTek) v4.5.1 or
later.

1. Download the zip of the
   [latest release](https://github.com/Sephiroth018/BattleTechInfoExporter/releases/latest).
2. Extract it into `BATTLETECH/Mods/`, so the mod ends up in
   `BATTLETECH/Mods/BattleTechInfoExporter/`.

From the next career load on, the export files appear in
`BATTLETECH/Mods/BattleTechInfoExporter/exports/`.

## Building

Requires the .NET SDK version pinned in `global.json` and a local BattleTech install, whose
assemblies the build references in place.

1. Create `Directory.Build.user.props` next to the solution (it's git-ignored) and point it at the
   game:

   ```xml
   <Project>
     <PropertyGroup>
       <BattleTechGameDir>C:\Path\To\BATTLETECH</BattleTechGameDir>
     </PropertyGroup>
   </Project>
   ```

2. Build:

   - `dotnet build` builds the mod and regenerates the schemas in `schemas/` with the schema
     generator (`BattleTechInfoExporter.SchemaGenerator`), from the models and their doc comments.
     It fails when a record, an exported property or a game enum has no description.
   - `dotnet build -p:DeployToGame=true` also copies the mod, `mod.json`, the changelog and the
     schemas into the game's `Mods/BattleTechInfoExporter/` folder, to test it in the game.
   - `dotnet build -c Release` also packages them as `artifacts/BattleTechInfoExporter-<version>.zip`.

The schemas are committed, so a change to the export format shows in their diff. To check export
files, e.g. the ones the game just wrote, against schemas generated from the built mod:

```bash
dotnet BattleTechInfoExporter.SchemaGenerator/bin/Debug/net9.0/BattleTechInfoExporter.SchemaGenerator.dll <game folder> --validate <game folder>/Mods/BattleTechInfoExporter/exports
```

The build derives the version from git, as the "Versioning" section of [CLAUDE.md](CLAUDE.md)
describes.

`dotnet tool restore` installs the ReSharper command line tools that format and inspect the code;
CLAUDE.md lists their commands.

## AI disclaimer

This project is developed almost entirely with AI. Code, documentation, issues, pull requests and
commit messages are written by [Claude Code](https://claude.com/claude-code) under the maintainer's
direction: the maintainer decides what gets built and how, reviews every change, and tests it in the
game before it's merged. Commits written by Claude carry a `Co-Authored-By: Claude` trailer, and
everything it posts on GitHub is marked as generated with Claude Code.

## License

[MIT](LICENSE)
