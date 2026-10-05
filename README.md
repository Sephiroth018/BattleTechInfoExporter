# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state of the story
campaign as JSON at key moments, for tools to read: for example a Claude session that plans mech
and pilot upgrades. In career mode it exports nothing.

The mod is a work in progress; see [Planned](#planned) for what's still missing.

## Export files

Two files in `Mods/BattleTechInfoExporter/exports/`, written together on every export:

- `game-state.json`: the career state.
- `rules.json`: the game's rules for this career, which the game state refers to by name instead of
  repeating thresholds and effects per entry.

Every file starts with `modVersion`, `exportedAt` and the `trigger` of the export that last wrote
it. A file is replaced in one step, never half-written, and only when its content apart from
`exportedAt` changed, plus once after every game start; a tool watching the files sees only real
changes.

### Game state

- **Company:** name, dropship, date, morale, Mercenary Review Board rating, MechTech and MedTech,
  reputation with every faction, and how many pilots the barracks hold.
- **Finances:** funds, days until the next report, the spending level and its options, and the
  expected expenses of the next report with their breakdown.
- **Position:** the system with its owner, planet tags and biomes, the travel status, and when
  travelling the destination and days left.
- **Pilots:** the commander and the roster with their skills, experience, primary abilities, health
  and injuries, status, spirits and service record.
- **Mechs:** the active and readying mechs of the mech bay with their status, position in the mech
  lab queue, the steps of a queued refit, loadout problems, armor, structure and hardpoints per
  location, every mounted component with its damage, and the performance summary with its ratings
  and the numbers behind them.
  - A mech in a refit also carries the loadout, tonnage, value and performance summary it will have
    once the refit is done.
- **Last lance:** the mechs and pilots last sent on a mission, the only assignment of pilots to
  mechs the game keeps; a mech or pilot that has left the company since is `null`. The lance
  configuration pre-fills from it only the mechs that can be fielded and the pilots who can pilot.
- **Storage:** the stored components with their working and damaged counts, the stored chassis
  with their stats (weight class, role, tonnage, maximum armor per location, hardpoints, jump jets)
  and the mech parts collected per mech. Readying a stored chassis gives the stock armor and the
  chassis' fixed components, but no weapons.
- **Stores:** the current system's system store, faction store and black market, where the company
  can use them, with the components, mech parts and (rarely) whole mechs they sell, their stock and
  prices. A whole mech carries the same chassis stats as a stored chassis.
- **Hiring hall:** the current system's pilots for hire with their skills, primary abilities,
  health, hiring cost, salary, and whether the company's rating and morale allow hiring them.
- **Contracts:** the contracts the Command Center offers, with their mission type, employer and
  target, difficulty and whether the reputation allows them, the negotiation, the lance limits, the
  biome, and for contracts in another system its planet tags and travel days. The mission types'
  descriptions are in the rules file.
  - The negotiation lists the values at each slider position (0, 25, 50, 75 and 100 %): the pay
    comes from the row at the pay share, the salvage from the row at the salvage share, and the
    reputation changes from the row at the share left over. Pay and salvage together can take at
    most 100 %; when the employer gains no reputation (its reputation change is `null`), they must
    take exactly 100 %. A contract that can't be negotiated has its fixed terms instead.
  - The game only generates a system's contracts when its Command Center's contract screen first
    opens, or after a contract; the mod never generates them itself, so it doesn't change the game.
    Until then, `contracts` is `null`.
- **Active contract:** the accepted travel contract, from accepting it until proceeding with it on
  arrival, with the same details, the terms it was accepted with, and the trip's remaining days as
  its travel days. It isn't repeated in the contracts.
- **Component definitions:** every component the file refers to, once per component type by id,
  with its tonnage, slots, cost and bonuses, and the stats of weapons, ammunition boxes and heat
  sinks.

### Rules

- **Morale levels.**
- **Reputation levels**, each with its contract difficulty limit, system store access and store
  price change.
- **Skills:** the training table of each skill with the experience cost of each level and the
  abilities and traits it unlocks, and the limits on choosing primary abilities.
- **Spirits levels:** the resolve cost of Precision Strike and Vigilance in normal, high and low
  spirits.
- **Mech parts per mech:** how many parts make a mech.
- **Mission types** with their descriptions.

## Triggers

The game state is exported when:

- **The career is loaded**, new or from a save.
- **The game saves the career**, manually or automatically, outside combat. Among others, it
  autosaves after arriving in a system, after a contract, after closing the financial report and
  after events.
- **A contract's results are applied**, salvage included, back on the career screens. Most
  contracts export through the autosave that follows; flashpoint contracts and contracts followed by
  a story contract, which the game doesn't autosave after, export directly.
- **The monthly financial report is shown**, with the expenses paid, to help choose next month's
  spending level. The autosave after closing it exports the chosen level.
- **A work order finishes:** a mech lab order (once the whole order is done), a medbay heal or an
  Argo upgrade. Orders finishing on the same day export together.
- **The game finishes generating the contracts** of the current system.
- **A store is closed**, from the main screen or the mech lab.
- **The mech bay changes:** a refit or repair is queued, a mech is readied or stored, an order is
  cancelled, the queue is reordered, or a mech, a stored chassis or mech parts are scrapped.

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
