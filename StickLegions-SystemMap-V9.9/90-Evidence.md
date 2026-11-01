# Evidence appendix — not needed in implementation chats

The V9 restructure read only `StickLegionsSystemMapV8Extended(10).md`; no C# source, scene, prefab or separate detailed ability map was supplied for this task. Tags carry forward that document's evidence and do not upgrade it. Document equivalence checks are in `92-Validation.md`. World-map and battle tags have their own basis, defined in the section after the original statements.

| Tag | Inherited basis |
| --- | --- |
| V8 | Direct source inspection reported by the V8 trigger update for the twelve named files below |
| V7 | Earlier direct ability integration inspection; unchanged files retain that basis |
| SID | All seven Sidestep partials inspected for the later continuous in-band retry update |
| V6 | Earlier direct inspection reported for UI (and superseded Sidestep partial evidence) |
| F | Reported modifier-framework/integration findings; bodies not freshly verified in V8 |
| B | Retained baseline evidence, not a fresh source inspection claim |
| K | Later Knockback-direction change reported, not compiled/run |

Combined tags identify mixed provenance. They are not ratings of correctness. Unsupported implementation areas are identified by Core `CORE-02`/`CORE-03`; they have no fabricated verified cards.

## Original evidence statement (preserved)

The V8 modifier-triggered ability update directly inspected the final corrected `ClashDirector2D`, `UnitClashParticipant2D`, `UnitModifierRuntime2D`, `ModifierDefinition2D`, `ModifierFrameworkTypes2D`, `ModifierValueResolver2D`, `UnitAbilityHolder2D`, `AbilityExecution2D`, `AbilityFrameworkTypes2D`, `AbilityValueResolver2D`, and the new `ModifierAbilityTriggerTypes2D` and `ModifierAbilityTriggerDispatcher2D`, all `.cs`, together with the implementation handoffs, which reported no unresolved defects. These supersede earlier evidence for those twelve files.

The V7 ability update directly inspected the ten ability files and `UnitCombatant2D`. `AbilityDefinition2D(9)`, `AbilityDelivery2D(5)`, `AbilityAreaQuery2D(4)`, `AbilityProjectile2D(4)`, `AbilityVisuals2D(4)`, `AbilityDebugGrant2D(8)` and `UnitCombatant2D(20260926-075341)` retain that evidence; V8 read their interfaces only through the inspected callers. Inventory names below omit upload suffixes.

The retained modifier integration evidence comes from `ModifierFramework-MapUpdateFindings-Chat1(2).md` and `ModifierFramework-MapUpdateFindings-Chat2(1).md`, covering nine framework and nine integration scripts. Those findings established effective acceleration/base damage, protected pair capture, captured one-Clash consumption, factual damage events, XP deferral/finalization and the retained timed-handle reactive exception. Direct inspection supersedes that evidence only for the files named above; the other integration bodies (scheduler, stat controller, reactive boost, health, XP) were not freshly verified.

The Sidestep in-band retry update directly inspected all seven `UnitSidestep2D` partial files and changed two of them: `UnitSidestep2D.Eligibility.cs` (band-crossing history replaced by continuous in-band eligibility) and `UnitSidestep2D.cs` (coordinator call renamed). This supersedes earlier Sidestep evidence for all seven partials. `UnitDefinition2D`, `UnitMotor2D` and `UnitClashParticipant2D` were not changed.

The prior V6 update directly inspected `UnitSidestep2D.Clash(3).cs`, `UnitSidestep2D.Lifecycle(7).cs` and `UnitStatsUI2D.cs`. Their retained entries inherit that earlier evidence. Other baseline formulas, progression, targeting, movement and helper internals retain V6's evidence rather than implying fresh inspection.

No compilation, Unity execution, prefab/asset inspection, project execution-order override inspection or performance test was performed for the V8 documentation update or the Sidestep in-band retry update. Apart from the two Sidestep partials named above, no C# code was changed. Implementation statements below inherit these evidence boundaries.

The later Knockback-direction update changed `ClashRules2D`, `CombatTypes2D`, `ClashCalculator2D` and `ClashDirector2D` to add a random Knockback angle. It was not compiled or run in Unity.

## Original remaining gaps (preserved)

The section references in this quoted historical material refer to the original V8 document, not the V9 file layout.

- UI code is documented, but display assignments are uninspected.
- All seven Sidestep partials were directly inspected for the in-band retry update, including path invalidation and the retry-cooldown helper. The changed eligibility behavior has not been compiled or checked in Unity Play Mode.
- Unrelated baseline code, actual prefab/asset assignments and execution-order overrides were not reverified. Findings-based framework/integration statements are distinguished from direct script inspection in Scope. No unresolved contradiction between the two findings documents remains; unresolved evidence is not treated as verified behavior.
- Ability ownership, delivery, payload and receiving-side modifier contracts were directly inspected in the supplied versions. Health/XP internals, targeting/detector bodies, scheduler internals and Dodge helper implementation retain earlier evidence; the inspected callers establish their use, not a fresh verification of those bodies.
- Ability prefab setup, Area collider coverage, collision layers, scene assignments and execution-order overrides remain unverified. Source inspection establishes buffer reuse and allocation locations, not measured performance.
- Modifier-triggered casting was verified by source inspection and the implementation handoffs only; no Unity Play Mode, lifecycle or physics check is implied. The handoffs record two deliberate diagnostic choices beyond the original roadmap: unexpected exceptions log as errors, and a faulted transaction warns `ClashTransactionFaulted` per matching opportunity (§2.9).
- The current modifier resolver confirms the supported-mode table in §3, including the `CasterLevel` restriction for `DamagePerTick` and `DodgeChance`. This is an implementation boundary, not an unresolved evidence gap.

## World-map and battle evidence (Roadmap V6)

