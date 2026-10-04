# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state as JSON at key
moments. The files are meant to be read by tools, for example a Claude session that plans mech and
pilot upgrades.

Only the story campaign is supported; in career mode the mod exports nothing.

**Status:** work in progress; checked items below are implemented.

## Exports

The **game state**, written to `Mods/BattleTechInfoExporter/exports/game-state.json` and
overwritten on every export:

- [x] The company: dropship, date, morale, MechTech and MedTech, reputation with every faction
- [x] Finances: funds, spending level and its options, the expected expenses of the next report
      with their breakdown
- [x] The current position: system, travel status, and when travelling the destination and days
      left
- [x] The game's rules for this career: morale levels, and reputation levels with their contract
      difficulty limits, system store access and store price changes
- [ ] Pilots
- [ ] Mechs with all their equipment and stats
- [ ] Storage: equipment, mechs and mech parts
- [ ] The current system's store and hiring hall
- [ ] Available missions
- [ ] The work queue

Written to separate files, if the game makes them accessible:

- [ ] The **mission outcome**, after a mission is completed
- [ ] The **salvage result**, after priority salvage is chosen

## Triggers

The game state is exported when:

- [x] The career is loaded: starting a new career, loading a save, and returning to the career
      screens after a mission
- [ ] The current system changes
- [ ] A mission is completed
- [ ] Salvage is chosen
- [ ] The financial report is triggered
- [ ] A work queue item finishes
- [ ] Possibly a key combination

## Installing

Requires BattleTech 1.9.1 with [ModTek](https://github.com/BattletechModders/ModTek) v4.5.1 or
later. Extract a release zip into `BATTLETECH/Mods/`, so the mod ends up in
`BATTLETECH/Mods/BattleTechInfoExporter/`.

## Building

Requires the .NET SDK version pinned in `global.json` and a local BattleTech install.

1. Create `Directory.Build.user.props` next to the solution (it's git-ignored) and point it at the
   game:

   ```xml
   <Project>
     <PropertyGroup>
       <BattleTechGameDir>C:\Path\To\BATTLETECH</BattleTechGameDir>
     </PropertyGroup>
   </Project>
   ```

2. Run `dotnet tool restore` once, then `dotnet build`. Every build copies the mod into the game's
   `Mods/BattleTechInfoExporter/` folder. A Release build (`dotnet build -c Release`) also packages
   it as `artifacts/BattleTechInfoExporter-<version>.zip`.

## AI disclaimer

This project is developed almost entirely with AI. Code, documentation, issues, pull requests and
commit messages are written by [Claude Code](https://claude.com/claude-code) under the maintainer's
direction: the maintainer decides what gets built and how, reviews every change, and tests it in the
game before it's merged. Commits written by Claude carry a `Co-Authored-By: Claude` trailer, and
everything it posts on GitHub is marked as generated with Claude Code.

## License

[MIT](LICENSE)
