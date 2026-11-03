# Stick Legions system map — 00 Core

Map version V9.9.2 (2026-11-03). History: `91-ChangeLog.md`. Human-readable summary: `ARCHITECTURE.md`.

Always attach this file. Attach only the subsystem files relevant to the task. This map describes autonomous top-down 2D combat: targeting and movement, Clash transactions, damage/death/XP, independent modifiers, and normal/triggered abilities; plus the world map of bouncing armies, settlements, persistent units with rosters and garrisons, deployment, overflow, allied interactions and contact classification; plus the battle arena (camera swap, terrain, perimeter, entry strips, retreat gates); plus battle deployment: a field battle started from a world-map contact, side layers, and the armies' real units spawned onto the arena to fight with the existing combat (Phase 4); plus battle endings: end detection, a final result lock, cleanup, routing, separation with army-pair immunity, abort at any stage and repeatable battles (Phase 5); plus regeneration: a shared healing tick for every activated living unit, parked or deployed (Phase 6); plus retreat: retreat-eligible units that fall below 30% HP become Escaping, receive the configured retreat modifier and walk to a gate to escape, and retreating armies recover after 20 map seconds (Phase 7); plus battle-start modifiers: configured lists frozen at each battle's commit and applied once to each deployed unit after it has started (Phase 8); plus conquest and sieges: capture of empty enemy settlements, sieges of defended ones with the roadmap's outcome table, and pending retreat rosters that place escaped defenders as retreating armies (Phase 9). Every Roadmap V6 phase is complete. It is an integration contract, not a replacement for source code.

## Rules most likely to be broken — read first

These are navigation pointers; the cited IDs hold the complete rules in their owning files.

