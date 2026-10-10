# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state of the story
campaign as JSON at key moments, for tools to read: for example a Claude session that plans mech
and pilot upgrades. In career mode it exports nothing.

The mod is a work in progress; see [Planned](#planned) for what's still missing.

## Export files

The files are in `Mods/BattleTechInfoExporter/exports/`. On every game start the mod deletes any
other file there, e.g. one an earlier version wrote, so a tool never reads a file that's no longer
kept current. Three of them hold the career and are written together on every career export:

- `game-state.json`: the career state, the next financial report included.
- `rules.json`: the game's rules for this career (morale and reputation levels, skill training
  tables, spirits levels, mech parts per mech, mission types, jump distance per number of jump
  jets, and the combat and campaign rules), which the game state and the mission outcome refer to
  by name instead of repeating thresholds and effects per entry.
- `star-systems.json`: every star system on the starmap, locked story systems included, keyed by
  the id of its active definition, with its name, owner, its tags (also those the starmap doesn't
  show, which events check), the biomes its
  contracts can be fought in, the maps the game can put its contracts on (selected as the contract
  generator does, from its biomes and map tags), its difficulty, whether its travel requirements
  are met, and the route from the current system: the days and C-Bills the trip takes as the
  starmap shows them while the ship is in the system (0 for the current system, `null` where there
  is no route). During a trip the days stay those from the current system's planet, including the
  part already travelled; the position's travel has that leg's length and the day the leg under way
  ends, to correct them. The story swaps
  some systems' definitions, e.g. when a system's owner changes; the entry is always the active
  one's. The game state repeats the entry of the current system, the travel destination, the
  accepted and every offered travel contract's system; a contract offered in the current system
  and the other files refer to a star system only by its id and name.

One more holds the latest mission:

- `mission-outcome.json`: written when a mission ends, before the salvage is chosen, and again
  once the salvage is final, after the priority salvage is confirmed. It holds the contract, the
  outcome and objectives, the rounds the mission took, the payment, reputation and experience, the
  lance with each pilot's injuries, kills and the day they are out of the med bay, each mech's
  armor, structure and component damage, the days and C-Bills its repair would take and the shots
  it fired from every ammo box and flamer, and the salvage: the pool to
  choose from, how many items the company gets and picks, the components it recovers from its own
  lost mechs, and everything it received, which is `null` until the salvage is final.

Two more hold the running battle, and exist only while one runs:

- `combat-map.json`: the battle's ground on the game's movement hex grid, written once when the
  battle begins, new or loaded from a save: every hex a unit can stand on with its terrain,
  elevation and building, the steps to its neighbors that slopes block, and the map's buildings.
  See "Combat map" below.
- `combat-state.json`: the battle as the player's HUD shows it, written when it is loaded
  from a save, when every phase begins and after every unit's activation, and deleted when the battle ends: when the
  after-action report is left, or when it is quit or restarted. It holds the contract, the map's id in the catalog, the round, the current phase,
  the lance's resolve, the units keyed by the game's unit id, the objectives the HUD lists with
  their status, progress line and target units in full view, the zones drawn on the map with their type, center, radius
  and objectives, and the damaged buildings. See "Combat state" below.

The last one describes the game rather than the career:

