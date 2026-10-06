# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state of the story
campaign as JSON at key moments, for tools to read: for example a Claude session that plans mech
and pilot upgrades. In career mode it exports nothing.

The mod is a work in progress; see [Planned](#planned) for what's still missing.

## Export files

The files are in `Mods/BattleTechInfoExporter/exports/`. Two of them hold the career and are
written together on every career export:

- `game-state.json`: the career state.
- `rules.json`: the game's rules for this career (morale and reputation levels, skill training
  tables, spirits levels, mech parts per mech, mission types), which the game state and the mission
  outcome refer to by name instead of repeating thresholds and effects per entry.

Two more hold the latest mission:

- `mission-outcome.json`: written when a mission ends, before the salvage is chosen. It holds the
  contract, the outcome and objectives, the payment, reputation and experience, the lance with
  each pilot's injuries, kills and the day they are out of the med bay, each mech's structure and
  component damage and the days and C-Bills its repair would take, and the salvage
  on offer: the pool to choose from, how many items the company gets and picks, and the components
  it recovers from its own lost mechs.
- `salvage-received.json`: written once the salvage is final, after the priority salvage is
  confirmed. It holds everything the company gets from the salvage of the mission in
  `mission-outcome.json`; a new mission outcome deletes it until its salvage is final.

The last one describes the game rather than the career:

- `catalog.json`: every chassis, mech, vehicle, turret and component the game has loaded for the
  career, DLC included, keyed by id, so tools can judge what else exists beyond what the career
  refers to, and what a mission's enemies can do.
  - **Chassis:** the frame: weight class, tonnage and bare tonnage, max jump jets, built-in heat
    dissipation, walk and sprint distance, melee values before upgrades, and per location max
    armor, structure, hardpoints and component slots; plus the stock mech, the mech the game treats as the chassis'
    stock loadout.
  - **Mechs:** a loadout on a chassis: value, tonnage, performance summary, whether it can come as
    salvage (hero variants can't), and per location armor and components, the chassis' fixed ones
    marked. A mech assembled from parts or bought comes with this loadout; a stored chassis is
    readied with only its fixed components.
  - **Vehicles:** weight class, tonnage, movement type, walk and sprint distance, and per location
    armor, structure and components. Armor and structure are as in combat, where the game cuts the
    values in a vehicle's definition to three quarters.
  - **Turrets:** weight class, tonnage, firing arc, and armor, structure and components as in
    combat.
  - **Components:** every component's stats and whether it can come as salvage.

  The other files refer to the catalog's chassis, mechs and components by id instead of repeating
  their stats or limits, e.g. a location's max armor; a file and the catalog belong together when
  their `modVersion` matches.

  The tutorial's target dummies and target vehicles, the copies of stock mechs the game makes for unlocked skins (e.g.
  the backers' Shadow Hawk-UMBRA) and the game's internal melee and AI weapons are left out, as are
  the game's role and other advisory texts, which often don't match the best way to use a mech.

Every file starts with `modVersion`, `exportedAt` and the `trigger` of the export that last wrote
it. A file is replaced in one step, never half-written, and only when its content apart from
`exportedAt` changed, plus once after every game start. Points in time are day numbers, comparable
with the company's `daysPassed`, so a day passing with nothing else happening changes only the company's day and date.

The game state holds:

- **Company:** name, date, morale, rating, MechTech and MedTech, and the reputation with every
  faction.
- **Finances:** funds, the day of the next financial report, the spending level and its expected
  expenses.
- **Work queue:** the timeline's entries in order, each with the day it finishes and the mech,
  pilot, destination or ship upgrade it applies to.
- **Ship:** the Argo's upgrades the engineering screen shows, each installed, installing,
  available or locked, with its required upgrades, price, upkeep, installation days and effects in
  the game's words and as values; `null` while the company still flies the Leopard.
- **Position:** the current system and, when travelling, the destination and day of arrival.
- **Pilots:** the commander and the roster with all their relevant stats.
- **Mechs:** the mech bay's mechs with their status, the day they're ready, loadout, armor and
  performance summary; a mech in a refit also carries what it will be once the refit is done, and
  a damaged mech the mech lab isn't working on the days and C-Bills its repair would take.
- **Mechs awaiting placement:** new mechs the game asks to place, store or scrap because every mech
  bay is full, with the loadout they come with.
- **Last lance:** the mechs and pilots last sent on a mission, the only assignment of pilots to
  mechs the game keeps.
- **Storage:** the stored components, chassis and mech parts.
- **Stores:** the current system's system store, faction store and black market with their stock
  and prices.
- **Hiring hall:** the current system's pilots for hire, like the roster's pilots plus hiring cost,
  salary and whether the company can hire them.
- **Contracts:** the contracts the Command Center offers with their description and terms, and the
  negotiation as the values at each slider position (pay and salvage together take at most 100 %,
  exactly 100 % when the employer gains no reputation); plus the accepted travel contract until
  it's proceeded with, with the day of arrival while travelling to it. The game generates a system's contracts only when the contract screen first
  opens; until then `contracts` is `null`.

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
- **An Argo upgrade is bought.**
- **Experience is spent:** a pilot's training is confirmed in the barracks.
- **A new mech needs a place:** the game asks where to put it because every mech bay is full, e.g.
  after salvage completes a mech.

The catalog is exported when the career is loaded, also after a mission, and written once after
every game start. The first time, it waits a moment for the game to load every vehicle and turret.

The mission files are exported when:

- **A mission ends**, still in combat: the mission outcome.
- **The salvage is final**, after the priority salvage is confirmed or right away when there is
  nothing to choose: the salvage received.

## Planned

- The active flashpoint.
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