Each map update for a Roadmap V6 phase (M01–M09) was written from the phase's attached source, which is the authority for behaviour. Verification comes only from the Unity results the user reported in that phase's handoff, which is the authority for verification. No map update ran Unity, compiled code or inspected scenes, prefabs or project settings. Unity coverage is credited only to results the handoff names; where a handoff reported a group of tests as passed without listing them (for example "I06 regressions"), coverage is attributed to that group and not to individual clauses.

| Tag | Basis |
| --- | --- |
| WM1 | Phase 1 source (I01/I02), covered by I01/I02 Unity compilation, Play Mode and acceptance results |
| WM1-SR | Phase 1 change that was source-reviewed only, not Unity-verified when written. Its one use (the freeze-during-dispatch fix) was later covered by an I05 check; see `WM-CONTACT-05` |
| WM2 | Phase 2 source (I03–I05), covered by a named I03–I05 Unity result or the Phase 2 acceptance list (the ownership audit passed in every run) |
| WM2-S | Phase 2 source only; not separately tested in Unity |
| WM3 | Phase 3 source (I06–I07), covered by a named I06/I07 Unity result. The I07 results were run in both perimeter modes |
| WM3-S | Phase 3 source only; not separately tested in Unity |
| WM4 | Phase 4 source (I08–I10), covered by a named Unity result in the I08, I09 or I10 reports |
| WM4-S | Phase 4 source only; not separately tested in Unity |
| WM4-SR | Phase 4 path that the I10 handoff lists as source-reviewed only; no Unity run |
| WM5 | Phase 5 source (I11–I13), covered by a named Unity result in the I11, I12 or I13 reports. The bracket names the chat, because only I13 ran on the final source |
| WM5-S | Phase 5 source only; not separately tested in Unity |
| WM5-SR | Phase 5 path that the I13 handoff lists as "untested, source review only"; no Unity run |
| WM6 | Phase 6 source (I14), covered by a named Unity result in the I14 handoff, or by the I14 Regen-tracking check that the I15 handoff reports passed. I14 ran on the final source |
| WM6-S | Phase 6 source only; not separately tested in Unity. Includes the paths the I14 handoff lists as "source only, untested" |
| WM7 | Phase 7 source (I15–I16), covered by a named Unity result in the I15 or I16 handoff, or by the Phase 7 closing check in `92-Validation.md`, which the user reported passed after M07. The bracket names the run |
| WM7-S | Phase 7 source only; not separately tested in Unity. Includes the paths the I15 and I16 handoffs list as "written but not exercised" |
| WM8 | Phase 8 source (I17), covered by a Unity result the I17 handoff reports for a Phase 8 acceptance item. The bracket quotes what the handoff observed |
| WM8-S | Phase 8 source only; not separately tested in Unity. Includes the paths the I17 handoff lists as "untested, source only" |
| WM9 | Phase 9 source (I18–I19), covered by a Unity result the I18 or I19 handoff reports. The bracket names the run: I18 checks 1–9 or I19 tests 1–9, numbered in the handoffs' order (Phase 9 basis). I18 results are credited only to paths I19 did not change |
| WM9-S | Phase 9 source only; not separately tested in Unity. Includes the paths the I18 and I19 handoffs list as untested or known gaps |

Bracket convention: `[WM3: I07 Retreat 5]` names the covering result; clauses of the same rule outside the bracket are source-derived. Combined brackets such as `[WM1, WM2]` on a card mean the card's contents come from both phases.

### Phase 3 basis (M03)

Source read: `BattleArena2D.cs`, `BattleArena2D.Perimeter.cs`, `BattleArena2D.Bodies.cs`, `BattleArenaTypes2D.cs`, `BattleLayers2D.cs`, `BattleArenaGate2D.cs`, `BattleCamera2D.cs`, `BattleRules2D.cs`, `BattleTestBody2D.cs`, `UnitDestinationOverride2D.cs`, `BattleArenaDebugPanel2D.cs`. World-map interfaces used by the arena (`WorldMapController2D` readiness, freezing, bounds, `LastSelectedBattle`; `WorldMapLayers2D`) were taken from the I07 handoff's signatures, not from re-attached source.

Reported Unity results (I07 handoff):
- everything compiles;
- "I06 regressions" passed. The handoff does not list their individual checks; its Works list attributes arena create/remove, terrain selection, camera swap, freeze ownership and the audit to I06, so those features are credited to that group only. View size from counts is not named among them; its count-following is credited only through the group;
- I07, in both perimeter modes: entry and containment; knockback at 40/80/150 u/s with 0 leaks; shrink-to-min while pushing outward with 0 leaks; eject → walk back in; Retreat 5 → 5 escapes and the single-gate test; resizing the Game view mid-test; 20 create/remove cycles with bodies, audit passing;
- the post-review fix (resize counted as re-entry; knockback hits Inside bodies only) was covered by those retests.

Not tested in Unity (WM3-S or untested):
- the I06 `frozenMap` fix (rollback when a `FrozenChanged` subscriber throws) and `ThrowOnFreezeTest`. That test hook does not appear in the attached Battle files and its result was not reported;
- disabling or destroying the battle camera or rig mid-arena; a scene unload while an arena exists;
- second `BattleArena2D` instance, missing camera rig, missing Wall layer;
- retreating bodies escaping while Entering; escapes after `EscapesAccepted` is closed; lost-body release;
- `UnitDestinationOverride2D.Changed` delivery and the Debug reason;
- real units and any combat integration.

### Phase 4 basis (M04)

