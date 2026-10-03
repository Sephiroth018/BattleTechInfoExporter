# BattleTechInfoExporter

A mod for BattleTech (Harebrained Schemes, 2018) that exports the career state as JSON at key
moments. The files are meant to be read by tools, for example a Claude session that plans mech and
pilot upgrades.

**Status:** work in progress, nothing implemented yet.

## Planned exports

The **game state**, written to one file:

- Pilots
- Mechs with all their equipment and stats
- Storage: equipment, mechs and mech parts
- The current system's store and hiring hall
- Available missions
- The current position and the work queue

Written to separate files, if the game makes them accessible:

- The **mission outcome**, after a mission is completed
- The **salvage result**, after priority salvage is chosen

## Triggers

The game state is exported when:

- The current system changes
- A mission is completed
- Salvage is chosen
- The financial report is triggered
- A work queue item finishes
- Possibly a key combination

## License

[MIT](LICENSE)
