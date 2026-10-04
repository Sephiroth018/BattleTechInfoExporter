# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state as JSON at key
moments. The files are meant to be read by tools, for example a Claude session that plans mech and
pilot upgrades.

Only the story campaign is supported; in career mode the mod exports nothing.

**Status:** work in progress; checked items below are implemented.

## Exports

The **game state**, written to `Mods/BattleTechInfoExporter/exports/game-state.json` and
overwritten on every export:

- [x] The company: dropship, date, morale, MechTech and MedTech, reputation with every faction,
      and how many pilots the barracks hold
- [x] Finances: funds, spending level and its options, the expected expenses of the next report
      with their breakdown
- [x] The current position: system with its planet tags and biomes, travel status, and when
      travelling the destination and days left
- [x] Pilots: the commander and the roster with their skills, experience, primary abilities,
      health and injuries, status, spirits and service record
- [x] Mechs: the active and readying mechs of the mech bay with their status, their position in
      the mech lab queue, the steps of a queued refit, loadout problems, armor, structure and
      hardpoints per location, every mounted component with its damage, and the performance
      summary with its ratings and the numbers behind them; for a mech in a refit also the
      loadout, tonnage, value and performance summary it will have once the refit is done
- [x] The last lance: the mechs and pilots last sent on a mission; the game keeps no other
      assignment of pilots to mechs. A mech or pilot that has left the company since is `null`. The
      lance configuration pre-fills from it only the mechs that can be fielded and the pilots who
      can pilot
- [x] Component definitions: the tonnage, slots, cost, bonuses and weapon, ammunition box or heat
      sink stats of every component the export refers to, listed once per component type by id
- [x] Storage: the stored components with their working and damaged counts, the stored mechs with
      their chassis stats, and the mech parts collected per mech. Stored mechs are bare chassis:
      readying one gives the stock armor and the chassis' fixed equipment, but no weapons
- [x] The current system's stores (system store, faction store, black market) that the company can
      use, with the components and mech parts they sell, their stock and prices
- [x] The current system's hiring hall: the pilots for hire with their skills, primary abilities,
      health, hiring cost, salary, and whether the company's rating and morale allow hiring them
- [x] Contracts: the contracts the Command Center offers, with their mission type, employer and
      target, difficulty and whether the reputation allows them, the pay, salvage and reputation
      changes of every combination the negotiation allows, the lance limits, the biome, and for contracts in another
      system its planet tags and travel days. The mission types' descriptions are in the rules file.
      **The game only generates a system's contracts when its Command Center's contract screen first
      opens, or after a contract; the mod never generates them itself, so it doesn't change the
      game. Until then, `contracts` is `null`.**
- [x] The active contract: the accepted travel contract, with the same details and the terms it was
      accepted with, from accepting it until proceeding with it on arrival. It isn't repeated in
      the contracts. While travelling to it, its travel days are the trip's remaining days
- [ ] The active flashpoint
- [ ] The ship and its upgrades; the mech lab queue is part of each mech

The game's **rules** for this career, written to `Mods/BattleTechInfoExporter/exports/rules.json`
alongside the game state, which refers to them by name. The file is only rewritten when the rules
change, e.g. after loading another career:

- [x] Morale levels, reputation levels with their contract difficulty limits, system store access
      and store price changes, and the skill training table with the experience cost of each level,
      the abilities and traits it unlocks, and the limits on choosing primary abilities, and the
      resolve cost of Precision Strike and Vigilance in normal, high and low spirits, the number of
      mech parts that make a mech, and the mission types' descriptions

Written to separate files, if the game makes them accessible:

- [ ] The **mission outcome**, after a mission is completed
- [ ] The **salvage result**, after priority salvage is chosen

## Triggers

The game state is exported when:

- [x] The career is loaded: starting a new career or loading a save
- [x] The game saves the career, manually or automatically, outside of combat. Among others, it
      autosaves after arriving in a system, after a contract, after the financial report is closed
      and after events
- [x] A contract's results are applied, including the chosen salvage, back on the career screens:
      through the autosave after it, or directly for the contracts the game doesn't autosave after
      (flashpoint contracts and those followed by a story contract)
- [x] The monthly expenses are paid, before the financial report is closed with next month's
      spending level
- [x] A work order finishes: a mech lab order (once the whole order is done), a heal in the medbay
      or an Argo upgrade. Orders finishing on the same day are exported together
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
