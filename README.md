# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state of the story
campaign as JSON at key moments, for tools to read: for example a Claude session that plans mech
and pilot upgrades. In career mode it exports nothing.

The mod is a work in progress; see [Planned](#planned) for what's still missing.

## Export files

The files are in `Mods/BattleTechInfoExporter/exports/`. Four of them hold the career and are
written together on every career export:

- `game-state.json`: the career state.
- `rules.json`: the game's rules for this career (morale and reputation levels, skill training
  tables, spirits levels, mech parts per mech, mission types, jump distance per number of jump
  jets, and the combat and campaign rules), which the game state and the mission outcome refer to
  by name instead of repeating thresholds and effects per entry.
- `star-systems.json`: every star system on the starmap, locked story systems included, keyed by
  id, with its name, owner, tags, biomes, difficulty, whether its travel requirements are met, and
  the days and C-Bills the trip from the current system takes as the starmap shows them (0 for the
  current system, `null` where there is no route). The game state and the mission outcome refer to
  a star system only by its id and name.
- `financial-report.json`: the next monthly financial report, as on the finance screen: the day it
  is due, the spending level and its options with their expected expenses (their cost multiplier
  and morale change are in the rules), and the expected expense lines for the ship, its upgrades,
  each mech and each pilot.

One more holds the latest mission:

- `mission-outcome.json`: written when a mission ends, before the salvage is chosen, and again
  once the salvage is final, after the priority salvage is confirmed. It holds the contract, the
  outcome and objectives, the rounds the mission took, the payment, reputation and experience, the
  lance with each pilot's injuries, kills and the day they are out of the med bay, each mech's
  armor, structure and component damage, the days and C-Bills its repair would take and the shots
  it fired from every ammo box and flamer, and the salvage: the pool to
  choose from, how many items the company gets and picks, the components it recovers from its own
  lost mechs, and everything it received, which is `null` until the salvage is final.

The last one describes the game rather than the career:

- `catalog.json`: every chassis, mech, vehicle, turret, component, terrain and biome the game has
  loaded for the career, DLC included, keyed by id, so tools can judge what else exists beyond what
  the career refers to, what a mission's enemies can do and what its ground does to them.
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
  - **Components:** every component's stats, the statistics it changes while mounted (e.g. a
    jump jet's jump distance, an upgrade's melee damage), and whether it can come as salvage; for
    jump jets, the chassis tonnage range that can mount them.
  - **Terrains:** what forest, water, rough ground, roads and the other terrains do to the units
    in them: move cost per unit type and weight class (`null` where impassable; open ground costs
    1.05 per meter), sprint multiplier, visibility, sensor range and signature, to-hit modifiers
    against and from the terrain, cover, heat sinking and heat per turn, damage dealt and taken,
    and the statistic changes a unit picks up in it, e.g. rough ground's extra instability. A cell
    has one terrain at most. Only what the game's combat code reads is exported; its tooltips also
    claim a stability damage multiplier that only the AI reads.
  - **Biomes:** keyed by the biome id the star systems refer to, what a map's biome does to every
    unit on it on top of the terrain: heat sinking, heat per turn and damage dealt, the only
    values the game reads from a biome.

  The other files refer to the catalog's chassis, mechs and components by id instead of repeating
  their stats or limits, e.g. a location's max armor; a file and the catalog belong together when
  their `modVersion` matches.

  The tutorial's target dummies and target vehicles, the copies of stock mechs the game makes for unlocked skins (e.g.
  the backers' Shadow Hawk-UMBRA) and the game's internal melee and AI weapons are left out, as are
  the game's role and other advisory texts, which often don't match the best way to use a mech.

Every file starts with `modVersion`, `exportedAt` and the `trigger` of the export that last changed
it. A file is replaced in one step, never half-written, and only when its content apart from
`exportedAt` and `trigger` changed, plus once after every game start. Points in time are day numbers, comparable
with the company's `daysPassed`, so a day passing with nothing else happening changes only the company's day and date.

The game state holds:

- **Company:** name, date, funds, morale, rating, MechTech and MedTech with the changes events made
  to them for a time and the day each ends, and the reputation with every faction.
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
- **Contracts:** the contracts the Command Center offers with their star system, description and
  terms, and the negotiation as the values at each slider position (pay and salvage together take at most 100 %,
  exactly 100 % when the employer gains no reputation); plus the accepted travel contract until
  it's proceeded with, with the day of arrival while travelling to it. The game generates a system's contracts only when the contract screen first
  opens; until then `contracts` is `null`.

The rules' skill training tables hold, per level, the experience it costs, the base hit chance it
gives (Gunnery for ranged attacks, Piloting for melee, before an attack's modifiers) and the
abilities it unlocks. Each ability has its activation, targeting, cooldown and uses, and the
statistics it changes: statistic, operation and value, the pilot or weapons it applies to, how long
the change lasts and what triggers it. Multi-Target and Sensor Lock change no statistics in the
game's data, because the game hardcodes them. Initiative is on the game's scale, where units act
from the lowest value up, so an ability that lets a unit act earlier lowers it.

The rules' `combat` object holds the combat constants the game's code reads, named by what they
are: to-hit modifiers (positive makes an attack harder, negative is a bonus), evasion, guard levels
and the damage they cut, line of fire, heat, stability, injuries, melee, critical hits, hit location
weights by attack direction, visibility and sensor locks, resolve and movement penalties. Values
the game hardcodes but a tool computes with are exported next to the constants, e.g. the rounding
and limits of the hit chance in `toHit`; rules it hardcodes with conditions are stated here: a melee
attack on a turret, a building, a prone or a shut-down mech always has the maximum hit chance; a
sensor blip shows the unit's type from Tactics 4 and its details from 7. Constants the game never
reads, reads only in code nothing calls, or reads only for animation, sound, tooltips or the AI are
left out, as are the melee damage multipliers the game ignores in favour of the chassis' own values.

The rules' `campaign` object holds the campaign constants the game's code reads, as the career's
difficulty settings adjust them: mech lab costs in tech points and C-Bills (an order takes its tech
points divided by the company's MechTech, rounded up, in days), med bay heal points and the pilot
death chances, hiring costs and the limits the Mercenary Review Board rating and the morale put on
hiring, how salvage picks come about and how a destroyed enemy mech yields its parts (1 for a
destroyed center torso, 2 for both legs, 3 for the head or an incapacitated pilot), the financial
report's upkeep and spending levels, contract pay, reputation and experience by outcome, alliances
and travel. Each mission type carries the multiplier of its contracts' pay. The reputation payment
adjustments the game's tooltips show are left out: its pay never applies them.

## Triggers

The game state is exported when:

- **The career is loaded**, new or from a save.
- **The game saves the career** outside combat, manually or automatically.
- **A contract's results are applied**, salvage included.
- **The monthly financial report is shown**, with the expenses paid.
- **A mech lab order finishes**, **a pilot is healed** in the med bay, or **an Argo upgrade finishes**,
  once per day; when several finish on the same day, the `trigger` names the last of them.
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

The mission file is exported when:

- **A mission ends**, still in combat, with the salvage received still `null`.
- **The salvage is final**, after the priority salvage is confirmed or right away when there is
  nothing to choose, with the salvage received filled in.

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

The version is `Major.Minor.Patch.Build`. The first three parts are set in `Directory.Build.props`;
the build number counts the commits since they last changed, so the commit that changes them is
build 0, as is a build with that change uncommitted.

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