- `catalog.json`: every chassis, mech, vehicle, turret, component, terrain, biome, map and event the
  game has loaded, DLC included, keyed by id, so tools can judge what else exists beyond
  what the career refers to, what a mission's enemies can do, what its ground does to them and
  which events can come.
  - **Chassis:** the frame: weight class, tonnage and bare tonnage, max jump jets, built-in heat
    dissipation, walk and sprint distance, the pathing capabilities it moves with (see "Combat
    map"), melee values before upgrades, and per location max
    armor, structure, hardpoints and component slots; plus the stock mech, the mech the game treats as the chassis'
    stock loadout.
  - **Mechs:** a loadout on a chassis: value, tonnage, performance summary, whether it can come as
    salvage (hero variants can't), and per location armor and components, the chassis' fixed ones
    marked. A mech assembled from parts or bought comes with this loadout; a stored chassis is
    readied with only its fixed components.
  - **Vehicles:** weight class, tonnage, movement type, walk and sprint distance, pathing
    capabilities, and per location
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
  - **Maps:** every map some star system's contracts can be fought on: name, biome, map tags, the
    contract generator's draw weight and the terrain coverage: each terrain's share of the map's playable cells by terrain id, plus `none` for the
    cells with no terrain, the bare biome. Cells are all the same size, so a share is an area
    share; the shares sum to 1. The same kind of cell is a different terrain per map, e.g. water is
    ice on a polar map, so the shares name the terrain directly. Buildings aren't terrain: a cell
    under a building counts as the terrain below it.

    The terrain a star system's contracts are likely fought on follows from the maps
    `star-systems.json` lists for the system, drawn by weight. The contract type and the maps the game recently offered filter
    further and aren't exported.
  - **Events:** every event: its title, whether it's about the company or a pilot, its draw weight,
    whether it comes only once, whether the daily roll can draw it (otherwise only another event's
    result schedules it), its requirements, the other pilots or mech it involves, and its options.
    Each option has its requirements and its outcomes, drawn by weight, and each outcome its results:
    tags added and removed, statistics changed, how many days the changes last before the game
    reverts them (`null` when they last), actions such as killing, dismissing or sidelining the
    pilot, and the events it schedules. A requirement names the scope it checks (the company, the
    current star system, the commander or a pilot), the tags that must and mustn't be there and the
    statistic comparisons that must hold; a statistic the scope doesn't have counts as 0. The game
    state holds the company's and every pilot's tags and the statistics events compare, and
    `star-systems.json` every star system's tags. Titles and option texts keep the game's placeholders
    for the pilots it picks, e.g. `{TGT_MW.Callsign}`; the events' story texts are left out.

    Once a day the game rolls for an event, except on the day the ship arrives at a star system and
    on days the story moves on. The chance rises with every roll that brings none and falls back once
    one comes. The roll draws by weight among the events the daily roll can draw whose requirements
    are met, leaving out one-time events that have come and the events drawn lately until no other
    can come. A pilot event is about a pilot without an event timeout who meets its requirements.
    Every option is shown; one whose requirements aren't met can't be picked.

  The other files refer to the catalog's chassis, mechs and components by id instead
  of repeating their stats or limits, e.g. a location's max armor; a file and the catalog belong
  together when their `modVersion` matches. The catalog also carries a `sourceFingerprint` of the game's data it
  was built from (see "Triggers").

  The tutorial's target dummies and target vehicles, the copies of stock mechs the game makes for unlocked skins (e.g.
  the backers' Shadow Hawk-UMBRA) and the game's internal melee and AI weapons are left out, as are
  the game's role and other advisory texts, which often don't match the best way to use a mech.

Every file starts with `modVersion`, `exportedAt` and the `trigger` of the export that last wrote
it. A file is replaced in one step, never half-written, and only when its content apart from
`exportedAt` and `trigger` changed, plus once after every game start, so a tool can skip copying
it when its timestamp hasn't changed. The game state's day and date count as content, so an export
on a new day writes it. Points in time are day numbers, comparable
with the company's `daysPassed`, so they don't count down from day to day.

The game state holds:

- **Company:** name, date, funds, morale, rating, MechTech and MedTech with the changes events made
  to them for a time and the day each ends, and the reputation with every faction.
- **Work queue:** the timeline's entries in order, each with the day it finishes and its `target`:
  the mech, pilot, destination or ship upgrade it applies to, by the entry's type.
- **Ship:** the Argo's upgrades the engineering screen shows, each installed, installing,
  available or locked, with its required upgrades, price, upkeep, installation days and effects in
  the game's words and as values; `null` while the company still flies the Leopard.
- **Financial report:** the next monthly financial report, as on the finance screen: the day it
  is due, the spending level and its options with their expected expenses (their cost multiplier
  and morale change are in the rules), and the expected expense lines for the ship, its upgrades,
  each mech and each pilot.
- **Position:** the current system in full, as in `star-systems.json`, the travel status, and,
  when travelling, the destination, also in full, the day of arrival, the days between the current
  system's planet and its jump point, and the day the leg under way ends (reaching the jump point,
  a jump, or reaching the planet after the last jump).
- **Pilots:** the commander and the roster with all their relevant stats, and the tags and
  statistics events check.
- **Mechs:** the mech bay's mechs with their status, the day they're ready, loadout, armor and
  performance summary; a mech in a refit also carries the refit's steps, each with only the fields
  its type uses, and what it will be once the refit is done, and
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
- **Contracts:** the contracts the Command Center offers, each with its description and terms,
  and the negotiation as the values at each slider position (pay and salvage together take at most 100 %,
  exactly 100 % when the employer gains no reputation), and its star system: by reference in the
  current system, in full, as in `star-systems.json`, with the route there for a travel contract;
  plus the accepted travel contract until it's proceeded with, with its star system in full and
  the day of arrival while travelling to it. The game generates a system's contracts only when the contract screen first
  opens; until then `contracts` is `null`.
- **Events:** the events other events' results scheduled, with the pilot they're about and the
  first and the last day they can come (assuming a roll every day), the events drawn lately, the
  one-time events that have come, and the company's tags and the statistics events compare. The
  chance of the next event isn't shown in the game and isn't exported.

The rules' skill training tables hold, per level, the experience it costs, the base hit chance it
gives (Gunnery for ranged attacks, Piloting for melee, before an attack's modifiers) and the
abilities it unlocks. Each ability has its activation, targeting, cooldown and uses, and the
statistics it changes: statistic, operation and value, the pilot or weapons it applies to, how long
the change lasts and what triggers it. Multi-Target and Sensor Lock change no statistics in the
game's data, because the game hardcodes them. Changes to initiative follow the HUD, where units act
from phase 5 down to 1, so a change that lets a unit act earlier is positive, unlike in the game's
data.

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

Every file has a JSON Schema (draft-04) in `Mods/BattleTechInfoExporter/schemas/`, e.g.
`game-state.schema.json`, generated from the same models the mod writes the files from, so it
always matches the mod version it ships with. It describes every property: its type, whether it can
be `null`, the values of enums, and what it means. A property that holds one of several record
shapes, such as a refit step, is any of them, without tying its `type` to a shape.

## Combat state

Every unit has its faction, allegiance (player, ally, enemy or neutral), visibility, position in
meters (`y` is the elevation), facing in degrees clockwise from the map's +Z axis and the terrain
it stands in by its catalog id (`null` on open ground). What else it carries follows the HUD:

- **The player's and allied units, and enemies in full view:** the mech, vehicle or turret, by its
  catalog id, or, for the player's own mechs, by their mech bay id in the game state, which holds
  their assigned armor; the pilot; and the state: armor front and rear and structure left per
  location in whole points as the paper doll shows them (cut off, a remainder below 1 shown as 1), evasion pips, the guard level with its sources (braced, cover, Bulwark), entrenched,
  prone, shut down, unsteady, heat and stability, whether the unit has activated this round, its
  initiative, every component's damage level with the rounds left in each ammo box and in weapons
  that carry their own, the pilot's injuries, health and bonus health left (which takes hits before they
  become injuries), together under `pilot`, which is `null` for a unit without one, the abilities that can be activated with their cooldowns and uses left,
  and what Precision Strike and Vigilance cost it now. A Sensor Lock brings an enemy into full view
  for the player's whole side, and allied units' sensors count for the player. An enemy in sight
  but hidden by ECM counts as in full view, although the HUD hides its pilot, heat, stability and
  initiative and it can't be targeted until spotted well enough or sensor locked.
- **Sensor blips:** the position and as much as the HUD shows: nothing more at the lowest level,
  the kind of unit at the next, and its tonnage, or a turret's weight class, at the highest.
- **Enemies out of sensor range:** where the player's side last detected them, without facing or
  terrain, until they are seen again. Destroyed enemies are left out.

The player's units carry their lines of fire to every enemy in full view, and those enemies theirs
to the player's units, as hovering over a target shows them from where the unit stands: the line
of fire (`clear`, `partiallyBlocked`, the game's obstructed, which still allows direct fire with
the penalties of the rules' `lineOfFire`, or `blocked`), whether a weapon could fire at the target directly, only
indirectly or not at all, or it is out of range, and whether it is in the firing arc without
turning. Phase and initiative are numbered as on the HUD, as in the rules: units act from 5 down
to 1.

Every player unit that can move also carries its `movement`, so a tool can plan a whole phase:
the hexes it can end a walk, sprint, reverse or jump on, read from the game's own pathing, so
terrain costs, slopes and other units count. A unit that has activated gets those of its next
activation, from where it stands; prone and shut down mechs and turrets get none. A move the unit
can't make is `null`: sprint for a legged or unsteady mech, jump for vehicles and mechs without
a working jump jet. It's laid out in
rows like the combat map's, explained by `movementLegend` at the file's root: per move, the
evasion pips a move ending on each hex gives (from the length of the path the unit would walk),
and per enemy in full view, whether the unit could fire at it from that hex (with the line of fire
from there and the longest weapons' ranges) and which side of it an attack would hit. Elevation
and terrain of each hex are in the combat map. The facing after the move isn't, as the unit can
turn at its end.

A zone's radius is that of the hexagon the HUD draws; whether a
unit is inside is decided per map cell. The hit chance isn't exported: it follows from the combat
rules and changes as soon as a unit moves.

Damaged buildings are listed by their id in the combat map, with the structure left; buildings at
full structure are left out. The HUD shows a building's structure when it is targeted. A destroyed
building also carries the hexes it stood on as they are now, with their elevation and terrain:
the combat map isn't rewritten, but units on its roof drop to the ground, which its rubble covers.
The blocked steps onto and off those hexes aren't read again, so they still follow the roof. A
building that explodes also changes the terrain around it, which isn't exported.

## Combat map

The map is laid out in rows of characters and numbers instead of an object per hex, so it stays
small enough for an AI chat to read whole. Its `hexGrid` spells out the grid: pointy-top hexes of
24 m in axial coordinates (`q`, `r`), hex (0, 0) at the map's center, hex (q, r) centered at
x = 24 · (q + r / 2), z = 24 · √3/2 · r, the same meters as the units' positions.

Each row lists consecutive hexes of one `r` from its first `q` on: their terrain as one character
of the `terrains` legend each (`.` is open ground), their elevations and the index of the building
on each in `buildings` (or `null`). Only the hexes a unit can stand on are listed, inside the
contract's encounter bounds; a row with a gap is split in two. A hex is what a unit standing on it
gets from the single map cell at its center: its height, a building's roof where one stands, and
its terrain. A cell has one terrain, the first of: map boundary, impassable, destroyed building,
deep water, water, the terrain of the building on it, road, the map's custom terrain, forest, rough
ground; otherwise it's open ground.

`blockedSteps` holds, per entry of `pathingGroups`, one character per hex: a base64 digit whose
bit i is set when the step to the neighbor at `hexGrid.directions[i]` is blocked. A unit moves with
its chassis' or vehicle's pathing capabilities (`movement.pathingId` in the catalog), and the
groups gather those that block the same steps; mechs and vehicles differ. A step is blocked as the
game's pathing blocks it on slopes: too steep a grade between path nodes, too steep ground or a
ledge in between. The game moves units over path nodes half a hex apart, so the step counts as
blocked only when every two-node route to the neighbor is. Terrain move costs and units in the way
aren't part of it; impassable terrain has its own legend character. A step to a hex that isn't
listed is blocked too.

Each building has its id, name, position and max structure; the combat state lists the damaged
ones.

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
- **An event is resolved:** its popup is closed after an option was picked.

The catalog is rebuilt when the career is loaded, also after a mission, but only when it's missing
or stale: written by another mod version, or with another `sourceFingerprint` than the game's
current data. The fingerprint hashes the game's manifest entries for everything the catalog reads
(each entry's id, file and last write time), so a game update, a DLC bought or removed, a mod
installed, updated, removed or disabled, or a ModTek merge re-run changes it. A change that touches
no file the catalog reads, such as a DLL mod patching definitions in memory, isn't detected; delete
`catalog.json` to force a rebuild. A rebuild waits a moment for the game to load every vehicle and
turret.

The mission file is exported when:

- **A mission ends**, still in combat, with the salvage received still `null`.
- **The salvage is final**, after the priority salvage is confirmed or right away when there is
  nothing to choose, with the salvage received filled in.

The combat state is exported when:

- **A phase begins**, before its first unit acts: the battle's first once the lance is deployed,
  and every later one, also one reached by reserving units.
- **A battle is loaded** from a save.
- **A unit's activation is done**, its attacks resolved; out of contact, once the player's units
  have all moved. Reserving a unit doesn't count.

The combat map is exported once when a battle begins: a new one when the briefing is dismissed,
or one loaded from a save.

Both are deleted when the game ends the battle (when the after-action report is left after the salvage, or when the battle is quit,
restarted or left by loading a save) and when a career is loaded, in case the game crashed during
one, so they never outlive their battle.

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

   - `dotnet build` builds the mod and regenerates the schemas in `schemas/` with the schema
     generator (`BattleTechInfoExporter.SchemaGenerator`), from the models and their doc comments.
     It fails when an exported property has no description.
   - `dotnet build -p:DeployToGame=true` also copies the mod and its schemas into the game's
     `Mods/BattleTechInfoExporter/` folder, to test it in the game.
   - `dotnet build -c Release` also packages them as `artifacts/BattleTechInfoExporter-<version>.zip`.

The schemas are committed, so a change to the export format shows in their diff. To check export
files against them, e.g. the ones the game just wrote:

```bash
dotnet BattleTechInfoExporter.SchemaGenerator/bin/Debug/net9.0/BattleTechInfoExporter.SchemaGenerator.dll <game folder> --validate <game folder>/Mods/BattleTechInfoExporter/exports
```

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
