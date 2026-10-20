# System map changelog

## V9.3.1 M03 review — 2026-10-18

Documentation only; no code changed. Corrections after re-checking against the Phase 3 source, the I07 handoff and the reported Unity results:
- Evidence: the handoff reports "I06 regressions" without listing them. View size following counts is now credited only to that group, not to a named check (`ARENA-VIEW-01`, `90-Evidence.md`, `92-Validation.md` A1 and acceptance table). Phase 3 stays complete, as the I07 handoff reports.
- `ARENA-CAM-01`: the I07 cycle audit runs after removal, so it covers the restored camera state only.
- `92-Validation.md` A10/A11: no longer state gate edges the handoff does not specify.
- `SETUP-07`, `ARENA-TERRAIN-01`: `WM_*` layer checks (matrix error, prefab refusal) apply only when `WorldMapLayers2D` is resolved.
- `ORDER-01`: the arena also reads `WorldMapController2D.Instance` and `WorldMapLayers2D`; the Awake ordering after the controller is declared order, not tested.
- `ARENA-BODY-02`: body counters persist across arenas until `ResetBodyCounters`.
- No rule ID added, renumbered or removed.

## V9.3 Phase 3 battle arena — M03, 2026-10-18

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 3 (implementation chats I06–I07). **Phase 3 is complete,** as the I07 handoff reports: the I07 checks passed in both perimeter modes, the I06 regressions passed, and no defect is known. View size following counts is covered only through the unlisted I06-regression group (corrected in V9.3.1).

**`07-WorldMap-Battle.md`**
- Header now describes the state after Phase 3.
- New "Battle arena (Phase 3)" section with 14 rules: `ARENA-LIFE-01`–`03`, `ARENA-CAM-01`, `ARENA-TERRAIN-01`, `ARENA-VIEW-01`, `ARENA-PERIM-01`, `ARENA-ENTRY-01`, `ARENA-BODY-01`, `ARENA-BODY-02`, `ARENA-GATE-01`, `ARENA-DEST-01`, `ARENA-TEST-01`, `ARENA-AUDIT-01`.
- Rewritten in place, same IDs: `WM-FREEZE-01` (arena owns the freeze; external unfreeze aborts it; the world-map debug Freeze toggle is still ungated), `WM-BATTLE-01` (arena panel can copy the pick).
- 9 new cards for 11 files: `BattleArena2D` (one card with a three-partial table), `BattleCamera2D`, `BattleRules2D` (with `BattlefieldRegion2D`, `BattlefieldSource`, `BattlefieldSelection`), `BattleLayers2D`, `BattleArenaTypes2D`, `BattleArenaGate2D`, `UnitDestinationOverride2D`, `BattleTestBody2D`, `BattleArenaDebugPanel2D`. The I07 handoff asked for "all eleven Battle files"; the partials share one card so each inventory entry keeps exactly one card.
- Pointer lines added to the `WorldMapController2D`, `WorldMapLayers2D` and `WorldMapDebugPanel2D` cards.

**`00-Core.md`**
- `CORE-02`: 79 files / 67 entries; Phase 3 scope established; remaining gaps listed.
- `CORE-03`: destination override is safe on parked units.
- `CORE-04`: tag summary; full definitions moved to `90-Evidence.md` (carried from M02).
- Three fragile-boundary rows; battle load-routing row; six ownership rows; eleven inventory rows.
- `ORDER-01`: `BattleArena2D` at −900, with its relation to the world map, the unit components and gate callbacks.
- `INIT-01` pointer; new `SETUP-07` (battle layers and matrix, cameras, arena placement, battlefield prefabs).
- Glossary: seven terms added (Arena, Battle view, Entry strip, Perimeter, Retreat gate, Arena body, Leak / re-entry); `UnitStatus` and Result lock rows clarified.

**`01-Movement-Sidestep.md`**: a note that the destination override and `IsFullyInside` exist but no movement rule changed.

**`90-Evidence.md`**: defines `[WM1]`, `[WM1-SR]`, `[WM2]`, `[WM2-S]`, `[WM3]`, `[WM3-S]` and records the Phase 3 basis.

**`92-Validation.md`**: Phase 3 known-answer checks.

**Departures from Roadmap §5 notes, kept from I06–I07:** both perimeter modes (OneWayEffector default); entry strips are pure geometry with no `SpawnBox2D`; the shrink trigger is Phase 5's; the override is wired into motor and Sidestep in Phase 4; body layer names are rules strings that Phase 4's rename must update.

