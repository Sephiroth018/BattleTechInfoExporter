# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state of the story
campaign as JSON at key moments, for tools to read: for example a Claude session that plans mech
and pilot upgrades. In career mode it exports nothing.

The mod is a work in progress; see [Planned](#planned) for what's still missing.

## Export files

Two files in `Mods/BattleTechInfoExporter/exports/`, written together on every export:

- `game-state.json`: the career state.
- `rules.json`: the game's rules for this career (morale and reputation levels, skill training
  tables, spirits levels, mech parts per mech, mission types), which the game state refers to by
  name instead of repeating thresholds and effects per entry.

Every file starts with `modVersion`, `exportedAt` and the `trigger` of the export that last wrote
it. A file is replaced in one step, never half-written, and only when its content apart from
`exportedAt` changed, plus once after every game start.

The game state holds:

- **Company:** name, date, morale, rating, MechTech and MedTech, and the reputation with every
  faction.
- **Finances:** funds, the spending level and the expected expenses of the next financial report.
- **Position:** the current system and, when travelling, the destination and days left.
- **Pilots:** the commander and the roster with all their relevant stats.
- **Mechs:** the mech bay's mechs with their status, loadout, armor, performance summary and place
  in the mech lab queue; a mech in a refit also carries what it will be once the refit is done.
- **Last lance:** the mechs and pilots last sent on a mission, the only assignment of pilots to
  mechs the game keeps.
- **Storage:** the stored components, chassis and mech parts.
- **Stores:** the current system's system store, faction store and black market with their stock
  and prices.
- **Hiring hall:** the current system's pilots for hire, like the roster's pilots plus hiring cost,
  salary and whether the company can hire them.
- **Contracts:** the contracts the Command Center offers with their terms, and the negotiation as
  the values at each slider position (pay and salvage together take at most 100 %, exactly 100 %
  when the employer gains no reputation); plus the accepted travel contract until it's proceeded
  with. The game generates a system's contracts only when the contract screen first opens; until
  then `contracts` is `null`.
- **Component definitions:** the stats of every component the file refers to, keyed by id, so the
  entries don't repeat them.

## Triggers

The game state is exported when:

- **The career is loaded**, new or from a save.
- **The game saves the career** outside combat, manually or automatically.
- **A contract's results are applied**, salvage included.
- **The monthly financial report is shown**, with the expenses paid.
- **A work order finishes:** a mech lab order, a medbay heal or an Argo upgrade.
- **The contracts of the current system are generated.**
- **A store is closed.**
- **The mech bay changes:** a refit, repair, readying or storing is queued, an order is cancelled,
  the queue is reordered, or a mech, chassis or mech parts are scrapped.

## Planned

- The active flashpoint.
- The ship and its upgrades.
- The mission outcome after a mission and the salvage result after priority salvage is chosen, each
  in its own file, if the game makes them accessible.
- An export on a key combination.

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

   - `dotnet build` builds the mod.
   - `dotnet build -p:DeployToGame=true` also copies it into the game's
     `Mods/BattleTechInfoExporter/` folder, to test it in the game.
   - `dotnet build -c Release` also packages it as `artifacts/BattleTechInfoExporter-<version>.zip`.

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
