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

## 0.76.0

Changed: combat-state.json (breaking), combat-map.json (breaking).

### combat-state.json (breaking)
- Removed `units.<id>.position`: replaced by `hex` and `elevation`.
- Added `units.<id>.hex`: the `q` and `r` of the hex the unit stands on, or was last detected
  on, the same hexes as combat-map.json `hexes[]`.
- Added `units.<id>.elevation`: the height the unit is at, `position.y` before.
- Type changed `objectives[].beacon`: the hex the beacon stands above, `q` and `r`, instead of a
  position in meters.
- Removed `zones[].center` and `zones[].radius`: replaced by `hexes`.
- Added `zones[].hexes[]`: the playable hexes whose center cell is in the zone, by `r`, then `q`.
  A unit standing on one is in the zone.
- Removed `movementLegend`, `units.<id>.movement.targets` and `units.<id>.movement.rows`:
  replaced by `movement.hexes`.
- Added `units.<id>.movement.canSprint` and `canJump`: whether the unit can make the move at all,
  the `null` rows' meaning before.
- Added `units.<id>.movement.hexes[]`: one object per hex some move ends on, by `r`, then `q`,
  with `q`, `r`, the evasion pips of `walk`, `sprint`, `reverse` and `jump` (`null` where that
  move doesn't reach the hex) and `linesOfFire`.
- Added `units.<id>.movement.hexes[].linesOfFire[]`: per enemy in full view, by unit id, its
  `target`, `blocking`, `fire` and the `side` an attack hits (`front`, `left`, `right`, `rear`,
  `prone` or `turret`), the attack characters before. An enemy the unit has no line of fire to
  and can't fire at indirectly from the hex has no entry.
- Meaning changed `units.<id>.linesOfFire[]`: listed in the order of the targets' unit ids.

### combat-map.json (breaking)
- Removed `buildings[].position`: replaced by `hex`.
- Added `buildings[].hex`: the hex the building's placement point lies in, which needn't be one
  it covers or a playable one. The hexes it covers name it in `hexes[].building`.
- Removed `rows`, `terrains` and `hexGrid.blockedStepsFormula`.
- Added `hexes[]`: one object per playable hex, by `r`, then `q`, with `q`, `r`, `elevation`,
  `terrain` (the terrain id, `null` on open ground), `building` (the building's id, `null` where
  there is none) and `blockedSteps`. A hex that isn't listed is outside the playable area.
- Added `hexes[].blockedSteps`: per entry of `pathingGroups`, the indices into
  `hexGrid.directions` of the blocked steps, instead of a base64 mask per hex.

## 0.75.1

Changed: game-state.json.

### game-state.json
- Values corrected: an export with `trigger` `jumped` is now written once the game has rolled the
  destination's stores and hiring hall. Before, it showed `stores` and `hiringHall` from a roll
  the game replaced a moment later; take both from a later export of that star system instead.

## 0.75.0

Changed: catalog.json.

### catalog.json
- Added `mechDefinitions.<id>.role`: the chassis' stock role the game shows for the mech, e.g.
  "Brawler". It describes the standard fitting, which a refit can change; no rule reads it.
- Added `mechDefinitions.<id>.description`: the mech's description the game shows, or its chassis'
  where the mech has none, as plain text.
  It describes the standard fitting, which a refit can change; no rule reads it.

## 0.74.0

Changed: game-state.json, combat-state.json, combat-map.json, catalog.json (breaking).

### game-state.json
- Type changed `mechs[].refit[].armor.front` and `rear` in steps of type `modifyArmor`: numbers
  like every other armor value, no longer integers. The values are still whole.

### combat-state.json
- Values corrected `units.<id>.state.mech.stability.current`, `unsteadyAbove` and
  `maxStability`: rounded to a tenth like every other position and armor value.
- Values corrected `damagedBuildings[].structure`: rounded to a tenth; damage that rounds away
  no longer lists a building.

### combat-map.json
- Values corrected `buildings[].maxStructure`: rounded to a tenth.

### catalog.json (breaking)
- Removed `support`, `energy`, `ballistic` and `missile` from
  `terrainDefinitions.<id>.damageDealt`, `terrainDefinitions.<id>.damageTaken` and
  `biomeDefinitions.<id>.damageDealt`: they repeated `byWeaponCategory.Support`, `Energy`,
  `Ballistic` and `Missile`. A weapon's multiplier is `all` times its category's in
  `byWeaponCategory`.
- Type changed `componentComparisons[].value` in every requirement of `eventDefinitions.<id>`:
  an integer like `componentChanges[].count`, no longer a number. The values are the same.

## 0.73.0

Changed: game-state.json (breaking), rules.json, star-systems.json, combat-state.json
(breaking), catalog.json (breaking).

### game-state.json (breaking)
- Added `workQueue[].kind`: what the entry is about (`mechLab`, `medBay`, `travel`, `shipUpgrade`
  or `financialReport`), which decides its `target`, instead of the game's `type`.
- Values corrected in every full star system entry's `stores` (see star-systems.json).
- Removed the `Item.<definition type>.<component id>` keys from `company.statistics`: the
  requirements comparing a stored component's count name the component now (see catalog.json),
  and compare `storage.components[].count`, 0 for a component not in storage.

### rules.json
- Meaning changed `combat.evasion.pipsByDistanceMoved`: stated that a jump in place gets the
  jump's pip, and that a mech moving more than 12 m gets its `EvasivePipsGainedAdditional`
  statistic on top, past its cap; re-derive the pips of mechs whose abilities raise that
  statistic.

### star-systems.json
- Values corrected `starSystems.<id>.stores.system[]`, `faction[]` and `blackMarket[]`: sorted
  ordinally like every other list; some systems kept their data's order.

### combat-state.json (breaking)
- Moved `units.<id>.state.precisionStrikeCost` and `vigilanceCost` to
  `units.<id>.state.resolveCosts`, which is `null` for every unit not in the player's lance:
  enemies and allies spend no resolve.
- Meaning changed `units.<id>.facingDegrees` and `terrain`: stated that blips have them at every
  level, as the blip's marker shows them.
- Meaning changed `mapId`: a map no star system's contracts are generated on, e.g. a story map,
  has an id but no entry in the catalog's `mapDefinitions`.
- Meaning changed `movementLegend.moves`: a digit can exceed the rules' evasion table, a jump in
  place gives one pip, and an attack is `..` only at a hex no move reaches.

### combat-map.json
- Meaning changed `mapId`, as in combat-state.json.

### catalog.json (breaking)
- Moved the company results' changes to a stored component's count from
  `eventDefinitions.<id>.options[].outcomes[].results[].statisticChanges[]`, where the statistic
  was named `Item.<definition type>.<component id>`, to `componentChanges[]` next to it: the
  `component` reference and the `count` of working copies added, negative for copies taken.
- Moved the company requirements' comparisons of a stored component's count from `comparisons[]`
  to `componentComparisons[]` next to it, in `eventDefinitions.<id>.requirements`,
  `additionalRequirements[]`, `additionalTargets[].requirements`, `options[].requirements[]` and
  `options[].outcomes[].results[].requirements`: the `component` reference, the `operator` and
  the `value`, compared with the working copies in storage.

## 0.72.2

Changed: mission-outcome.json.

### mission-outcome.json
- Values corrected `trigger` and `salvage.received`: the file is written again once the salvage is
  final, as `salvageChosen` with the salvage received; from 0.69.0 on it kept the write from the
  mission's end, and `salvage.received` stayed `null`.
- Values corrected `objectives[].objectiveIds`: a contract objective that groups no others has its
  own id alone instead of an empty list; the list is never empty.

## 0.72.1

Changed: game-state.json, combat-state.json, combat-map.json, catalog.json.

### game-state.json
- Values corrected `contracts[].target.id` and `activeContract.target.id` of a story contract
  that only travels: `INVALID_UNSET`, the name of the faction the game leaves unset, instead of
  `""`.

### combat-state.json
- Values corrected `units.<id>.side.faction.id` and `buildingSides[].side.faction.id`:
  `Player1sMercUnit` for the player's team instead of `""`.
- Values corrected `units.<id>.pilot.name`: the generic enemy and turret pilots lose the game's
  placeholder last name, "Elite Brawler" instead of "Elite Brawler NONE", "Automatic" instead of
  "Automatic NONE".
- Values corrected `damagedBuildings[].destroyedHexes[].elevation`: rounded to 0.1 m without the
  float noise (208.2, not 208.1999969482422).
- Values corrected `units.<id>.position`, `objectives[].beacon` and `zones[].center`: rounded to
  0.1 m like the elevations.
- Values corrected `units.<id>.facingDegrees`: rounded to 0.1°, from 0 to below 360.

### combat-map.json
- Values corrected `rows[].elevations[]`: rounded to 0.1 m without the float noise; code comparing
  them for equality with other heights can now do so.
- Values corrected `buildings[].position`: rounded to 0.1 m.
- Values corrected `rows[].blockedSteps`: a step is open when the game's path nodes lead to the
  neighbor's center through the two hexes and the two next to both, not only straight across, so
  fewer steps are blocked; re-derive which hexes can be reached from where, and the routes and
  islands drawn from them. The steps still approximate the game's pathing: where a unit's
  movement in the combat state disagrees, the movement is right.

### catalog.json
- Removed `componentDefinitions.upgrades.TargetDummyMod`: the tutorial target dummies' upgrade,
  which no mech outside the tutorial mounts.
- Removed `componentDefinitions.ammunitionBoxes.Ammo_AmmunitionBox_Generic_Flamer`,
  `AmmunitionBox_Flamer` and `Ammo_AmmunitionBox_Generic_SRMInferno`: the game's debug and
  placeholder boxes for ammo the weapons carry themselves, which no store, salvage or mech has.
- Meaning changed `componentDefinitions.weapons.<id>.ammoCategory` of the flamers and inferno
  racks, the weapons with `internalAmmoCapacity`: their ammo category (`Flamer`, `Inferno Rack`)
  instead of `null`. Such a weapon fires its own ammo first and then boxes of that category; the
  catalog has none, so its ammo is still `internalAmmoCapacity`. Code that took a non-null
  `ammoCategory` to mean the weapon needs boxes must check `internalAmmoCapacity` first.

## 0.72.0

Changed: rules.json (breaking), catalog.json.

### rules.json
- Added `combat.weaponCategories.<category>`, keyed by the catalog's weapon `category`, `Melee`
  for melee and jump attacks: `vehicleDamageMultiplier` and `turretDamageMultiplier` multiply every
  hit's damage against a vehicle or turret, and `isAffectedByEvasion` says whether evasive pips
  apply. Melee and jump attacks do double damage to vehicles and turrets; re-derive their damage
  against those.
- Meaning changed `combat.evasion.weaponTypesAffected`: the pips apply only where the weapon's
  category is affected too, which `Support` isn't; re-derive the hit chances of small lasers,
  machine guns and flamers against evasive targets, which ignore the pips.
- Added `combat.evasion.pipsStrippedPerAttack`: the pips the target and any unit a missed shot hits
  lose when an attack ends, hit or miss.
- Added `combat.damage`: `globalMultiplier` on every weapon's damage and
  `artilleryAgainstBuildingsMultiplier` on artillery and mortar damage to buildings.
- Added `combat.strayShots`: which units a missed ranged shot can hit instead of the target.
- Added `combat.coil`: whether a COIL weapon's evasive pips multiply its shots or its damage, and
  whether they count after a jump.
- Moved `combat.toHit.damagedWeapon` and `combat.toHit.weaponInDamagedLocation` to
  `combat.toHit.attackerDamage.mech`; added `combat.toHit.attackerDamage.vehicle` with a vehicle's
  modifiers for its own damage.
- Meaning changed `combat.lineOfFire.obstructedToHit`, `obstructedDamageMultiplier` and
  `indirectFireDamageMultiplier`: stated not to apply against buildings, as in the game.
- Meaning changed `combat.guard`: stated not to reduce stability damage, as in the game.

### catalog.json
- Added `componentDefinitions.weapons.<id>.structureDamage`: damage per shot straight to the
  structure past any armor, e.g. the Gauss rifles' 5.
- Added `componentDefinitions.weapons.<id>.coil`: a COIL weapon's `evasiveExponent` and
  `heatDivisor`, `null` for every other weapon.
- Added `byWeaponCategory` to `terrainDefinitions.<id>.damageDealt` and `damageTaken` and to
  `biomeDefinitions.<id>.damageDealt`: the same category multipliers keyed by weapon category, as
  rules.json's `combat.weaponCategories`, 1 for `Melee` and `AMS`.
- Meaning changed `terrainDefinitions.<id>.toHitAgainstRanged` and `toHitAgainstMelee`: stated not
  to apply to turrets; `damageTaken`: stated not to apply to buildings, as in the game.

## 0.71.0

Changed: game-state.json, star-systems.json, catalog.json.

### game-state.json
- Added `contractEmployers[]` to every full star system entry (`position.starSystem`,
  `position.travel.destination`, `activeContract.starSystem` and `contracts[].starSystem`), as in
  star-systems.json.

### star-systems.json
- Added `starSystems.<id>.contractEmployers[]`: the factions that can hire the company for the
  system's generated contracts, each `employer` with the `targets[]` it can hire it against there.

### catalog.json
- Added `factionDefinitions.<id>`, keyed by faction id: each faction's `allies[]` and `enemies[]`.

## 0.70.0

Changed: game-state.json (breaking), star-systems.json.

### game-state.json
- Removed `workQueue[].target` from the entry of type `FinancialReport`, which was always `null`;
  the other types keep it, each with the reference shape its type carries.
- Values corrected `company.tags[]` and `pilots[].tags[]`: sorted, instead of the order the career
  added them in.

### star-systems.json
- Values corrected `starSystems.<id>.tags[]` and `biomes[]`: sorted, like every other list of
  references.

## 0.69.0

Changed: mission-outcome.json, combat-state.json (breaking).

### mission-outcome.json
- Added `objectives[].id`, the game's id of the objective as the after-action report lists it, and
  `objectives[].objectiveIds`, the ids combat-state.json's `objectives[].id` had for the objectives
  it groups, so the two files' objectives can be matched.

### combat-state.json
- Values corrected `units.<id>.movement.rows[].attacks[]`: a turret's side is `T` instead of `.`;
  `movementLegend.side` lists it. Re-derive which hexes a move reaches where `..` was read as
  unreachable.

## 0.68.0

Changed: game-state.json (breaking), star-systems.json (breaking), mission-outcome.json (breaking),
combat-state.json (breaking), catalog.json (breaking).

### game-state.json
- Meaning changed: every reference to a mech bay mech (a mech order's `workQueue[].target`,
  `lastLance[].mech`, `financialReport.expectedExpenses.mechs[].mech`) carries the mech's name as
  `mechs[].name` has it, no longer the chassis name with the variant. Re-derive a mech's chassis
  from `mechs[].chassis`.
- Type changed `mechs[].loadout.locations[].armor`, `mechs[].afterRefit.locations[].armor` and
  `mechsAwaitingPlacement[].loadout.locations[].armor`: `{front: {current, assigned}, rear}` with
  `rear` `null` outside the torso; `rearArmor` removed. Every file nests a location's armor as
  `{front, rear}` now.
- Type changed `mechs[].refit[]` steps of type `modifyArmor`: `frontArmor` and `rearArmor` replaced
  by `armor: {front, rear}`.
- Type changed `contracts[]`: one shape for every offered contract, with `starSystem` always the
  full entry as in star-systems.json; the contract in the current system is the one whose
  `starSystem.id` is `position.starSystem.id`, and its route is 0 days and 0 C-Bills.
- Type changed `company.reputation`: an object keyed by faction id instead of a list; each entry
  keeps its `faction` reference.

### star-systems.json
- Type changed `starSystems.<id>.tags[]`: bare tag ids, as on the company and the pilots, instead
  of `{id, name}`.

### mission-outcome.json
- Type changed `lance[].damagedLocations[].armor`: `{front: {current, assigned}, rear}`;
  `rearArmor` removed.
- Meaning changed `lance[].mech.name`: the mech's name, as in game-state.json.

### combat-state.json
- Type changed `units.<id>.state.locations[].armor`: `{front, rear}` with `rear` `null` outside a
  mech's torso; `rearArmor` removed.
- Meaning changed `units.<id>.bayMech.name`: the mech's name, as in game-state.json.

### catalog.json
- Added `id` to every entry of every root object, as star-systems.json has it.
- Type changed `vehicleDefinitions.<id>.locations[].armor` and `turretDefinitions.<id>.armor`:
  `{front, rear}` with `rear` always `null`, as every other location's armor.
- Type changed `vehicleDefinitions.<id>.locations[].components[]` and
  `turretDefinitions.<id>.components[]`: `{component, isFixed}` entries as on mechs, all fixed,
  instead of bare references.
- Removed `componentDefinitions.<group>.<id>.bonuses`: UI texts restating the numbers.
- Meaning changed `eventDefinitions.<id>.name` and `options[].name`: plain text, without formatting
  tags, with a switch on a pilot's gender (`{TGT_MW.Gender?NonBinary:are|Default:is}`) resolved to
  its default wording; the callsign and pronoun placeholders stay. Every reference to an event
  (`forcedEvents[].event`, the game state's `events`) carries the same plain name.

## 0.67.0

Changed: game-state.json (breaking), rules.json (breaking), mission-outcome.json,
combat-state.json (breaking), catalog.json (breaking).

### game-state.json
- Renamed `contracts[].negotiation.valuesByShare[].share` to `sharePercent`, and
  `activeContract.terms.shares.pay` and `.salvage` to `payPercent` and `salvagePercent`: a value from
  0 to 100 says so in its name, where the rules' `*Share` values are fractions from 0 to 1.
- Renamed `events.scheduledEvents[].chance` to `chancePercent`, for the same reason.
- Values corrected `mechs[].loadout.locations[].armor.current`, `rearArmor.current` and `structure`:
  rounded to a tenth, so float noise (34.3999977 for 34.4) no longer shows.

### rules.json
- Renamed `combat.stability.unsteadyThresholdPercent` to `unsteadyAbovePercent`, as the combat
  state names the threshold.
- Values corrected `target` of every statistic change (`skills.<skill>.levels[].abilities[].effects[]`
  and the ones in `combat`): one that was `NotSet` is now the target the game resolves it to,
  `SingleTarget` for an `OnHit` or `OnWeaponFire` trigger and `Creator` otherwise. Re-derive whose
  statistic a weapon's on-hit effect changes: the unit hit, not the firing unit.

### mission-outcome.json
- Values corrected `lance[].damagedLocations[].armor.current`, `rearArmor.current` and `structure`:
  rounded to a tenth, as in game-state.json.

### combat-state.json
- Type changed `units.<id>.state.mech.stability`: an object `{current, unsteadyAbove, maxStability}`
  instead of the current instability alone, mirroring `heat`, so the distance to unsteady and to a
  knockdown can be read.

### catalog.json
- Renamed `eventDefinitions.<id>.options[].outcomes[].results[].forcedEvents[].chance` to
  `chancePercent`.
- Values corrected `componentDefinitions.<group>.<id>.effects[].target`: as in rules.json; most
  weapons' on-hit effects were `NotSet` and are now `SingleTarget`.
- Values corrected `terrainDefinitions.<id>.sprintMultiplier`: `null` on the impassable terrains
  whose `moveCost` is already `null`, instead of the game's 99999.9 sentinel.
- Removed `terrainDefinitions.DesignMaskMapBoundary`: no playable hex has it, like the other
  non-terrain masks already left out.

## 0.66.0

Changed: game-state.json (breaking), rules.json (breaking), mission-outcome.json (breaking),
combat-state.json (breaking), catalog.json (breaking). Renames and moves only; no value changed.

### game-state.json
- Renamed `hiringHall[].hireCost` to `price`, and `ship.upgrades[].purchaseCost` to `price`: what
  the company pays to buy something is `price` everywhere, as in the stores.

### rules.json
- Renamed `moraleLevels[].name` to `level`, as in every other level table.
- Moved `campaign.hiring.mercenaryReviewBoardLevels` to the root `mercenaryReviewBoardLevels`,
  next to the other level tables the game state refers to.
- Renamed `moraleLevels[].resolvePerTurn` to `resolvePerRound`.
- Renamed `combat.resolve.baselineGainMultiplier` to `baselineGainScalesWith`.

### mission-outcome.json
- Renamed `payment` to `pay`, as in the contracts' terms.
- Moved `mercenaryReviewBoardReputation` to `reputation.mercenaryReviewBoard`, next to `employer`
  and `target`.
- Renamed `lance[].ammunition[].used` to `shotsFired`.

### combat-state.json
- Renamed `units.<id>.facing` to `facingDegrees`, as every other angle names its unit.
- Renamed `units.<id>.state.mech.heat.dissipationPerRound` to `dissipation`, as on the catalog's
  chassis, heat sinks and mech stats.
- Renamed `units.<id>.state.components[].ammo` to `shotsLeft`.
- Renamed `units.<id>.linesOfFire[].level` to `blocking`.

### catalog.json
- Renamed `componentDefinitions.<group>.<id>.cost` to `value`, as on chassis and mechs.
- Renamed `componentDefinitions.weapons.<id>.instability` to `stabilityDamage`, as the UI names it.
- Renamed `chassisDefinitions.<id>.heatDissipation` to `dissipation`, as on heat sinks and mech
  stats; type changed from integer to number.
- Renamed `turretDefinitions.<id>.firingArc` to `firingArcDegrees`, as in the rules.
- Renamed `terrainDefinitions.<id>.rangedTargetability` to `toHitAgainstRanged`,
  `meleeTargetability` to `toHitAgainstMelee` and `toHitFrom` to `toHitFromRanged`: to-hit
  modifiers where a positive value makes the attack harder, like the rules' `combat.toHit` and the
  weapons' `accuracyModifier` and `refireModifier`.
- Renamed `terrainDefinitions.<id>.heatPerTurn` and `biomeDefinitions.<id>.heatPerTurn` to
  `heatPerRound`.

## 0.65.0

Changed: game-state.json (breaking), rules.json (breaking), combat-state.json, catalog.json
(breaking).

### game-state.json
- Type changed `mechs[]`: one shape per `status`. `readyOnDay` is only on a mech the mech lab holds
  (`readying`, `inMaintenance`), never `null`; `refit` and `afterRefit` only on `inMaintenance`,
  never `null`; `repair` only on `ready`.
- Type changed `contracts[].negotiation`: one shape per `canNegotiate`. `valuesByShare` is only
  on a negotiable contract; a fixed contract carries `pay`, `salvage` and `reputation` directly
  instead of `fixedTerms`.
- Moved `activeContract.terms.payShare` and `salvageShare` to `activeContract.terms.shares.pay` and
  `.salvage`; `shares` is `null` for fixed terms.

### rules.json
- Removed `skills.guts.levels[]` and `skills.tactics.levels[]` `baseHitChancePercent`,
  `baseMeleeHitChancePercent` and `movementAfterStandingUpPercent`, which were always `null`.
  `skills.gunnery.levels[]` keep `baseHitChancePercent`, `skills.piloting.levels[]` the other two,
  never `null`.
- Removed `combat.hitTables.vehicle.prone`, which was always `null`.
- Type changed every statistic change's `appliesTo` (`skills.<skill>.levels[].abilities[].effects[]`
  and the ones in `combat`): `weaponSubType`, `weaponType`, `weaponCategory` and `ammoCategory` are
  replaced by `filter`, `{kind, value}` or `null`: the one filter the game applies, the first set
  in that order, with `value` typed by `kind` (a weapon sub type or type enum value, or a category
  name). Re-derive which weapons an effect applies to from `filter` alone.

### combat-state.json
- Values corrected `units.<id>.tonnage` and `weightClass`: set for every unit in full view, not
  only for sensor blips.

### catalog.json
- Type changed `componentDefinitions.<group>.<id>.effects[].appliesTo`: as the rules' statistic
  changes.
- Type changed `eventDefinitions.<id>.options[].outcomes[].results[].actions[]`: one shape per kind
  of action, keyed by `type`; `value` is replaced by the typed field the kind needs: `days`
  (`MechWarrior_SetTimeout`), `injuries` (`MechWarrior_AddInjuries`, `MechWarrior_SubtractInjuries`),
  `callsign` (`MechWarrior_Callsign`), `pilot` (`MechWarrior_AddHiring`, a pilot reference),
  `itemCollection` (`System_ShowRewards`), `flashpoint` (`System_AddFlashpoint`); the kill, dismiss,
  heal and single injury actions carry only `type`.

## 0.64.0

Changed: game-state.json.

### game-state.json
- Changed `contracts`: never `null` anymore. The current system's contracts are generated for
  the export when the game hasn't yet, so every export in a system lists them; a system that
  offers no contracts has an empty list, and so has the destination of a trip before the jump
  into it.
- Added `trigger` values `jumped`, the ship jumped into its destination system, and `arrived`,
  it reached the planet there after the trip. Like the other career triggers, they also appear
  in rules.json and star-systems.json.

## 0.63.0

Changed: game-state.json, catalog.json.

### game-state.json
- Added `trigger` values `purchaseCompleted` and `saleCompleted`: something was bought or sold in
  a store, once per purchase or sale (a stack sold at once is one sale). Like the other career
  triggers, they also appear in rules.json and star-systems.json.

### catalog.json
- Added `chassisDefinitions.<id>.value`: the chassis' C-Bill value, which a stored chassis' sell
  price and a mech's, chassis' or mech parts' scrap value are computed from with the rules'
  `campaign.finances.sellShare` and `scrapShare`.

## 0.62.1

No export file changed. Stale schemas of earlier versions are now removed from `schemas/`.

## 0.62.0

Changed: combat-state.json.

### combat-state.json (breaking)
- Replaced `units.<id>.state.isDestroyed` with `units.<id>.state.deathMethod`: `null` while the
  unit lives, otherwise how it was destroyed, as the game records it: `HeadDestruction`,
  `CenterTorsoDestruction` (also a turret's), `LegDestruction` (both legs), `AmmoExplosion`,
  `ComponentExplosion`, `EngineDestroyed`, `GyroDestroyed`, `PilotKilled` (also an incapacitated
  pilot), `CockpitDestroyed`, `VitalComponentDestroyed`, `VehicleLocationDestroyed`,
  `PilotEjection`, `PilotEjectionNoMessage`, `PilotEjectionActorDisabled`, `DespawnedNoMessage`,
  `DespawnedEscaped` or `Unknown`.
- Changed `units`: destroyed enemies, which were left out, stay listed in full view (their wrecks
  are on the map), with `linesOfFire` and `movement` `null`, and no longer appear in the player's
  units' `linesOfFire`. An enemy that withdrew or was removed by the mission's script
  (`DespawnedEscaped`, `DespawnedNoMessage`) leaves no wreck and stays left out.
- Changed `objectives[].targetUnitIds`: lists destroyed enemy targets too, now that they are in
  `units`; the units left to destroy are those with `state.deathMethod` `null`.

## 0.61.1

No export file changed. This file is first shipped with the mod.

## 0.61.0

Changed: game-state.json.

### game-state.json
- Added `hiringHall[].tags`: the candidate's pilot tags, which the events' requirements check,
  as `pilots[].tags` has them for the barracks pilots.

## 0.60.0

Changed: star-systems.json, catalog.json.

### star-systems.json
- Added `starSystems.<id>.stores`: object with `system`, `faction` and `blackMarket`, each the ids
  of the item collections that store draws its stock from on arrival, resolved in the catalog's
  `itemCollectionDefinitions`. An empty list means the star system has no such store.

### catalog.json
- Added `itemCollectionDefinitions`: root object keyed by item collection id, each with the
  weighted entries a draw can yield: `components`, `mechs`, `mechParts` (each with `weight` and
  `count`, `null` for unlimited copies), `collections` (nested collections drawn in turn) and
  `emptyWeight`, the weight of a draw that yields nothing.

## 0.59.0

Changed: combat-state.json, combat-map.json.

### combat-state.json (breaking)
- Moved `units.<id>.faction` and `units.<id>.allegiance` to `units.<id>.side`: object with `faction`
  (reference) and `allegiance` (`player`, `ally`, `enemy`, `neutral`), the unit's team's side.
- Moved `units.<id>.state.heat` and `units.<id>.state.stability` to `units.<id>.state.mech`: object
  with `heat` and `stability`, what only a mech has; `null` for vehicles and turrets.
- Type changed: `units.<id>.state.mech.heat` is now an object with `current` (the heat now),
  `dissipationPerRound` (the heat shed at the end of its activation, after the terrain it stands in,
  the biome and effects), `overheatsAbove` (the heat above which it overheats, after the pilot's
  Guts and effects) and `maxHeat` (the heat it shuts down at, after heat banks and effects); was
  the heat number alone.
- Added `units.<id>.state.pilot.skills`: object with `gunnery`, `piloting`, `guts`, `tactics`, the
  skill levels the game computes with now, effects included, for every pilot in full view, enemies
  included.
- Added `objectives[].targetBuildingIds`: ids of the buildings the objective is about (to destroy
  or defend), as `buildings[].id` in combat-map.json.
- Added `objectives[].beacon`: `{ x, y, z }` in meters where the HUD draws the objective's beacon
  (2 m above the ground), e.g. where announced reinforcements will land; `null` where it draws none
  (an objective marking its target units, or a finished one).
- Added `landingZones[]`: where a dropship or drop pods are about to land, which the HUD marks as
  dangerous (a unit standing there is crushed), each with `id` (the dropship's or lance spawner's),
  `kind` (`dropship`, `dropPod`) and `hexes[]` (`{ q, r }` axial hex coordinates: a dropship's whole
  footprint, or one hex per drop pod). Entries appear when a landing is readied and vanish when it
  happens; only story missions have them, a generated mission's reinforcements arrive at a zone of
  type `regionDef_HostileDropZone` in `zones[]`.
- Added `buildingSides[]`: the buildings on a side, as the map highlights them, each with `id` (as
  `buildings[].id` in combat-map.json) and `side` (same shape as `units.<id>.side`), in the order
  of their ids; buildings not listed are on no side. A building's side can change during the
  battle.
- Values corrected: `zones[].objectiveIds` now holds only ids in `objectives[]`; was also ids of
  objectives the HUD doesn't list. Empty where the zone belongs only to those.

### combat-map.json
- Meaning changed: `buildings[]` now lists a dropship only if it had landed when the file was
  written; was every dropship, also one hovering or not yet arrived. A dropship landing later
  stands on hexes the map shows as free and can appear in the combat state's building lists
  without being in the map.

## 0.58.0

Changed: game-state.json, star-systems.json, catalog.json.

### game-state.json
- Added `company.tags`: the company's tags, which the events' requirements check.
- Added `company.statistics`: object keyed by the game's statistic name, the company's statistics
  some event's requirements compare, as they compare them (a number; a statistic the company
  doesn't have is 0).
- Added `pilots[].tags` and `pilots[].statistics`: the same for each pilot, the commander included.
- Added `events`: object with `recentlyDrawnEvents[]` (references to the events the daily roll
  drew lately, which it doesn't draw again until no other event can come), `usedOneTimeEvents[]`
  (references to the one-time events the daily roll has drawn, never drawn again) and
  `scheduledEvents[]` (the events results have scheduled, in the order the game rolls them), each
  with `event` (reference), `pilot` (pilot reference; `null` when the game picks one when it comes),
  `earliestOnDay` and `latestOnDay` (the first day it can come and the day it comes for sure, on the
  `company.daysPassed` scale, assuming a roll every day) and `chance` (percent per daily roll from
  `earliestOnDay` on).

### star-systems.json
- Meaning changed: `starSystems.<id>.tags[]` now lists every tag of the star system, also those
  the starmap doesn't show (e.g. `planet_pop_none`), named as the starmap names them or by id where
  the game has no name; was only the tags the starmap panel shows.

### catalog.json
- Added `eventDefinitions`: root object keyed by event id, each with `name` (title, with the game's
  placeholders such as `{TGT_MW.Callsign}`), `scope` (`Company`, `MechWarrior`, `Commander`, ...),
  `weight` (draw weight), `isOneTime`, `canBeDrawn` (whether the daily roll can draw it; false
  means only another event's result schedules it), `requirements` (`null` when none),
  `additionalRequirements[]`, `additionalTargets[]` (the other pilots or the mech it involves, each
  with `scope` and `requirements`) and `options[]`.
- Added `eventDefinitions.<id>.options[]`: each with `id`, `name` (its text), `requirements[]` (all
  must be met to pick it) and `outcomes[]`, each with `weight` and `results[]`, each with `scope`,
  `requirements` (`null` when it always applies), `addedTags[]`, `removedTags[]`,
  `statisticChanges[]` (`statistic`, `value`, `isSet`; a boolean statistic as 1 or 0),
  `durationDays` (days before the tag and statistic changes are reverted; `null` when they last),
  `actions[]` (`type`, e.g. `MechWarrior_Kill`; `value` as the game's data gives it, `null` when
  none) and `forcedEvents[]` (`event` reference, `scope`, `minDaysWait`, `maxDaysWait`, `chance` in
  percent, `keepsPilot`).
- Added the event requirement shape, used by every `requirements` above: `scope` (whose tags and
  statistics are checked: the company, a pilot or the commander in game-state.json, the current
  star system in star-systems.json), `requiredTags[]`, `excludedTags[]` and `comparisons[]`
  (`statistic`, `operator` such as `GreaterThanOrEqual`, `value`; a statistic the scope doesn't
  have counts as 0).

## 0.57.0

Changed: game-state.json, star-systems.json, financial-report.json, catalog.json.

### game-state.json (breaking)
- Added `financialReport`: object with `dueOnDay`, `spending` and `expectedExpenses`, as
  financial-report.json held them at its root.
- Type changed: `position.starSystem` and `position.travel.destination` are now the full star
  system entry, as in star-systems.json (`id`, `name`, `owner`, `tags`, `biomes`, `maps`,
  `difficulty`, `canTravelTo`, `route`); were `{ id, name }`.
- Added `position.travel.jumpPointDays`: the days between the current star system's planet and its
  jump point, which every route from the current system includes in full.
- Added `position.travel.legEndsOnDay`: the day the leg under way ends (the jump point is reached, a
  jump is done or, after the last jump, the planet is reached), on the `company.daysPassed` scale.
- Type changed: `activeContract.starSystem` is now the full star system entry; was `{ id, name }`.
- Type changed: `contracts[].starSystem` is now the full star system entry for a contract in another
  star system (a travel contract); a contract in the current star system keeps `{ id, name }`.
  Tell the two apart by whether `id` equals `position.starSystem.id`.
- Meaning changed: `company.daysPassed` and `company.currentDate` are now always the latest
  export's: an export on a new day writes the file; were those of the export that last changed
  something else, so a passing day alone didn't write the file.

### star-systems.json (breaking)
- Added `starSystems.<id>.id`: the key repeated, the active definition's id.
- Added `starSystems.<id>.owner`, `starSystems.<id>.tags[]`, `starSystems.<id>.biomes[]`: the
  active definition's owner (reference), tags (references, as the starmap shows them) and biomes
  (references), from the catalog's removed `starSystemDefinitions`. Only the active definition is
  exported; a system's inactive definitions are gone.
- Added `starSystems.<id>.maps[]`: references to the maps the system's contracts can be fought on,
  as the contract generator selects them; the inverse of the catalog's removed
  `mapDefinitions.<id>.starSystems`.
- Moved `starSystems.<id>.travelDays` and `starSystems.<id>.travelCost` to
  `starSystems.<id>.route`: object with `days` and `cost`, 0 and 0 for the current system; the whole
  object is `null` where the game finds no route, always while `canTravelTo` is false.
- Meaning changed: `starSystems.<id>.route.days` during a trip now stays the trip from the current
  system's planet, the whole leg to its jump point included, also the part already travelled; was
  the starmap's countdown of what's left. Correct it mid-trip with `position.travel.jumpPointDays`
  and `position.travel.legEndsOnDay` in game-state.json.

### financial-report.json (breaking)
- Removed. Moved `dueOnDay`, `spending` and `expectedExpenses` to `financialReport` in
  game-state.json. The file is deleted from `exports/` when the game starts.

### catalog.json (breaking)
- Moved `starSystemDefinitions` to star-systems.json: each system's active definition's `name`,
  `owner`, `tags` and `biomes` are on its entry there.
- Moved `mapDefinitions.<id>.starSystems` to star-systems.json `starSystems.<id>.maps`.

## 0.56.0

No export file changed.

## 0.55.0

Changed: combat-state.json.

### combat-state.json
- Added `movementLegend`: how to read the units' `movement`: `moves` (what each character of a
  move row means: the evasion pips a move ending on the hex gives), `fire` (first character of an
  attack: how the unit could fire at the target from the hex: clear, partially blocked, indirect,
  out of range or none) and `side` (second character: the side of the target an attack from the
  hex hits).
- Added `units.<id>.movement`: where a player unit can end its move, read from the game's own
  pathing (terrain costs, slopes and other units included); `null` for allied, enemy and neutral
  units and for a unit that can't move (a turret, a prone or shut down mech). A unit that has
  activated carries its next activation's options from where it stands. `targets[]`: the unit ids
  of the enemies in full view, in the order of `attacks`. `rows[]`: consecutive reachable hexes
  of one row from (`q`, `r`) on, with `walk`, `sprint`, `reverse` and `jump` (one character per
  hex as `movementLegend.moves` says; `sprint` `null` for a legged or unsteady mech, `jump`
  `null` for a vehicle and a mech without a working jump jet) and `attacks[]` (one string per
  target, two characters per hex: fire and side). Elevation and terrain of each hex are in
  combat-map.json.

## 0.54.0

Changed: combat-state.json, combat-map.json, catalog.json.

### combat-state.json
- Added `damagedBuildings[]`: the buildings of combat-map.json's `buildings` below their max
  structure, destroyed ones included, in the order of their ids; the others are undamaged. Each
  with `id` (as in `buildings[].id`), `structure` (what is left) and `destroyedHexes[]`: the hexes
  the building stood on as they are now (`q`, `r`, `elevation` in meters, `terrain` as a terrain
  id or `null` for open ground), `null` while it stands. The blocked steps around a destroyed
  building aren't updated.

### combat-map.json
- New file. Written once when a battle begins (new, or loaded from a save) and deleted with
  combat-state.json: the ground of the running battle on the game's 24 m movement hex grid, in
  rows of characters and numbers.
- Added `mapId`: the map's key in catalog.json's `mapDefinitions`; `null` where unknown.
- Added `hexGrid`: the grid spelled out: `size` (meters between neighboring hex centers),
  `orientation` (`pointyTop`), `centerFormula` (a hex's world position, in the units' meters),
  `directions[]` (the six neighbor offsets `{ q, r }`, in the bit order of `blockedSteps`) and
  `blockedStepsFormula` (how to read `rows[].blockedSteps`).
- Added `terrains`: the legend of `rows[].terrains`: each character's terrain id, as in
  catalog.json's `terrainDefinitions`; `.` is open ground (`null`).
- Added `pathingGroups[]`: groups of the game's pathing ids (catalog.json's
  `movement.pathingId`) that block the same steps; `rows[].blockedSteps` has one entry per group.
- Added `buildings[]`: every building on the map, in the order of their ids: `id`, `name`,
  `position` (`x`, `y`, `z` in meters, `y` the elevation; the placement point, not necessarily the
  center) and `maxStructure`.
- Added `rows[]`: consecutive hexes a unit can stand on in one row, from (`q`, `r`) on, one entry
  per hex in each list: `terrains` (one legend character per hex), `elevations[]` (meters a unit
  standing on the hex is at, a building's roof where one stands), `buildings[]` (index into the
  root's `buildings`, or `null`) and `blockedSteps[]` (per pathing group, one base64 digit per hex
  whose bit i is set when the step to the neighbor at `hexGrid.directions[i]` is blocked by
  slopes). A row with a gap is split in two; hexes outside the playable area aren't listed, and a
  step onto one is blocked too.

### catalog.json
- Added `chassisDefinitions.<id>.movement.pathingId` and
  `vehicleDefinitions.<id>.movement.pathingId`: the game's pathing capabilities the unit moves
  with, which name its entry in combat-map.json's `pathingGroups`.

## 0.53.0

Changed: game-state.json, combat-state.json.

### game-state.json (breaking)
- Moved `workQueue[].mech`, `workQueue[].pilot`, `workQueue[].destination` and
  `workQueue[].shipUpgrade` to `workQueue[].target`: the one reference the entry's `type` uses
  (mech, pilot, star system or ship upgrade); `null` for the financial report and where the game
  can't resolve it.
- Removed from `mechs[].refit[]` the keys a step's `type` doesn't use, which were always `null`.
  Every step has `type` and `isDone`; `installComponent` and `removeComponent` add `component`,
  `damageLevel` and `location`; `repairComponent` adds `component`, `damageLevel` and `location`
  (`null` for a component in storage); `modifyArmor` adds `location`, `frontArmor` and
  `rearArmor`; `repairStructure` adds `location` and `structure`.
- Meaning changed: `company.daysPassed` and `company.currentDate` are now those of the export that
  last changed the file, like `exportedAt`: a day passing with nothing else happening no longer
  rewrites the file; was the current day on every export. Don't read today's day from them.

### combat-state.json (breaking)
- Moved `units.<id>.state.pilotInjuries`, `units.<id>.state.pilotHealth` and
  `units.<id>.state.pilotBonusHealth` to `units.<id>.state.pilot.injuries`,
  `units.<id>.state.pilot.health` and `units.<id>.state.pilot.bonusHealth`; `pilot` is `null`
  for a unit without one.

## 0.52.1

No export file changed.

## 0.52.0

Changed: star-systems.json, catalog.json.

### star-systems.json (breaking)
- Moved `starSystems.<id>.owner`, `starSystems.<id>.tags` and `starSystems.<id>.biomes` to
  catalog.json's `starSystemDefinitions.<id>`. The key stays the id of the system's active
  definition, always a key of `starSystemDefinitions`; `name`, `difficulty`, `canTravelTo`,
  `travelDays` and `travelCost` stay.

### catalog.json
- Added `starSystemDefinitions`: root object keyed by star system definition id, each with
  `name`, `owner` (faction reference), `tags[]` (references, as the starmap shows them) and
  `biomes[]` (references into `biomeDefinitions`). Every definition the game has loaded is in it,
  the inactive versions the story swaps in and out included (e.g. `_Contested`, `_Flipped`).
- Meaning changed: `mapDefinitions.<id>.starSystems[]` now lists the star systems of every
  definition, inactive ones included; was the active definitions only.

## 0.51.1

No export file changed.

## 0.51.0

Changed: rules.json, combat-state.json, catalog.json.

### rules.json (breaking)
- Meaning changed: the `value` of a statistic change to `BaseInitiative`, `PhaseModifier` or
  `PhaseModifierSelf` (in `skills.<skill>.levels[].abilities[].effects[]`,
  `combat.resolve.inspiredEffects[]`, `combat.resolve.initiativeDelayEffects[]`,
  `combat.visibility.firedWeaponsEffects[]` and `combat.visibility.sensorLock.targetEffects[]`)
  now follows the HUD's phase numbering, 5 down to 1: its sign is flipped, so a positive change
  makes the unit act earlier, and a `Set` on `BaseInitiative` holds a HUD phase; was the game's
  internal scale, where units act from 1 up and a lower value acts earlier. Re-derive any
  initiative computed from these effects.

### combat-state.json (breaking)
- Meaning changed: `phase` and `units.<id>.state.initiative` are now numbered as on the HUD,
  from 5 down to 1; was the game's internal scale, from 1 up. Re-derive turn order.

### catalog.json (breaking)
- Meaning changed: the `value` of a statistic change to `BaseInitiative`, `PhaseModifier` or
  `PhaseModifierSelf` in `componentDefinitions.<group>.<id>.effects[]` and
  `terrainDefinitions.<id>.stickyEffects[]` follows the HUD's numbering, as in rules.json above;
  was the game's internal scale.

## 0.50.1

No export file changed.

## 0.50.0

Changed: combat-state.json.

### combat-state.json
- New file. The running battle as the player's HUD shows it, written when a battle is loaded from
  a save, when every phase begins and after every unit's activation once its attacks have
  resolved; deleted when the after-action report is left, when the battle is quit or restarted,
  and on a career load, so it exists only while its battle runs.
- Added `contract`: the contract being fought, as mission-outcome.json's `contract` (`id`,
  `name`, `type`, `displayStyle`, `employer`, `target`, `difficulty`, `starSystem`).
- Added `mapId` (the map's key in catalog.json's `mapDefinitions`, `null` where unknown), `round`
  (from 1), `phase` (the current phase on the game's internal scale, units act from 1 up) and
  `resolve` (the lance's resolve now).
- Added `units`: root object keyed by the game's unit id: the player's and allied units, and the
  enemies and neutral units the HUD shows or last showed; destroyed enemies are left out. Each
  with `faction` (reference), `allegiance` (`player`, `ally`, `enemy`, `neutral`), `visibility`
  (`full`, `blipMaximum`, `blipType`, `blipMinimum`, `lastSeen`), `kind` (`mech`, `vehicle`,
  `turret`; `null` where the HUD doesn't tell), `tonnage` and `weightClass` (what a blip shows;
  `null` otherwise), `definition` (the catalog reference; `null` for the player's mechs and
  blips), `bayMech` (the player's mech as in game-state.json's `mechs`; `null` otherwise),
  `position` (`x`, `y`, `z` in meters, `y` the elevation; the last detected position for
  `lastSeen`), `facing` (degrees clockwise from the map's +Z axis; `null` when last seen),
  `terrain` (terrain id; `null` on open ground and when last seen), `pilot` (reference; only at
  `full`), `state` (only at `full`, else `null`) and `linesOfFire[]` (from the player's units to
  every enemy in full view and from those enemies to the player's units; `null` for the others).
- Added `units.<id>.state`: `isDestroyed`, `locations[]` (`location`, `armor`, `rearArmor`
  (`null` outside a mech's torso), `structure`, whole points as the paper doll shows them),
  `evasionPips`, `guard` (`level`, `isBraced`, `hasCover`, `hasBulwark`), `isEntrenched`,
  `isProne`, `isShutDown`, `isUnsteady`, `heat` and `stability` (`null` for vehicles and
  turrets), `hasActivated`, `initiative` (the phase the unit acts in, on the scale of `phase`),
  `components[]` (`component`, `location`, `damageLevel`, `ammo`: rounds left in an ammo box or
  a weapon carrying its own, `null` for other components), `pilotInjuries`, `pilotHealth` and
  `pilotBonusHealth` (`null` without a pilot), `abilities[]` (`ability`, `cooldown` in
  activations, 0 when ready, `usesLeft` or `null` without a limit), `precisionStrikeCost` and
  `vigilanceCost` (resolve).
- Added `units.<id>.linesOfFire[]`: `target` (a key of `units`), `level` (`clear`,
  `partiallyBlocked`, `blocked`), `fire` (`direct`, `indirect`, `outOfRange`, `none`) and
  `isInFiringArc`.
- Added `objectives[]`: the HUD's objective list in its order, finished ones included: `id`,
  `title`, `status`, `isPrimary`, `progress` (the HUD's progress line; `null` where it shows
  none) and `targetUnitIds[]` (keys of `units` while in full view).
- Added `zones[]`: the zones drawn on the map: `id`, `type` (the game's zone type id), `center`
  (`x`, `y`, `z`), `radius` (meters, of the hexagon the HUD draws), `isPreview` and
  `objectiveIds[]`.

## 0.49.0

Changed: game-state.json, financial-report.json, mission-outcome.json, catalog.json.

### game-state.json (breaking)
- Meaning changed: the `name` of every mech and chassis reference is now the chassis name and
  the variant as the mech bay shows them, e.g. `Zeus - ZEU-6S`; was `Zeus (ZEU-6S)`, with the
  mech's nickname instead of the chassis name for a mech bay mech. Affects `mechs[].chassis`,
  `mechsAwaitingPlacement[].chassis`, `workQueue[].mech`, `lastLance[].mech`,
  `storage.chassis[].chassis`, `storage.mechParts[].mech`, `stores.<store>.mechs[].mech` and
  `stores.<store>.mechParts[].mech`. `mechs[].name` keeps the nickname. Re-derive anything
  matched on mech names.

### financial-report.json (breaking)
- Meaning changed: `expectedExpenses.mechs[].mech.name` as in game-state.json above.

### mission-outcome.json (breaking)
- Meaning changed: `lance[].mech.name`, `salvage.pool.mechParts[].mech.name`,
  `salvage.automatic.mechParts[].mech.name` and `salvage.received.mechParts[].mech.name` as in
  game-state.json above.

### catalog.json (breaking)
- Meaning changed: `chassisDefinitions.<id>.name`, `chassisDefinitions.<id>.stockMech.name`,
  `mechDefinitions.<id>.name` and `mechDefinitions.<id>.chassis.name` as in game-state.json
  above.

## 0.48.0

No export file changed. `modVersion` now has four parts (`0.48.0.0`): the fourth is the build
number, which never changes the format; compare versions on the first three.

## 0.47.0

Changed: catalog.json.

### catalog.json
- Added `mapDefinitions`: root object keyed by map id: every map some star system's contracts
  can be fought on, each with `name`, `biome` (reference into `biomeDefinitions`), `tags[]`
  (map tags), `starSystems[]` (references to the star systems whose contracts the game can put
  on the map, selected as the contract generator does), `weight` (the draw weight among a
  system's maps) and `terrainCoverage` (each terrain's share of the map's playable cells, keyed
  by terrain id, plus `none` for cells without terrain; shares sum to 1). The contract type and
  the recently offered maps filter further and aren't exported.

## 0.46.0

Changed: catalog.json.

### catalog.json
- Added `sourceFingerprint`: a hash of the game's data the catalog was built from. The catalog
  is now rebuilt on a career load only when it's missing, malformed, written by another
  `modVersion` or with another fingerprint, so its `exportedAt` can be older than the career
  files'.

## 0.45.0

Changed: rules.json, financial-report.json, catalog.json.

### rules.json
- Added `contractTypes[].payMultiplier`: multiplies the pay of the type's contracts; `null` for
  priority missions, whose pay the story sets.
- Added `skills.<skill>.levels[].movementAfterStandingUpPercent`: Piloting only: the share of
  its walk, sprint and backward distance a mech keeps in the round it stands up; `null` on the
  other skills.
- Added `combat`: the combat constants the game's code reads, as objects per subject. A to-hit
  modifier is added to the attack's difficulty: positive makes the shot harder, negative is a
  bonus. Constants the game never reads or reads only for presentation or the AI are left out.
- Added `combat.toHit`: how a hit chance follows from its difficulty: `difficultySteps[]`
  (`fromDifficulty`, `chanceLostPerPoint`), `roundedToPercent`, `minChancePercent`,
  `maxChancePercent`, `canTotalModifierBeNegative`, `rangeBands` (`withinMinimumRange`, `short`,
  `medium`, `long`, `maximum`), `targetWeightClass` (`mech`, `vehicle`, `turret`, each by
  `light`, `medium`, `heavy`, `assault`), `elevation` (`levelHeight` in meters,
  `modifierPerLevel`, `usesMultipleLevels`, `appliesPenalties`), `targetShutdown`,
  `targetProne`, `attackerSprinted`, `attackerWalked` (`mech` by weight class, `vehicle`),
  `attackerStoodUp`, `attackerOverheated`, `armMountedWeapon`, `damagedWeapon`,
  `weaponInDamagedLocation` (`penalized`, `nonFunctional`), `precisionStrike`,
  `mechFiringArcDegrees` and `turretGunnery`.
- Added `combat.evasion`: `pipsByDistanceMoved[]` (`moreThan` in meters, `pips`),
  `modifierByPips[]` (from one pip up; each pip beyond the last entry adds the difference of the
  last two), `weaponTypesAffected[]`, `vehiclePipMultiplier` and `canVehiclesBeEvasive`.
- Added `combat.guard`: `damageMultiplierByLevel[]` (indexed by guard level, from unguarded up),
  `unsteadyBreaksGuard`, `solidDamageBreaksGuard`, `sensorLockBreaksGuard` and
  `canVehiclesBeGuarded`.
- Added `combat.lineOfFire`: `obstructedToHit`, `obstructedDamageMultiplier`,
  `indirectFireToHit`, `indirectFireDamageMultiplier`, `clearRatio`, `obstructedRatio` (shares
  of clear sight lines: clear at or above the first, obstructed at or above the second, blocked
  below) and `minRatioFromUnits`.
- Added `combat.heat`: `maxHeat` (shutdown), `overheatsAbove`, `walkHeat`, `sprintHeat`,
  `jumpHeat` (`unitSize` in meters, `perUnit`, `minimum`), `engineDamageHeat`,
  `engineHeatSinks`, `engineHeatSinkDissipation`, `maxHeatAfterRestart`,
  `heatGeneratedMultiplier`, `heatSinkMultiplier` and `overheatStructureDamageByWeightClass`
  (`light`, `medium`, `heavy`, `assault`).
- Added `combat.stability`: `unsteadyThresholdPercent`, `levels`, `levelsRecovered`
  (`stationary`, `walked`, `jumped`, `stoodUp`, `braced`, `deferred`), `levelsAdded`
  (`deathFromAbove`, `falling`), `entrenchedInstabilityMultiplier`,
  `minStabilityPerDestroyedLocation`, `onlyLegsRaiseMinStability` and `instabilityFromDamage`
  (`sideTorsoDestroyed`, `centerTorsoDamaged`, `armDestroyed`, `legDamaged`, shares of the
  stability bar).
- Added `combat.injuries`: `fromSideTorsoDestruction`, `fromAmmoExplosion` and
  `fromHeatShutdown`.
- Added `combat.melee`: `deathFromAboveToHit` and `maxHeightDifference` (meters). Damage and
  instability are the chassis' in the catalog.
- Added `combat.criticalHits`: `minChance`, `aiAttackerMultiplier` and `emptySlotsAbsorbCrits`.
- Added `combat.hitTables`: relative weights per location by attack direction: `mech` and
  `vehicle`, each with `fromFront`, `fromLeft`, `fromRight`, `fromBack` (location name to
  weight), `prone` (`null` for vehicles) and `artillery[]` (the locations an artillery strike
  hits); `calledShot` (`weightMultiplier`, `degradePerHit`) and `clusteredHits`
  (`sameLocation`, `adjacentLocation`, `otherLocation`, `isHeadMultiplied`,
  `canHeadBeClustered`).
- Added `combat.visibility`: `spotterRange` and `sensorRange` (meters),
  `spotterRangePerTactics`, `sensorHysteresis`, `shutdownSpotterMultiplier`,
  `proneSpotterMultiplier`, `shutdownSignatureModifier`, `shutdownVisibilityModifier`,
  `ghostedUnitsHideBlips`, `sensorLock` (`evasivePipsStripped`, `targetEffects[]` as statistic
  changes) and `firedWeaponsEffects[]` (statistic changes).
- Added `combat.resolve`: `min`, `max`, `startsAt`, `inspiredAt`, `inspireCost`,
  `isAutoInspired`, `inspiredEffects[]` (statistic changes), `baselineGainPerRound`,
  `baselineGainMultiplier` (`none`, `livingUnits`, `livingElitePilots`),
  `addsMoraleLevelResolve`, `addsUnitResolveBonuses`, `onlyBiggestUnitResolveBonus`,
  `eventThresholds` (`majorityHit`, `majorityMiss`, `minorArmorDamage`, `majorArmorDamage`,
  shares), `fromEvents` (resolve gained per event: `enemyDestroyed` by weight class,
  `enemyDestroyedInMelee`, `enemyCriticalHit`, `enemyAmmoExplosion`, `enemyWeaponDestroyed`,
  `enemyLocationDestroyed`, `enemyKnockedDown`, `deathFromAboveDealt`, `enemyMinorArmorDamage`,
  `enemyMajorArmorDamage`, `majorityHit`, `majorityMiss`, `objectiveCompleted`,
  `objectiveFailed`), `initiativeDelayEffects[]` (statistic changes) and `canAiGainResolve`.
- Added `combat.movement`: `overheatedPenalty`, `legDamagePenalty` (`penalized`,
  `nonFunctional`, `destroyed`; shares lost per leg), `minMultiplier`, `pivotInPlaceThreshold`
  (meters) and `hexWidth` (meters).
- Added `campaign`: the campaign constants the game's code reads, as the career's difficulty
  settings adjust them, as objects per subject.
- Added `campaign.mechLab`: costs as `{ techPoints, cBills }`: `structureRepair` (per point),
  `destroyedLocationRepairMultipliers`, `componentRepair` (per slot), `install` (per slot, by
  `ballistic`, `missile`, `energy`, `support`, `ammunition`, `jumpJet`, `heatSink`, `other`),
  `armorChange` (per point), `uninstallTechPointsPerSlot`, `readyMechTechPoints` and
  `cancelRefundShare`. An order takes its tech points divided by the company's `mechTech`,
  rounded up, in days.
- Added `campaign.medBay`: `healPointsBase`, `healPointsPerMedTech`, `injuryCost`,
  `lethalInjuryCost`, `incapacitatedCost` and `deathChance` (`incapacitated`, `lethal`,
  `reductionPerGuts`).
- Added `campaign.hiring`: `costPerBaseSkillPoint`, `salaryPerBaseSkillPoint`,
  `salaryPerBonusSkillPoint` (per report), `pilotsPerSystem`, `roninChance`,
  `generatedPilotHealth`, `mercenaryReviewBoardLevels[]` (`level`, `startsAt`,
  `maxHireableSkillTotal`, `contractReputationMultiplier`; what `company.mercenaryReviewBoard`
  refers to), `moraleLimits[]` (`belowMorale`, `maxHireableSkillTotal`),
  `maxPilotsPerBarracksPod` and `maxMechsPerMechBayPod`.
- Added `campaign.salvage`: `defaultPotential`, `victory`, `retreat` and `defeat` (`chance`,
  `chanceLostPerMechDestroyed`), `priorityShare`, `maxPriorityPicks`, `floorBonus`,
  `pickWeights` (`weapon`, `component`, `mechPart`), `rareSwaps` (`weapons` and `upgrades`,
  each `veryRare` and `rare` with `rarities[]` and `chanceOffset`, and `divisor`),
  `culledComponentRarity`, `destroyedMechRecoveryChance` and `assembledMechsComeEquipped`.
- Added `campaign.finances`: `daysPerReport`, `mechCostPerReport`, `shipMaintenance`
  (`leopard`, `argo`), `spendingLevels[]` (`level`, `costMultiplier`, `moraleChange`; what
  financial-report.json's `spending.level` refers to), `jumpCost`, `sellShare`,
  `sellDamagedShare`, `scrapShare`, `maxDebt`, `badFaithMoraleChange` and
  `catastropheMoraleChange`.
- Added `campaign.contracts`: `payPerDifficulty`, `payVariance`, `guaranteedPayShare`,
  `goodFaithPayShare`, `noFaithPayShare`, `reputation` (`basePerDifficulty`, `baseAddition`,
  `negotiablePerDifficulty`, `negotiableAddition`, and `employer`, `target`,
  `mercenaryReviewBoard` each with `completed`, `goodFaith`, `badFaith` multipliers),
  `experience` (`perDifficulty`, `failed`, `retreatedGoodFaith`, `retreatedBadFaith`),
  `maxGlobalDifficulty`, `difficultyVariance`, `maxPerSystem`, `renewedPerRefresh`,
  `refreshDays` and `removedPerCompleted`.
- Added `campaign.alliances`: `reputationNeeded`, `reputationAfterBreaking` and `cooldownDays`.
- Added `campaign.travel`: `daysPerJump`, `daysPerJumpFromFuelStation`,
  `defaultDaysToJumpPoint` and `maxJumpDistance` (light years).

### financial-report.json (breaking)
- Removed `spending.options[].moraleChange`; look it up by `level` in rules.json's
  `campaign.finances.spendingLevels[]`.

### catalog.json
- Added `terrainDefinitions`: root object keyed by design mask id: what forest, water, rough
  ground, roads and the other terrains do to a unit in them, each with `name`, `moveCost`
  (`mech`, `tracked`, `wheeled`, each by `light`, `medium`, `heavy`, `assault`: the movement a
  step into the terrain costs per meter, open ground costs 1.05; `null` where impassable),
  `sprintMultiplier`, `visibilityMultiplier`, `visibilityHeight` (meters),
  `sensorRangeMultiplier`, `signatureMultiplier`, `rangedTargetability`, `meleeTargetability`
  and `toHitFrom` (to-hit modifiers), `grantsGuarded`, `heatSinkMultiplier`, `heatPerTurn`,
  `damageDealt` and `damageTaken` (`all`, `support`, `energy`, `ballistic`, `missile`
  multipliers) and `stickyEffects[]` (statistic changes a unit picks up in it). A cell has one
  terrain at most.
- Added `biomeDefinitions`: root object keyed by the biome id the star systems refer to: what a
  map's biome does to every unit on top of the terrain: `name`, `heatSinkMultiplier`,
  `heatPerTurn` and `damageDealt` (multipliers as above).

## 0.44.0

Changed: mission-outcome.json, catalog.json.

### mission-outcome.json
- Added `rounds`: the combat rounds the mission took, counted from 1.
- Added `lance[].ammunition[]`: one object per ammo box and per weapon that carries its own ammo
  (flamers, Infernos), in the mech's component order, with `component` (component reference),
  `location` and `used`, the shots fired, counting those fired from a box destroyed since. The
  capacity is the catalog's `capacity` (ammo boxes) or `internalAmmoCapacity` (weapons).

### catalog.json
- Added `componentDefinitions.weapons.<id>.internalAmmoCapacity`: the shots per mission of a
  weapon that carries its own ammo instead of using ammo boxes; `null` for every other weapon.

## 0.43.0

Changed: mission-outcome.json, salvage-received.json.

### mission-outcome.json
- Added `salvage.received`: object with `components[]` (`component`, `count`, `damagedCount`) and
  `mechParts[]` (`mech`, `count`), everything the company got: the priority salvage, the salvage
  drawn at random and `salvage.automatic`; `null` when the mission ends, filled in once the
  salvage is final, when the file is written again with `trigger` `salvageChosen`. The offer
  (`salvage.total`, `priority`, `pool`, `automatic`) stays as it was.

### salvage-received.json (breaking)
- Removed. Its `received` is now `salvage.received` in mission-outcome.json. A leftover file from
  an earlier version is never deleted by the mod; delete it by hand.

## 0.42.0

Changed: game-state.json, financial-report.json.

### game-state.json (breaking)
- Moved `company.finances.funds` to `company.funds`.
- Moved `company.finances.nextReportOnDay` to `dueOnDay` in financial-report.json.
- Moved `company.finances.spending` to `spending` in financial-report.json.
- Moved `company.finances.expectedExpenses` to `expectedExpenses` in financial-report.json.
- Removed `company.finances`.

### financial-report.json
- New file. The next monthly financial report as the finance screen projects it, written with the
  other career files on every career export.
- Added `dueOnDay`: the day the report is due, on the `company.daysPassed` scale.
- Added `spending`: `level` (the spending level confirmed at the last report) and `options[]`
  (`level`, `expectedExpenses`, `moraleChange`), unchanged shape.
- Added `expectedExpenses`: `total`, `ship` (`name`, `amount`), `shipUpgrades[]` (`upgrade`,
  `amount`), `mechs[]` (`mech`, `amount`), `pilots[]` (`pilot`, `amount`), unchanged shape.

## 0.41.1

No export file changed.

## 0.41.0

Changed: game-state.json.

### game-state.json (breaking)
- Meaning changed: `trigger` no longer takes `workOrderCompleted`; a finished work order now
  exports as `mechLabOrderCompleted` (a mech lab order: repair, refit, readying),
  `pilotHealed` (a med bay heal) or `shipUpgradeCompleted` (a ship upgrade), still once per day.
  When several kinds finish on the same day, the trigger names the one the game completed last.
  Re-derive any match on `workOrderCompleted`.

## 0.40.0

Changed: mission-outcome.json.

### mission-outcome.json
- Added `lance[].damagedLocations[].armor`: `current` and `assigned`, as in the mech bay, and
  `lance[].damagedLocations[].rearArmor` the same, `null` outside the torso.
- Meaning changed: `lance[].damagedLocations[]` now lists every location that lost armor or
  structure; was only the locations with structure damage. Re-derive structure damage from
  `structure` alone, not from an entry's presence.

## 0.39.0

Changed: game-state.json.

### game-state.json
- Added `company.temporaryMechTechChanges[]` and `company.temporaryMedTechChanges[]`: the changes
  events made to `company.mechTech` and `company.medTech` for a number of days, each `amount` and
  `endsOnDay` (the last day it applies, on the `company.daysPassed` scale; reverted as the next day
  starts), ordered by end day. `mechTech` and `medTech` include them until they end.

## 0.38.0

Changed: rules.json, catalog.json.

### rules.json
- Added `jumpDistances[]`: `jumpJets` (the number of working jump jets, from 1 up) and `distance`
  (meters, before the jump jets' `JumpDistanceMultiplier` effects, which add to a multiplier of 1);
  the last entry applies to any higher number of jump jets.

### catalog.json
- Added `componentDefinitions.jumpJets.<id>.minTonnage` and `maxTonnage`: the chassis tonnage
  range that can mount the jump jet, both inclusive.
- Added `componentDefinitions.*.<id>.effects[]` (every group: `weapons`, `ammunitionBoxes`,
  `heatSinks`, `jumpJets`, `upgrades`): the statistics the component changes while mounted, in the
  same shape as the ability effects of `rules.json` (`statistic`, `operation`, `value`,
  `appliesTo`, `duration`, `target`, `trigger`); `trigger` is `Passive` for a change that always
  applies.

## 0.37.0

Changed: rules.json.

### rules.json
- Added `skills.gunnery.levels[].baseHitChancePercent`: the hit chance of ranged attacks at the
  level, before an attack's modifiers; `null` on the other skills' levels.
- Added `skills.piloting.levels[].baseMeleeHitChancePercent`: the hit chance of melee attacks at
  the level, before an attack's modifiers; `null` on the other skills' levels.
- Added `skills.*.levels[].abilities[].activationTiming`, `targeting` (`null` for an ability that
  isn't aimed), `cooldown` (rounds before it can be used again; `null` without one), `uses` (per
  mission; `null` without a limit) and `effects[]`: the statistics the ability changes, each with
  `statistic`, `operation`, `value` (a boolean, integer or number, as the statistic requires),
  `appliesTo` (`null` for the unit's own statistic; else `collection`, `weaponSubType`,
  `weaponType`, `weaponCategory`, `ammoCategory` naming the pilot or weapons it applies to),
  `duration` (`count` and `unit`; `null` when it lasts the whole mission), `target` (`Creator` for
  the ability's own unit, else the unit it targets) and `trigger`. Empty for Multi-Target and
  Sensor Lock, which the game hardcodes. Initiative values are on the game's scale, where units
  act from the lowest value up, so an ability that acts earlier lowers it.

## 0.36.0

Changed: game-state.json, star-systems.json, mission-outcome.json.

### game-state.json (breaking)
- Renamed `position.system` to `position.starSystem`.
- Removed `position.owner`, `position.tags` and `position.biomes`: read them from
  star-systems.json by the star system's id.
- Removed `position.travel.owner`; `position.travel` is `{ destination, arrivesOnDay }`.
- Removed `contracts[].travel`; its star system, tags and days are in star-systems.json.
- Added `contracts[].starSystem`: reference to the star system the mission is fought in, always
  set, also for a contract in the current system. Travel days are star-systems.json's
  `travelDays`.
- Removed `activeContract.travel`.
- Added `activeContract.starSystem`: reference, always set.
- Added `activeContract.arrivesOnDay`: the day of arrival while travelling to the contract's star
  system, as `position.travel.arrivesOnDay`; `null` otherwise.

### star-systems.json (breaking)
- Removed `starSystems.<id>.system`; the id is the entry's key.
- Added `starSystems.<id>.name`: the name the UI shows, as in the references to the star system.

### mission-outcome.json (breaking)
- Renamed `contract.system` to `contract.starSystem`.

## 0.35.0

Changed: game-state.json, mission-outcome.json, salvage-received.json, catalog.json.

### game-state.json (breaking)
- Removed `componentDefinitions`: resolve every component reference (`{ id, name, type }`, `type`
  naming the group) in catalog.json's `componentDefinitions` of the same `modVersion`.
- Removed `mechs[].weightClass` and `mechsAwaitingPlacement[].weightClass`: read them from the
  catalog's `chassisDefinitions` by the chassis id.
- Removed `mechs[].loadout.tonnage` (`used`, `max`); the same in `mechs[].afterRefit` and
  `mechsAwaitingPlacement[].loadout`.
- Added `mechs[].loadout.usedTonnage`: the tonnage of the chassis, armor and components; the
  same in `mechs[].afterRefit` and `mechsAwaitingPlacement[].loadout`. The maximum is the catalog
  chassis' `tonnage`.
- Removed `mechs[].loadout.locations[].armor.max` and `rearArmor.max`; `armor` and `rearArmor`
  are `{ current, assigned }`. The maximum is the catalog chassis location's `maxArmor`.
- Type changed: `mechs[].loadout.locations[].structure` is now a number, the structure left (zero
  when destroyed); was `{ current, max }`. The maximum is the catalog chassis location's
  `structure`.
- Removed `mechs[].loadout.locations[].hardpoints` and `mechs[].loadout.locations[].slots`: read
  them from the catalog chassis location. The three location changes apply to `mechs[].afterRefit`
  and `mechsAwaitingPlacement[].loadout` too.
- Removed `storage.chassis[].weightClass`, `tonnage`, `maxArmor`, `hardpoints` and `maxJumpJets`;
  an entry is `{ chassis, count }`.
- Removed `stores.*.mechs[].weightClass`, `tonnage`, `maxArmor`, `hardpoints` and `maxJumpJets`
  (`system`, `faction`, `blackMarket`); an entry is `{ mech, count, price }`.

### mission-outcome.json (breaking)
- Removed `componentDefinitions`: resolve references in catalog.json.
- Type changed: `lance[].damagedLocations[].structure` is now a number, the structure left; was
  `{ current, max }`.

### salvage-received.json (breaking)
- Removed `componentDefinitions`: resolve references in catalog.json.

### catalog.json
- Added `chassisDefinitions.<id>.locations[].slots`: the location's component slots, which a
  loadout's components fill by their own `slots`.

## 0.34.0

Changed: star-systems.json.

### star-systems.json
- New file. Every star system on the starmap, locked story systems included, written with the
  other career files on every career export.
- Added `starSystems.<id>`: keyed by star system id, each with `system` (reference), `owner`
  (faction reference), `tags[]` (planet tags as the starmap panel shows them), `biomes[]`,
  `difficulty` (as the starmap shows it), `canTravelTo` (whether the travel requirements are met),
  `travelDays` and `travelCost` (days and C-Bills of the trip from the current system, as the
  starmap shows them; 0 for the current system, `null` where the game finds no route, always while
  `canTravelTo` is false). The contracts' `travel.days` in game-state.json come from the same
  route search.

## 0.33.0

Changed: mission-outcome.json.

### mission-outcome.json
- Added `lance[].pilotReadyOnDay`: the day the pilot is out of the med bay, as the barracks counts
  it once the contract is resolved; `null` for an uninjured or killed pilot.
- Added `lance[].repair`: `days` and `cost` of the mech bay's Repair of the mech, as
  `mechs[].repair` in game-state.json; `null` without damage and for a lost mech.

## 0.32.0

Changed: catalog.json.

### catalog.json
- Added `vehicleDefinitions.<id>`: every vehicle, DLC included, with `name`, `weightClass`,
  `tonnage`, `movementType`, `movement` (`walk`, `sprint` in meters) and `locations[]` (front to
  rear, then the turret if any) with `location`, `armor`, `structure` and `components[]`
  (component references). Armor and structure are the combat values, three quarters of the game's
  definition. The tutorial's target vehicles are left out.
- Added `turretDefinitions.<id>`: every turret with `name`, `weightClass`, `tonnage`, `firingArc`
  (degrees), `armor`, `structure` (combat values) and `components[]`.

## 0.31.0

Changed: game-state.json, mission-outcome.json, salvage-received.json, catalog.json.

### game-state.json (breaking)
- Removed `mechs[].role`, `mechsAwaitingPlacement[].role`, `storage.chassis[].role` and
  `stores.*.mechs[].role` (`system`, `faction`, `blackMarket`).
- Added `componentDefinitions.*.<id>.isSalvageable`: whether the component can come as salvage.

### mission-outcome.json
- Added `componentDefinitions.*.<id>.isSalvageable`.

### salvage-received.json
- Added `componentDefinitions.*.<id>.isSalvageable`.

### catalog.json
- New file. Every chassis, mech and component the game has loaded for the career, DLC included.
  It describes the game, not the career; it's written on a career load and once after every game
  start. Left out: the tutorial's target dummies, the stock mech copies the game makes for unlocked
  skins, and the game's internal melee and AI weapons.
- Added `chassisDefinitions.<id>`: `name` (with the variant), `weightClass`, `tonnage`,
  `initialTonnage` (the bare chassis), `maxJumpJets`, `heatDissipation` (per round, on top of heat
  sinks), `movement` (`walk`, `sprint` in meters), `melee` (`damage`, `stabilityDamage`,
  `deathFromAboveDamage`, `deathFromAboveSelfDamage`, before upgrades), `stockMech` (reference;
  `null` when its definition is missing) and `locations[]` (head to legs) with `location`,
  `maxArmor` (`front`, `rear`; `rear` `null` outside the torso), `structure` and `hardpoints`
  (`ballistic`, `energy`, `missile`, `support`).
- Added `mechDefinitions.<id>`: `name`, `chassis` (reference), `value` (C-Bills), `usedTonnage`,
  `stats` (as `mechs[].loadout.stats` in game-state.json), `isSalvageable` (whether defeating it
  can give its parts as salvage; hero variants can't) and `locations[]` with `location`, `armor`
  (`front`, `rear`) and `components[]` (`component`, `isFixed`), in the game's order.
- Added `componentDefinitions`: the same groups and entry shapes as the other files'
  `componentDefinitions`, for every component the game has loaded.

## 0.30.0

Changed: game-state.json.

### game-state.json
- Added `mechs[].repair`: `days` and `cost` (C-Bills) the mech bay's Repair would take if ordered
  now, as the Repair confirmation prices it; `null` for an undamaged mech and for a mech in
  maintenance or being readied. `days` is the repair alone; an order queued behind others
  finishes later.

## 0.29.0

Changed: game-state.json.

### game-state.json (breaking)
- Meaning changed: points in time are day numbers on the `company.daysPassed` scale; were
  countdowns in days. Re-derive remaining days as `<day> - company.daysPassed`.
- Renamed `mechs[].daysUntilReady` to `mechs[].readyOnDay`; `null` when the mech is `ready`
  (was 0).
- Renamed `pilots[].daysUntilReady` to `pilots[].readyOnDay`; `null` when the pilot is `ready`
  (was 0).
- Renamed `pilots[].spirits.daysRemaining` to `pilots[].spirits.endsOnDay`; `null` for normal
  spirits and when the game sets no end.
- Renamed `pilots[].serviceRecord.daysOnCrew` to `pilots[].serviceRecord.hiredOnDay`.
- Renamed `company.finances.daysUntilReport` to `company.finances.nextReportOnDay`.
- Renamed `position.travel.daysLeft` to `position.travel.arrivesOnDay`.
- Added `activeContract.travel.arrivesOnDay`: the day of arrival while travelling to the
  contract's system; `null` otherwise.
- Meaning changed: `activeContract.travel.days` is now `null` while travelling to the system (use
  `arrivesOnDay`); was the trip's remaining days.
- Added `workQueue[]`: the timeline's entries by the day they finish, each with `type` (the game's
  work order type name), `finishesOnDay`, and at most one target set, the others `null`: `mech`
  (reference, mech lab), `pilot` (reference, med bay), `destination` (star system reference,
  travel) or `shipUpgrade` (reference); the financial report has none. Entries finishing on the
  same day are ordered mech lab, med bay, travel, financial report, ship upgrade.
- Removed `mechs[].workQueuePosition`. Re-derive the mech lab order from the order of
  `workQueue[]` entries with a `mech`.

## 0.28.0

Changed: game-state.json, rules.json, mission-outcome.json.

### game-state.json
- Added `ship`: the Argo; `null` while the company still flies the Leopard. `ship.upgrades[]`
  lists the upgrades the engineering screen shows, each with `upgrade` (reference), `status`
  (`installed`, `installing`, `available` or `locked`), `category` (reference, the screen's
  group), `location` (the game's dropship location), `details` (description text),
  `requiredUpgrades[]` (references that must be installed first), `purchaseCost` (C-Bills),
  `upkeep` (C-Bills per report once installed), `installDays` and `effects[]` with `statistic`
  (the game's company statistic name), `value`, `isSet` (sets the statistic instead of adding)
  and `description` (the game's text, e.g. "+2 Morale").
- Added `trigger` value `shipUpgradeStarted`: an Argo upgrade was bought.
- Values corrected: `contracts[].description` and `activeContract.description` are plain text;
  had the game's formatting tags (e.g. `<i>Argo</i>`).

### rules.json
- Values corrected: `contractTypes[].description`, `skills.*.description` and
  `skills.*.levels[].abilities[].description` are plain text; had the game's formatting tags.

### mission-outcome.json
- Values corrected: `objectives[].title` is plain text; had the game's formatting tags.

## 0.27.0

Changed: mission-outcome.json, salvage-received.json.

### mission-outcome.json
- New file. The latest mission as it ended, written in combat when the mission ends, before the
  salvage is chosen, with `trigger` `missionCompleted`.
- Added `contract`: `id` (`null` where the game keeps no reference), `name`, `type` (reference to
  `contractTypes` in rules.json), `displayStyle`, `employer`, `target`, `difficulty` and `system`
  (star system reference).
- Added `state`: the game's contract state (completed, retreated from or failed), and
  `isGoodFaithEffort`: whether a retreated or failed contract still counts as a good faith effort.
- Added `objectives[]`: `title`, `isPrimary`, `status` (the game's objective status).
- Added `payment` (C-Bills, with the good faith modifier and bonuses applied), `reputation`
  (`employer`, `target`; `null` for a faction that doesn't gain reputation),
  `mercenaryReviewBoardReputation` (the rating change) and `experiencePerPilot`.
- Added `lance[]`: the company's units in lance order, each with `pilot` (`id`, `name`,
  `callsign`), `injuries` (including those from before the mission), `health` (injuries the pilot
  can take before being incapacitated), `isIncapacitated`, `isKilled`, `mechKills`, `otherKills`,
  `mech` (`id` as `mechs[].id` in game-state.json, `name`), `isMechLost` (destroyed and not
  recovered), `damagedLocations[]` (`location`, `structure` as `{ current, max }`, head to legs,
  structure damage only) and `damagedComponents[]` (`component`, `location`, `damageLevel`).
- Added `salvage`: `total` (items of the pool the company gets), `priority` (how many it picks
  itself; the rest are drawn at random), `pool` and `automatic` (the surviving components of the
  company's own lost mechs, on top of `total`), each `components[]` (`component`, `count`,
  `damagedCount`) and `mechParts[]` (`mech`, `count`).
- Added `componentDefinitions`: as in game-state.json, for the components the file refers to.

### salvage-received.json
- New file. Written once the salvage is final, after the priority salvage is confirmed or right
  away when there is nothing to choose, with `trigger` `salvageChosen`; a new mission outcome
  deletes it, so it always belongs to the mission-outcome.json next to it.
- Added `contract`: as in mission-outcome.json.
- Added `received`: everything the company gets (priority, random and automatic salvage), as
  `components[]` and `mechParts[]` in mission-outcome.json's `salvage`.
- Added `componentDefinitions`: as in game-state.json.

## 0.26.0

Changed: game-state.json.

### game-state.json
- Added `mechsAwaitingPlacement[]`: the new mechs the game asks to place, store or scrap because
  every mech bay is full, in neither the mech bay nor storage until then, each with `chassis`
  (reference), `weightClass`, `role` and `loadout` (as `mechs[].loadout`: the stock loadout for a
  mech assembled from parts or bought, only the fixed components for a salvaged chassis).
- Added `trigger` value `mechPlacementShown`: the placement dialog is shown, e.g. after salvage
  completes a mech, with the contract's results already applied.

## 0.25.0

Changed: game-state.json.

### game-state.json
- Added `contracts[].description` and `activeContract.description`: the text the contract details
  show.

## 0.24.0

Changed: game-state.json.

### game-state.json
- Added `trigger` value `experienceSpent`: a pilot's training was confirmed in the barracks.

## 0.23.0

Changed: game-state.json.

### game-state.json (breaking)
- Removed `contracts[].negotiation.options[]` (every slider combination).
- Added `contracts[].negotiation.valuesByShare[]`: one row per slider position, `share` (0 to 100
  percent), `pay`, `salvage` (`total`, `priority`) and `reputation` (`employer`, `target`);
  `null` when the contract can't be negotiated. A row is not a deal: pay comes from the row at the
  pay share, salvage from the row at the salvage share, reputation from the row at the share left
  over (pay and salvage shares add up to at most 100, exactly 100 when the employer gains no
  reputation).
- Added `contracts[].negotiation.fixedTerms`: the terms of a contract that can't be negotiated,
  in the shape of `activeContract.terms` (`payShare` and `salvageShare` `null`, `pay`, `salvage`,
  `reputation`); `null` otherwise.
- Renamed `storage.mechs[]` to `storage.chassis[]`.
- Renamed `storage.equipment[]` to `storage.components[]`.
- Renamed `stores.*.equipment[]` to `stores.*.components[]` (`system`, `faction`, `blackMarket`).
- Renamed `mechs[].loadout.locations[].equipment[]` to `mechs[].loadout.locations[].components[]`;
  the same in `mechs[].afterRefit`.
- Added `storage.chassis[].maxArmor[]`: per location, head to legs, `location`, `front` and `rear`
  (`null` outside the torso), the most armor it can take.
- Added `stores.*.mechs[]`: the whole mechs a store sells, each `mech` (reference),
  `weightClass`, `role`, `tonnage`, `maxArmor[]` (as `storage.chassis[].maxArmor[]`),
  `hardpoints`, `maxJumpJets`, `count` (`null` when unlimited) and `price` (C-Bills for one mech,
  after discount and reputation). Buying one puts the mech with its stock loadout into the mech
  bay.
- Values corrected: `contracts[].biome` and `activeContract.biome` are `null` for every biome the
  contract screens hide (generic and below); was `null` for generic only.
- Values corrected: a reference whose game definition is missing (modded ammo, component or
  contract type) now has its id as `name` and the export goes on; the whole export used to fail.

## 0.22.0

Changed: rules.json.

### rules.json
- Added `exportedAt`: when the file was last written, ISO 8601 with offset. The file is now
  rewritten like game-state.json: only when its content apart from `exportedAt` changed, when
  another trigger exported it, and once after every game start.
- Added `trigger`: what caused the export that last wrote the file, the same values as in
  game-state.json. The file now starts with `modVersion`, `exportedAt` and `trigger`, like
  game-state.json.

## 0.21.1

No export file changed.

## 0.21.0

Changed: game-state.json.

### game-state.json
- Added `trigger` value `mechBayChanged`: the mech bay changed without a day passing (a refit or
  repair queued, a mech readied or stored, an order cancelled, the work queue reordered, or a
  mech, stored chassis or mech parts scrapped).

## 0.20.0

Changed: game-state.json.

### game-state.json
- Added `trigger` values, next to `careerLoaded`: `manualSave`, `autosave` (any save outside
  combat, e.g. after arriving in a system, after a contract, after the financial report is closed,
  after an event), `contractCompleted` (a contract's results applied, for contracts without an
  autosave), `financialReportShown` (the monthly report is shown; the autosave after closing it
  carries the chosen spending level), `workOrderCompleted` (a mech lab order, a heal or an Argo
  upgrade finished, once per day), `contractsGenerated`, `storeClosed`.
- Meaning changed: `trigger` `careerLoaded` now occurs only when a career is started or a save is
  loaded; was also written on returning from a mission, with the state from before the contract's
  results.

## 0.19.0

Changed: game-state.json.

### game-state.json (breaking)
- Moved `mechs[].tonnage`, `mechs[].value`, `mechs[].stats` and `mechs[].locations` to
  `mechs[].loadout.tonnage`, `mechs[].loadout.value`, `mechs[].loadout.stats` and
  `mechs[].loadout.locations`: the current loadout, as the mech bay shows it.
- Added `mechs[].afterRefit`: object of the same shape as `mechs[].loadout` (`tonnage`, `value`,
  `stats`, `locations`), the loadout once every step of `mechs[].refit` is finished, as the mech
  lab shows it; `null` without a work order and while readying.

## 0.18.0

Changed: game-state.json.

### game-state.json (breaking)
- Added `activeContract`: the accepted travel contract, from acceptance until proceeding with it
  on arrival; `null` without one. Same properties as a `contracts[]` entry, with `terms` (one
  negotiation option: `payShare`, `salvageShare`, `pay`, `salvage`, `reputation`, the terms it was
  accepted with) instead of `negotiation`. Its `travel.days` equals `position.travel.daysLeft`
  while travelling there; `travel` is `null` once arrived.
- Meaning changed: `contracts[]` no longer contains the active contract; was listing accepted
  priority and story contracts, with their negotiation options, for the whole journey. Read the
  accepted contract from `activeContract`.

## 0.17.0

Changed: game-state.json.

### game-state.json
- Added `position.tags[]`: `{ id, name }` per player-visible planet tag of the current system, as
  the starmap's system panel shows them.
- Added `position.biomes[]`: `{ id, name }` per biome the system's missions can be on, named like
  `contracts[].biome`.

## 0.16.0

Changed: game-state.json.

### game-state.json
- Added `mechs[].workQueuePosition`: position of the mech's work order in the mech lab queue,
  counted from 1; the mech techs work only on the first. `null` without a work order.

## 0.15.0

Changed: game-state.json.

### game-state.json (breaking)
- Meaning changed: `company.finances.expectedExpenses.mechs[].mech.id` is now the mech's own id,
  matching `mechs[].id`; was the mech's definition id, shared by mechs of the same variant.
  Re-derive any link from an expense line to its mech by `mechs[].id`.

## 0.14.0

Changed: game-state.json, rules.json.

### game-state.json (breaking)
- Moved `rules` to the root of rules.json: `rules.moraleLevels`, `rules.reputationLevels`,
  `rules.skills`, `rules.spiritsLevels`, `rules.mechPartsPerMech` and `rules.contractTypes` are
  now `moraleLevels`, `reputationLevels`, `skills`, `spiritsLevels`, `mechPartsPerMech` and
  `contractTypes` there. Values in game-state.json still refer to them by name.

### rules.json
- New file. Holds `modVersion` and, at its root, the rule tables that were `rules` in
  game-state.json: `moraleLevels`, `reputationLevels`, `skills`, `spiritsLevels`,
  `mechPartsPerMech`, `contractTypes`. No `exportedAt`; rewritten only when the rules change
  (e.g. after loading another career).

## 0.13.0

Changed: game-state.json.

### game-state.json
- Added `lastLance[]`: the mechs and pilots last sent on a mission, in slot order, which the lance
  configuration pre-fills from. Each entry has `mech` (`{ id, name }`, `id` matching
  `mechs[].id`, name with the variant) and `pilot` (`{ id, name, callsign }`). A mech or pilot
  that has left the company since is `null`; an entry where both are gone is left out.

## 0.12.0

Changed: game-state.json.

### game-state.json
- Added `contracts[]`: the contracts the Command Center offers; `null` until the game has
  generated the current system's contracts (when the contract screen first opens in a system, or
  after a contract). Each with `id` (the contract's data file, `null` where the game keeps no
  reference), `name`, `type` (`{ id, name }`, described in `rules.contractTypes`), `displayStyle`
  (regular, story, restoration or flashpoint), `employer`, `target`, `difficulty` (on the scale
  of `rules.reputationLevels[].maxContractDifficulty`, two per skull), `meetsReputation` (the
  game's verdict; story, restoration and flashpoint contracts are always allowed),
  `negotiation` (`canNegotiate`, `options[]` with `payShare`, `salvageShare` (percent, `null`
  for fixed terms), `pay`, `salvage` `{ total, priority }`, `reputation` `{ employer, target }`,
  each `null` when the faction gains no reputation), `lanceLimits` (`maxMechs`, `minTonnage`,
  `maxTonnage`, `mechs[]` with `minTonnage` and `maxTonnage` per slot; every limit `null` when
  there is none), `biome` (`{ id, name }`, `null` where the game has none) and `travel` (`null`
  for a contract in the current system; else `system`, `tags[]` planet tags, `days` travel days,
  `null` if the game finds no route).
- Added `rules.contractTypes[]`: `{ type: { id, name }, description }` per mission type.

## 0.11.0

Changed: game-state.json.

### game-state.json
- Added `stores`: object with `system`, `faction` and `blackMarket`, each `null` if the system has
  none or the company can't use it. A store has `faction` (`{ id, name }`, whose reputation sets
  its prices), `equipment[]` (`component` `{ id, name, type }`, `count`, `price`) and
  `mechParts[]` (`mech` `{ id, name }`, `count`, `price`); `count` is `null` when unlimited,
  `price` the C-Bills for one copy as the store shows it. The stock shows the system of the last
  export.
- Added `hiringHall[]`: the pilots for hire, each with `id`, `name`, `callsign`, `type`, `age`,
  `gender`, `expertise`, `skills`, `abilities[]`, `health` as in `pilots[]`, plus `hireCost`
  (one-time C-Bills), `salary` (per financial report) and `canHire` (whether the Mercenary Review
  Board rating and morale allow hiring).
- Added `company.maxPilots`: how many pilots the barracks hold, not counting the commander.

## 0.10.0

Changed: game-state.json.

### game-state.json
- Added `storage`: object with `equipment[]` (`component` `{ id, name, type }`, `count` working
  copies, `damagedCount`; definitions in `componentDefinitions`), `mechs[]` (stored chassis:
  `chassis` `{ id, name }`, `weightClass`, `role`, `tonnage`, `hardpoints` `{ ballistic, energy,
  missile, support }` of all locations together, `maxJumpJets`, `count`; readying one gives the
  stock armor and fixed equipment, no weapons) and `mechParts[]` (`mech` `{ id, name }`,
  `count`).
- Added `rules.mechPartsPerMech`: the parts that assemble into a mech.

## 0.9.0

Changed: game-state.json.

### game-state.json (breaking)
- Added `componentDefinitions`: root object with `weapons`, `ammunitionBoxes`, `heatSinks`,
  `jumpJets` and `upgrades`, each keyed by component id. Every entry has `name`, `tonnage`,
  `slots`, `cost`, `bonuses[]`; weapons add `category`, `ammoCategory` (`null` when none),
  `damage`, `instability`, `shotsWhenFired`, `projectilesPerShot`, `heatDamage`, `heatGenerated`,
  `ranges` `{ min, short, medium, long, max }` in meters, `accuracyModifier`,
  `criticalChanceMultiplier`, `refireModifier`, `isIndirectFireCapable`; ammunition boxes add
  `ammoCategory`, `capacity`; heat sinks add `dissipation`. A component the game can't find has
  no entry.
- Removed `mechs[].locations[].equipment[].type`, `.tonnage`, `.slots`, `.cost`, `.bonuses`,
  `.description`, `.weapon`, `.ammoBox` and `.heatSink`. An entry is now
  `{ component, damageLevel, isFixed }`; look its stats up in `componentDefinitions`. The
  description text is no longer exported.
- Type changed: `mechs[].locations[].equipment[].component` and `mechs[].refit[].component` are
  now `{ id, name, type }`; were `{ id, name }`. `type` names the `componentDefinitions` group
  that holds the definition (`weapons`, `ammunitionBoxes`, `heatSinks`, `jumpJets`, `upgrades`).

## 0.8.1

No export file changed.

## 0.8.0

Changed: game-state.json.

### game-state.json (breaking)
- Added `rules.spiritsLevels[]`: `{ level, precisionStrikeCost, vigilanceCost }` for `normal`,
  `high` and `low` spirits: the resolve a Precision Strike and Vigilance cost.
- Type changed: `pilots[].spirits` is never `null`; was `null` in normal spirits. A pilot in normal
  spirits has `{ "level": "normal", "daysRemaining": null }`, so `level` always matches a row of
  `rules.spiritsLevels`.

## 0.7.1

Changed: game-state.json.

### game-state.json
- Values corrected: `mechs[].refit[].component.name` for install and remove steps is the
  component's display name (e.g. "SRM6 +"); was its id (e.g. `Weapon_SRM_SRM6_0-STOCK`).

## 0.7.0

Changed: game-state.json.

### game-state.json
- Added `mechs[]`: the active and readying mechs of the mech bay, each with `id` (the mech's own
  id), `name`, `chassis` `{ id, name }`, `weightClass`, `role`, `bay` and `position` (counted
  from 1), `status` (`ready`, `readying`, `inMaintenance`), `daysUntilReady` (0 when ready),
  `refit` (`null` without a work order and while readying), `isFieldable`, `problems[]` (the mech
  lab's loadout warnings in the game's words), `tonnage` `{ used, max }`, `value` (C-Bills),
  `stats` and `locations[]`.
- Added `mechs[].locations[]`: per body location, from head to legs: `location`, `armor`
  `{ current, assigned, max }`, `rearArmor` (same shape, `null` outside the torso), `structure`
  `{ current, max }`, `hardpoints` `{ ballistic, energy, missile, support }`, `slots`
  `{ used, max }` and `equipment[]`: every mounted component with `component` `{ id, name }`,
  `type`, `damageLevel`, `isFixed`, `tonnage`, `slots`, `cost`, `bonuses[]`, `description` and
  the stats of its type: `weapon` (damage, heat, stability damage, ranges in meters, modifiers),
  `ammoBox` (`ammoCategory`, `capacity`) or `heatSink` (`dissipation`); the others `null`.
- Added `mechs[].refit[]`: the steps of the queued mech lab work order, each with `type`
  (`installComponent`, `removeComponent`, `repairComponent`, `modifyArmor`, `repairStructure`),
  `isDone`, and only the values its type uses (`component`, `damageLevel`, `location`,
  `frontArmor`, `rearArmor`, `structure`), the rest `null`.
- Added `mechs[].stats`: `ratings` (the six stat bars, up to 10), `movement` (`walk`, `sprint`,
  `jump` in meters), `heat` (`dissipation`, `alphaStrike`, `averageJump`, `shutdown`),
  `firepower` (`damage`, `stabilityDamage`), `range` (`max`, `optimal` in meters), `durability`
  (`armor`, `structure`, `deathFromAboveSelfDamage`) and `melee` (`damage`, `stabilityDamage`,
  `deathFromAboveDamage`).

## 0.6.0

Changed: game-state.json.

### game-state.json
- Added `pilots[]`: the commander and the roster as the barracks lists them, each with `id`,
  `name` (full name without callsign), `callsign`, `type` (`commander`, `vanguard`, `ronin`,
  `regular`), `age`, `gender`, `expertise` (barracks title), `skills` `{ gunnery, piloting,
  guts, tactics }`, `experience` `{ unspent, spent }`, `abilities[]` (primary abilities only,
  `{ id, name }` referring to `rules.skills`), `health`, `injuries`, `status` (`ready`, `injured`,
  `unavailable`), `daysUntilReady` (0 when ready), `spirits` (`{ level, daysRemaining }`, `null`
  unless an event left the pilot in `high` or `low` spirits) and `serviceRecord` `{ missions,
  mechKills, otherKills, ejections, lifetimeInjuries, daysOnCrew }`.
- Added `rules.skills`: `maxPrimaryAbilities`, `maxPrimaryAbilitiesPerSkill` and, per skill
  (`gunnery`, `piloting`, `guts`, `tactics`), `description` and `levels[]` for levels 2 to 10 with
  `level`, `experienceCost` (from the level below) and `abilities[]` (`ability` `{ id, name }`,
  `isPrimary`, `description`) unlocked at the level.

## 0.5.0

Changed: game-state.json.

### game-state.json (breaking)
- Moved `moraleLevels` to `rules.moraleLevels`.
- Added `rules`: the game's level tables for this career.
- Added `rules.reputationLevels[]`: from `LOATHED` to `HONORED`, each with `level` (the value
  `company.reputation[].level` uses), `startsAt` (lowest reputation value in the level),
  `maxContractDifficulty` (highest difficulty of the faction's contracts offered at this level,
  including the career's current global difficulty), `canUseSystemStore` (`false` at `LOATHED`)
  and `storePriceChangePercent` (as the UI shows it, e.g. `10` or `-5`).

## 0.4.0

Changed: game-state.json.

### game-state.json (breaking)
- Moved `company.funds` to `company.finances.funds`.
- Moved `company.monthlyExpenses` to `company.finances.expectedExpenses.total`.
- Added `moraleLevels[]`: `{ name, startsAt, resolvePerTurn }` per morale level; `startsAt` the
  value the level starts at, `resolvePerTurn` its resolve per combat turn.
- Added `company.morale`: `{ value, level }`, `level` a name in `moraleLevels`.
- Added `company.finances`: object with `funds`, `daysUntilReport`, `spending` (`level`, the one
  confirmed at the last report, and `options[]` with `level`, `expectedExpenses` and
  `moraleChange` for all five levels) and `expectedExpenses` (`total`, `ship` `{ name, amount }`,
  `shipUpgrades[]` `{ upgrade, amount }`, `mechs[]` `{ mech, amount }`, `pilots[]`
  `{ pilot, amount }`; lines rounded one by one as on the finance screen, so they can differ
  from `total` by a few C-Bills). A mech reference is `{ id, name }` with the variant in the name,
  e.g. "Phoenix Hawk (PXH-1)"; a pilot reference is `{ id, name, callsign }`.
- Added `company.mechTech` and `company.medTech`.
- Added `position.travel`: `{ destination, owner, daysLeft }` while travelling, `daysLeft` for the
  whole trip; `null` while in a system.

## 0.3.0

Changed: game-state.json.

### game-state.json
- New file. Written on every career load (new campaign, loaded save, return to the career screens
  after a mission); only the story campaign is exported.
- Added `modVersion`, `exportedAt`, `trigger`: the export that wrote the file (`trigger` is
  `careerLoaded`).
- Added `company`: `name`, `dropship`, `funds`, `monthlyExpenses`, `daysPassed`, `currentDate`,
  `mercenaryReviewBoard` `{ value, level }` and `reputation[]` per faction (`faction`
  `{ id, name }`, `value`, `level`, `isAllied`, `isEnemy`, `isDisplayed`).
- Added `position`: `system` `{ id, name }`, `owner` `{ id, name }` and `travelStatus`.