**Untested and carried forward:** the I06 `frozenMap` fix and `ThrowOnFreezeTest` (the hook is not in the attached Battle files; no result reported); camera or rig disabled/destroyed mid-arena; scene unload with an arena; second instance and missing-rig paths; escape while Entering; closed-gate path; lost-body release; real units. Possible Phase 4 issue recorded in `ARENA-VIEW-01`: enemy search reach 8 versus view size 18. `WorldMapAssets2D` signatures are still inferred. Earlier carried items from M02 are unchanged.

**Source facts worth a later decision (not defects, not changed):** a retreating body escapes on gate contact even while Entering; `EscapesAccepted` has a public setter; a body deactivated without unregistering is released as lost and fails the audit; `TryRegisterBody` does not check the body's layer, circle offset or shared GameObject.

## V9.2.1 M02 review — 2026-10-17

Documentation only; no code changed. Corrections after re-checking against Phase 2 source and the reported Unity results:
- `WM-DEPLOY-01`: a nested `DeployArmy` during settlement work can run later in the same step, not only next step; a closed garrison drops its request silently.
- `WM-SPAWN-01`, `WM-DEPLOY-02`: `ArmySpawned` with Deploy/Overflow guarantees a filled army only via `TryDeployFromRoster`; `TrySpawnArmy` announces empty armies with any reason.
- `WM-CONTACT-01`: settlement intake checks a nonzero `SettlementId`, not registry membership.
- `WM-TIME-01`, `WM-RECRUIT-01`, `WM-UNIT-01`: frozen-by-handler stop of settlement work; dropped re-issued recruits; reserve root refusal and identity local transform. `SETUP-06` reserve-scale wording corrected.
- Evidence brackets narrowed where they claimed more than the named results (`WM-SCOPE-01`, `WM-FACTION-01`, `WM-XFER-01`, `WM-REMOVE-01`, `WM-ALLY-01`, `WM-CLASS-01`).
- `92-Validation.md`: W12 does not depend on counts; added P17.

## V9.2 Phase 2 world map — M02, 2026-10-17

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 2 (implementation chats I03–I05). **Phase 2 is complete:** the acceptance list passed in Unity, and no defect is known.

**`07-WorldMap-Battle.md`**
- 19 new rules: `WM-UNIT-01`/`02`, `WM-OWN-01`/`02`, `WM-ROSTER-01`/`02`, `WM-RECRUIT-01`, `WM-FACTION-01`, `WM-XFER-01`/`02`, `WM-DEPLOY-01`/`02`, `WM-PLACE-01`, `WM-IGNORE-01`, `WM-OVERFLOW-01`, `WM-ALLY-01`–`03`, `WM-AUDIT-01`.
- Rewritten in place, same IDs: `WM-SCOPE-01`, `WM-LIFE-01`, `WM-TIME-01`, `WM-FREEZE-01`, `WM-SPAWN-01`/`02`, `WM-REMOVE-01`, `WM-MOVE-01`–`03`, `WM-SETTLE-01`/`02`, `WM-CONTACT-01`/`04`/`05`, `WM-CLASS-01`/`02`.
- 5 new cards: `UnitReserve2D`, `UnitRoster`, `UnitHandle`, `UnitRosterTypes2D`, `UnitPrefabCatalog2D`.
- Updated cards: controller, Army2D, Settlement2D, contacts, types, rules and debug panel.
- The three new controller partials (`.Units`, `.Deployment`, `.Interactions`) and `AlliedInteractionRecord` are documented inside the single controller card, with a partials table, so each inventory entry keeps exactly one card. The I05 handoff had listed them as separate cards.

**Integration rules in other files**
- `TARGET-01` and the `UnitCombatant2D` card (01): faction setter, parked units only.
- `UnitRuntimeStats2D` card (04): read-only `Definition`.
- `UnitDefinition2D` card (Core): `SettlementOnly`.
- `03-Health-Death-XP.md` is unchanged; Phase 2 touches no health rule.

**Removed APIs, documented as removed**
- `Army2D.DisplayCount`/`SetDisplayCount`, `Settlement2D.DisplayCount`/`SetDebugDisplayCount`.
- The count parameter of `TrySpawnArmy`/`TrySpawnArmyAtRandom`/`Army2D.Initialize`.
- Changed: `DestroyArmy` gains `discardUnits`, and `Settlement2D.SetFaction` now returns bool.

**`00-Core.md`**
- `CORE-02`: 68 files / 58 entries; Phase 2 scope established; remaining gaps listed.
- `CORE-03`: parking pointer.
- `CORE-04`: defines `[WM2]` and `[WM2-S]`.
- Fragile-boundary, load-routing, ownership and inventory rows; `ORDER-01` step order and plain-object note; `INIT-01` pointer; `SETUP-06` placement mask, reserve scale and recruitable prefab.
- Glossary: seven terms added; Contact intent row updated; Reserve/Deployed/… row rewritten for `UnitStatus`.

