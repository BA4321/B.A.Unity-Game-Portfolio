# Stick Legions system map — 91 Changelog

## V9.9.2 Cleanup — 2026-11-03

No rule, ID, evidence tag or contract card changed.

- Every file now has a title in the same form (`Stick Legions system map — NN Name`); `00-Core.md` states the map version.
- `92-Validation.md`: one file title, every section one heading level lower, and the first section retitled "V9 restructure validation — 2026-10-12" with a note that its answers, file count and sizes describe the V9 restructure, not the current map.
- Added `ARCHITECTURE.md`, a one-page summary for human readers. It is not an implementation file and holds no rules.
- Checked: all 198 rule definitions are unique and every backticked rule reference resolves; every file reference resolves; the source inventory still has 100 files / 75 logical entries (`CORE-02`).

## V9.9.1 M09 review — 2026-10-31

Documentation only; no code changed. Corrections after re-checking V9.9 against the Phase 9 source, Roadmap V6 and the I18 and I19 results:
- **Who reported what** (`07` header, `CORE-02`, this changelog's V9.9 entry): the I18 handoff reports the I18 checks and the I19 handoff the I19 tests; only the I19 handoff states that together they meet every acceptance item. V9.9 had attributed both to the I19 handoff.
- **Brackets that overclaimed:**
  - `WM-BATTLE-01` and `BATTLE-START-01` no longer credit I19 test 7 to sieges starting: test 7 used the debug evacuation, not a siege.
  - `WM-TIME-01` step 1 (the queued siege `SettlementCaptured`) is now WM9-S: no I19 test names that event.
  - `92-Validation.md` G7: the overflow is credited to I18 check 1 only; I19 test 1 covers the ordinary capture.
- **The setup-refusal counter** (`BATTLE-SIEGE-01`, `CORE-02`, G6): it grows on every step the touching pair is the selected battle pick, which is every touching step only when it is the only candidate.
- **Settlement-only defenders at a capture** (`WM-CAPTURE-01`): the release bypasses `UnitHealth2D` and `UnitDeath2D` (no `Died`, no kill XP) and is not counted in the side's `Dead`. The Core load-routing row no longer points this to `03`, and now names `05` for the holder's Start that delivery waits for.
- **`BATTLE-RECOVER-01`** gains a siege bullet: when a siege's end becomes pending, the capture, flags and return are already applied, the map stays frozen, and `SettlementCaptured` can precede the delayed `BattleEnded`. `ORDER-01` qualifies the same timing.
- **"Twice the normal rate"** is now "by default" wherever it appears (the multiplier is a setting).
- **`90-Evidence.md`:** I19's `BattleTypes2D` change added outcome fields only, so the I18 siege-start credit stands.
- **G19** was rewritten: the old example (attackers still queued) was undecided for that reason alone. It now isolates a settlement-only defender keeping the defender side alive.
- No rule ID was added, renumbered or removed. Phase 9 stays complete.

## V9.9 Phase 9 conquest and sieges — M09, 2026-10-31

Documentation only; no source code was changed by this update. It covers World Map & Battle Roadmap V6 Phase 9, from implementation chats I18 (capture, siege start and setup, with an interim siege end) and I19 (siege outcomes, the siege capture and pending retreat rosters). No map update was written after I18, so both are recorded here.

**Phase 9 is complete.** The I18 and I19 handoffs report that every one of their checks passed in Unity, each I19 test with its post-battle audit passing, and the I19 handoff that they meet every Phase 9 acceptance item together with earlier phases' checks (`92-Validation.md`, Phase 9). With it, every Roadmap V6 phase is complete. The untested paths and known gaps are preserved, not closed (`CORE-02`; `90-Evidence.md`, Phase 9 basis).

**`07-WorldMap-Battle.md`**
- Header:
  - state after Phase 9; a conquest-and-sieges bullet; the world-map and battle-start bullets say Capture and Siege take effect and the defender lists are used;
  - the "not implemented" line now names only the shrink trigger, and records that the I16 deferral was implemented;
  - tag range `[WM1]`–`[WM9-S]`; a Phase 9 evidence note.
- New sections and rules:
  - "Capture (Phase 9)": `WM-CAPTURE-01` (ordinary capture; siege capture with its checks, five steps and undo; record; event timing).
  - "Pending retreat rosters (Phase 9)": `WM-PENDING-01` (third owner kind, placement split by capacity, blocked retries, debug tools, known gaps).
  - "Sieges (Phase 9)": `BATTLE-SIEGE-01` (trigger, pre-arena checks, garrison side, rates, return spot, context, refusals, arena and commit, the garrison as a battle side) and `BATTLE-SIEGE-02` (outcome classification, attacker win and refused capture, other outcomes, siege return, a comparison with the roadmap table, departures, abort).
  - `WM-IMMUNE-01` keeps its ID and is extended to army–settlement immunity; its section is retitled.
- Rewritten in place, same IDs:
  - `WM-SCOPE-01`: Capture and Siege take effect; who sets the retreat flag.
  - `WM-TIME-01`: nine steps (event flush first; captures after allied interactions; pending rosters at the end of settlement work); new consequences.
  - `WM-FREEZE-01`: captures and pending rosters wait; `SetFaction`'s in-battle guard.
  - `WM-OWN-01`: three roster kinds; the siege-capture release; `RemoveInternal` clears `Owner` (open since M06, settled).
  - `WM-OWN-02`: settlement destroyed mid-siege (known gap); pending rosters.
  - `WM-ROSTER-01`: pending-roster capacity, faction and owner references.
  - `WM-FACTION-01`: the in-battle refusal, the siege bypass and the deferred-recruit drop.
  - `WM-XFER-01`: pending↔pending refused. `WM-XFER-02`: `CanTransfer` gap; the hook covers a garrison side.
  - `WM-DEPLOY-01`, `WM-DEPLOY-02`: Blocked while in battle; `startRetreating`; the EscapedDefenders reason; handlers not isolated.
  - `WM-OVERFLOW-01`: the in-battle block; the new owner's overflow.
  - `WM-AUDIT-01`: pending rosters audited.
  - `WM-REMOVE-01`: capture callers; the `FrozenChanged` question answered from `WorldMapController2D.cs` (still untested); unisolated handlers.
  - `WM-SETTLE-01`: siege setup. `WM-SETTLE-02`: the event pointer.
  - `WM-CONTACT-05`, `WM-BATTLE-01`: sieges start from the pick; immune army–settlement pairs skipped.
  - `WM-REGEN-02`: pending-roster units heal (source-derived). `WM-RETREAT-01`: the siege and escaped-defender callers.
  - `ARENA-LIFE-02`: `TryCreateWithBattlefield`. `ARENA-TERRAIN-01`: public `ValidateBattlefieldPrefab`, `BattlefieldSource.Siege`. `ARENA-GATE-01`: siege gates.
  - `SIDE-01`: the siege mapping is used.
  - `BATTLE-LIFE-01`: `IsRosterInBattle` covers a garrison side.
  - `BATTLE-START-01`: Siege picks, `TryStartSiege`, the shared refusals. `BATTLE-START-02` and `BATTLE-EDGE-01`: field only.
  - `BATTLE-START-03`: shared by sieges; one freeze; the M08 commit-exception note replaced by the I18 fix and its residue.
  - `BATTLE-SPAWN-01`: the defender rate. `BATTLE-SPAWN-02`: the owner and faction checks against the side's roster; the settlement-only drop on army sides only.
  - `BATTLE-UNIT-02`: escape ownership in a siege.
  - `BATTLE-ABORT-01`: the siege abort's return. `BATTLE-END-04`, `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`: the siege branch; field routing still never moves units.
  - `BATTLE-AUDIT-01`: siege totals.
  - `BATTLE-RETREAT-01`, `BATTLE-RETREAT-03`: settlement-only defenders never roll and are refused even when forced.
  - `BATTLE-DELIVER-03`: the siege freeze has a caller; `BattleKind.Siege`.
- Cards, still 33:
  - `WorldMapController2D`: ten partials, with the `.Capture` and `.Siege` API, events, invariants and partials table.
  - `Settlement2D`: siege setup, `SetFactionAfterSiege`, `DropDeferredRecruitment`, the evacuation menu.
  - `UnitReserve2D`, `UnitRoster`, `UnitRosterTypes2D` (`PendingRetreat`), `WorldMapTypes2D` (`EscapedDefenders`; `BattleReturn` dropped).
  - `Army2D` (callers of `SetRetreating`), `UnitRegeneration2D` (pending units), `WorldMapDebugPanel2D` (no pending rosters, from the handoff).
  - `BattleArena2D`, `BattleRules2D` (multiplier, `ComputeSiegeDefenderSpawnRate`, `BattlefieldSource.Siege`).
  - `BattleManager2D` (`TryStartSiege`, siege counters, menus, partials table), `BattleTypes2D` (`BattleKind.Siege`, `SiegeOutcomeKind`, siege and outcome fields, garrison side), `BattleSides2D`, `BattleManagerDebugPanel2D` (siege and owner readouts).

**`00-Core.md`**
- Header (Phase 9; every phase complete).
- Fragile boundaries: the start row (Siege pick, commit throw); the routing row (the siege capture is the only routing path that moves units); two new rows (the besieged garrison as a battle side; the siege capture order).
- `CORE-02` reconciled with the final implementation:
  - 100 files / 75 entries, ten controller partials;
  - Phase 9 established;
  - "not implemented" now lists only the shrink trigger. Capture, sieges and pending retreat rosters left it with Unity evidence: I18 checks 1–7, I19 tests 1–9;
  - a new out-of-roadmap-scope line (from the roadmap);
  - a new known-gaps list, so no gap is dropped.
- `CORE-03` (pending-roster units stay parked; settlement-only release); `CORE-04` (`[WM9]`, `[WM9-S]`).
- Load routing (a conquest-and-sieges row); ownership rows (two new partials, roster, settlement, start, routing, delivery, types, sides); inventory (two new rows, ten roles edited).
- `ORDER-01`: the controller row and the capture-event timing.
- `SETUP-07`: siege setup. `UnitDefinition2D` card: settlement-only readers.
- Glossary: Contact intent, Roster, Side, Spawn queue, Routing, Battle-start modifier list and Pending retreat roster rewritten; Pair immunity widened; new Settlement-only defender, Garrison side, Siege setup, Capture, Escaped defenders.

**`90-Evidence.md`**
- Defines `[WM9]` and `[WM9-S]`.
- New Phase 9 basis: files read and not attached; I18 checks 1–9 and I19 tests 1–9; attribution limits; the source-only list; facts settled by M09 source.
- M09 update notes: in the Phase 5 source-reviewed list (`FrozenChanged`); in the Phase 6 basis (parking disables no component); in the Phase 8 source-only list (the siege freeze's caller, the castle list, the commit fix).

**`92-Validation.md`**
- Phase 9 known-answer checks G1–G20, the superseded answers (W9, W10, F11, B16), the acceptance table and structural checks.
- F11 updated in place.

**Decisions kept from I18–I19** (handoff decisions, recorded as such):
- the third owner kind `UnitRosterKind.PendingRetreat`, owned by the controller, unbounded, keeping the defenders' faction, never taking settlement-only units, never exchanging with another pending roster, audited and registered the moment it holds units;
- the siege capture runs inside the normal end's routing, while frozen and current, with every check first; only the ending siege may call it; `SetFactionAfterSiege` skips only the in-battle guard;
- settlement-only defenders never retreat (even forced), keep the defender side alive, and die with a captured settlement if still owned by it;
- mobile dropped defenders leave with the escapees (a departure from §1.7's wording);
- recruits queued during a siege are dropped at a capture;
- `SettlementCaptured` is queued to the controller's next step;
- a refused capture changes no owner;
- every non-capture outcome and the abort go through the siege return with army–settlement immunity;
- escaped-defender placement runs at the end of settlement work, with indefinite retries and at most 16 armies per roster per step;
- the `BattleReturn` spawn reason was dropped;
- a bad siege setup refuses with no global wait;
- the defender queue is the whole garrison, at twice the normal rate by default;
- gates only on the attacker's edge;
- the commit runs inside the preparation's exception handling.

**Untested and carried forward:**
- the `[WM9-S]` list in `90-Evidence.md`. In particular:
  - a settlement destroyed mid-siege;
  - settlement-only defenders dying with a settlement;
  - the capture refusal and undo paths;
  - the siege no-return-room refusal and the in-place fallback;
  - a throw during commit;
  - the setup-refusal counter growth;
  - `CanTransfer` not refusing pending rosters;
  - the debug panel not showing them;
  - unisolated `ArmySpawned`/`ArmyDestroyed` handlers;
- from M08: the `[WM8-S]` list; delivery timing depends on frame rate;
- from M05: the post-battle audit counts every projectile in flight; `LastSelectedBattle` can be stale; an orphan-arena audit keeps the old battle id; whether I11 changed `UnitClashParticipant2D` is not established; `UnitReactiveTurnBoost2D.ClearOwnedModifiers` is untested; the `[WM5-SR]` items;
- not implemented: the shrink trigger.

The question "whether `ReleaseOwnership` clears `Owner`" is settled and no longer carried.

**Source facts worth a later decision (not defects, not changed):**
- after an incomplete siege end, `SettlementCaptured` can be raised before the delayed `BattleEnded`;
- a throw in the commit leaves a consumed battle id and raised retreat-roll counters;
- the siege immunity decides the outcome only after an abort, a refused capture or a no-return-room refusal (a surviving attacker is otherwise retreating);
- the source comments of `IsRosterInBattle` still say "army";
- the manager and the controller each have a `TotalSiegeCaptures` counter;
- `GetDefenderModifiers` returns the town list for any kind other than `Castle` (carried from M08).

**Not attached to M09, possibly stale:** `01`–`06` were not attached. Phase 9 changed no combat source, but their cards may still describe the defender lists or the siege freeze as unused before Phase 9; check them when they are next edited.

## V9.8.2 M08 second review — 2026-10-27

Documentation only; no code changed.
- `90-Evidence.md`, Phase 8 basis: `UnitModifierRuntime2D.cs` was supplied by the user as one of the handoff's on-request files, not read "at the user's request".
- `BATTLE-DELIVER-03`, Diagnostics: the audit's frame, holder and level checks cover delivered units still on the field, as `90-Evidence.md` already says.
- No rule ID added, renumbered or removed. Phase 8 stays complete.

## V9.8.1 M08 review — 2026-10-27

Documentation only; no code changed. Corrections after re-checking the V9.8 update against the Phase 8 source, `UnitModifierRuntime2D.cs`, Roadmap V6 and the I17 results:
- `BATTLE-LIFE-01`: the step order says each spawn queues its battle-start request (it was in `ORDER-01` only).
- `BATTLE-UNIT-04` step 3: parking also removes a delivered battle-start list; bracket gains I17 "parked units carried 0 instances".
- `BATTLE-START-03`: the commit-exception note no longer says nothing would clear the arena. The manager would not, but an arena removal from elsewhere would, and it is not an abort with no current battle.
- `BATTLE-DELIVER-03`: the siege source string gains " (empty)" for an empty defender list. The audit's last-battle check runs once that battle is no longer current, including while its end is pending.
- `92-Validation.md`: F3 notes that the `[+A, +B]` log needs `logRetreats`; F4 and F10 state that a retreat modifier is configured; the Phase 8 structural list names the two rules edited here.
- No rule ID added, renumbered or removed. Phase 8 stays complete.

## V9.8 Phase 8 battle-start modifiers — M08, 2026-10-27

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 8 (implementation chat I17, battle-start modifier lists). **Phase 8 is complete:** the I17 handoff reports every acceptance item passed in Unity (`92-Validation.md`, Phase 8). The handoff also reports the drops with requests still pending, on death, at the lock and at an abort. That closes those clauses of the M07 carried item; the normal-end drop stays source-only and cannot hold a request in practice.

**`07-WorldMap-Battle.md`**
- Header: state after Phase 8; a battle-start bullet; the "not implemented" line no longer lists battle-start lists (the siege freeze hook has no caller); tag range `[WM1]`–`[WM8-S]`; Phase 8 evidence note. The battle-deployment intro and the unit-retreat intro point to the new section.
- New "Battle-start modifiers (Phase 8)" section: `BATTLE-DELIVER-03` (settings, freeze at commit, the Phase 9 siege hook and its one-freeze rule, request at spawn, once per unit per battle, battle-start before retreat, counts, audit).
- Rewritten in place, same IDs:
  - `BATTLE-START-03`: the freeze and `LastCommittedBattle` in the commit; a new note that the commit steps after the arena's creation run outside the preparation's exception handling.
  - `BATTLE-SPAWN-02`: the request after listing, before the deployment retreat check.
  - `BATTLE-LOCK-01`: step 6 names the battle-start lists; requests are refused after the lock; the bracket now has WM8.
  - `BATTLE-ABORT-01`: bracket only.
  - `BATTLE-DELIVER-01`: BattleStart has a caller; queue order; the debug hold; `ApplyOrdered` capture and the NotProcessed behaviour; battle-start result recording.
  - `BATTLE-DELIVER-02`: the battle-start list is included; the normal-end drop finds nothing in practice; the runtime cleanup is stated from source.
- No new cards (`07` still holds 33). Updated cards: `BattleRules2D` (three lists, two getters, empty-entry warnings), `BattleManager2D` (I17 API, counters, hold, audit, listing, four context menus, siege hook, partials table), `BattleTypes2D` (context, side and unit battle-start fields). The `BattleManagerDebugPanel2D` card was rewritten from the re-attached source: it shows no Phase 5 readouts; "Cancel retreat" does nothing since I15; I17 added a hold toggle, a battle-start line per side and a battle-start audit button.

**Card notes in other files**
- `04`: `UnitModifierRuntime2D` card. Battle callers now include the battle-start lists. `CanReceiveModifiers`, the convenience `ApplyOrdered` (capture once; unreached entries NotProcessed on a throw) and the OnDisable, `Died` and zero-health `ClearAll` are stated from the runtime source re-attached at M08. No rule changed.
- `05`: `UnitAbilityHolder2D` card. The `didStart` wait now has Unity evidence (WM8); the disabled-holder case is source-only. No rule changed.

**`00-Core.md`**
- Header; the delivery fragile-boundary row; `CORE-02` (Phase 8 established; the Phase 7 delivery item; the not-implemented list); `CORE-04` (`[WM8]`, `[WM8-S]`).
- Load-routing row for retreat and delivery; two ownership rows and two inventory rows edited (no file added; still 98 files / 75 entries).
- `ORDER-01` manager row: each spawn queues its battle-start request.
- Glossary: Modifier delivery names both kinds; "Battle-start modifier list" added.

**`90-Evidence.md`**: defines `[WM8]` and `[WM8-S]`, records the Phase 8 basis, the seven I17 results, attribution limits and the source-only list, and notes in the Phase 7 source-only list which delivery-drop clauses I17 closed. **`92-Validation.md`**: Phase 8 known-answer checks F1–F14, the acceptance table and structural checks; Phase 7 E8 and E10 verification updated in place.

**Decisions kept from I17:**
- One delivery path, with BattleStart independent of Retreat.
- The request is made in the spawn call, before the deployment retreat check.
- The freeze runs at commit, right after the retreat rolls.
- A field battle uses one list for both sides; defender lists are keyed by `SettlementKind`.
- Phase 9 must call `FreezeSiegeBattleStartModifiers` instead of the field freeze; a second freeze throws by design.
- The delivery hold is debug-only and persists across battles.
- Delivery, removal and ownership are unchanged from I15.

**Untested and carried forward:**
- the `[WM8-S]` list in `90-Evidence.md`;
- delivery timing depends on frame rate;
- whether `ReleaseOwnership` (through `UnitRoster.RemoveInternal`) clears `Owner`;
- from M05: the post-battle audit counts every projectile in flight; `LastSelectedBattle` can be stale; an orphan-arena audit keeps the old battle id; whether I11 changed `UnitClashParticipant2D` is not established; `UnitReactiveTurnBoost2D.ClearOwnedModifiers` is untested; the `[WM5-SR]` items;
- later phases: the shrink trigger; capture, sieges, the siege battle-start freeze's caller, pending retreat rosters and capacity splits (Phase 9).

**Source facts worth a later decision (not defects, not changed):**
- an exception in the commit steps after the arena's creation (for example a second freeze in Phase 9) would leave the arena built and the map frozen with no current battle (`BATTLE-START-03`);
- `GetDefenderModifiers` returns the town list for any kind other than `Castle`;
- a BattleStart delivery counts as delivered even when every entry is rejected, and there is no session total for battle-start drops;
- the debug panel's battle line says "running" while the result is locked, and its "Cancel retreat" button does nothing since I15.

## V9.7.3 M07 second review — 2026-10-26

Documentation only; no code changed.
- `BATTLE-RETREAT-02`: a maximum-only `HealthChanged` (for example a level-up in battle) can latch a retreat without damage; source-derived, untested. `92-Validation.md` E22 added.
- `BATTLE-RETREAT-04`: the motor steers to the gate once the unit is Inside; while Entering it still follows its Entry override.
- No rule ID added, renumbered or removed. Phase 7 stays complete.

## V9.7.2 M07 review — 2026-10-26

Documentation only; no code changed. Corrections after re-checking the V9.7 and V9.7.1 updates against the Phase 7 source, Roadmap V6 and the reported I15, I16 and closing-check results:
- `07` header: a retreat starts at a later manager step, or the same step for a unit latched at deployment, not always the next one.
- `BATTLE-RETREAT-03`: the start loop scans the whole field each step (cost proportional to the field), not only latched units; a unit whose retreat steering is refused has no override, so nothing keeps it from Sidestepping (source, untested).
- `BATTLE-DELIVER-01`: a request counts as completed once `ApplyOrdered` has been called; a missing modifier runtime counts as a drop.
- `BattleManager2D` card: the six I15 context-menu names given in full; `92-Validation.md` closing check step 2 likewise.
- `90-Evidence.md`: I16 changed none of the I15 files; the closing check's source version is not stated (no longer "presumably" the final one); the source-only list no longer names Sidestep while retreating, which the closing check covers, and adds the refused-steering case.
- `ORDER-01`: the shared Play Mode session claim covers the Phase 4–7 tests.
- No rule ID added, renumbered or removed. Phase 7 stays complete.

## V9.7.1 Phase 7 closing check — 2026-10-26

Documentation only; no code changed. The user reported after M07 that the tests went well and everything supposed to work works in Unity. That is credited to the Phase 7 closing check in `92-Validation.md`, as a group of four steps, since the observations were not listed one by one. **Phase 7 is complete:** every acceptance item now has a passing Unity result.
- `07`: header (Phases 1–7 complete); `BATTLE-UNIT-02` and `BATTLE-RETREAT-04` brackets.
- `00`: header; `CORE-02` (Phase 7 established); `CORE-04`.
- `01`: the Phase 7 battle note's bracket (no Sidestep while retreating). `05`: `UnitAbilityHolder2D` card (casting while retreating; card tag `WM7`).
- `90-Evidence.md`: the `[WM7]` definition includes the closing check; the two attribution limits note their closure; the source-only list no longer includes Sidestep while retreating.
- `92-Validation.md`: E11 and E12, the Unity/source lists and the Phase 7 acceptance table updated in place.
- No rule ID added, renumbered or removed.

## V9.7 Phase 7 retreat — M07, 2026-10-26

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 7 (implementation chats I15, unit retreat, and I16, retreating armies).

**Phase 7 is implemented but not marked complete.** Every I15 and I16 check passed on its source, and nothing is known to be broken; both handoffs report Phase 7 complete. Two acceptance items have no named Unity result: "retreating units keep fighting" and "escaped units never respawn that battle". Both rest on source. `92-Validation.md` gives one run that closes both.

**Phase 6 is now complete.** The I15 handoff reports that the I14 Regen-tracking check passed, closing M06's open "dead units are skipped" item (`92-Validation.md` D6 and the Phase 6 acceptance table, updated in place).

**`07-WorldMap-Battle.md`**
- Header: state after Phase 7; Phase 6 complete; unit-retreat and retreating-army bullets; the "not implemented" list; the Phase 9 deferral; tag range.
- New "Retreating armies (Phase 7)" section: `WM-RETREAT-01` (timer on `Army2D`, recovery step, due check and tolerance, consequences), with the deferral of pending retreat rosters and capacity splits to Phase 9.
- New "Unit retreat (Phase 7)" section: `BATTLE-RETREAT-01` (eligibility at commit), `BATTLE-RETREAT-02` (low-health latch, deployment check), `BATTLE-RETREAT-03` (retreat start), `BATTLE-RETREAT-04` (retreating units, escape, `UnitRetreated`), `BATTLE-DELIVER-01` (delivery request, wait, apply), `BATTLE-DELIVER-02` (drops and removal).
- Rewritten in place, same IDs: `WM-SCOPE-01` (recovery clears the flag), `WM-TIME-01` (step 5 recovery; steps renumbered 1–7), `WM-FREEZE-01` (recovery paused), `WM-UNIT-01` (Escaping location, `TryMarkEscaping`, Escaping accepted by `ReturnToReserve`), `WM-UNIT-02` (every status set), `WM-OWN-01` (Escaping owned; pending retreat rosters deferred; `ReleaseOwnership` accepts Escaping), `WM-XFER-01` (Escaping skipped), `WM-ALLY-02` (timers kept; record timer fields), `WM-ALLY-03` (absorbed armies never recover), `WM-AUDIT-01` (Escaping checked like Deployed), `WM-IMMUNE-01` ("where it matters today"), `WM-REGEN-02` (Escaping units heal; the latch hears heals), `ARENA-GATE-01` (the latch is manager policy), `BATTLE-LIFE-01` (step order, `UnitRetreated`), `BATTLE-START-03` (rolls at commit), `BATTLE-SPAWN-02` (activation frame, subscription, deployment check), `BATTLE-UNIT-01` (detach list), `BATTLE-UNIT-02` (Escaping → Escaped, counts, retreat sources), `BATTLE-UNIT-04` (detach, Escaping, modifier removal), `BATTLE-LOCK-01` (delivery drop as step 6; no retreat after the lock), `BATTLE-END-04` and `BATTLE-ABORT-01` (delivery drops, Escaping units parked), `BATTLE-ROUTE-01` (timer starts at the unfreeze). The battle-deployment intro names nine partials.
- No new cards (`07` still holds 33). Updated cards: `WorldMapController2D` (eight partials, `.Retreat` API, record fields), `Army2D` (timer API), `UnitReserve2D` (`TryMarkEscaping`), `UnitRosterTypes2D` (Escaping now set), `WorldMapRules2D` (`retreatRecoverySeconds`), `WorldMapDebugPanel2D` (retreat controls), `BattleRules2D` (three retreat settings), `BattleManager2D` (nine partials, `UnitRetreated`, I15 counters, debug tools and menus, internal delivery API), `BattleTypes2D` (I15 context, side and unit fields), `BattleManagerDebugPanel2D` (note only: not re-attached).

**Integration notes in other files**
- `01`: a Phase 7 battle note: the Retreat override is what keeps retreating units from Sidestepping. No movement rule changed.
- `03`: `HEALTH-05` (the battle debug health tools use the debug setter); `UnitHealth2D` card (battle `HealthChanged` subscriber).
- `04`: `MOD-APPLY-01` (the delivery path as a caller); `UnitModifierRuntime2D` card (battle callers, removal on parking).
- `05`: `UnitAbilityHolder2D` card (the delivery waits for its `didStart`; retreat adds no casting gate). No rule changed.

**`00-Core.md`**
- Header; three fragile-boundary rows; `CORE-02` (98 files / 75 entries; Phase 6 established; Phase 7 implemented, not complete; not-implemented list with the Phase 9 deferral; every status set; post-lock delivery drops); `CORE-03` (delivery waits for Start); `CORE-04` (`[WM7]`, `[WM7-S]`).
- Load-routing row for retreat; three ownership rows added, two edited; three inventory rows added, two edited.
- `ORDER-01`: the manager row (retreat starts → deliveries), the controller row (recovery step), the meeting points (`HealthChanged` latch, `ApplyOrdered`), and why retreat starts never run inside a transaction.
- Glossary: the `UnitStatus` row (Escaping), Map time and Lost unit edited; six terms added (Retreat-eligible unit, Retreat latch, Retreating unit, Modifier delivery, Retreating army / recovery, Pending retreat roster).

**`90-Evidence.md`**: defines `[WM7]`, `[WM7-S]` and updates `[WM6]`; records the Phase 7 basis, the I15 and I16 results, attribution limits and the source-only list; the Phase 6 dead-units limit notes its closure. **`92-Validation.md`**: Phase 7 known-answer checks E1–E21, the acceptance table with its two open items and a closing check, structural checks; Phase 6 D6 and acceptance updated in place.

**Departures and decisions kept from I15–I16:** retreat eligibility is rolled at commit for queued units only, after every pre-arena check (at that point every participant is queued); `DebugSetRetreating(_, false)` does nothing; a unit that escapes before it has started never gets the retreat modifier; the recovery timer lives on `Army2D`, not in a controller list; pending retreat rosters and capacity splits are deferred to Phase 9.

**Untested and carried forward:** the two open Phase 7 items; the `[WM7-S]` list in `90-Evidence.md`; whether `ReleaseOwnership` (through `UnitRoster.RemoveInternal`) clears `Owner`; modifier delivery timing depends on frame rate. From M05: the post-battle audit counts every projectile in flight; `LastSelectedBattle` can be stale; an orphan-arena audit keeps the old battle id; whether I11 changed `UnitClashParticipant2D` is not established; `UnitReactiveTurnBoost2D.ClearOwnedModifiers` is untested; the `[WM5-SR]` items. Later phases: the shrink trigger, battle-start modifier lists (Phase 8), capture, sieges and pending retreat rosters (Phase 9).

**Source facts worth a later decision (not defects, not changed):**
- a unit latched at deployment starts retreating while still Entering, so it can escape through the gate just inside its own edge almost at once (Roadmap §5 lists this as expected);
- I16 test 2 reports that the timer pauses during the end delay; from source the flag is set only after the end delay, so no timer exists then;
- `TryStartRetreat` does not mark a unit failed when its body is unregistered; the sweep removes it instead.

## V9.6.2 M06 second review — 2026-10-26

Documentation only; no code changed. `92-Validation.md` Phase 6 structural checks now list the `WM-AUDIT-01` bracket edit made in V9.6.1. Everything else re-checked against the Phase 6 source, Roadmap V6 and the I14 results without change. The dead-unit check (D6) has not been reported yet, so Phase 6 stays not marked complete. No rule ID added, renumbered or removed.

## V9.6.1 M06 review — 2026-10-25

Documentation only; no code changed. Corrections after re-checking the V9.6 update against the Phase 6 source, Roadmap V6 and the reported I14 results:
- `HEALTH-05`: the test 3 bracket no longer says deployed units were seen healing in the locked delay (that is the extra check); the amount refusals (non-positive or non-finite amount → Ineligible; no gain → AlreadyFull) are stated; "only caller" is limited to the I14 source. `HEALTH-04`: the debug setter also reconciles an inactive maximum.
- `WM-REGEN-01`: the interval is read every step, so lowering it at runtime can also trigger the leftover reset (also in `90-Evidence.md` and the V9.6 source facts).
- `WorldMapController2D` card: the tracking stop condition is stated in source terms (`Owner` null, GameObject destroyed or no health); whether `ReleaseOwnership` clears `Owner` is not established, so a dead tracked unit may keep logging at 0 HP with status Dead.
- `WM-AUDIT-01`: bracket gains I14 test 4.
- `ORDER-01`: "meet only through" no longer contradicts the added regeneration meeting point.
- `90-Evidence.md`: `CombatLock2D` was wrongly listed among files the handoff reports unchanged; the test 2 remark no longer assumes the unit was parked after a normal end.
- No rule ID added, renumbered or removed. Phase 6 stays not marked complete pending the dead-unit check.

## V9.6 Phase 6 regeneration — M06, 2026-10-25

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 6 (implementation chat I14).

**Phase 6 is implemented but not marked complete.** Every check the I14 handoff reports passed on the final source (`90-Evidence.md`), and nothing is known to be broken. One acceptance item, "dead units are skipped", has no named Unity result: it rests on source. The I14 handoff reports the acceptance as passed, but its six checks do not name dead units. `92-Validation.md` gives a short check that closes the item.

**`07-WorldMap-Battle.md`**
- Header: state after Phase 6, regeneration bullet, "not implemented" list and tag range updated.
- New "Regeneration (Phase 6)" section: `WM-REGEN-01` (tick owner and timing), `WM-REGEN-02` (who heals, skips and the tick report).
- Rewritten in place, same IDs: `WM-LIFE-01` (Awake creates the service), `WM-TIME-01` (step 6, regeneration on scaled time), `WM-FREEZE-01` (ticks continue while frozen), `WM-UNIT-02` (`UnitHandle.Health`; `HasEverActivated`, not `IsDead`), `BATTLE-LOCK-01` (healing is not gated), `BATTLE-UNIT-04` (pointer: HP rises while parked).
- New card `UnitRegeneration2D`; `07` now holds 33 cards. Updated cards: `WorldMapController2D` (seven partials, `.Regeneration` API and Editor menus), `WorldMapRules2D` (interval), `UnitHandle` (`Health`).

**Integration rules in other files**
- `03`: new `HEALTH-05` (regeneration heal and the development-only debug setter); `HEALTH-04` rewritten (regeneration is the only gameplay healing path; inactive units reconcile only through the heal and are never clamped to death); `UnitHealth2D` card.
- `04`: `UnitRuntimeStats2D` card only: `EffectiveMaximumHealth` is read on inactive, awakened units (the notification behaviour there rests on the I14 handoff signature). `MOD-APPLY-03` unchanged: no regeneration modifier field.
- `06`: `UI-01` bracket gains I14 test 5; no rule text changed.

**`00-Core.md`**
- Header; one fragile-boundary row; `CORE-02` (95 files / 75 entries; Phase 6 listed as implemented, not complete; regeneration removed from the gaps; locked units heal); `CORE-03` (the tick never wakes a unit); `CORE-04` (`[WM6]`, `[WM6-S]`).
- Load-routing row for regeneration; two ownership rows added, one edited; two inventory rows added, two edited.
- `ORDER-01`: the regeneration step in the controller row, where it sits relative to the manager, scheduler, director, arena and health, and `UnitRegeneration2D` as a plain object. `INIT-01` pointer.
- Glossary: "Regeneration tick" and "Activated unit" added; "Map time" notes that regeneration does not use it. `UnitDefinition2D` card: `healthRegenFraction`.

**`90-Evidence.md`**: defines `[WM6]`, `[WM6-S]` and records the Phase 6 basis, attribution limits and the source-only list. **`92-Validation.md`**: Phase 6 known-answer checks D1–D12, the acceptance table with its open item, and structural checks.

**Departure from Roadmap §5 notes, kept from I14:** the suggested name `UnitRegeneration2D` is a plain object owned and stepped by the world-map controller, not a per-unit component, because a component on a parked unit never runs.

**Handoff suggestions not applied:** `WM-SCOPE-01` was not edited, because it covers contact intents and nothing it states changed. `UnitReserve2D`'s card is unchanged (I14 reports the file unchanged; the tick only reads `Units`).

**Untested and carried forward:** dead units skipped (the open acceptance item); the `[WM6-S]` list in `90-Evidence.md`. From M05: the post-battle audit counts every projectile in flight, including non-battle ones; modifier application is not gated by the lock; `LastSelectedBattle` can be stale; an orphan-arena audit keeps the old battle id; whether I11 changed `UnitClashParticipant2D` is still not established; `UnitReactiveTurnBoost2D.ClearOwnedModifiers` is still untested; the `[WM5-SR]` items. Later phases: the shrink trigger, retreat eligibility and Escaping, `UnitRetreated`, the retreat modifier, retreat recovery after 20 s, battle-start modifiers, capture and sieges.

**Source facts worth a later decision (not defects, not changed):**
- a zero `healthRegenFraction` or a missing definition is counted Ineligible on every tick, with no separate counter;
- `UnitHandle.Health` caches a null result permanently;
- the leftover reset described in the source as protection against a long hitch can only act when one fixed step is longer than the interval, or when the interval is lowered at runtime below the accumulated time, because the controller passes `Time.fixedDeltaTime` and reads the interval every step;
- an active unit's heal runs the existing maximum reconciliation, so in principle it can be the caller that performs an unattributed clamp death (`HEALTH-04`); with no maximum-health modifier and levels only rising, no current path lowers the maximum;
- the debug status and tracking readouts read `MaximumHealth`, which can notify (`STAT-01`).

## V9.5.3 M05 third review — 2026-10-25

Documentation only; no code changed.
- `WM-IMMUNE-01`: added where the immunity matters today. After a normal end that leaves both armies, at least one is retreating, so the pair bounces regardless; until Phase 7 recovery, immunity decides the outcome only after an abort or a no-return-room refusal (source-derived).
- `92-Validation.md` C10 reworded accordingly; I13 test 6 does not say which end preceded it.
- No rule ID added, renumbered or removed. Phase 5 stays complete.

## V9.5.2 M05 second review — 2026-10-25

Documentation only; no code changed. Corrections after a second pass over the Phase 5 source:
- `BATTLE-LOCK-01`: not every lock effect is isolated. Closing escapes, each motor and the projectile sweep have their own exception handling; storing the result and the callback-free `CombatLock2D` writes do not.
- `BATTLE-RETURN-01`: the search tries every angle at one distance before the next distance.
- `WorldMapController2D` card: `ApplyBattleReturn` also returns false when the controller is not ready.
- Battle deployment intro: the pointer to the Phase 5 section is named instead of "next section".
- No rule ID added, renumbered or removed. Phase 5 stays complete.

## V9.5.1 M05 review — 2026-10-25

Documentation only; no code changed. Corrections after re-checking the V9.5 update against the Phase 5 source, Roadmap V6 and the reported I11–I13 results:
- `92-Validation.md` C9: a Decisive end that removes the loser leaves the winner where it stood (`ApplyBattleReturn` moves no lone survivor), and grants no pair immunity (granted only while both armies remain). The earlier answer said the winner was placed at a return spot and made immune. The `BATTLE-RETURN-01` bracket now reads I13 test 2 as the lone-survivor case.
- `92-Validation.md` C1: final-batch deaths include each side's last unit, not exactly one.
- `BATTLE-UNIT-04`: a successful quarantine retry is logged; only failed retries are silent.
- `BATTLE-AUDIT-01`: the audit also waits while an end is pending.
- `BATTLE-ABORT-01`: arena removal with no current battle (recovery) is not an abort.
- `TRIGGER-04`: the inserted lock sentence no longer breaks the rule's sequence.
- `ARENA-BODY-01`: pointer to `BATTLE-UNIT-04` and `BATTLE-END-04` added.
- `CORE-02`: the Phase 4 list again includes abort, which V9.5 had dropped. Glossary: the Lost unit and `UnitStatus` (Discarded) rows cover escaped and quarantined units destroyed without dying.
- No rule ID added, renumbered or removed. Phase 5 stays complete.

## V9.5 Phase 5 repeatable battles — M05, 2026-10-25

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 5 (implementation chats I11–I13). **Phase 5 is complete:** every Phase 5 acceptance item has a passing Unity result. The I13 handoff reports the acceptance as passed on the final source, including 20 consecutive battles. Its one gap, Clash and cast refusal after the lock, is covered by the I11 lock test (no deaths, Clashes, casts or trigger warnings; no projectiles in flight), per the user's evidence correction. That result is recorded as I11 testing, not as a rerun on the final I13 source. Source-reviewed-only paths and source-only clauses are tagged `[WM5-SR]` and `[WM5-S]`.

**`07-WorldMap-Battle.md`**
- Header describes the state after Phase 5; the "battle ends only by abort" paragraph is replaced by what is still not implemented.
- New "Pair immunity (Phase 5)" section: `WM-IMMUNE-01`.
- New "Battle ending, cleanup and return (Phase 5)" section with 11 rules: `BATTLE-END-01`–`BATTLE-END-04`, `BATTLE-LOCK-01`, `BATTLE-LOCK-02`, `BATTLE-UNIT-04`, `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `BATTLE-RECOVER-01`, `BATTLE-AUDIT-01`.
- Rewritten in place, same IDs: `WM-SCOPE-01` (routing sets the retreat flag, nothing clears it), `WM-OWN-01` (lost-unit callers), `WM-XFER-02` (the battle hook at the normal end), `WM-AUDIT-01` (run by the post-battle audit), `WM-REMOVE-01` (battle callers of `DestroyArmy`), `WM-BATTLE-01` (immune pairs skipped), `ARENA-VIEW-01` and the arena section intro (the shrink trigger is still not implemented), `ARENA-GATE-01` (the lock closes escapes), `BATTLE-LIFE-01` (step order, idle branch, OnDisable recovery, events), `BATTLE-START-01` (no session limit; recovery and immunity refusals), `BATTLE-START-02` (return-spot check), `BATTLE-START-03` (immunity refusal versus the retry wait; commit additions), `BATTLE-SPAWN-02` (the record keeps the reactive boost), `BATTLE-UNIT-01` (death batch, deaths after lock, corpses destroyed at the end), `BATTLE-UNIT-02` (escape through the shared removal), `BATTLE-UNIT-03` (unlock after parking), `BATTLE-ABORT-01` (projectile sweep, army return, orphan arena, lock released last).
- New card `CombatLock2D`; `07` now holds 32 cards. Updated cards: `WorldMapController2D` (six partials, `.Separation` API), `Army2D` (`PlaceAt`), `UnitReserve2D`, `BattleArena2D`, `BattleRules2D` (ending and immunity fields), `BattleManager2D` (seven partials, events, counters, debug), `BattleTypes2D` (I11–I13 additions). The `BattleManagerDebugPanel2D` card notes that the panel was not re-attached.
- `CombatLock2D` lives in the combat layer but its card is in `07` with the other Phase 5 battle cards (Core card home "WorldMap").

**Integration rules in other files**
- `02`: `CLASH-05` (lock gate in `ClashDirector2D`); director card.
- `03`: `HEALTH-01` (damage gate; forced and clamp deaths not gated); `UnitHealth2D` and `UnitDeath2D` cards.
- `04`: `REACTIVE-03` (`ClearOwnedModifiers`); reactive card.
- `05`: `ABILITY-02`, `TRIGGER-04` (casting gate); `PROJECTILE-01` (in-flight registry and sweep, kept under the existing ID); holder and projectile cards.
- `01` and `06` were not attached and are unchanged. Phase 5 uses the motor's existing Disabled state; the motor's behaviour at the lock is taken from the manager's source comment (`MOVE-03`, `SIDESTEP-LIFE-02`).

**`00-Core.md`**
- Header; `CORE-02` (93 files / 74 entries; Phase 5 established; remaining gaps; post-lock gates); `CORE-04` (`[WM5]`, `[WM5-S]`, `[WM5-SR]`).
- Fragile-boundary row on the result lock rewritten; two rows added (normal-end order; routing and return). Load-routing row for battle endings. Ownership: six rows added, five edited. Inventory: seven rows added, two edited.
- `ORDER-01`: the manager's full step order, the idle branch, `CombatLock2D` as a meeting point, and why −12000 precedes the map. `SETUP-07`: battle-unit container and audit, `maxBattleSeconds` for test releases.
- Glossary: Result lock, Battle context, Corpse, Contact intent and the `UnitStatus` row (Escaped) updated; nine terms added.

**`90-Evidence.md`**: defines `[WM5]`, `[WM5-S]`, `[WM5-SR]` and records the Phase 5 basis, with the I11, I12 and I13 results kept apart. **`92-Validation.md`**: Phase 5 known-answer checks C1–C18, acceptance and structural checks; V9 restructure check 2 and Phase 4 checks B11, B12, B15, B16 and B17 are marked superseded.

**Departures from Roadmap §1.3/§5 notes, kept from I11–I13:** an abort also separates the armies, grants pair immunity and removes armies left empty (no retreat flags), so a still-touching pair does not restart a battle on the next step; routing never moves units, so dropped and quarantined units count as an army's remaining units; the I13 context menus are in `.Routing.cs`, because `.Debug.cs` was not attached to I13.

**Untested and carried forward:** the `[WM5-SR]` items (no-return-room refusal, OnDisable recovery, orphan arena, abort through an outside unfreeze with `DestroyArmy` inside `FrozenChanged`, a real teardown refusal or exception); the `[WM5-S]` list in `90-Evidence.md`. Later phases: the shrink trigger, retreat eligibility and Escaping, `UnitRetreated`, retreat recovery after 20 s, regeneration, battle-start modifiers, capture and sieges. Earlier carried items from M01–M04 are unchanged.

**Source facts worth a later decision (not defects, not changed):** the post-battle audit fails if any projectile is in flight, including one from a unit outside the battle; modifier application and scheduled modifier work are not gated by the lock (only their damage is); a step with no battle candidates leaves `LastSelectedBattle` holding an older pick; an orphan-arena removal schedules the audit without updating the audited battle id; the unused `sessionRefusalLogged` field is kept for older tools.

## V9.4.4 M04 fourth review — 2026-10-23

Documentation only; no code changed. One correction: the `01` Sidestep card no longer states that Phase 4 changed exactly four partials. It says which four were read for M04 and that the three partials not re-attached (`.Lifecycle`, `.Path`, `.Clash`) are not established as unchanged. Everything else re-checked without change: Core `ORDER-01`, `SETUP-01`–`SETUP-03`, `SETUP-07` and glossary; the `01`, `02`, `04`, `05`, `06` integration rules and cards; the `07` Phase 4 rules and cards; `90-Evidence.md` and `92-Validation.md`; the I11 handoff from V9.4.3. No rule ID added, renumbered or removed.

## V9.4.3 M04 third review — 2026-10-23

Documentation only; no code changed. Corrections after re-checking the delivered map against the Phase 4 source, Roadmap V6, the I10 handoff and the reported I08–I10 results:
- `BATTLE-UNIT-01` no longer says the manager never destroys the GameObject: an abort destroys remaining corpses (`BATTLE-ABORT-01`).
- `BATTLE-UNIT-02`: retreat on real units comes only from debug tools (`DebugSetRetreating` via `SetBodyRetreating`), not only from one method.
- `BATTLE-SPAWN-02`: Start runs later, before the unit's first update callback (the earlier "before the next Update" restated an engine-timing comment); a failed preparation keeps the writes already made until the next deployment; a self-reference removed.
- Evidence brackets narrowed: `WM-UNIT-01` (no named audit ran while units were Deployed), `WM-AUDIT-01` (Deployed/Escaped checks mid-battle are source only), `92-Validation.md` B12 (session-limit clause is source only).
- `BATTLE-EDGE-01`: "perpendicular edges at large views" is recorded as named by the I10 handoff, without assigning it a code path.
- `ARENA-VIEW-01`: the I10 handoff closes the search-reach item (24 ≥ 18); the half-width remark is marked as a source-side observation.
- V9.4 "source facts": `LogCollisionMatrixProblems` is called only by the debug test *in the attached source*; the arena partials were not attached (`SIDE-01` already says so).
- I11 handoff: `UnitClashParticipant2D.cs` and `UnitDeath2D.cs` moved from full source to on request; signatures aligned with the `BattleArena2D` card; lifecycle notes added for the session limit, abort reuse and corpse cleanup.
- Phase 3 contracts re-checked against the M03 text (`ARENA-LIFE-03`, `ARENA-VIEW-01`, `ARENA-BODY-01`/`02`, `ARENA-GATE-01`, `ARENA-DEST-01`); no change needed. No rule ID added, renumbered or removed.

## V9.4.2 M04 second review — 2026-10-22

Documentation only; no code changed. Corrections after a second full pass over the Phase 4 source, the I10 handoff and the reported Unity results:
- Escaped units are no longer called "re-parked": they are inactive under the parked-unit root and still owned, but not parked (glossary `Parked`). Edited: the `07` header, `WM-UNIT-01`, `BATTLE-UNIT-02`, the `UnitReserve2D` card, `CORE-02`, the ownership row and the `UnitStatus` glossary row.
- `MOVE-04` no longer claims that Awake sets clean speed on first activation; that `INIT-01` detail was not re-verified.
- `ORDER-01`: the Phase 2 calls from the world map into parked units are acknowledged; "ran in one scene" replaced by "one Play Mode session" (one scene is not established by source).
- `BATTLE-START-03`: a refused `TryCreate` also counts as a preparation failure. `BATTLE-SPAWN-02`: the arena/reserve block is checked before the drop checks. `BATTLE-ABORT-01`: the direct abort also runs when `Remove()` refuses because the arena is busy.
- `BATTLE-LIFE-01` bracket names what ran through the manager. `CORE-02` corpse-cleanup wording and the "dropped unit" glossary row aligned with `BATTLE-UNIT-01` and `BATTLE-SPAWN-02`.
- No rule ID added, renumbered or removed. The I11 handoff is unchanged.

## V9.4.1 M04 review — 2026-10-22

Documentation only; no code changed. Corrections after re-checking against the Phase 4 source, the I10 handoff and the reported Unity results:
- Evidence: escape handling under `BattleManager2D` is source-only. The I09 Retreat N result ran with the deleted spawner and covers the arena escape only (`BATTLE-UNIT-02`, `WM-UNIT-01`, `90-Evidence.md`, `92-Validation.md` B16). `MOVE-04` no longer claims a "facing inward" result.
- `BATTLE-START-01`, `WM-CONTACT-05`: dispatch stops only when the start commits; a preparation failure unfreezes the map in the same call and dispatch continues.
- `WM-XFER-02`: the battle gate is reachable during an abort, while teardown's unfreeze callbacks run before `ArenaRemoved` clears the battle.
- `ARENA-BODY-01`, `BATTLE-SPAWN-02`: bodies already released by the arena are not unregistered again; a dropped unit that was destroyed or moved to another owner does not stay in the army.
- `07` header, `CORE-02`: Escaped units are inactive in the reserve but not parked. `SETUP-01`: the child-detector requirement applies to battle binding.
- `92-Validation.md`: W15 ("nothing starts") and P12 (refusal text) marked superseded.
- I11 handoff: arena signatures no longer state parameter names or a delegate type the attached files do not show; `UnitMotor2D.cs` added on request.
- No rule ID added, renumbered or removed.

## V9.4 Phase 4 battle deployment — M04, 2026-10-22

Documentation only; no source code changed by this update. It covers World Map & Battle Roadmap V6 Phase 4 (implementation chats I08–I10). **Phase 4 is complete,** as the I10 handoff reports: every Phase 4 acceptance check passed in Unity. Source-reviewed-only paths and checks without a named Unity result are tagged separately (`[WM4-SR]`, `[WM4-S]`).

**`07-WorldMap-Battle.md`**
- Header describes the state after Phase 4.
- New "Side layers (Phase 4)" section: `SIDE-01` (side mapping and side layers), `SIDE-02` (binding contract).
- New "Battle deployment (Phase 4)" section with 11 rules: `BATTLE-LIFE-01`, `BATTLE-START-01`–`03`, `BATTLE-EDGE-01`, `BATTLE-SPAWN-01`/`02`, `BATTLE-UNIT-01`–`03`, `BATTLE-ABORT-01`.
- Rewritten in place, same IDs: `WM-SCOPE-01`, `WM-FREEZE-01` (Freeze toggle locked during an arena), `WM-UNIT-01` (deployment status, re-parking), `WM-UNIT-02` (statuses), `WM-OWN-01` (`ReleaseOwnership`), `WM-XFER-01`, `WM-XFER-02` (live `IsRosterInBattle`), `WM-ALLY-01`, `WM-AUDIT-01` (Deployed/Escaped checks), `WM-CONTACT-05`, `WM-BATTLE-01` (a FieldBattle pick starts a battle), `ARENA-VIEW-01` (search reach), `ARENA-BODY-01`, `ARENA-BODY-02`, `ARENA-GATE-01`, `ARENA-DEST-01` (motor and Sidestep read the override; prefabs must carry it), `ARENA-TEST-01`.
- 6 new cards for 7 files: `BattleManager2D` (one card with a two-partial table), `BattleTypes2D`, `BattleSides2D` (with `BattleSideLayers2D`), `UnitSideBinder2D`, `UnitSideBindingTest2D`, `BattleManagerDebugPanel2D`. `07` now holds 31 cards.
- Updated cards: `WorldMapController2D`, `UnitReserve2D`, `UnitHandle`, `UnitRosterTypes2D`, `WorldMapDebugPanel2D`, `BattleArena2D`, `BattleRules2D`, `BattleLayers2D`, `UnitDestinationOverride2D`.

**Integration rules in other files**
- `01`: the Phase 3 note is replaced; `TARGET-02` (mask rebinding, live filter refresh), `MOVE-01` (override steering, facing/velocity mismatch), `MOVE-02` (reconcile under an override), `MOVE-03` (override → Sidestep → seeking), new `MOVE-04` (deployment reset), `SIDESTEP-GATE-02` (pre-roll override gate), `SIDESTEP-PATH-01` (Perimeter in preflight), `SIDESTEP-LIFE-02` (override and Perimeter cancellation). Cards: targeting, motor, Sidestep.
- `02`: `CLASH-01` (empty-mask refusal, rebinding); detector card.
- `03`: `UnitDeath2D` card names the battle `Died` subscriber. No health rule changed.
- `04`: `REACTIVE-01` (Perimeter counts as Wall); reactive card.
- `05`: holder card (`SetEnemyClashMask`).
- `06`: `UI-01` rewritten (full refresh on re-enable after Start); UI card.

**`00-Core.md`**
- `CORE-02`: 86 files / 73 entries; Phase 4 scope established; remaining gaps listed; `IsRosterInBattle always returns false` removed; statuses Deployed, Escaped and Dead now set.
- `CORE-03`: the battle spawn path is the activator. `CORE-04`: `[WM4]`, `[WM4-S]`, `[WM4-SR]`.
- Four fragile-boundary rows added, two rewritten; a battle load-routing row; five ownership rows added, two edited; seven inventory rows.
- `ORDER-01`: `BattleManager2D` at −12000 and how the manager, map, arena, motor and Sidestep relate. No spawner row: `BattleUnitTestSpawner2D` (I09) was deleted in I10 and is not in the inventory.
- `INIT-01`, `SETUP-01` (override references, battle-deployable root requirements), `SETUP-02` (Perimeter optional for units), `SETUP-03` (enemy Clash mask; the old prefab value 16 reported wrong), `SETUP-07` (layer names, side layers, battle manager).
- Glossary: status row rewritten; Contact intent, Parked and Arena body rows updated; seven terms added.

**`90-Evidence.md`**: defines `[WM4]`, `[WM4-S]`, `[WM4-SR]` and records the Phase 4 basis. **`92-Validation.md`**: Phase 4 known-answer checks and acceptance; the V9 restructure check 15 (UI re-enable) is marked superseded.

**Departures from Roadmap §5/§6 notes, kept from I10:** spawn spacing uses `EntrySpawnGap` plus same-step reservations instead of `minSpawnSpacing`; a blocked side's credit is held at 1; a unit that cannot be deployed stays parked and owned; side layers are resolved separately from the arena's `BattleLayers2D`.

**Untested and carried forward:** the `[WM4-SR]` items (spawn exceptions, lost units, corpses with destruction on death off, "perpendicular edges at large views"); the `[WM4-S]` list in `90-Evidence.md`; "view size follows counts" still has no named Unity check. Phase 5 items: battle endings, result lock, routing, projectile sweep, removing armies left empty, corpses blocking entry strips. Earlier carried items from M01–M03 are unchanged.

**Source facts worth a later decision (not defects, not changed):** a start refusal waits 1 map second for every pair, not only the refused one; a unit dropped after binding keeps that side's layers until its next deployment; `Battle Units (active)` persists after battles; the world map now depends on `BattleManager2D.Instance`; `BattleSideLayers2D.LogCollisionMatrixProblems` is called only by the debug test; the enemy search reach (24) exceeds the maximum view half-height (18) but not its half-width at 16:9 (32).

**Not attached, comment-only per the I09 handoff:** the `UnitDestinationOverride2D` header and a stale `BattleLayers2D` comment ("Phase 4 renames").

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