Source read in full: `BattleManager2D.cs`, `BattleManager2D.Spawning.cs`, `BattleTypes2D.cs`, `BattleSides2D.cs`, `UnitSideBinder2D.cs`, `UnitSideBindingTest2D.cs`, `BattleRules2D.cs`, `BattleManagerDebugPanel2D.cs`, `UnitReserve2D.cs`, `WorldMapController2D.Interactions.cs`, `WorldMapController2D.Contacts.cs`, `WorldMapDebugPanel2D.cs`, `UnitStatsUI2D.cs`, `UnitMotor2D.cs`, `UnitSidestep2D.cs`, `UnitSidestep2D.Eligibility.cs`, `UnitSidestep2D.Interruptions.cs`, `UnitSidestep2D.WallClearance.cs`, `UnitReactiveTurnBoost2D.cs`, `UnitTargeting2D.cs`, `UnitClashDetector2D.cs`, `UnitAbilityHolder2D.cs`.

Not re-attached; taken from the I10 handoff's signatures or earlier map rules: the `BattleArena2D` partials (`TryCreate`, `Remove`, `TryRegisterBody`, `UnregisterBody`, `TryFindEntryPoint`); `BattleLayers2D.cs` and `UnitDestinationOverride2D.cs` (the I09 handoff reports comment-only changes); `UnitDeath2D.cs` (the `Died` timing comes from `DEATH-01`); `UnitHandle.cs`; `UnitRosterTypes2D.cs`; `WorldMapController2D.cs` and `.Units.cs`; the Sidestep `.Lifecycle`, `.Path` and `.Clash` partials. The I08 and I09 results come from the user's summary of those handoffs, not the full handoffs.

Reported Unity results:
- **I08:** the renamed layers keep their indices (11 `Clash_SideA`, 12 `Clash_SideB`, 15 `Unit_SideA`, 16 `Unit_SideB`); the rules asset strings were updated, and arena create/remove plus test bodies still work; `UnitSideBindingTest2D` passed (a never-awakened clone bound A, then B, then A; root and Clash layers correct; targeting, detector and ability masks correct; visual and UI layers untouched; faction unchanged; nothing woke; an active unit refused; a rebind after Awake refreshes the live targeting filter); the side matrix report is clean; the negative asset-string check behaved as expected.
- **I09** (with a temporary spawner, since deleted): compiles; real units walk in from their edges and fight, in both perimeter modes; "Sidestep under override" stays 0 while Sidesteps occur once units are inside; Retreat N steers units to the gates and they escape with 0 lost; shrinking during a fight causes 0 leaks; the audit passes after deaths and after removing the arena mid-fight, with the map unfrozen and one camera enabled; I07 test bodies and the non-battle combat scene behave as before.
- **I10:** compiles; battles start from the selected FieldBattle pick; units keep their faction and get the right side layers and masks; 80 vs 20 spawns at 6/s vs 3/s, the first unit immediately, with no overlaps and no burst after Hold; no Sidestep before entering, and ejected units walk back in; a failed preparation restores the map and camera; the stats UI refreshes on re-enable; only one battle runs per session; abort (audits pass, map unfrozen, one camera); the Freeze lock during a battle; the in-battle transfer refusal; the new contact log text. Known answers reported for I09 and I10: probe 0, lost 0, leaks 0, 6/s and 3/s, about 13.2 s for 80 units.

Attribution limits:
- The I09 checks ran with the deleted spawner, not `BattleManager2D`. They are credited to the motor, Sidestep, arena and reserve behaviour they exercised. Manager paths are credited only to I10 results.
- I10's "audits pass" is credited to the abort case. Death ownership release under `BattleManager2D` has no separately named audit.
- "No burst after Hold" covers the simulated block (`DebugHold`). A real "entry strip full" block shares the same credit handling but was not separately named.
- The in-battle transfer refusal does not say which gate refused (map freeze or `IsRosterInBattle`).
- The enemy search reach of 24 is a user-reported asset change; the map inspects no assets.
- "View size follows counts" still has no named Unity check (Phase 3 carry-over).

Source-reviewed only (`[WM4-SR]`, as listed by the I10 handoff): the spawn exception paths; the lost-unit paths; corpses when destruction on death is off; "perpendicular edges at large views" (recorded as named; the handoff does not say more).

Source only (`[WM4-S]`) or untested: edge-choice arithmetic and near-tie randomness; refusal order and the global retry wait; the preparation exception path; a `BattleStarted` handler that aborts; the manager disabled mid-battle; a second manager instance; the other drop cases (missing components, trigger circle, binding, reset or registration refusal); a unit already dead at spawn; death release itself; the abort's Escaped → Reserve step and its destroyed-unit path; `MOVE-04` refusals; `Perimeter` in Sidestep preflight and contact cancellation and in the reactive boost; live `CurrentLevel` reads after deployment; a second battle after an abort (refused by the session limit).

### Phase 5 basis (M05)

Source read in full: `BattleManager2D.cs`, `BattleManager2D.Start.cs`, `BattleManager2D.Spawning.cs`, `BattleManager2D.Ending.cs`, `BattleManager2D.Units.cs`, `BattleManager2D.Routing.cs`, `BattleManager2D.Debug.cs`, `BattleTypes2D.cs`, `BattleRules2D.cs`, `WorldMapController2D.Contacts.cs`, `WorldMapController2D.Separation.cs`, `Army2D.cs`, `CombatLock2D.cs`, `UnitHealth2D.cs`, `UnitAbilityHolder2D.cs`, `AbilityProjectile2D.cs`, `UnitReactiveTurnBoost2D.cs`, `ClashDirector2D.cs`. The `CLASH-05` lock gate is in `ClashDirector2D` (the I13 handoff did not know which file I11 changed). Whether I11 also changed `UnitClashParticipant2D.cs` is not established; it was not attached.