**Evidence**
- `WM-CONTACT-05` (freeze during dispatch) is upgraded from `[WM1-SR]` to Unity-verified through the I05 check.
- The final `.Contacts.cs` change (log text and comments only) was not re-run in Unity. It changes no rule.

**Untested and carried forward:** overlapping placement rings; an army bouncing back through its ignored source settlement; `ArmySpawned`/`AlliedInteractionApplied` handlers that freeze or redeploy; the rollback path; thousands of units; HighestLevelFirst with live levels; an owner destroyed outside the API; scene-unload ordering.

**Outdated source comment noted, not changed:** `UnitReserve2D.Transfer` ("In-battle blocking is I05's").

`90-Evidence.md` is unchanged; WM tags remain defined in Core.

## V9.1 Phase 1 world map — M01, 2026-10-15

Documentation only; no source code changed by this update. Covers World Map & Battle Roadmap V6 Phase 1 (implementation chats I01/I02; implementation dates not supplied).

- Added `07-WorldMap-Battle.md`: 22 new rule IDs (`WM-SCOPE`, `WM-LIFE`, `WM-TIME`, `WM-FREEZE`, `WM-SPAWN`, `WM-REMOVE`, `WM-MOVE`, `WM-SETTLE`, `WM-CONTACT`, `WM-CLASS`, `WM-BATTLE` families; `SETUP-06` lives in Core) and 11 contract cards for 12 new source files.
- New architecture: single `WorldMapController2D` (order −1000) owning registry, bounds, MapTime, PhysicsStep, freezing, spawn placement, removal and the contact pipeline; `Army2D` constant-speed marbles owning their own bounce (one reflection per step with jitter and axis nudging); static `Settlement2D` with CircleCollider2D and runtime ids; pure contact classifier; WM_* layers isolated from all other layers.
- Contact pipeline: callback intake → per-pair deduplication (one report per pair per physics step) → touch episodes (`IsNewContact`) → classification of 10 kinds into 8 intents → role ordering → random battle-candidate selection → `ContactReported` dispatch that stops if a handler freezes the map. Intent only; no gameplay effects.
- Post-I02 fix to `WorldMapController2D.Contacts.cs` (freeze during dispatch) documented as `[WM1-SR]`: source-reviewed, not Unity-verified.
- `00-Core.md`: rewrote `CORE-02` (world-map armies exist, no persistent rosters; inventory now 60 files / 53 entries) and `CORE-04` (defines `[WM1]`, `[WM1-SR]`); added world-map rows to fragile-boundary, load-routing, ownership and inventory tables; added the controller to `ORDER-01` with its separation from combat ordering; pointer added to `INIT-01`; new `SETUP-06` for WM layers/matrix/controller placement; five glossary terms added; Reserve/Result lock/Settlement glossary rows clarified against world-map terms.
- `92-Validation.md`: added Phase 1 known-answer checks separating Unity-verified results from source-only conclusions.
- No existing V9 rule ID was renumbered or removed. `01`–`06` and `90-Evidence.md` were not changed; the evidence appendix does not yet define the WM tags (Core does).

## V9 modular restructure — 2026-10-12

- Replaced the monolithic current map with `00-Core.md` and six task-loaded subsystem documents.
- Moved behavior into stable-ID pipeline rules; standardized 42 logical script cards as Owns / API / Events / Invariants / Never owns.
- Retained all 48 source filenames, including seven partials for one Sidestep component.
- Consolidated ownership, execution order, scene/layer requirements and glossary in Core. Removed duplicate dependency/bridge indexes and both large graphs.
- Moved inherited verification scope/gaps to Evidence; removed source-local formulas/defaults/overload signatures from routine context where source is the appropriate authority.
- Marked battle-result lock and deployment/escape vocabulary undocumented, rather than inventing rules. Distinguished pre-Awake grants from initialized casting eligibility.
- Added 15 old/new retrieval checks and structural validation. No gameplay/source code changed.

## Inherited update lineage (dates not supplied)

- Baseline/V6: combat ownership, movement/progression/Dodge and earlier Sidestep/UI inspection.
- Modifier integration findings: effective acceleration/base damage, protected pair reads, captured one-Clash consumption, factual damage events, XP deferral and reactive timed-handle exception.
- V7: ten ability files and combatant integration; independent projectile and shared delivery contracts.
- V8: modifier-triggered ability capture/request/dispatch, paired reaction boundary and diagnostics.
- Later Sidestep retry: continuous in-band opportunities replaced band-crossing history; all seven partials inspected, two changed.
- Later Knockback direction: independent random angle samples added to rules/contracts/calculator/director.

Detailed evidence attribution is in `90-Evidence.md`; lineage does not imply successful compilation or Play Mode verification.
