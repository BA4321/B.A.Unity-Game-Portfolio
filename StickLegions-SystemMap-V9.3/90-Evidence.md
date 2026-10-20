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

Each map update for a Roadmap V6 phase (M01–M03) was written from the phase's attached source, which is the authority for behaviour. Verification comes only from the Unity results the user reported in that phase's handoff, which is the authority for verification. No map update ran Unity, compiled code or inspected scenes, prefabs or project settings. Unity coverage is credited only to results the handoff names; where a handoff reported a group of tests as passed without listing them (for example "I06 regressions"), coverage is attributed to that group and not to individual clauses.

| Tag | Basis |
| --- | --- |
| WM1 | Phase 1 source (I01/I02), covered by I01/I02 Unity compilation, Play Mode and acceptance results |
| WM1-SR | Phase 1 change that was source-reviewed only, not Unity-verified when written. Its one use (the freeze-during-dispatch fix) was later covered by an I05 check; see `WM-CONTACT-05` |
| WM2 | Phase 2 source (I03–I05), covered by a named I03–I05 Unity result or the Phase 2 acceptance list (the ownership audit passed in every run) |
| WM2-S | Phase 2 source only; not separately tested in Unity |
| WM3 | Phase 3 source (I06–I07), covered by a named I06/I07 Unity result. The I07 results were run in both perimeter modes |
| WM3-S | Phase 3 source only; not separately tested in Unity |

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

## Information intentionally removed from routine context

- Repeated dependency and bridge tables: replaced by core ownership/source indexes, subsystem rules and concise cards.
- Repeated event/placement/configuration prose: event consumers live in cards, event ordering in its pipeline, placement/setup once in Core.
- Exact tunable formulas, field defaults, constructor overload signatures, source-local method sequences and query buffer/subdivision constants: read current source when changing those details. Public integration entry-point names remain in cards. Cadence/expiry boundaries, probability comparison/cap, source/recipient eligibility and asymmetric validation remain where cross-system correctness depends on them.
- Old large Mermaid graphs: removed because the ownership/routing tables and ordered pipelines carry their useful information with less duplication.
- Asset values, collider coverage, execution-order overrides and runtime/performance results: still unverified, never filled from assumptions.

This is a contract-preserving restructure, not a byte-for-byte archive of all original implementation detail. Original source inventory is fully retained; original document remains in version history. No older memory/roadmap was treated as current code evidence.