| Fragile boundary | Read before changing it |
| --- | --- |
| Both participants prepare before either snapshot; both controllers settle before both read barriers | Clash: `CLASH-07`, `CLASH-08`, `CLASH-09`; Modifiers: `STAT-04` |
| Both results → captured consumption → both XP awards → both reactions → triggered casts | Clash: `CLASH-10`, `CLASH-11`, `CLASH-12`, `CLASH-13`, `CLASH-14` |
| Ordinary stat reads can notify; protected reads cannot mutate | Modifiers: `STAT-01`, `STAT-04` |
| Nested damage finalizers must respect victim-side XP deferral | Health: `XP-02`, `XP-03` |
| One-Clash consumption preserves pending triggers; explicit removal cancels them | Abilities: `TRIGGER-06` |
| Pre-Awake/inactive grants are supported; initialized progression is still a casting prerequisite | Abilities: `ABILITY-01`, `ABILITY-02`; Core: `CORE-03` |
| A locked result is final: the end check and lock never run inside a transaction; three combat gates (damage, Clash acceptance, casting) close for locked units, motors are Disabled, escapes close and battle projectiles are removed | Battle: `BATTLE-END-01`, `BATTLE-LOCK-01`; Health: `HEALTH-01`; Clash: `CLASH-05`; Abilities: `ABILITY-02`, `TRIGGER-04`, `PROJECTILE-01` |
| A target staying in band can retry Sidestep after cooldown | Movement: `SIDESTEP-GATE-01`, `SIDESTEP-GATE-02` |
| Payload selection freezes at cast; modifier values freeze at application | Abilities: `ABILITY-03`, `DELIVERY-03` |
| Bounce is Army2D-owned and independent of classification; classification reads live state one step later | World map: `WM-MOVE-02`, `WM-CONTACT-04` |
| Contact handlers may freeze mid-dispatch; removed armies must be ignored | World map: `WM-CONTACT-05`, `WM-REMOVE-01` |
| Only `UnitReserve2D` changes unit ownership; only the battle spawn path activates a parked unit; parked units never run Awake; reads use serialized data or `didAwake` | World map: `WM-OWN-01`, `WM-UNIT-01`, `WM-UNIT-02`; Battle: `BATTLE-SPAWN-02`; Core: `CORE-03` |
| Populated armies are announced only after being filled; owners holding units are never destroyed silently | World map: `WM-DEPLOY-02`, `WM-OWN-02`, `WM-REMOVE-01` |
| Allied effects run after dispatch, reclassify from live state, and stop if anything freezes the map | World map: `WM-ALLY-01`, `WM-TIME-01` |
| The arena owns the map freeze; an unfreeze from elsewhere aborts it; teardown order is fixed | Battle: `ARENA-LIFE-02`, `ARENA-LIFE-03`; World map: `WM-FREEZE-01` |
| The arena never owns body GameObjects: never parent real units under its Root, and unregister a body before deactivating or destroying it | Battle: `ARENA-BODY-01` |
| A centre outside the view is a leak unless announced (`ForceReenter`) or caused by an aspect change; no Sidestep under any destination override, and the Entry override ends only on reaching Inside | Battle: `ARENA-BODY-02`; Movement: `SIDESTEP-GATE-02`, `SIDESTEP-LIFE-02` |
| Bind → motor reset → mark Deployed → reparent → activate → register, all in one call; unregister a unit's body before deactivating or destroying it | Battle: `BATTLE-SPAWN-02`, `SIDE-02`, `ARENA-BODY-01`; Movement: `MOVE-04` |
| A death releases ownership immediately, inside the damage path; an escape stays owned as Escaped; an abort re-parks every living unit as Reserve | Battle: `BATTLE-UNIT-01`, `BATTLE-UNIT-02`, `BATTLE-ABORT-01`; World map: `WM-OWN-01` |
| A battle starts only from the step's FieldBattle or Siege pick, inside contact dispatch; every check that changes nothing runs before the arena exists, and any later failure, a throw in the commit included, removes the arena | Battle: `BATTLE-START-01`–`BATTLE-START-03`, `BATTLE-SIEGE-01`; World map: `WM-CONTACT-05` |
| Side is physics, never faction; layers and masks are bound at every deployment while parked, and prefab masks are not authoritative | Battle: `SIDE-01`, `SIDE-02`; Movement: `TARGET-01`, `TARGET-02` |
| Normal end: projectiles → park field and escaped units → corpses → routing while still frozen → lock release → arena removal → `Current` cleared; `BattleEnded` only after a complete teardown; the lock is released after parking on every path | Battle: `BATTLE-END-04`, `BATTLE-UNIT-04`, `BATTLE-RECOVER-01`, `BATTLE-ABORT-01` |
| Field routing and every return never move units between rosters; the siege capture is the only routing path that does, and it runs every check before its first change; armies left empty go through `DestroyArmy`, never discard; pair and army–settlement immunity count map time and block only battle candidacy | Battle: `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `BATTLE-SIEGE-02`; World map: `WM-CAPTURE-01`, `WM-IMMUNE-01`, `WM-REMOVE-01` |
| Regeneration is the only healing path. The tick filters by `HasEverActivated`, never `IsDead`. It heals inactive units without waking them or clamping them to death. It reads neither map time nor the result lock | Health: `HEALTH-04`, `HEALTH-05`; World map: `WM-REGEN-01`, `WM-REGEN-02`, `WM-UNIT-02`; Core: `CORE-03` |
| Unit retreat is latched: `HealthChanged` (possibly inside a Clash) only records the trigger; the start runs in the manager's FixedUpdate; Escaping keeps the unit active, on the field and owned; nothing clears a retreat | Battle: `BATTLE-RETREAT-02`–`BATTLE-RETREAT-04`; World map: `WM-UNIT-01`, `WM-OWN-01`; Movement: `SIDESTEP-GATE-02` |
| Battle modifier deliveries wait for the unit's Start, apply once in the manager's FixedUpdate and are dropped when the unit leaves, at the lock, at the end and at an abort; modifier application itself is not lock-gated. Battle-start lists are frozen at commit (a context accepts one freeze) and requested in the spawn call before any retreat request | Battle: `BATTLE-DELIVER-01`–`BATTLE-DELIVER-03`, `BATTLE-START-03`, `BATTLE-LOCK-01`; Modifiers: `MOD-APPLY-01`, `MOD-REMOVE-01` |
| An army's recovery timer lives on `Army2D` and starts only when the flag changes to true; merges and transfers never call `SetRetreating`; recovery runs after allied interactions and only while unfrozen | World map: `WM-RETREAT-01`, `WM-ALLY-02`, `WM-TIME-01`; Battle: `BATTLE-ROUTE-01` |
| A besieged garrison is a battle side until `Current` clears: its faction change, deployment, overflow, capture and transfers are refused or Blocked; only the siege capture's internal `SetFactionAfterSiege` bypasses the guard, during the siege's normal end | World map: `WM-FACTION-01`, `WM-DEPLOY-02`, `WM-XFER-02`, `WM-CAPTURE-01`; Battle: `BATTLE-LIFE-01`, `BATTLE-SIEGE-01` |
| Siege capture order: every check first → settlement-only defenders still in the garrison die → mobile defenders to a pending retreat roster → faction change → attackers into the garrison → `DestroyArmy` → `SettlementCaptured` queued for the controller's next step; a pending retreat roster is registered the moment it holds units and keeps them owned until placed | World map: `WM-CAPTURE-01`, `WM-PENDING-01`, `WM-OWN-01`; Battle: `BATTLE-SIEGE-02` |

## Reading and maintenance contract

- **CORE-01 — One home per rule.** Bold ID definitions are authoritative within this map. Tables/cards elsewhere are indexes and pointers, not additional rule definitions. Keep an ID stable when moving or refining its rule; never reuse it for unrelated behavior. New behavior receives a new ID. When editing code, cite the IDs relied on and identify any whose behavior changed. Source is needed for exact signatures, numeric tuning, validation implementation and lifecycle callbacks touched by the task.
- **CORE-02 — Documented scope.** The inventory is 100 source files / 75 logical entries: seven Sidestep partials form one entry/component, ten WorldMapController2D partials form one entry/component, three BattleArena2D partials form one entry/component, and nine BattleManager2D partials form one entry/component.
  - **Established by Roadmap V6 Phases 1–2** (`07-WorldMap-Battle.md`):
    - world-map armies and settlements;
    - persistent units with one owner each, parked in a reserve outside battle;
    - army rosters and settlement garrisons, with FIFO/HighestLevelFirst order;
    - recruitment, gated transfers, manual deployment into the world map, overflow and placement;
    - the three allied contact interactions (retreat transfer, retreat merge, garrison join).
  - **Established by Roadmap V6 Phase 3** (`07-WorldMap-Battle.md`):
    - arena creation and teardown far from the map, owning the map freeze, with a world/battle camera swap;
    - battlefield selection by map region, with fallback and generated ground;
    - a shrink-only battle view sized from unit counts;
    - a perimeter in two modes, entry strips and entry-point search;
    - entry tracking of registered bodies (Entering/Inside, leaks, re-entries);
    - retreat gates with escape of registered retreating bodies;
    - the `UnitDestinationOverride2D` value component;
    - debug test bodies and an arena audit.
  - **Established by Roadmap V6 Phase 4** (`07`, with integration rules in `01`, `02`, `04`, `05`, `06`):
    - a field battle started from the step's FieldBattle pick (one at a time; the Phase 4 one-per-session limit was lifted in Phase 5);
    - side mapping and side layers (`Unit_SideA/B`, `Clash_SideA/B`), with targeting, detector and ability masks bound at every deployment;
    - timed spawning of real units in roster order onto the arena, with a motor deployment reset;
    - destination-override steering in the motor, Sidestep paused under any override, and `Perimeter` treated as `Wall` by Sidestep and the reactive turn boost;
    - death (ownership released), escape (inactive in the reserve, still owned) and abort (every living unit re-parked);
    - live `IsRosterInBattle`; stats UI refresh on re-enable.
  - **Established by Roadmap V6 Phase 5** (`07`, with integration rules in `02`, `03`, `04`, `05`):
    - end detection with death batches, elimination and an optional timeout;
    - an immutable result and the result lock: `CombatLock2D` gates damage, Clash acceptance and casting for the battle's units, motors are Disabled, escapes and spawning stop, and battle projectiles are swept;
    - the end delay and the normal end: every living unit parked as Reserve, corpses destroyed, the arena removed, `BattleEnded`;
    - field routing (retreat flags) and removal of armies left empty;
    - separation, with return spots found before the start, and army-pair battle immunity on the world map;
    - abort at every stage, with the same cleanup, separation and immunity but no result or retreat flags;
    - repeatable battles, one at a time; recovery of an incomplete end, an orphan arena and quarantined units; a post-battle audit;
    - an explicit clear for the reactive turn boost's handles.
  - **Established by Roadmap V6 Phase 6** (`07`, with the heal rule in `03`; card notes in `04` and `06`). The "dead units are skipped" item, open at M06, passed in the I14 Regen-tracking check reported with I15:
    - one shared regeneration tick, owned and stepped by the world-map controller every `RegenerationIntervalSeconds` (2) of scaled fixed time, frozen or locked;
    - every activated living unit in the reserve registry (parked, deployed, Escaping, Escaped or quarantined) heals its definition's `healthRegenFraction` (0.02) of its effective maximum through `UnitHealth2D.ApplyRegeneration`; never-activated units are skipped;
    - regeneration is the only gameplay healing path and is not gated by the result lock.
  - **Established by Roadmap V6 Phase 7** (`07`, with integration notes in `01`, `03`, `04`, `05`). Every I15 and I16 check passed, and the two items M07 left open ("retreating units keep fighting", "escaped units never respawn that battle") passed in the Phase 7 closing check reported after M07 (`92-Validation.md`, Phase 7):
    - retreat eligibility rolled once per participant at each battle's commit (`escapeChance` 0.4; settlement-only units never);
    - a latched low-health trigger (below `escapeHealthFraction` 0.3, including at deployment) that healing never cancels;
    - the Escaping status, gate steering, no Sidestep under the Retreat override, and the `UnitRetreated` event;
    - one shared battle modifier delivery path (the retreat modifier; since Phase 8 also the battle-start lists): wait for Start, self-cast at the current level, once, dropped when the unit leaves or the battle locks, ends or aborts;
    - army retreat recovery after `RetreatRecoverySeconds` (20) of map time, with merge receivers and leftover donors keeping their own timers.
  - **Established by Roadmap V6 Phase 8** (`07`, with card notes in `04` and `05`). Every acceptance item passed in Unity in I17 (`92-Validation.md`, Phase 8):
    - battle-start modifier lists in `BattleRules2D`: a field list, plus town and castle defender lists (used by sieges since Phase 9);
    - the lists frozen at each battle's commit (null entries removed; a field battle gives both sides the field list), so rules edits during a battle change nothing;
    - one BattleStart request per unit per battle in the spawn call, before any retreat request, delivered through the shared path: after the unit has started, self-cast at its current level, entries in list order, removed when the unit leaves; never into another battle;
    - a debug delivery hold, a battle-start audit and per-unit delivery diagnostics.
  - **Established by Roadmap V6 Phase 9** (`07`; I18 and I19 changed no combat source, so no rule in `01`–`06` changed; `01`–`06` were not attached to M09). The I18 and I19 handoffs report that every one of their checks passed in Unity, and the I19 handoff that they, with earlier phases' checks, meet every Phase 9 acceptance item (`92-Validation.md`, Phase 9):
    - capture: an ordinary army touching an empty enemy settlement takes it over; the settlement changes faction, the army becomes its garrison and is removed, a pending deployment is cancelled and overflow is checked for the new owner (`WM-CAPTURE-01`);
    - sieges: an ordinary army touching a defended enemy settlement fights its whole garrison (settlement-only units included) on the settlement's authored terrain and edges; the defender is always Side A, spawns at twice the normal rate by default and gets the town or castle defender list; retreat gates only on the attacker's edge; an invalid setup refuses with nothing changed (`BATTLE-SIEGE-01`);
    - the siege outcome table, applied at the normal end while the map is still frozen: an attacker win captures the settlement (settlement-only defenders still in the garrison die, mobile ones go to a pending retreat roster); any other outcome makes a surviving attacker retreat; every surviving non-capturing attacker is placed away with army–settlement immunity (`BATTLE-SIEGE-02`, `WM-IMMUNE-01`);
    - pending retreat rosters, the third owner kind, placing escaped defenders as retreating armies split by `MaxUnitsPerArmy` and retrying while blocked, audited and healed meanwhile (`WM-PENDING-01`; the I16 deferral is closed);
    - the besieged garrison as a battle side: faction change refused, deployment and overflow Blocked; the commit inside the preparation's exception handling; the `SettlementCaptured` event.
  - **Not implemented:**
    - the shrink trigger (shrinking is debug-only).
  - **Out of Roadmap V6 scope** (from the roadmap, not source): automatic recruitment, settlement creation, army AI and world generation. Only the APIs they would call and debug triggers exist.
  - **Known gaps after Phase 9** (untested or source facts the I18/I19 handoffs list; details in the cited rules and `90-Evidence.md`, Phase 9 basis): a settlement destroyed mid-siege discards its garrison, deployed defenders included, which the sweep then reports lost (`WM-OWN-02`); `CanTransfer` does not refuse pending retreat rosters (`WM-XFER-02`); `WorldMapDebugPanel2D` does not show them (`WM-PENDING-01`); `ArmySpawned` and `ArmyDestroyed` handlers are not isolated (`WM-REMOVE-01`); a siege-setup refusal adds to `TotalStartsRefused` on every step the touching pair is the selected pick (`BATTLE-SIEGE-01`); settlement-only defenders dying with a captured settlement, the capture refusal and undo paths, the siege no-return-room refusal and the in-place return fallback are source only; delivery timing depends on frame rate (`BATTLE-DELIVER-01`).
  - Every `UnitStatus` value is set: Reserve, Deployed, Escaping, Escaped, Dead and Discarded.
  - After a result lock, damage, Clash acceptance and casting are gated for the battle's units, pending modifier deliveries are dropped and no retreat starts (`BATTLE-LOCK-01`); `ForceDeath` and maximum-health clamp deaths are not gated, modifier application is not gated (only its delivery requests are dropped), regeneration heals locked units by design (`HEALTH-05`), and the battle debug kill refuses after the lock. Summon is a damage-origin enum value without an established delivery system. No production ability-removal API is established.
- **CORE-03 — Initialization is not eligibility.** The initialization responsibilities below do not establish an extra cross-component Awake order. Do not infer initialized progression from component presence, or generalized inactive-unit behavior from the holder's supported pre-Awake `Grant`. The detailed behavior of other components before first activation is not established here. World-map recruitment creates units that have never run Awake and are kept parked (`WM-UNIT-01`). Reads through `UnitHandle` use serialized fields or a `didAwake` check (`WM-UNIT-02`), and `UnitCombatant2D.SetFaction` is safe on such units. `UnitDestinationOverride2D` has no Unity messages and is safe on parked units (`ARENA-DEST-01`). The battle spawn path is the only activator (`BATTLE-SPAWN-02`). While the unit is still parked it binds side layers and masks (`SIDE-02`) and resets the motor (`MOVE-04`), using plain field writes only; it then activates the unit, whose own Awake, OnEnable and Start run in the usual order (`INIT-01`). A later re-activation reruns OnEnable only; `UnitStatsUI2D` refreshes then (`UI-01`). Other components' behaviour on re-activation is established only where a rule names it. The regeneration tick never activates a unit. It skips units whose `HasEverActivated` is false before touching their health. It heals inactive units (parked, Escaped, quarantined) without waking them, using component `enabled` flags and `didAwake` instead of `isActiveAndEnabled`, and it reads the effective maximum on the inactive, awakened unit (`WM-REGEN-02`, `HEALTH-05`). `UnitHandle.Health` uses GetComponent only (`WM-UNIT-02`). Battle modifier deliveries treat a unit as started only in a later frame than its activation and, when it has an enabled ability holder, once that holder's `didStart` is true; a redeployed unit runs no Start again (`BATTLE-DELIVER-01`). Units of a pending retreat roster stay parked as Reserve, never activated by it, until they are placed in an army (`WM-PENDING-01`); a siege capture deactivates and destroys a settlement-only defender it releases, whether or not that unit was ever activated (`WM-CAPTURE-01`).
- **CORE-04 — Evidence tags.** `[V8]`, `[V7]`, `[SID]`, `[V6]`, `[F]`, `[B]`, `[K]` identify inherited evidence, not fresh source or runtime verification. The V9 restructure inspected only the supplied map. World-map and battle tags record the basis of each Roadmap V6 rule: `[WM1]`–`[WM9]` — written from that phase's source and covered by a named Unity result the user reported for that phase; `[WM1-SR]` — source-reviewed only at the time; `[WM2-S]`–`[WM9-S]` — that phase's source only; `[WM4-SR]`, `[WM5-SR]` — paths the I10 or I13 handoff lists as source-reviewed only. Phase 5 brackets name the chat whose run covers the clause (I11, I12 or I13), because only I13 ran on the final source. Phase 6 results come from I14, on the final source, plus the Regen-tracking check reported with I15. Phase 7 brackets name I15 (unit retreat), I16 (retreating armies) or the Phase 7 closing check reported after M07. Phase 8 results come from I17, reported per acceptance item. Phase 9 brackets name I18 (capture and siege start, on the I18 source) or I19 (siege outcomes and pending retreat rosters, on the final source); I18 results are credited only to paths I19 did not change. A bracket such as `[WM3: I07 Retreat 5]` names the covering result; clauses outside it are source-derived. `90-Evidence.md` defines every tag. Configuration tables state requirements; they do not claim actual prefab/scene compliance.

### Load routing

| Task | Files in addition to this core |
| --- | --- |
| Targeting, movement, Sidestep | `01-Movement-Sidestep.md`; add Modifiers if changing effective stats |
| Clash resolution or ordering | `02-Clash-Transaction.md`, `03-Health-Death-XP.md`, `04-Stats-Modifiers.md`; add Abilities when touching trigger capture/dispatch |
| Damage, death, kill XP | `03-Health-Death-XP.md`; add the relevant producer: Clash, Modifiers or Abilities |
| Modifier scaling, lifetime, scheduled work | `04-Stats-Modifiers.md`; add Health for DoT or Abilities for triggers |
| Abilities and triggered casts | `05-Abilities-Triggered-Casts.md`; add Clash for transaction boundaries, Modifiers for request lifetime, Health for damage/XP |
| Health/level display | `06-UI.md`; add Health if changing emitted events |
| World map, armies, settlements, contacts, rosters, deployment | `07-WorldMap-Battle.md`; add combat files only when a task crosses into battle or touches a unit integration point (faction setter: Movement; `settlementOnly`: Core; definition access: Modifiers) |
| Battle arena, camera swap, perimeter, entry and retreat steering, battle layers | `07-WorldMap-Battle.md`; add `01-Movement-Sidestep.md` when changing how motor or Sidestep react to the destination override or `Perimeter` |
| Battle start, side layers and binding, spawning, death/escape/abort handling | `07-WorldMap-Battle.md`; add `01` for the motor reset or targeting mask, `02` for the detector mask, `05` for the ability mask, `03` for death timing, `06` for unit UI on re-enable |
| Battle end, result lock, cleanup, routing, separation, pair immunity, recovery, post-battle audit | `07-WorldMap-Battle.md`; add `03` for the damage gate, `02` for the Clash gate, `05` for the casting gate and projectile sweep, `04` for the reactive turn-boost clear, `01` for the motor's Disabled state |
| Regeneration and healing | `07-WorldMap-Battle.md` (tick, eligibility) and `03-Health-Death-XP.md` (heal path); add `04` for effective-maximum reads on inactive units, `06` for the display after redeployment |
| Unit retreat, Escaping, battle modifier delivery, battle-start modifier lists, retreating-army recovery | `07-WorldMap-Battle.md`; add `03` for the `HealthChanged` latch, `04` for `ApplyOrdered` and modifier removal on parking, `01` for Sidestep under the Retreat override, `05` for the holder's Start that delivery waits for |
| Capture, sieges (start, setup, outcomes, siege return, army–settlement immunity), pending retreat rosters and escaped defenders | `07-WorldMap-Battle.md`; add `04` for defender-list delivery through `ApplyOrdered`, `05` for the holder's Start that delivery waits for, `01` when changing Sidestep or the motor under the gate override. The settlement-only defenders' death at a capture is a release and a Destroy in `07` (`WM-CAPTURE-01`); it does not pass through `03` |
| Audit provenance or restructure | `90-Evidence.md`, `91-ChangeLog.md`, `92-Validation.md` — omit from implementation chats |

Follow cross-file IDs only when that boundary is affected. A broad transaction change may legitimately require more than three subsystem files. Request exact source filenames from the inventory rather than attaching the entire project.

## Authoritative ownership

| Concern | Owner |
| --- | --- |
| Faction identity and enemy relation (changeable only while parked) | `UnitCombatant2D` |
| Target selection/search deadlines | `UnitTargeting2D` |
| Major state, clean forward speed, applied Sidestep bonus, Rigidbody motion, Knockback, destination-override steering and the deployment reset | `UnitMotor2D` |
| Sidestep observed target/retry cooldown | `UnitSidestep2D.Eligibility.cs` |
| Sidestep lifecycle/bonus bookkeeping | `UnitSidestep2D.Lifecycle.cs` |
| Sidestep geometry/progress/movement intent | `UnitSidestep2D.Path.cs` |
| Sidestep Wall queries/classification | `UnitSidestep2D.WallClearance.cs` |
| Current health, authoritative damage result and the regeneration heal (the only gameplay healing path) | `UnitHealth2D` |
| One-time death and destruction scheduling | `UnitDeath2D` |
| Vulnerable expiration | `UnitVulnerability2D` |
| Level, XP, requirements and bounty | `UnitProgression2D` |
| Per-victim lethal attribution, pending award and deferral depth | `UnitKillExperience2D` |
| Effective stat calculation and final bounds | `UnitRuntimeStats2D` |
| Contributions, handle/bundle identity, aggregates and legacy timed expiry | `UnitStatModifierController2D` |
| Independent modifier instances, frozen values, lifetime, visual leases, pending trigger requests | `UnitModifierRuntime2D` |
| Modifier authored settings / validation and scaling | `ModifierDefinition2D` / `ModifierValueResolver2D` |
| Scheduled ordering, cancellation identities and global creation sequence | `ModifierScheduler2D` |
| Pooled modifier clones and lease generations | `ModifierVisualPool2D` |
| Reactive loss/Wall filters, subscriptions, two source handles and their explicit clear | `UnitReactiveTurnBoost2D` |
| Unit Clash cooldown, snapshots, trigger capture, participant effects and retained reaction record | `UnitClashParticipant2D` |
| Batch/pair deduplication, preparation, paired capture/consumption, result/XP/reaction/trigger boundaries | `ClashDirector2D` |
| Two Dodge samples and two Knockback-angle samples | `ClashDirector2D` |
| Numerical Clash resolution / pure Dodge arithmetic | `ClashCalculator2D` / `DodgeCalculator2D` |
| Trigger group ordering, saved aim, per-request invocation and diagnostics | `ModifierAbilityTriggerDispatcher2D` |
| Ability ownership, normal timers/selection, triggered entry and shared execution guard | `UnitAbilityHolder2D` |
| Authored ability values / frozen cast construction | `AbilityDefinition2D` / `AbilityValueResolver2D` |
| Category preparation and committed dispatch | `AbilityExecution2D` |
| Area collection and root-distance filtering | `AbilityAreaQuery2D` |
| Ability recipient gates, damage/XP and modifier delivery | `AbilityDelivery2D` |
| Projectile flight, expiry, encounter memory, impact Dodge sampling and the static in-flight registry | `AbilityProjectile2D` |
| Scene-owned ability visual lifetime/world scale | `AbilityVisuals2D` |
| Health-bar/level-sprite display | `UnitStatsUI2D` |
| Base unit configuration / shared Clash configuration | `UnitDefinition2D` / `ClashRules2D` |
| World-map registry, army/settlement ids, bounds, MapTime, PhysicsStep, freezing, army creation, army removal | `WorldMapController2D` |
| Contact intake/deduplication, touch episodes, report dispatch, battle-candidate selection | `WorldMapController2D.Contacts.cs` |
| Reserve creation, ownership audit, deferred-recruitment flush, owner-loss handling | `WorldMapController2D.Units.cs` |
| Deployment requests, placement, populated army creation/rollback, overflow | `WorldMapController2D.Deployment.cs` |
| Allied interactions, transfer gate and gated transfers | `WorldMapController2D.Interactions.cs` |
| Return spots, battle return placement and army-pair battle immunity; siege return spot, siege return and army–settlement immunity | `WorldMapController2D.Separation.cs` |
| Regeneration service creation and stepping, tick logging, Editor test menus | `WorldMapController2D.Regeneration.cs` |
| Retreating-army recovery (due check, flag clear, recovery log) | `WorldMapController2D.Retreat.cs` |
| Ordinary captures, capture records and counters, `SettlementCaptured` | `WorldMapController2D.Capture.cs` |
| The siege capture, queued capture events, pending retreat rosters (`PendingRetreatRoster2D`) and their placement | `WorldMapController2D.Siege.cs` |
| Regeneration tick: scaled-time accumulator, registry snapshot, skips and tick report | `UnitRegeneration2D` |
| Parked storage, unit registry/ids, arrival sequence, deployment and Escaping status marks and re-parking, every unit-ownership change | `UnitReserve2D` |
| One owner's unit membership (garrison, army or pending retreat roster), arrival-order storage, read order, capacity, garrison faction label | `UnitRoster` |
| Persistent unit identity and wake-safe reads | `UnitHandle` |
| Contact table, intents and role order | `WorldMapContactClassifier2D` (in `WorldMapContacts2D`) |
| Army direction, bounce, constant velocity, retreat flag and its recovery deadline, army roster, source-settlement collision ignore, battle-return teleport (`PlaceAt`, controller only) | `Army2D` |
| Settlement kind, faction, radius, garrison roster, recruitment entry, pending deployment request and retry timers, authored siege setup, the faction change and its in-battle guard | `Settlement2D` |
| World-map tuning / faction colours / WM layer indices | `WorldMapRules2D` / `FactionPalette2D` / `WorldMapLayers2D` |
| Arena lifecycle, Root and its contents, map-freeze ownership while an arena exists, view sizes, arena audit | `BattleArena2D` |
| Perimeter walls, retreat gates, entry-strip geometry, shrink stepping | `BattleArena2D.Perimeter.cs` |
| Registered arena bodies, entry state, wall ignores, arena-set overrides, leak/re-entry/escape/lost counters | `BattleArena2D.Bodies.cs` |
| World/battle Camera enablement, battle camera pose and size | `BattleCamera2D` |
| A unit's destination override value and its `Changed` event | `UnitDestinationOverride2D` |
| Battle tuning and battlefield regions / battle layer indices | `BattleRules2D` / `BattleLayers2D` |
| Running battle and all manager state, counters and events; step order; subscriptions; the combat-lock roots it locked | `BattleManager2D` |
| Start trigger, pre-arena checks (including return placements), refusals, refusal wait or pair immunity, commit; siege checks, siege sides and context, siege refusals | `BattleManager2D.Start.cs` |
| Spawn credit and cadence, entry-point reservations, the per-unit deployment sequence, the `Battle Units (active)` container | `BattleManager2D.Spawning.cs` |
| End check, death batches, timeout, result lock and its effects, end delay, normal end, abort | `BattleManager2D.Ending.cs` |
| Shared unit removal, quarantine and its retry, escaped parking, corpses, death, escape, sweep, lost units, Sidestep probe | `BattleManager2D.Units.cs` |
| Routing, army-return and immunity calls, siege outcomes (the capture call, refused captures, the siege return), recovery of a pending end or orphan arena, post-battle audit | `BattleManager2D.Routing.cs` |
| Retreat eligibility rolls, the low-health latch and its subscriptions, the deployment check, retreat starts | `BattleManager2D.Retreat.cs` |
| Battle modifier delivery requests, the wait for start, application and drops; the battle-start list freeze (field, or siege since I18) and the per-unit battle-start request | `BattleManager2D.Delivery.cs` |
| Battle context, result, side and deployed-unit index records (not owners), including each side's frozen battle-start list, the siege setup and outcome fields and the garrison side | `BattleTypes2D` |
| Locked unit roots (read by health, the Clash director and the ability holder) and the blocked-damage count | `CombatLock2D` |
| Field side mapping, siege side constants (defender A); resolved side layers and enemy masks | `BattleSides2D` (with `BattleSideLayers2D`) |
| Per-deployment layer and mask writes (each mask stays owned by its component) | `UnitSideBinder2D` |

## Source inventory and one-line roles

All names below are `.cs` files. Card home identifies the one place with the script's contract card. Partial responsibilities live in the single Sidestep card, the single WorldMapController2D card, the single BattleArena2D card and the single BattleManager2D card.

| Source basename | Role | Card home |
| --- | --- | --- |
| UnitDefinition2D | Shared base unit, progression, movement, Sidestep, reactive and regeneration configuration | Core |
| ClashRules2D | Shared targeting/Clash timing, calculation and Vulnerable configuration | Core |
| CombatTypes2D | Faction, major state and immutable Clash communication types | Clash |
| CombatDamageTypes2D | Generalized damage request/result types | Health |
| CombatModifierTypes2D | Stat entries, handles, bundles, scopes and embedded bundle mapper | Modifiers |
| UnitCombatant2D | Identity and targetability facade | Movement |
| UnitTargeting2D | Nearest valid enemy selection | Movement |
| UnitMotor2D | State, steering (target or destination override), forward momentum, Knockback, intent execution, deployment reset | Movement |
| UnitSidestep2D | Single component coordinator and Unity entry points | Movement |
| UnitSidestep2D.Eligibility | In-band opportunities and unit-wide retry cooldown | Movement |
| UnitSidestep2D.Lifecycle | Session start/end and exact bonus cleanup | Movement |
| UnitSidestep2D.Path | Snapshotted half-circle and once-per-step intent | Movement |
| UnitSidestep2D.WallClearance | Full-body Wall preflight and contact classification | Movement |
| UnitSidestep2D.Interruptions | Motor subscription and active-session validity | Movement |
| UnitSidestep2D.Clash | Accepted-Clash cancellation and clean velocity rebuild | Movement |
| UnitProgression2D | Level/XP advancement and bounty | Health |
| UnitVulnerability2D | Extend-only Vulnerable timer | Health |
| UnitRuntimeStats2D | Derived effective stats | Modifiers |
| UnitStatModifierController2D | Contribution storage, aggregation and notifications | Modifiers |
| UnitReactiveTurnBoost2D | Timed loss/Wall additive turn effects | Modifiers |
| UnitHealth2D | Authoritative damage, health mutation and the regeneration heal | Health |
| UnitDeath2D | One-time death lifecycle | Health |
| UnitKillExperience2D | Victim attribution and deferred award | Health |
| UnitClashDetector2D | Clash Range trigger-to-request adapter | Clash |
| ClashDirector2D | Scene batch and paired transaction owner | Clash |
| ClashCalculator2D | Pure pair calculation | Clash |
| DodgeCalculator2D | Pure shared chance/sample helper | Clash |
| UnitClashParticipant2D | Unit cooldown, capture and result orchestration | Clash |
| ModifierDefinition2D | Modifier authoring and child/trigger references | Modifiers |
| ModifierFrameworkTypes2D | Context, resolved values, instance/schedule identities | Modifiers |
| ModifierValueResolver2D | Pure modifier validation/scaling | Modifiers |
| UnitModifierRuntime2D | Independent instances and pending trigger ownership | Modifiers |
| ModifierScheduler2D | Shared fixed-time scheduled work | Modifiers |
| ModifierVisualPool2D | Persistent modifier visual pool | Modifiers |
| ModifierApplicationDebug2D | Optional manual modifier diagnostics | Modifiers |
| ModifierAbilityTriggerTypes2D | Copied opportunities and leased capture buffers | Abilities |
| ModifierAbilityTriggerDispatcher2D | Transaction-local trigger execution/diagnostics | Abilities |
| AbilityDefinition2D | Ability category, values, modifier list and art settings | Abilities |
| AbilityFrameworkTypes2D | Invocation, cast, request-owner and result contracts | Abilities |
| AbilityValueResolver2D | Pure ability resolution and cast capture | Abilities |
| UnitAbilityHolder2D | Ownership, timers, selection and cast commitment | Abilities |
| AbilityExecution2D | Prepared category dispatch | Abilities |
| AbilityAreaQuery2D | Complete deduplicated Area collection | Abilities |
| AbilityDelivery2D | Synchronous recipient bridge | Abilities |
| AbilityProjectile2D | Independent projectile physics, encounters and in-flight registry | Abilities |
| AbilityVisuals2D | Per-scene ability presentation service | Abilities |
| AbilityDebugGrant2D | Optional grants and normal-state diagnostics | Abilities |
| UnitStatsUI2D | Event-driven health and level display | UI |
| WorldMapController2D | Single map controller: registry, bounds, map time, freezing, spawning | WorldMap |
| WorldMapController2D.Contacts | Contact intake, deduplication, episodes, classification dispatch, battle pick | WorldMap |
| WorldMapController2D.Units | Reserve, audit, deferred-recruitment flush, owner loss | WorldMap |
| WorldMapController2D.Deployment | Deployment, placement, overflow, settlement work (including escaped-defender armies) | WorldMap |
| WorldMapController2D.Interactions | Allied interactions, gated transfers, interaction record | WorldMap |
| WorldMapController2D.Separation | Return spots, battle return, army-pair immunity; siege return spot, siege return, army–settlement immunity | WorldMap |
| WorldMapController2D.Regeneration | Regeneration service creation and stepping, tick log, Editor test menus | WorldMap |
| WorldMapController2D.Retreat | Retreating-army recovery after the map-time timer | WorldMap |
| WorldMapController2D.Capture | Captures of empty enemy settlements, capture records, `SettlementCaptured` | WorldMap |
| WorldMapController2D.Siege | Siege capture, pending retreat rosters and their placement, siege debug menus | WorldMap |
| Army2D | Constant-speed bouncing army marble owning a unit roster and its retreat timer | WorldMap |
| Settlement2D | Static town/castle with garrison, recruitment, pending deployment and authored siege setup | WorldMap |
| UnitReserve2D | Parked-unit storage, deployment and Escaping status and the only ownership mutator | WorldMap |
| UnitRoster | Logical owner of units (garrison, army or pending retreat roster) | WorldMap |
| UnitHandle | Persistent unit identity | WorldMap |
| UnitRegeneration2D | Shared healing tick over the reserve registry; tick report and heal-outcome types | WorldMap |
| UnitRosterTypes2D | Unit status, roster kind (garrison, army, pending retreat)/order, transfer and recruit results | WorldMap |
| UnitPrefabCatalog2D | Debug list of recruitable unit prefabs | WorldMap |
| WorldMapContacts2D | Contact kinds/intents, pair key, report and pure classifier | WorldMap |
| WorldMapTypes2D | Settlement kind, spawn reason, deploy request/result and placement types | WorldMap |
| WorldMapRules2D | Shared world-map tuning asset | WorldMap |
| FactionPalette2D | Faction colours with generated fallback | WorldMap |
| WorldMapLayers2D | WM layer resolution and matrix reporting | WorldMap |
| WorldMapMath2D | Pure rotation and axis-nudge helpers | WorldMap |
| WorldMapAssets2D | Generated placeholder sprites, material and labels | WorldMap |
| WorldMapDebugPanel2D | Optional IMGUI spawn, roster, deployment and contact diagnostics | WorldMap |
| BattleArena2D | Arena service: lifecycle (including siege arenas on an explicit prefab), map freeze, camera swap, terrain, teardown, audit | WorldMap |
| BattleArena2D.Perimeter | Walls, retreat gates, entry strips, shrink | WorldMap |
| BattleArena2D.Bodies | Body registration, entry/leak tracking, escapes, entry points, test bodies | WorldMap |
| BattleCamera2D | World/battle camera swap and battle view size | WorldMap |
| BattleRules2D | Battle tuning (including the battle-start modifier lists and the siege defender spawn multiplier), battlefield regions and selection | WorldMap |
| BattleLayers2D | Battle layer resolution and matrix reporting | WorldMap |
| BattleArenaTypes2D | Edges, sides, perimeter modes, body states, ArenaBody | WorldMap |
| BattleArenaGate2D | Retreat gate trigger reporter | WorldMap |
| UnitDestinationOverride2D | Destination that replaces target steering in motor and Sidestep while active (unit integration point) | WorldMap |
| BattleTestBody2D | Debug stand-in unit body | WorldMap |
| BattleArenaDebugPanel2D | Optional IMGUI arena and test-body diagnostics | WorldMap |
| BattleSides2D | Field side mapping, siege side constants and resolved side layers (`BattleSideLayers2D`) | WorldMap |
| UnitSideBinder2D | Per-deployment layer and mask binding of a parked unit | WorldMap |
| UnitSideBindingTest2D | Optional Play Mode side-binding check | WorldMap |
| BattleManager2D | Battle service: config, state, events, step order, subscriptions | WorldMap |
| BattleManager2D.Start | Start trigger, pre-arena checks (field and siege), refusals, commit | WorldMap |
| BattleManager2D.Spawning | Spawn cadence and the per-unit deployment sequence | WorldMap |
| BattleManager2D.Ending | End check, result lock, end delay, normal end, abort | WorldMap |
| BattleManager2D.Units | Shared removal, quarantine, death, escape, sweep, probe | WorldMap |
| BattleManager2D.Routing | Routing, siege outcomes, army return, recovery, post-battle audit | WorldMap |
| BattleManager2D.Retreat | Retreat eligibility, low-health latch, retreat start | WorldMap |
| BattleManager2D.Delivery | Battle modifier delivery path and battle-start list freeze | WorldMap |
| BattleManager2D.Debug | Development tools, status text, context menus | WorldMap |
| BattleTypes2D | Battle context (including siege setup and outcome), side (army or garrison) and deployed-unit records | WorldMap |
| BattleManagerDebugPanel2D | Optional IMGUI battle diagnostics and tools | WorldMap |
| CombatLock2D | Battle result lock gate read by health, Clash director and ability holder | WorldMap |

## Execution and initialization

**ORDER-01 — Declared callback order.** Lower values execute earlier within the same callback phase. These attributes do not create loops, guarantee relative order for equal values, or override unknown project settings.

| Component | DefaultExecutionOrder | Relevant phase/role |
| --- | ---: | --- |
| BattleManager2D | -12000 | FixedUpdate: battle only. Subscriptions → quarantine prune and retry → with a battle: corpse prune → unit sweep (dead, lost or deactivated units unregistered) and Sidestep probe → end check, or once locked the end delay and normal end → spawn tick (each spawn queues its battle-start request) → retreat starts → modifier deliveries; without a battle: pending end → orphan arena → post-battle audit. Before scheduler, director, map and arena |
| ModifierScheduler2D | -11000 | FixedUpdate: scheduled work before director |
| AbilityVisuals2D | -11000 | Update/LateUpdate only: presentation |
| ClashDirector2D | -10000 | FixedUpdate: close previous request batch; synchronous pair transactions |
| WorldMapController2D | -1000 | FixedUpdate: world map, plus the regeneration tick. Queued siege-capture events (frozen or not) → classify/dispatch previous step's contacts → allied interactions → captures (unfrozen only) → step/map time → retreat recovery (unfrozen only) → settlement work, ending with the pending retreat rosters → regeneration step (every ready step, frozen or not), before Army2D (default 0) |
| BattleArena2D | -900 | FixedUpdate: battle arena only. Shrink step → perimeter layout on the live view → body tracking and Entry/Retreat overrides, before Sidestep (−150) and motor (−100), which read those overrides, and default-order test bodies. By declared order its Awake follows the world-map controller's, so WM layers are normally resolved for its matrix report (source; not separately tested) |
| UnitCombatant2D | -300 | Facade lifecycle order |
| UnitDeath2D | -250 | Death callbacks; no fixed loop implied |
| UnitKillExperience2D | -225 | Attribution callbacks; no fixed loop implied |
| UnitHealth2D | -200 | FixedUpdate: maximum-health reconciliation |
| UnitTargeting2D | -200 | FixedUpdate: target maintenance |
| UnitSidestep2D | -150 | FixedUpdate: eligibility/session coordination |
| UnitMotor2D | -100 | FixedUpdate: physical execution |

World-map and combat components share no Clash transaction. They meet through `BattleManager2D` (contact handler, reserve status and ownership operations, binder, motor reset and Disabled state, `Died` subscription, the `HealthChanged` retreat latch, modifier delivery through `UnitModifierRuntime2D.ApplyOrdered`, routing, army return and pair-immunity calls, the projectile sweep) and `CombatLock2D` (a static set written only by the manager in its FixedUpdate and read by health, director and holder, with no callbacks), and through the arena's overrides, which the motor and Sidestep read in the same step after the arena writes them. Since I14 they also meet through the regeneration tick, which calls `UnitHealth2D.ApplyRegeneration` on registry units from the controller's FixedUpdate (−1000). That is after the battle manager (−12000), the scheduler (−11000) and the director (−10000) have done the step's work, before the arena (−900) and the units' own health FixedUpdate (−200), and never inside a Clash transaction (`WM-REGEN-01`). The world map also calls `BattleManager2D.Instance` for `IsRosterInBattle` (true for a besieged garrison since I18), and since I19 the siege capture reads the manager's `Current` and `IsEnding` to accept only its own ending siege as caller. The −1000 value is meaningful only relative to Army2D and the manager; its numeric position among combat values carries no designed ordering. `BattleManager2D` (−12000) runs before the scheduler and director, so the end check and the lock see each step's complete combat work and never run inside a transaction (`BATTLE-END-01`, `BATTLE-LOCK-01`), and retreat starts and modifier deliveries never run inside a Clash transaction or protected capture; a latch recorded during a step's Clash or DoT work therefore starts its retreat at the next manager step (`BATTLE-RETREAT-03`); before the map (−1000), so a battle that ends in a step is fully returned before the map's step, and a siege capture applied by that end has its `SettlementCaptured` raised at the start of the map's step, after `BattleEnded` when the teardown completed (after an incomplete end it can come first, `WM-CAPTURE-01`); and before the arena's tracking pass, so dead or lost units are unregistered first. The first spawn of each battle runs inside the map's FixedUpdate (contact dispatch); later spawns run in the manager's FixedUpdate. When a unit activated during the FixedUpdate phase receives its own first FixedUpdate is not established by source. By declared order the manager's Awake precedes the map's and the arena's, so it subscribes lazily in Start and in each FixedUpdate. `BattleArena2D` reaches the world map only through `WorldMapController2D.Instance` (when no map is assigned), `IsReady`, `IsFrozen`, `MapBounds`, `SetFrozen`, `FrozenChanged` and read-only `WorldMapLayers2D` indices, and calls no combat component. Retreat-gate escapes, and the manager's escape handling, run in physics trigger callbacks (after the step's simulation), not in FixedUpdate. `BattleCamera2D` declares no order; its camera-count monitor runs in LateUpdate. World map, arena and combat ran in one Play Mode session in the Phase 4–7 Unity tests. World-map ordering detail: `WM-TIME-01`. `UnitReserve2D`, `UnitRoster`, `UnitHandle` and `UnitRegeneration2D` are plain C# objects with no Unity callbacks.

**ORDER-02 — Unspecified order and direct calls.** Health versus targeting at equal order is unspecified. Participant, detector, reactive boost, UI, holder and projectile declare no separate order. Sidestep partials add no separate component/order. Accepted-Clash preparation and triggered dispatch use synchronous director calls. No relative order among different holders/projectiles or between reactive Wall handling and Sidestep Wall cancellation is specified. Normal casts run in Update; starting grants in Start; triggers run in the director's FixedUpdate before movement. `Time.time` in that fixed callback is fixed-step time. Ability visual order does not place it in a FixedUpdate gameplay transaction.

**INIT-01 — Initialization responsibilities.** Progression initializes configured starting level/XP requirement/bounty; runtime stats derives queried values; health starts at effective maximum; motor starts at configured speed clamped to effective maximum; targeting allocates reusable queries. Sidestep resolves peers, validates geometry/configuration and prepares Wall queries. Reactive boost caches Wall and validates peers in Awake. Runtime attaches to its controller and subscribes while enabled; scheduler exists before scene load; modifier pool and ability visual hosts are lazy. Holder waits for initialized progression when necessary. Apply `CORE-03` when working with inactive units. World-map initialization: `WM-LIFE-01`, `WM-SETTLE-02`, `WM-UNIT-01`, `WM-REGEN-01`. Battle-arena initialization: `ARENA-LIFE-01`, `ARENA-CAM-01`; `UnitDestinationOverride2D` needs none. Recruited units run none of the unit initialization above until the battle spawn path activates them (`BATTLE-SPAWN-02`), after side binding (`SIDE-02`) and the motor reset (`MOVE-04`) while still parked. Battle manager initialization: `BATTLE-LIFE-01`.

## Scene, placement and references

**SETUP-01 — Unit root and assets.** Required same-root peers are listed below; do not infer that components are automatically installed. The four `*Definition2D`/rules assets are shared ScriptableObjects. Definition assignment is authored separately. UI may be on root or child. Clash detector belongs on a Clash Range object and finds its participant in parents. A single scene director is required with Physics 2D simulation in Fixed Update mode; detector uses an assigned director or singleton fallback.

| Component | Required peers/configuration; exceptions |
| --- | --- |
| UnitCombatant2D | Motor, health |
| UnitTargeting2D | Unit definition, Clash rules, combatant, enemy-query mask |
| UnitMotor2D | Unit definition, Rigidbody2D, targeting, runtime stats; resolves Sidestep but motor validation does not require it; optional same-root destination override (one on another GameObject disables the motor) |
| UnitProgression2D | Unit definition |
| UnitRuntimeStats2D | Unit definition, Clash rules, progression, vulnerability; controller optional, including valid idle-disabled controller |
| UnitHealth2D | Combatant, runtime stats, death |
| UnitDeath2D | Health, motor |
| UnitKillExperience2D | Combatant, health, progression |
| UnitClashParticipant2D | Unit definition, combatant, motor, same-root Sidestep, progression, runtime stats, vulnerability, health, kill XP; optional same-root holder |
| UnitSidestep2D | Unit definition, targeting, motor, body circle and geometry in `SETUP-02`; optional same-root destination override (one on another GameObject disables Sidestep) |
| UnitReactiveTurnBoost2D | Unit definition; same-root participant, health, controller; exact Wall layer; invalid initialization disables component |
| UnitStatModifierController2D | No required serialized dependency |
| UnitModifierRuntime2D | Same-root combatant, health, death, controller; progression for convenience capture; kill XP for damage integration; no RequireComponent installation |
| UnitAbilityHolder2D | Same-root combatant/progression/health/death; targeting only for normal Projectile; starting list, enemy Clash and Wall masks; repairs wrong-root/missing peer references locally |
| UnitClashDetector2D | Parent participant, local trigger circle, director, nonempty opposing Clash mask; on a child of the unit root, not the root (`SIDE-02`) |
| UnitStatsUI2D | Health/progression on enable; optional fill Transform, renderer and sprite array |
| UnitDestinationOverride2D | None; required on the root of every battle-deployable unit (`BATTLE-SPAWN-02`) |

Battle deployment also requires on the unit root a solid `CircleCollider2D` with a positive world radius, `UnitMotor2D`, `Rigidbody2D`, `UnitHealth2D`, `UnitDeath2D`, `UnitTargeting2D`, `UnitClashParticipant2D` and `UnitDestinationOverride2D`, plus at least one child Clash detector belonging to that participant. A unit failing these is dropped from the battle and stays parked (`BATTLE-SPAWN-02`, `SIDE-02`). The root layer and the targeting, detector and ability enemy masks saved on the prefab are overwritten at each deployment.

**SETUP-02 — Body and Wall.** Sidestep requires a solid centered same-root CircleCollider2D, zero offset, positive finite world radius and approximately uniform absolute X/Y world scale. Exact layer `Wall` must exist. `Perimeter` is optional for units: when it exists, Sidestep preflight and contact cancellation and the reactive Wall handle treat it like `Wall` (`SIDESTEP-PATH-01`, `SIDESTEP-LIFE-02`, `REACTIVE-01`). Sidestep excludes triggers from preflight; both Sidestep and reactive contacts inspect the contacted collider's own layer. Body/Wall (and, in battle, Body/Perimeter, `SETUP-07`) collision matrix must permit solid callbacks. Invalid Sidestep configuration/geometry/references disables Sidestep; missing/wrong-root Sidestep disables the participant. Reactive validation does not independently validate collider solidity, matrix or geometry.

**SETUP-03 — Ability colliders and masks.** Area uses enemy Clash layers: a queryable enabled collider's filled volume must contain its unit root throughout gameplay, with local detector/owner mapping to the same-root combatant. Detector enablement is not an Area gate; root containment is a prefab invariant, not runtime validation. Projectile contacts resolve combatants through parents. Collider/root layers may differ, but mappings must identify the same unit. Projectile layer interactions must permit intended enemy Clash and Wall contacts; masks cannot create disabled physics interactions. Modifiers introduce no new layer requirement. In battle, deployment sets the holder's enemy Clash mask and each detector's opposing mask to the enemy side's Clash layer (`SIDE-02`); the holder's Wall mask is unchanged. The I08 handoff reports that the old prefab ability enemy mask value, 16 (a body layer), was wrong under the matrix; deployment overwrites it in battle, and outside battle the prefab value still applies.

**SETUP-04 — Projectile prefab.** Inactive, unparented root at scale one; enabled AbilityProjectile2D; exactly one enabled centered trigger CircleCollider2D and one Dynamic Rigidbody2D, simulation authored off, gravity/linear damping zero, exactly FreezeRotation constraints. Visual root is a direct child; no additional colliders/Rigidbodies anywhere below it. Payload/geometry initialize before object/simulation activation. Art faces local +X or uses a rotated visual child.

**SETUP-05 — Shared hosts and optional tools.** Scheduler is persistent and auto-created before scene load; modifier visual pool is persistent/lazy; neither goes on units. AbilityVisuals2D creates one host per loaded scene, not on units/effects. Its spawn needs finite position, positive world scale, supported duration, usable parent scale and same-scene parent. Calculator/dispatcher/execution/query/delivery are plain C#; resolvers/contracts/Dodge helper are not attachable components. Debug harnesses are optional; their production release shells are empty. Visual assignments and actual scenes/assets remain outside verification.

**SETUP-06 — World-map layers and controller.** Exact layers `WM_Army`, `WM_Settlement` and `WM_Bounds` must exist, or the controller disables itself and settlements log an error. `WM_Army` must collide with all three WM layers (error logged otherwise). Every WM layer must ignore every named non-WM layer (warning logged otherwise), so combat layers never interact with the world map. Settlement↔Settlement, Settlement↔Bounds and Bounds↔Bounds are not checked. Source only reports; it never edits the matrix. Armies and bounds receive layers in code; settlements set their own root layer in Awake. Exactly one controller, at the world origin with identity rotation and scale 1 (warning only). The world map depends on FixedUpdate collision callbacks; Physics 2D simulation mode is not validated by world-map source (`SETUP-01` already requires Fixed Update). World-map placement and overlap checks use `WorldMapLayers2D.AllMask` only. The parked-unit root is created in code under the controller with identity local transform, so with the controller at scale 1 a recruit keeps its prefab root's scale (keep it 1; Roadmap §5). Recruitable prefabs must pass `UnitReserve2D.ValidatePrefab`: a prefab asset whose root has `UnitCombatant2D` and a `UnitRuntimeStats2D` with a definition. Settlement requirements: `WM-SETTLE-01`.

**SETUP-07 — Battle camera, battle layers and arena placement.** Source reports problems; it never edits project settings.
- **Layers.** Exact layers `Perimeter` and `RetreatingPlace` must exist, and the two body layers named in `BattleRules2D` must exist and differ (`Unit_SideA`/`Unit_SideB`, renamed in I08 from `FactionABody`/`FactionBBody` at the same indices; the I08 report gives 15/16, with `Clash_SideA`/`Clash_SideB` at 11/12). Otherwise battle layers stay unresolved and no arena can be created. The `Wall` layer named there is optional: if missing, a warning is logged and entry points ignore terrain obstacles.
- **Side layers.** The rules' four side names (both body and both Clash layers) must resolve to four distinct layers whose body layers match the arena's, or every battle start is refused (`SIDE-01`). `BattleRules2D.OnValidate` warns when two names are equal or a name is missing from Tags and Layers. The side matrix report (`SIDE-01`) checks Clash A↔B (must interact), same-side Clash and body↔Clash pairs (should not) and Clash against `WM_*` (must not, when WM layers are resolved).
- **Matrix** (reported at arena Awake): `Perimeter` and `RetreatingPlace` must interact with both body layers (error). Body layers that do not collide with themselves or each other warn. `Perimeter` or `RetreatingPlace` interacting with any other named layer, including themselves and each other, warns; against a `WM_*` layer it is an error, but only when `WorldMapLayers2D` is already resolved (otherwise it is reported as a warning).
- **Cameras.** One `BattleCamera2D` with a world camera (assigned, else the `MainCamera`-tagged camera at Awake) and a battle camera on a different GameObject. Both GameObjects stay active; only Camera components are toggled. The battle camera renders to the screen (no target texture), should be Untagged, must not carry an enabled AudioListener (one is disabled with a warning) and is forced orthographic. The world camera should carry the enabled AudioListener (warning otherwise). Exactly one camera should be enabled at any time; other enabled scene cameras fail the monitor and the audit.
- **Arena.** Exactly one `BattleArena2D`. `ArenaCenter` must be far enough from `MapBounds` that the largest view at `ClearanceAspect`, plus ground margin and clearance, does not overlap them (checked at creation). The arena root is created at runtime as a scene root at world scale 1.
- **Battle manager.** Exactly one `BattleManager2D`; a second disables itself. The deployed-unit container `Battle Units (active)` is created at runtime as a scene root at world scale 1 and persists after battles; the post-battle audit expects it to have no children (`BATTLE-AUDIT-01`). `auditAfterEachBattle` (default on) is heavy; turn it off when measuring hitches. Roadmap §6: test releases with a nonzero `maxBattleSeconds`.
- **Battlefield prefabs.** No Camera or AudioListener; nothing on a `WM_*` layer (checked only when WM layers are resolved), `Perimeter` or `RetreatingPlace`; root scale 1 preferred.
- **Siege setup (I18).** Every settlement that can be besieged needs a siege battlefield prefab asset (not a scene object) meeting the battlefield-prefab rules, and two distinct attacker and defender edges (`WM-SETTLE-01`). Otherwise every siege of it is refused with nothing changed (`BATTLE-SIEGE-01`). Assets are not inspected by this map.

## Glossary — defined once

| Term | Meaning |
| --- | --- |
| GameplayTime | Modifier scheduler's scaled fixed clock, `Time.fixedTimeAsDouble`. |
| ApplicationTime | Current scheduled event timestamp during catch-up; otherwise fixed time. |
| Time.time | Scaled Unity time used by ability timers/casts and current Vulnerable movement reads; fixed-step time inside FixedUpdate. |
| Transaction time | Time supplied to a Clash snapshot/result, including Vulnerable snapshot reads. |
| Map time | `WorldMapController2D.MapTime`: fixed-step seconds accumulated only while the map is ready and unfrozen; the clock for world-map timers (pair immunity and retreat recovery included), distinct from GameplayTime and Time.time. Regeneration does not use it. |
| Regeneration tick | One healing pass by `UnitRegeneration2D` over a snapshot of the reserve registry, every `RegenerationIntervalSeconds` (2) of scaled fixed time accumulated on every ready controller step, frozen or not (`WM-REGEN-01`, `WM-REGEN-02`). |
| Activated unit | A unit the battle spawn path has activated at least once: `UnitHandle.HasEverActivated`, set at first deployment and never reset. Before that its health has not initialized (`CurrentHealth` 0, `IsDead` true). |
| Physics step | `WorldMapController2D.PhysicsStep`: count of ready controller FixedUpdates, frozen or not; stamps contact reports. |
| Touch episode | Consecutive physics steps in which one army/army or army/settlement pair is reported; ends when a step lacks it or the map freezes. |
| Contact report | `WorldMapContactReport`: one classified touching pair for one physics step. |
| Contact intent | `WorldMapContactIntent`: what a contact kind does. Since Phase 2 the allied intents take effect; since Phase 4 a FieldBattle pick starts a battle; since Phase 5 an immune army pair is not a battle candidate; since Phase 9 Capture captures an empty settlement and a Siege pick starts a siege, unless the army–settlement pair is immune. |
| Parked | A persistent unit that is inactive under the reserve root with status Reserve; never awakened unless it has been activated. Escaped units are also inactive under that root but are not parked (status Escaped). |
| Roster / garrison | `UnitRoster`: the single logical owner of units. A garrison is a settlement's unbounded roster; an army roster has `MaxUnitsPerArmy` capacity; since I19 a pending retreat roster is the third kind. |
| Settlement-only unit | Unit whose definition sets `SettlementOnly`; counts in its garrison and never enters an army or a pending retreat roster. |
| Settlement-only defender | A settlement-only unit of a besieged garrison: it deploys and fights, never rolls retreat eligibility and never retreats, even when forced; while it lives on the field or in the queue the defender side has remaining units. If the attacker wins, one still owned by the garrison (dropped or quarantined) dies with the settlement (`BATTLE-SIEGE-01`, `WM-CAPTURE-01`). |
| Eligible units | A roster's units that may join an army: `Count − SettlementOnlyCount`. |
| Arrival sequence | Per-reserve increasing number assigned on recruitment and on every transfer; roster storage and tie-break order. |
| Garrison limit / overflow | Town 200 / castle 300 total-garrison threshold at or above which exactly `OverflowArmySize` eligible units leave as an army; it never blocks arrivals. |
| Allied interaction | Retreat transfer, retreat merge or garrison join applied by the controller to touching allied pairs. |
| Arena | The battle area `BattleArena2D` builds under its runtime Root far from the world map; it exists only while `HasArena`. |
| Battle view | The battle camera's live orthographic view rectangle (`GetViewRect`); the perimeter, gates and entry tracking follow it. |
| Entry strip | Region beyond a wall's outer face where bodies of that edge appear before walking in. |
| Perimeter | The four `Perimeter`-layer walls just outside the battle view; bodies may enter, and once Inside cannot leave. |
| Retreat gate | `RetreatingPlace` trigger strip just inside a gate edge; a registered retreating body touching it escapes. |
| Arena body | `ArenaBody`: the arena's tracking record for one registered body (test body or deployed real unit); not an owner. |
| Leak / re-entry | Leak: an Inside body's centre found outside the view without announcement. Re-entry: an announced (`ForceReenter`) or aspect-change return to Entering. |
| Seeking / Knockback / Disabled / Dead | Motor major states; Disabled is gameplay state, distinct from a disabled component/root. |
| Sidestep | Subordinate locomotion session, not a UnitMajorState value. |
| Targetable | Combatant predicate for targeting/Clash; not a universal damage/modifier receiving gate. |
| Processed | Participant result accepted at entry; may be lethal, dodged or zero-damage. |
| Dodged | Explicit sampled result; never inferred from zero/rejected damage. |
| Committed | Irreversible point of a particular operation (damage, modifier application or cast); does not promise successful delivery or continued presence. |
| Pending trigger request | Runtime-owned captured opportunity that can be canceled or committed independently of active instance membership. |
| Handle | Controller-scoped identity of one stat entry; controller plus nonzero entry ID. |
| Bundle | Controller-scoped group identity for atomic framework stat installation/exact removal. |
| Instance ID | Runtime-scoped identity of one independent modifier application. |
| Schedule ID | Scheduler identity used to cancel scheduled work. |
| Lease | Pool clone plus generation identity preventing stale release of a reused clone. |
| Normal invocation | Held ability selected automatically with own/universal timer policy. |
| Triggered invocation | Pending-request-authorized cast; holder need not own the asset. |
| Captured set | Exact pre-result identities/opportunities; later callback-created instances cannot join it. |
| Settlement | Expiry reconciliation before callback-free protected capture. (World-map settlements are `Settlement2D`, not this term.) |
| Protected read | Settled controller barrier excluding mutation during paired snapshot reads. |
| Notification scope | Nestable controller notification deferral/coalescing; acquisition can itself settle overdue entries. |
| Requested damage / actual loss | Producer amount versus authoritative health loss reported by DamageResult2D. |
| Clean forward speed | Motor momentum excluding its separately stored Sidestep bonus and Knockback velocity. |
| Reserve / Deployed / Escaping / Escaped / Dead / Discarded | `UnitStatus` values. Reserve: parked, including units queued to spawn. Deployed: active in a battle, still owned by its army. Escaping (I15): a deployed unit that started retreating; still active, on the field and owned, walking to a gate (`BATTLE-RETREAT-03`). Escaped: left the battle through a gate; inactive under the parked root and still owned until the battle ends or aborts, when it is parked as Reserve. Dead: died in battle; no longer owned. Discarded: explicitly destroyed by debug or owner-loss code, or a battle unit (deployed, escaping, escaped or quarantined) destroyed without dying; no longer owned. `Army2D.IsRetreating` is an army classification/visual flag, not Escaping. `ArenaBodyState.Escaped` is arena bookkeeping, not this status. |
| Result lock | The point where a battle's decided result is stored and becomes final: `CombatLock2D` gates damage, Clash acceptance and casting for the battle's units, motors are Disabled, escapes and spawning stop, and battle projectiles are removed (`BATTLE-LOCK-01`). Not a synonym for Clash commitment or world-map battle selection. |
| Side (A/B) | `ArenaSide`: a battle's physics side, selecting layers and masks only. Field battles put the higher `FactionId.Value` on A; sieges put the defender on A. Never a faction. |
| Garrison side | A siege's defender side: its owner is the besieged settlement's garrison (`BattleSide.IsGarrison`, `Army` null), its queue the whole garrison in roster order, its rate twice the normal one by default; the garrison counts as in battle until the battle is no longer current (`BATTLE-SIEGE-01`). |
| Siege setup | A settlement's authored siege battlefield prefab and distinct attacker and defender entry edges, used for every siege of it regardless of contact direction (`WM-SETTLE-01`, `BATTLE-SIEGE-01`). |
| Capture | A settlement changing faction to an ordinary enemy army whose units all become its garrison, the army removed: immediately for an empty settlement, or after an attacker win in a siege (`WM-CAPTURE-01`). |
| Escaped defenders | After a siege capture, the mobile defenders still owned by the garrison (escapees, plus dropped or recovered quarantined units); they move to a pending retreat roster of their faction and are placed as retreating armies (`WM-CAPTURE-01`, `WM-PENDING-01`). |
| Side layers | `Unit_SideA/B` (unit root bodies) and `Clash_SideA/B` (Clash Range objects), resolved per battle as `BattleSideLayers2D`. |
| Battle context | `BattleContext`: one battle's record (pick, sides, layers, axis, result, return placements, end diagnostics); an index, not an owner. |
| Spawn queue / field list / dropped unit | A side's queue is its army-eligible units in roster order at battle start (a siege defender's queue is the whole garrison); its field list holds its deployed living units; a dropped unit failed deployment and stays parked in its army for the rest of the battle. |
| Spawn credit | A side's fractional allowance, growing at its spawn rate; each whole credit spawns one unit; held at 1 while blocked. |
| Lost unit | A deployed (or Escaping), escaped or quarantined unit destroyed without dying (released as Discarded), or a deployed unit deactivated or released outside the battle path (parked as Reserve); counted in `LostUnits`. |
| Corpse | A dead deployed unit's GameObject after its ownership is released, until `UnitDeath2D`, the normal end or an abort destroys it. |
| Death batch | The deaths recorded between two consecutive end checks; simultaneous for the result (`BATTLE-END-01`). |
| End delay | `endDelaySeconds` (1) after the lock during which the final view stays on screen before cleanup (`BATTLE-LOCK-02`). |
| Normal end | The cleanup after a locked result and its end delay: park, remove corpses, route, return, unlock, remove the arena, then `BattleEnded` (`BATTLE-END-04`). Not an abort. |
| Routing | Setting army retreat flags from the result at the normal end; in field routing units never change roster (`BATTLE-ROUTE-01`). Siege routing applies the siege outcome table, including the capture on an attacker win (`BATTLE-SIEGE-02`). |
| Return spot | One of two separated world-map placements for a battle's armies, searched before the start and applied at the end or abort (`BATTLE-RETURN-01`). |
| Pair immunity / army–settlement immunity | A map-time window during which two armies, or since I18 an army and a settlement, cannot become a battle candidate against each other; they still bounce, the armies can fight others, and a capture is not blocked (`WM-IMMUNE-01`). |
| Quarantined unit | A living unit that could not be parked: deactivated in place, still owned with its status unchanged, re-parked by a retry every manager step (`BATTLE-UNIT-04`). |
| Pending end | A normal end whose arena teardown did not complete; retried each idle manager step, with `BattleEnded` raised late (`BATTLE-RECOVER-01`). |
| Orphan arena | An arena an abort could not remove because it was busy; removed at the next idle manager step (`BATTLE-RECOVER-01`). |
| Retreat-eligible unit | A participant whose once-per-battle roll at commit succeeded (`escapeChance`, 0.4); settlement-only units never are (`BATTLE-RETREAT-01`). |
| Retreat latch | `BattleUnit.RetreatTriggered`: set once an eligible field unit's HP is below `escapeHealthFraction` (0.3) of its maximum, from `HealthChanged` or the deployment check; never cleared (`BATTLE-RETREAT-02`). |
| Retreating unit | A field unit whose retreat has started: status Escaping, Retreat override toward the nearest gate, no Sidestep, otherwise fighting normally (`BATTLE-RETREAT-03`, `BATTLE-RETREAT-04`). Not the army flag. |
| Modifier delivery | A battle request to apply a copied modifier list to one field unit once it has started, with the unit as its own caster at its current level; one per kind (Retreat, BattleStart) per unit per battle (`BATTLE-DELIVER-01`). |
| Battle-start modifier list | A `BattleRules2D` list applied once to each unit of a battle after it has started: the field list on both sides of a field battle; in a siege, the town or castle defender list on the defender side only. Frozen onto the sides at commit (`BATTLE-DELIVER-03`). |
| Retreating army / recovery | An army with `Army2D.IsRetreating` (white flag): it bounces off enemies and follows the allied table. Recovery clears the flag `RetreatRecoverySeconds` (20) of map time after it was set (`WM-RETREAT-01`). |
| Pending retreat roster | `PendingRetreatRoster2D` / `UnitRosterKind.PendingRetreat` (I19): the third owner kind. It holds escaped defenders of a captured settlement, parked, with their faction, until they are placed as retreating armies of at most `MaxUnitsPerArmy`; unbounded, never fights, never takes settlement-only units, never exchanges units with another pending roster (`WM-PENDING-01`). |

## Shared configuration cards

### UnitDefinition2D [B, WM2, WM6, WM9]
Owns: authored unit/progression/movement/Sidestep/reactive values, raw level scaling, the world-map `settlementOnly` flag, and `healthRegenFraction` (I14: fraction of the current effective maximum healed per regeneration tick; range 0–1, default 0.02, clamped in OnValidate; 0 disables regeneration for the unit).  
API: level-value getters and read-only configuration properties, including `StartingUnitLevel`, `SettlementOnly` and `HealthRegenFraction` (serialized; safe to read on parked, never-awakened units, `CORE-03`); consult source for formulas/defaults.  
Events: none documented.  
Invariants: `INIT-01`; `CORE-01`; consuming subsystem rules; `SettlementOnly` is read through `UnitHandle` and rosters (`WM-UNIT-02`, `WM-ROSTER-01`), and since Phase 9 by siege spawning, retreat and capture (`BATTLE-SPAWN-02`, `BATTLE-RETREAT-01`, `BATTLE-RETREAT-03`, `WM-CAPTURE-01`); `HealthRegenFraction` is read by the regeneration tick (`WM-REGEN-02`, `HEALTH-05`).  
Never owns: runtime health/XP, movement, timers, effective stats or transaction state.

### ClashRules2D [B,K]
Owns: targeting interval, global Clash cooldown setting, calculation parameters, Vulnerable multipliers and outcome effects.  
API: CreateCalculationSettings; read-only configuration.  
Events: none documented.  
Invariants: `SETUP-01`; `CALC-01`, `CALC-04` in Clash.  
Never owns: current target, per-unit cooldown/Vulnerable expiry or transaction execution.