Not re-attached; taken from the I13 handoff's signatures or earlier map rules: `WorldMapController2D.cs` (`DestroyArmy`, `SetFrozen`, `AuditUnits`) and its other partials; `UnitReserve2D.cs` (`ReturnToReserve`, `ReleaseOwnership`, `TryMarkDeploying`); the `BattleArena2D` partials (`Remove`, `EscapesAccepted`, `UnregisterBody`, `AuditArena`); `BattleCamera2D.cs`; `UnitMotor2D.cs` (`SetDisabled`, `TryPrepareForDeployment`); `UnitDeath2D.cs`; `UnitHandle.cs`; `WorldMapMath2D.cs`; `BattleManagerDebugPanel2D.cs`; the map files `01-Movement-Sidestep.md` and `06-UI.md`. The motor's behaviour when Disabled at the lock is taken from the manager's source comment and existing rules (`MOVE-03`, `SIDESTEP-LIFE-02`), not from motor source.

Reported Unity results. The user's I13 handoff (written for M05) lists them; the user's own note adds the I11 lock-test result:
- **I11** (on the I11 source): the result-lock test passed with no deaths, Clashes, casts or trigger warnings and no projectiles in flight (the user's evidence correction, reported by the I11 handoff; this resolves the I13 handoff's missing Clash and cast evidence and is not a rerun on the final source); the damage-over-time gate after the lock (blocked damage rose, no deaths after the lock); a blocked spawn that stalled until a configured timeout and then cleaned up normally.
- **I12, before its review fixes and file split:** normal 80 vs 20 end and cleanup; forced timeout with units queued; survivor identity, ownership, HP, XP and level kept; mutual elimination with corpse cleanup; escape cleanup; abort while fighting and during the locked delay; camera, map, audit and cleanup-counter checks.
- **I12, after the split:** compiled; Inspector and context menus kept; the 80 vs 20 and locked-delay-abort regressions passed. The simulated incomplete end then needed manual arena removal and the quarantine manual handling; both are automatic since I13.
- **I13, on the final source after both review fixes:** (1) compiles, saved values kept, the three I13 context menus appear; (2) decisive 80 vs 20: losing army removed, winner stays ordinary, keeps every unit including queued ones and reappears just apart from the battle spot, `BattleEnded` once, audit passes, HP/XP/level kept; (3) timeout with units queued: nobody dies, both armies keep every unit and show the white flag, placed apart, no new battle between them, audit passes; (4) mutual last kills: BothLose, empty armies removed, map unfrozen, one camera, audit passes; (5) abort right after the start, while fighting and during the locked delay: units returned, map unfrozen, one camera, armies apart with no white flag, no immediate new battle, audit passes; (6) immunity: the pair cannot battle for about 1 s of map time and bounces instead, each army can still fight a third army; (7) recovery: "Skip arena removal at next end" recovers at the next step with `BattleEnded` once, "Fail next unit park" re-parks on retry with `QuarantinedCount` 0; (8) 20 consecutive battles with mixed ends and aborts: ended + aborted = 20, audits failed 0, lost 0, cleanup problems 0, quarantined 0, deaths/escapes after lock 0/0; (9) no visible hitch at battle end, with `auditAfterEachBattle` off.

