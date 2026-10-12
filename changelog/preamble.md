# Changelog of the export format

This file is for the tool that reads the mod's export files, typically a Claude chat holding copies
of the files in `Mods/BattleTechInfoExporter/exports/` and parsing code written against them. It
lists, per mod version, what changed in those files. The installed mod ships it next to `exports/`,
so the copy there is the one matching the files.

How to use it:

1. Every export file starts with `modVersion`. Read it from your copies: it's the version they
   were written by.
2. Apply every entry above that version, newest first, down to and excluding it.
3. Each entry starts with the files whose content changed. A file not listed changed only its
   header (`modVersion`, `exportedAt`, `trigger`), so your copy of it stays valid and needn't be
   copied again. A file marked `(breaking)` needs its reading code updated.
4. A bullet is a JSON path from the file's root and what happened to it: added, removed, renamed,
   moved, a meaning or type changed, or values corrected. Where it says what to re-derive, earlier
   conclusions drawn from that value were based on wrong or differently defined data.

Paths: `a.b` is property `b` of object `a`; `pilots[]` an element of the `pilots` array; `<id>`
a key of an object keyed by id. The files are listed in a fixed order: game-state.json,
rules.json, star-systems.json, financial-report.json (0.42.0 to 0.56.0), mission-outcome.json,
salvage-received.json (0.27.0 to 0.42.0), combat-state.json, combat-map.json, catalog.json.
Versions are the first three parts of `modVersion`; its fourth part, the build number, never
changes the format.