Attribution limits:
- I11 and I12 results are credited to the behaviour they exercised as it was then. Where the final source changed that behaviour (the I12 review fixes and split, I13's routing, recovery and quarantine retry), the clause is credited only to I13 or marked source.
- No I13 test names a Clash or cast refused after the lock; that sub-item rests on the I11 lock test.
- The incomplete-end and quarantine paths were reached through debug switches (`DebugSkipArenaRemovalAtNextEnd`, `DebugFailNextPark`), not through a real refusal or exception.
- "Abort works at every stage" covers the preparation failure (I10), the start, fighting and the locked delay (I12, I13). Aborts during the normal end are refused by design.
- `UnitReactiveTurnBoost2D.ClearOwnedModifiers` has no named result.
- The post-battle audit's projectile check counts every projectile in flight, not only battle ones.

Source-reviewed only (`[WM5-SR]`, as listed by the I13 handoff): the refusal for "no return room" before a battle starts (not set up); recovery from OnDisable while an end is pending; removing an orphan arena (from source this cannot actually happen); an abort caused by an outside unfreeze, which runs `DestroyArmy` from inside `FrozenChanged` (not verified, since `WorldMapController2D.cs` was not attached; *update M09:* that file, read at M09, raises `FrozenChanged` after its army loop, so a removal in a handler does not disturb the loop; still untested); a real refusal or exception in arena teardown.

Source only (`[WM5-S]`) or untested: the effect order inside the lock and the ungated paths (ForceDeath, clamp deaths, modifier application); the end-delay arithmetic; where Update-driven deaths land; the stale-pick case in battle selection; the return-spot search geometry and the end-time no-room fallback; extend-not-shorten, pruning and refusals of pair immunity; the destroyed-quarantine and reactive-clear paths; escape handling on the final source; refusal order in `TryStartFieldBattle`.

### Phase 6 basis (M06)

Source read in full (I14's new and changed files): `UnitRegeneration2D.cs`, `WorldMapController2D.Regeneration.cs`, `UnitHealth2D.cs`, `UnitDefinition2D.cs`, `UnitHandle.cs`, `WorldMapRules2D.cs`, `WorldMapController2D.cs`.

Not attached. These are taken from the I14 handoff's signatures or earlier map rules: `UnitReserve2D.Units` (owned living units; a unit leaves at death through `ReleaseOwnership`); `UnitRuntimeStats2D.EffectiveMaximumHealth` (reconciles modifier expiry first, may publish ModifiersChanged, usable on an inactive awakened unit); `UnitStatModifierController2D.ReconcileForRead`; `UnitDeath2D.HasDied` (set before `Died`, never reset); `CombatLock2D.IsLocked`; `UnitStatsUI2D` (full refresh on every re-enable after Start, `UI-01`). The handoff lists `UnitReserve2D`, `UnitStatsUI2D`, `UnitRuntimeStats2D`, `UnitStatModifierController2D`, `UnitDeath2D` and `UnitRoster` as unchanged; `CombatLock2D` is not in that list, only covered by signature. The handoff's on-request files were not requested: `UnitModifierRuntime2D.cs` (what `ReconcileExpiredStats` does on an inactive unit), `BattleManager2D.Units.cs` (whether parking or quarantine ever disables a unit's combatant, runtime stats or death component, which would refuse parked heals) and `UnitRosterTypes2D.cs`. Those questions stay open in source terms. *Update M09:* `BattleManager2D.Units.cs` and `UnitRosterTypes2D.cs` were read at M09. Parking (`ReturnToReserve`) and quarantine only deactivate the GameObject and clear the reactive turn boost's handles; neither disables a component, so the parking question is answered in source terms. The `UnitModifierRuntime2D.cs` question is not revisited here. I14 test 2 shows that a unit parked after deployment (the Regen menu refuses never-activated units; whether after a normal end or an abort is not stated) keeps those components enabled in practice. Quarantine is not covered.

Reported Unity results (I14 handoff, all on the final source):
1. compiles, and the "Regen" context menus appear;
2. a parked unit at 50% is full after exactly 25 ticks, about 2 s apart, and stays inactive;
3. deployed units heal, and ticks continue while the map is frozen and during the locked delay;
4. never-activated units are skipped, and the audit still shows 0 awakened components;
5. the health bar is correct after redeployment;
6. Phase 5 still holds over consecutive battles with mixed ends and aborts;
- extra: a deployed unit reached 100% at tick 25 inside a long locked delay, so the lock does not block healing.

Attribution limits:
- **Dead units skipped.** No I14 result named it, so M06 did not mark Phase 6 complete. *Update M07:* the I15 handoff reports that the I14 Regen-tracking check (the closing check M06 proposed) passed with the I15 run. That closes the item, and Phase 6 is complete (`92-Validation.md`). The run was on the I15 source, which does not change the regeneration files.
- **Test 6** is credited as a Phase 5 regression group. Its individual checks are not listed.
- **Test 3** names deployed healing and continued ticking while frozen and locked, but no tick count. The 25-tick count for a deployed unit comes from the extra check, which ran inside a locked delay.
- **Test 2** reports the parked case at exactly 25 ticks. Whether the 25th heal ended at the cap or through the 1e-4 snap is not distinguishable from the result.
- **Test 5** names the health bar after redeployment. The I14 handoff does not say how much HP regeneration had restored before it.

Source only (`[WM6-S]`) or untested: the per-unit exception path (Failed); the inactive-unit refusal instead of a clamp death; a zero `healthRegenFraction` (counted Ineligible each tick); the `didAwake` guard for a unit marked activated that never ran Awake; leftover time reset after a step longer than the interval or a runtime-lowered interval (with `Time.fixedDeltaTime` as the input, a frame hitch adds steps rather than lengthening one); Escaped and quarantined units healing by name; a player build without the debug setter; the active-path clamp death; the accumulation tolerance arithmetic; the effective-maximum notification on an inactive unit (handoff signature).

### Phase 7 basis (M07)

Source read in full. I15 (unit retreat): `BattleManager2D.cs`, `.Start.cs`, `.Spawning.cs`, `.Units.cs`, `.Ending.cs`, `.Debug.cs`, the new `.Retreat.cs` and `.Delivery.cs`, `BattleTypes2D.cs`, `BattleRules2D.cs`, `UnitReserve2D.cs`. I16 (retreating armies): the new `WorldMapController2D.Retreat.cs`, `WorldMapController2D.cs`, `.Interactions.cs`, `Army2D.cs`, `WorldMapRules2D.cs`, `WorldMapDebugPanel2D.cs`, `BattleManager2D.Routing.cs`. Map files `00`, `01`, `03`, `04`, `05`, `07`, `90`, `91`, `92`; Roadmap V6.

Not attached. These are taken from the handoffs' signatures, the callers in the attached source or earlier map rules: `UnitModifierRuntime2D.cs` (`ApplyOrdered`, `CanReceiveModifiers`, the OnDisable cleanup, `MOD-REMOVE-01`); `UnitHealth2D.cs` (`HealthChanged` timing, `DebugSetHealthFraction`); `BattleArena2D` partials (`SetBodyRetreating`, gate escapes, `EscapesAccepted`); `UnitDestinationOverride2D.cs`; the Sidestep partials and `UnitMotor2D.cs` (override behaviour, `01`); `UnitAbilityHolder2D.cs` (`didStart`, read by the delivery wait); `UnitHandle.cs`; `UnitRoster.cs`; `UnitRosterTypes2D.cs`; `WorldMapController2D.Contacts.cs`, `.Separation.cs`, `.Units.cs`, `.Deployment.cs`, `.Regeneration.cs`; `Settlement2D.cs`; `BattleManagerDebugPanel2D.cs`. The handoffs offered `Contacts`, `Settlement2D`, `UnitRoster`, `UnitModifierRuntime2D` and `UnitHealth2D` on request; they were not requested, and the rules that touch them say what rests on signatures.

Reported Unity results.
- **I15** (on the I15 source; I16 changed none of the I15 files, and its only battle-manager change was a comment and the return summary in `BattleManager2D.Routing.cs`): (1) compiles; (2) natural retreat, gate escape and escape counts; (3) the 30% threshold: 31% doesn't retreat, 20% does, ineligible units don't; (4) healing never cancels a retreat; (5) the retreat modifier is applied once and removed when the unit leaves; (6) the deployment check; (7) no escape after the lock, and an abort while units are retreating; (8) Phase 5 and 6 regressions; plus the I14 Regen-tracking check (Phase 6).
- **I16** (on the final source): (1) compiles; (2) a decisive loser recovers 20 map s after the unfreeze; the timer pauses during the end delay; (3) after a timeout both armies recover in the same step; (4) a merge receiver keeps its own timer, including a partial merge where the donor keeps its leftover units and its own timer; (5) an army absorbed by a garrison, or emptied into an ordinary ally, never recovers; (6) freezing pauses the timer; Flag-, Flag+ and T work; (7) a recovered army can battle again; (8) Phase 5 and 6 regressions.

Attribution limits:
- **Retreating units keep fighting.** No I15 or I16 result names it. From source the retreat path touches no Clash, Dodge, casting or targeting code (`BATTLE-RETREAT-04`). M07 therefore did not mark Phase 7 complete. *Update V9.7.1:* covered by the Phase 7 closing check, step 3.
- **Escaped units never respawn that battle.** No I15 or I16 result names it. From source the spawn queue is a start snapshot that only moves forward, and an Escaped unit is not parked, so the spawn path would drop it anyway (`BATTLE-UNIT-02`). Phase 4 B16 also rested on source. Not marked complete at M07 for the same reason. *Update V9.7.1:* covered by the Phase 7 closing check, step 4.
- **Phase 7 closing check** (reported after M07). The user reported that the tests went well and that everything supposed to work works in Unity. M07 had asked only for the closing check in `92-Validation.md`, so the report is credited to that check's four steps as a group: the individual observations (Clashes, Dodges, casts, targeting, probe 0, the count identity) were not listed one by one. It was run after I16; the report does not state the source version.
- **"The timer pauses during the end delay"** (I16 test 2) is recorded as reported. From source, routing sets the flag only after the end delay, while the map is still frozen, so the timer does not exist during the delay; what the run shows is that the time between the lock and the unfreeze does not count.
- **"Natural retreat"** is credited to the latch, the start and gate steering as exercised together; the handoff does not list what was observed.
- **"Healing never cancels a retreat"**: the handoff does not say whether regeneration or the debug heal raised the HP.
- **"Phase 5 and 6 regressions"** (I15 test 8, I16 test 8) are credited as groups. Their individual checks are not listed, so no Phase 7 clause rests on them alone.
- The I15 and I16 handoffs both say Phase 7 is complete once both passed. M07 did not mark it complete because of the two items above, following M06's handling of an acceptance item with no named result. With the closing check reported passed, V9.7.1 marks Phase 7 complete.

Source only (`[WM7-S]`) or untested. From I15 "written but not exercised": an Escaping status refusal; a refused retreat steering; a missing modifier runtime; a quarantined Escaping unit; modifier entries rejected at application. From I16: the in-battle guard in recovery; the stop-on-freeze break inside the recovery loop; `retreatRecoverySeconds` 0; recovery while an end's teardown is pending; the old 10-argument `AlliedInteractionRecord` constructor. Also: the roll arithmetic and the settlement-only skip; health at exactly the threshold; latches set from inside a Clash transaction and the one-step start lag; the delivery wait conditions and the frame-rate dependence of its timing; delivery drops at the lock, end and abort with a request still pending (*update M08:* the lock and abort drops, and the drop on death, are covered by I17 with the delivery hold, `[WM8]`; the normal-end drop stays source-only and finds nothing in practice, `BATTLE-DELIVER-02`); `UnitRetreated` timing; a retreating unit escaping while still Entering; a retreating unit with refused steering (no override, so nothing stops it Sidestepping); the recovery tolerance arithmetic and step count; recovery after allied interactions in the same step.

### Phase 8 basis (M08)

Source read in full (I17's changed files): `BattleManager2D.cs`, `.Start.cs`, `.Spawning.cs`, `.Delivery.cs`, `.Debug.cs`, `BattleTypes2D.cs`, `BattleRules2D.cs`, `BattleManagerDebugPanel2D.cs`. Also read in full: `UnitModifierRuntime2D.cs`, one of the handoff's on-request files, which the user supplied at the start of M08. It was used to confirm three things: OnDisable, `Died` and a `HealthChanged` to zero or below call `ClearAll`; what `CanReceiveModifiers` checks; and that `ApplyOrdered` leaves unreached entries NotProcessed if it throws. Map files `00`, `04`, `05`, `07`, `90`, `91`, `92`; Roadmap V6.

Not attached. These are taken from the I17 handoff's signatures, the callers in the attached source or earlier map rules: `ModifierFrameworkTypes2D.cs` (the default result is NotProcessed); `UnitAbilityHolder2D.cs` (`didStart`); `UnitHandle.cs` (`CurrentLevel`); `UnitReserve2D.cs` (`ReturnToReserve`); `BattleManager2D.Retreat.cs`, `.Units.cs` and `.Ending.cs` (`RollRetreatEligibility`, `Detach`, `LockResult`, `FinishBattle`, `HandleAbort`, each dropping requests as the handoff states); `Settlement2D.cs`, `WorldMapTypes2D.cs` (`SettlementKind`) and `BattleSides2D.cs` (`SiegeDefenderSide`, documented under `SIDE-01`). The handoff's other on-request files were not requested.

Reported Unity results (I17 handoff). Phase 8 is implemented, compiles and passed in Unity. Each acceptance item passed with the expected results:
1. **Waits until the unit has started.** Every delivery came at least one frame after activation, and the Battle-start audit reported OK.
2. **Uses the unit's current level.** The level at delivery matched the unit's level, and a unit that levelled up got its new level in the next battle.
3. **Follows list order.** The log showed `[+A, +B]`.
4. **Happens once.** Each unit had exactly its list's instances, plus the retreat modifier when it retreated. Per side, requested equals delivered plus dropped.
5. **Never leaks into another battle.** Parked units carried 0 instances after a battle, and a re-fight of the same armies delivered fresh lists.
6. **Drops with requests still pending,** using the delivery hold: on death, at the lock and at an abort. The handoff says this closes the M07 carried item.
7. **Lists are frozen at commit.** Editing the rules mid-battle changed nothing in that battle.

Attribution limits:
- The results are reported per item, with the observations above; no counts, battle sizes or list contents beyond `[+A, +B]` are given. The handoff names no source change after the tests, so they are taken as run on the source read here.
- Item 1 rests on the audit's checks. They cover each delivered unit still on the field when the audit ran: delivery frame after activation, holder `didStart`, level ≥ 1. Units that had already left the field are not checked by it.
- Items 3 and 4 do not say which side's units were inspected. That both sides of a field battle get the same list rests on source.
- Item 6 covers the drops on death, at the lock and at an abort. It closes those clauses of the M07 item. The drop at an escape or by the sweep with a request pending is not named. The normal-end drop cannot hold a request in practice, because the lock drops every request and requests are refused while locked.
- Whether the hold was set from the panel or the context menu is not stated.
- Whether the battle-start list applied before the retreat modifier on units that had both is not stated; item 4 counts instances only.

Source only (`[WM8-S]`) or untested. From the I17 handoff: the siege freeze (no caller before Phase 9; *update M09:* called since I18 and covered by I18 check 6, `[WM9]`); a delivery dropped because the unit has no modifier runtime; a rejected battle-start entry; a destroyed asset in the rejection log; the delivery wait with a disabled ability holder. Also:
- null-entry removal at the freeze, and an empty list (no request, no counters);
- the throw on a second freeze, and that a throw in the commit steps after the arena's creation is outside the preparation's exception handling (`BATTLE-START-03`; *update M09:* I18 moved those steps inside it, so a throw there is now a preparation failure; still untested);
- the defender-kind mapping (`Castle` → castle list, any other kind → town list); *update M09:* the castle list reaching defenders only is Unity through I18 check 6 (`[WM9]`); the town list stays source-only;
- the battle-start-before-retreat queue order;
- an exception inside `ApplyOrdered`;
- the hold persisting across battles;
- the frame-rate dependence of the wait (carried from I15/I16).

### Phase 9 basis (M09)

No map update was written after I18, so M09 covers I18 and I19 together.

**Source read in full.**
- I19's 12 files: `WorldMapController2D.Siege.cs` (new), `WorldMapController2D.cs`, `.Capture.cs`, `.Deployment.cs`, `.Units.cs`, `WorldMapTypes2D.cs`, `Settlement2D.cs`, `UnitRoster.cs`, `UnitRosterTypes2D.cs`, `UnitReserve2D.cs`, `BattleManager2D.Routing.cs`, `BattleTypes2D.cs`.
- I18's other changed files: `WorldMapController2D.Contacts.cs`, `.Interactions.cs`, `.Separation.cs`, `BattleArena2D.cs`, `BattleRules2D.cs`, `BattleManager2D.Start.cs`, `.Spawning.cs`, `.Retreat.cs`, `.Debug.cs`, `BattleManagerDebugPanel2D.cs`.
- Also read: `BattleManager2D.cs`, `.Ending.cs` and `.Units.cs`, so that `IsRosterInBattle` covering a garrison side, the siege cleanup and abort path, the escaped parking and the quarantine retry rest on source.
- Maps `00`, `07`, `90`, `91`, `92`; Roadmap V6; the I18 and I19 handoffs.

**Not attached.** These are taken from the handoffs' signatures, the attached callers or earlier map rules:
- `Army2D.cs`: `SetRetreating`, `PlaceAt`, `IsInitialized`, `IsRemoved`.
- `BattleSides2D.cs`: the siege constants and their use.
- `BattleManager2D.Delivery.cs`: `FreezeSiegeBattleStartModifiers`, documented at M08 and called from `.Start.cs`.
- `BattleArena2D.Perimeter.cs` and `.Bodies.cs`: the gate mask and escapes.
- `UnitRegeneration2D.cs`: pending-roster units healing through the registry, which rests on `WM-REGEN-02` from I14 source.
- `UnitHandle.cs`, `WorldMapContacts2D.cs`, `WorldMapDebugPanel2D.cs`: the handoff states that the panel does not show pending rosters.
- The map files `01`–`06`.

The handoffs' on-request files (`WorldMapDebugPanel2D.cs`, `UnitRegeneration2D.cs`, `Army2D.cs`) were not requested.

**Reported Unity results.** Numbered here in the handoffs' order.
- **I18** (compiled in Unity 6000.3.9f1; on the I18 source, with an interim siege end that applied no outcome):
  1. capture: the faction changes, the units join the garrison, the army is removed and `SettlementCaptured` fires; a pending deployment is cancelled; overflow (200 → town) sends out 100; the unit audit passes;
  2. the defender is Side A for both faction orders;
  3. 80 vs 80 spawns at 6/s (attacker) and 12/s (defender);
  4. authored edges are used whatever the contact direction, and retreat leaves only through the attacker's edge;
  5. settlement-only defenders deploy and never retreat, even when "Retreat N" forces it;
  6. the castle defender list reaches defenders only, and the battle-start audit is OK;
  7. an invalid siege setup starts nothing, and other battles still start;
  8. siege end and abort, running and locked: the post-battle audit passes, the attacker is placed away, and there is no immediate re-siege;
  9. review checks: `SetFaction` is refused mid-siege, and a deployment queued during a siege's end waits and runs later.
- **I19** (compiled in Unity 6000.3.9f1; on the final source; the handoff reports a post-battle audit PASSED with each):
  1. regression: a field battle and an I18 empty-settlement capture behave as before;
  2. defender wins: the settlement is unchanged; the attacker is removed, or retreating with escapees;
  3. BothLose: defender escapees are back in the garrison; the attacker is removed or retreating;
  4. timeout: the attacker keeps every living unit, retreats, and is placed away with army–settlement immunity; the garrison keeps every living defender;
  5. attacker wins: the settlement is captured, the attackers now garrison it, and the army is removed;
  6. attacker wins with escaped defenders: they move to pending retreat roster P1 and are then placed as a retreating army of the old faction;
  7. split and blocked placement:
     - 7a: 450 evacuated units became 200 + 200 + 50 in one step; the hold kept all 450 waiting, and the audit lists the pending roster;
     - 7b: with blockers, one "no room" line, blocked attempts rose and nothing was lost; once the blockers were removed, the armies were placed;
  8. repeated and large: about 5 field battles and 5 sieges, including a castle of 300 against 200 attackers; refused 0, cleanup problems 0, lost 0;
  9. abort: siege Aborted, no flags, everyone back with their own side.
- The I19 handoff states that earlier phases' checks plus these results meet every Phase 9 acceptance item, and that M09 can record Phase 9 as complete.

**Attribution limits.**
- **I18 checks 1–7 and 9** ran on the I18 source. I19 did not change `.Start.cs`, `.Spawning.cs`, `.Retreat.cs`, `BattleRules2D`, `BattleArena2D` or `.Separation.cs`, and its `BattleTypes2D` change added outcome fields only, so the siege-start clauses are credited to I18. Check 1's capture path was changed by I19 (record fields only), and I19 test 1 covers it on the final source. Check 9 involves `Settlement2D.cs` and `.Deployment.cs`; I19 changed both but kept the guards the checks exercised.
- **I18 check 8** covered the interim siege end and abort, which applied no outcome and which I19 replaced. Only the placement away from the settlement and the army–settlement immunity carry over: they use the same `.Separation.cs` calls. No outcome clause rests on it.
- **I19 tests 7a and 7b** used the debug evacuation (`DebugEvacuateToPendingRetreat`), not a siege capture. The capacity split and blocked placement run through the same placement code a siege capture uses. A siege leaving more than 200 escaped defenders was not named.
- **I19 tests 2 and 3** name the attacker's flag or removal. Its placement away and immunity after a defender win or a BothLose are not named; test 4 names them for a timeout, and test 9 for an abort ("everyone back with their own side", with no mention of placement).
- **"Refused 0"** in test 8 is quoted as reported. The handoff does not say which counter it read.
- **The acceptance item "repeated field/siege battles and large garrisons work"** rests on test 8, which ran about 10 battles. It is not the 20-battle run of Phase 5.
- **Recovery of escaped-defender or siege-attacker armies** after 20 map s is not named. It rests on the Phase 7 rule (`WM-RETREAT-01`).

**Source only (`[WM9-S]`) or untested.**
- From the I19 handoff:
  - settlement-only defenders dying with a captured settlement (no tool can set one up);
  - the capture refusal and undo paths (an unparkable unit; a throw part-way);
  - `CanTransfer` not refusing pending rosters;
  - `WorldMapDebugPanel2D` not showing pending rosters;
  - unisolated `ArmySpawned` and `ArmyDestroyed` handlers;
  - delivery timing depending on frame rate.
- From the I18 handoff:
  - a settlement destroyed mid-siege;
  - the capture undo paths (label mismatch, nothing moved);
  - a throw during commit;
  - the siege no-return-room refusal and the in-place fallback with no return spot;
  - `TotalStartsRefused` growing every step at a bad siege setup.
- Also, from source:
  - the step positions of the capture and the event flush, the queued event's timing after `BattleEnded`, and its order before a delayed `BattleEnded` after an incomplete end;
  - the deferred-recruit drop;
  - the quarantine parking before a capture;
  - the bug paths that keep part of an army;
  - the town defender list;
  - siege rates other than 80 → 12;
  - a settlement-only defender keeping the defender side alive;
  - healing while pending;
  - the failed-placement retry and the 16-armies cap;
  - the debug evacuation's refusals;
  - the pending↔pending transfer refusal;
  - army–settlement immunity's refusals, extension and pruning;
  - a siege and a field battle competing in one step;
  - same-step placement after a siege's end;
  - the commit's exception handling and the residue it leaves.

**Settled by M09 source.**
- `UnitRoster.RemoveInternal` clears `Owner`, which settles the question open since M06 (`WM-OWN-01`).
- `BattleManager2D.IsRosterInBattle` compares each side's `Roster`, so it covers a garrison side; the source comments still say "army".
- The commit steps after the arena's creation now run inside the preparation's exception handling. This closes the M08 note in `BATTLE-START-03`.

## Information intentionally removed from routine context

- Repeated dependency and bridge tables: replaced by core ownership/source indexes, subsystem rules and concise cards.
- Repeated event/placement/configuration prose: event consumers live in cards, event ordering in its pipeline, placement/setup once in Core.
- Exact tunable formulas, field defaults, constructor overload signatures, source-local method sequences and query buffer/subdivision constants: read current source when changing those details. Public integration entry-point names remain in cards. Cadence/expiry boundaries, probability comparison/cap, source/recipient eligibility and asymmetric validation remain where cross-system correctness depends on them.
- Old large Mermaid graphs: removed because the ownership/routing tables and ordered pipelines carry their useful information with less duplication.
- Asset values, collider coverage, execution-order overrides and runtime/performance results: still unverified, never filled from assumptions.

This is a contract-preserving restructure, not a byte-for-byte archive of all original implementation detail. Original source inventory is fully retained; original document remains in version history. No older memory/roadmap was treated as current code evidence.
