# Restructure validation

Method: manual, source-grounded two-pass document comparison by the same editor. Answers were obtained from the original map and then from the restructured rule homes, and compared for meaning. This is not a blind independent-model benchmark, source-code validation, compilation or Unity test. Automated checks verify structure and that each original evidence anchor/new rule ID exists; they do not prove semantic equivalence.

## 15 known-answer retrieval checks

Original line numbers refer to the exact attached `StickLegionsSystemMapV8Extended(10).md`. The two answers are recorded separately; PASS means the documented contract matches, including the deliberate “not established” answer.

| # | Question | Original-map answer and evidence | New-map answer and rule homes | Result |
| --- | --- | --- | --- | --- |
| 1 | When do triggered casts run relative to ClashResultApplied? | After both retained Clash reactions and before the next pair, only on normal transaction completion. (lines 312) | After first/second reactions; before next pair, after normal completion. `CLASH-13`; `CLASH-14` | PASS |
| 2 | Can a unit die after battle-result lock; which gates close then? | Not established: no battle-result lock or post-lock gate implementation is documented. (Whole-document absence check: no battle-result lock contract) | Undocumented boundary; neither post-lock death nor immunity is established. `CORE-02` | PASS |
| 3 | Can reconciliation between the two numerical snapshots invoke modifier mutation callbacks? | Both recipients settle first and both protected barriers precede either snapshot; protected reads reject mutation. (lines 306, 1161) | Both settle before either barrier; both barriers precede reads; protected mutation throws. `CLASH-08`; `CLASH-09`; `STAT-04` | PASS |
| 4 | If the first result kills its source, is the original second result skipped? | No: attempt the retained second receiver if it exists; receiver entry guards may still reject. (lines 309) | Source death does not cancel the second attempt; receiver existence/entry guards still apply. `CLASH-11`; `RESULT-01` | PASS |
| 5 | Can nested DoT or ability damage finalize XP during an active Clash deferral? | Finalizer leaves pending data untouched while victim deferral depth is active. (lines 377) | Nested finalizers obey victim-side deferral; pending data remains until allowed. `XP-02`; `XP-03` | PASS |
| 6 | Can a consumed one-Clash modifier still trigger; can Remove cancel it afterward? | Consumption keeps pending request; Remove cancels even after consumption, while its return bool describes active-instance removal only. (lines 487, 488) | Pending request survives consumption; explicit Remove cancels it even afterward with active-membership-only bool. `TRIGGER-06` | PASS |
| 7 | Does OnClashDodge include Projectile Dodge, and can a newly applied trigger join this pair? | No: holder processed Clash plus Dodged required; later instances do not join captured set. (lines 467, 463) | Projectile Dodge never qualifies; trigger capture excludes later applications. `TRIGGER-02`; `TRIGGER-03` | PASS |
| 8 | Whose level and aim does a triggered Projectile use; what happens to normal cooldowns? | Modifier holder level at execution; Clash opponent position refreshed if targetable enemy, otherwise saved; normal timers untouched. (lines 473, 477) | Holder is caster at execution, with refreshed-or-saved opponent aim; no normal timer reads/writes. `TRIGGER-03`; `TRIGGER-04`; `TRIGGER-05` | PASS |
| 9 | When do ability modifier values freeze, and what timestamp starts their lifetime? | List selection freezes at cast; numeric modifier values resolve/freeze at application using scheduler ApplicationTime, not cast time. (lines 449) | Frozen cast list, current asset values at application, per-instance freeze and ApplicationTime. `ABILITY-03`; `DELIVERY-03` | PASS |
| 10 | Can expired modifier instances still exist; is damage exactly at expiry owed? | Yes: stats reconcile away but identity/schedules/visual remain for owed work; damage at expiry occurs, child emission must be strictly earlier. (lines 400, 401, 404) | Expired stats do not erase owed work; expiry damage inclusive, child emissions exclusive. `MOD-TICK-01`; `MOD-CHILD-01`; `MOD-EXPIRY-01` | PASS |
| 11 | Does false CanTriggerDamageEffects suppress factual events or XP? | No: actual positive loss still emits events; subscribers enforce secondary-effect permission; XP has a separate flag. (lines 367) | Factual events remain; secondary effects inspect flag, attribution uses separate XP permission. `HEALTH-02`; `HEALTH-03`; `XP-01` | PASS |
| 12 | Must a target leave/re-enter the distance band before another Sidestep? | No: continuous in-band opportunity; pre-roll rejection has no cooldown, every roll/started cleanup leads to retry cooldown; target changes preserve it. (lines 252, 1004) | No crossing requirement; gates rerun each in-band step and unit-wide cooldown survives target change. `SIDESTEP-GATE-01`; `SIDESTEP-GATE-02` | PASS |
| 13 | Can a living Disabled unit receive Area damage/modifiers without being targetable? | Yes if payload-wide receiving prerequisites pass; missing modifier prerequisites rejects the whole hit before damage. (lines 437, 437) | CanBeTargeted is not the receiving gate; missing modifier dependencies excludes the whole payload. `DELIVERY-01` | PASS |
| 14 | Does pre-Awake Grant prove an inactive unit can cast before progression initializes? | No: Grant is supported before Awake/inactive, but casting requires initialized progression; no general inactive-unit guarantee follows. (lines 418, 424) | Grant supported, casting still gated by initialized progression; generic inactive lifecycle is undocumented. `CORE-03`; `ABILITY-01`; `ABILITY-02` | PASS |
| 15 | Does UI re-enable refresh missed health/level changes immediately? | No: only subscriptions resume; initial full refresh is Start, later refresh waits for applicable events. (lines 1869) | No immediate refresh on re-enable; resumes subscriptions and waits for event. `UI-01` | PASS |

## Structural checks

- 48/48 original source filenames retained; 42/42 logical entries have exactly one contract card.
- All 42 cards contain Owns, API, Events, Invariants and Never owns.
- 101 unique rule definitions; no duplicate IDs or unresolved rule references.
- Seven implementation files, six subsystem homes; support documents are not required for normal implementation context.
- No V8 section-number references in implementation documents. Evidence appendix retains clearly marked historical references.
- Both original graphs and duplicated dependency/bridge tables removed.
- No claim that battle lock/deployment/escape systems already exist; inactive Grant is kept distinct from initialized casting.

## Size comparison

Token figures below are estimates from Unicode character count ÷ 4, not model-tokenizer measurements. Exact UTF-8 bytes are also shown. Actual prompt size depends on tokenizer and source attachments.

| Document/context | UTF-8 bytes | Estimated tokens |
| --- | ---: | ---: |
| Original monolith | 173,262 | ≈43,269 |
| 00-Core.md | 22,467 | ≈5,607 |
| 01-Movement-Sidestep.md | 8,129 | ≈2,020 |
| 02-Clash-Transaction.md | 9,905 | ≈2,459 |
| 03-Health-Death-XP.md | 6,719 | ≈1,669 |
| 04-Stats-Modifiers.md | 15,570 | ≈3,876 |
| 05-Abilities-Triggered-Casts.md | 16,656 | ≈4,152 |
| 06-UI.md | 1,554 | ≈387 |
| All seven implementation files | 81,000 | ≈20,169 |
| Core + Movement + Modifiers | 46,166 | ≈11,503 |
| Core + Clash + Health + Modifiers | 54,661 | ≈13,611 |
| Core + Abilities + Clash + Modifiers | 64,598 | ≈16,093 |

Implementation-document character count is **53.4% lower** than the original, before selective loading. Evidence/history/this validation are excluded from routine context; even including those files does not require loading them into implementation chats. Subsystems were not padded to meet a token target.

## Limits and follow-up use

The 15 questions sample fragile integration contracts; they do not exhaustively prove every clause. Detailed formulas/defaults/signatures intentionally removed under the requested source-first policy need current C# source when edited. In a future implementation chat, attach Core plus routed subsystem files, ask for answers with stable IDs, and compare to this answer key. Run gameplay tests separately when code changes.

Original SHA-256: `1b19bd9828b89f31e408a4e7279871d8182b9b88f9ea63c9665f25b9e42fc5a3`.

---

# Phase 1 world-map validation — M01, 2026-10-15

Method: each answer was derived from the attached Phase 1 source (authority for behavior) and then checked against the I01/I02 results supplied in the M01 brief (authority for verification). The brief named I02 acceptance checks A–H as passed but did not give their individual contents, so Unity evidence below is attributed only to the result categories the brief names. No Unity session was run for this update.

Verification column: **Unity** = covered by a named I01/I02 result; **Self-test** = covered by `RunSelfTest` (33 pure checks), which passed in Unity; **Source** = derived from source only, not separately tested; **Source-reviewed** = post-I02 change, not Unity-verified.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| W1 | Same faction, neither retreating? | AlliedOrdinaryArmies → BounceTransfersAvailable | `WM-CLASS-01` | Unity (all 10 kinds) + Self-test |
| W2 | Same faction, exactly one retreating? | AlliedRetreatingAndOrdinaryArmy → RetreatTransfer; retreating donor is ArmyA | `WM-CLASS-01`, `WM-CLASS-02` | Unity + Self-test |
| W3 | Same faction, both retreating? | AlliedRetreatingArmies → RetreatMerge | `WM-CLASS-01` | Unity + Self-test |
| W4 | Retreating army touches allied settlement? | RetreatingArmyAlliedSettlement → JoinGarrison | `WM-CLASS-01` | Unity + Self-test |
| W5 | Ordinary army touches allied settlement? | OrdinaryArmyAlliedSettlement → Bounce | `WM-CLASS-01` | Unity + Self-test |
| W6 | Enemy armies, one or both retreating? | EnemyArmiesWithRetreating → Bounce (both-retreating included) | `WM-CLASS-01` | Unity + Self-test |
| W7 | Retreating army touches enemy settlement, empty or defended? | RetreatingArmyEnemySettlement → Bounce, count ignored | `WM-CLASS-01` | Unity + Self-test |
| W8 | Enemy armies, neither retreating? | EnemyOrdinaryArmies → FieldBattle; a battle candidate | `WM-CLASS-01`, `WM-BATTLE-01` | Unity + Self-test |
| W9 | Ordinary army touches enemy settlement with count 0? | OrdinaryArmyEmptyEnemySettlement → Capture; not a battle | `WM-CLASS-01` | Unity + Self-test |
| W10 | Ordinary army touches enemy settlement with count > 0? | OrdinaryArmyDefendedEnemySettlement → Siege; a battle candidate. Count is the `Settlement2D.DisplayCount` placeholder | `WM-CLASS-01` | Unity + Self-test |
| W11 | RetreatMerge with equal counts: who donates? | Higher id is ArmyA (donor); lower, older id receives. Unequal counts: smaller is ArmyA | `WM-CLASS-02` | Self-test (pure function); no separate live equal-count merge result was reported |
| W12 | Role order for every other army kind? | Lower id is ArmyA; settlement reports always put the army in ArmyA | `WM-CLASS-02` | Self-test (army default); settlement placement Source |
| W13 | Both armies' callbacks plus Enter+Stay fire in one step — how many reports? | One per pair per physics step; extras counted in `DuplicateCallbacksMerged` | `WM-CONTACT-02` | Unity (deduplication) |
| W14 | A pair touches for N consecutive steps — what is reported? | N reports via `LastStepContacts`/`ContactReported`; `IsNewContact` only on the first; episode counter and log once | `WM-CONTACT-03` | Unity (once per touch); per-step dispatch count Source |
| W15 | Several battle candidates in one step? | One chosen uniformly at random into `LastSelectedBattle`; continuing pairs are also candidates; nothing starts | `WM-BATTLE-01` | Unity (Converge 4); continuing-pair candidacy Source |
| W16 | What does freezing stop, and what not? | Army simulation, bounces, MapTime, contact intake/classification and spawning stop; `Time.timeScale` and `PhysicsStep` do not; episodes cleared, so still-touching pairs are new after unfreeze | `WM-FREEZE-01` | Unity (freeze with many armies) for motion; remaining clauses Source |
| W17 | How does MapTime advance? | +`Time.fixedDeltaTime` per ready, unfrozen controller FixedUpdate, after classification and the step increment | `WM-TIME-01` | Source |
| W18 | A `ContactReported` handler freezes the map mid-dispatch? | Dispatch stops without exception; `LastStepContacts` stays readable; undelivered pairs are not queued and reappear as new episodes after unfreeze | `WM-CONTACT-05` | Source-reviewed (post-I02 fix) |
| W19 | After `DestroyArmy`, can the army still be reported? | No: `IsRemoved` is set at once and intake/revalidation drop it; `ArmyDestroyed` fires once | `WM-REMOVE-01` | Unity (no unintended effects / regression); direct-Destroy path Source |
| W20 | Are bounds contacts reported? | No; they only bounce | `WM-CONTACT-01`, `WM-MOVE-02` | Source |

## Unity-verified versus source-only

- Unity-verified (I01/I02): compilation and Play Mode; constant-speed movement and clean bounces off bounds, settlements and armies; settlement CircleCollider2D retest; all 10 kinds reported once per touch; classifier self-test; deduplication; Converge 4 selection; no unintended gameplay effects; movement regression; freeze with many armies; fallback-rules cleanup.
- Source only: MapTime arithmetic and pause, PhysicsStep counting while frozen, continuing-pair battle candidacy, bounds non-reporting, direct-Destroy behavior, settlement id order, spawn refusal while frozen.
- Not Unity-verified: freeze-during-dispatch fix in `WorldMapController2D.Contacts.cs` (W18).

## Structural checks

- 60 source files / 53 logical entries; 12 new world-map files form 11 entries (two controller partials); each new entry has exactly one card in `07`.
- All 11 new cards contain Owns, API, Events, Invariants and Never owns.
- 22 new rule definitions (21 `WM-*` in `07`, `SETUP-06` in Core); every `WM-*`/`SETUP-06` reference in `00` and `07` resolves; no new duplicates.
- Existing V9 IDs in `00` were edited in place only (`CORE-02`, `CORE-04`, `ORDER-01`, `INIT-01`). `01`–`06` were not attached or changed, so the earlier 101-definition count was not re-run.

## Size

Same estimation method as above (characters ÷ 4).

| Document | UTF-8 bytes | Estimated tokens |
| --- | ---: | ---: |
| 00-Core.md (updated) | 27,547 | ≈6,872 |
| 07-WorldMap-Battle.md | 19,065 | ≈4,747 |
| Core + World map | 46,612 | ≈11,619 |

---

# Phase 2 world-map validation — M02, 2026-10-17

Method: answers are derived from the attached Phase 2 source, which is the authority for behaviour. They were checked against the I03–I05 results in the M02 brief, which is the authority for verification. The brief names the I05 checks and the Phase 2 acceptance list as passed. It reports the I04 tests (13) and I02/I03 regressions as passed without giving their contents, so Unity evidence is attributed only to named results. No Unity session was run for this update.

Verification: **Unity** = a named I05 check or Phase 2 acceptance item; **Source** = derived from source only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| P1 | Deploy Half with 7 eligible? | 3 units (⌊7/2⌋) | `WM-DEPLOY-01` | Unity (acceptance) |
| P2 | Deploy All with 250 eligible? | 200 (capped at `MaxUnitsPerArmy`) | `WM-DEPLOY-01` | Unity (acceptance) |
| P3 | Deploy Half with 1 eligible; with 0 eligible? | 1 (minimum 1); 0 eligible → Rejected, any existing pending request untouched | `WM-DEPLOY-01` | Source |
| P4 | Garrison of 10 settlement-only + 4 normal, Deploy All? | Army of 4; the 10 stay | `WM-DEPLOY-01`, `WM-XFER-01` | Unity (acceptance: settlement-only units stay) |
| P5 | Castle at 280 gains 50? | One 100-unit Overflow army; 230 stay | `WM-OVERFLOW-01` | Unity (acceptance; I05 same-step via garrison join) |
| P6 | Town at 450, all eligible, room available? | Three 100-unit armies in one step; 150 stay | `WM-OVERFLOW-01` | Source |
| P7 | Town at 250 with only 80 eligible? | No army; never smaller than 100 | `WM-OVERFLOW-01` | Source |
| P8 | Deployment with every placement slot blocked? | Nothing changes; request stays pending and retries every `PlacementRetrySeconds` of map time | `WM-DEPLOY-01`, `WM-DEPLOY-02` | Unity (acceptance: blocked placement loses nothing and retries) |
| P9 | Retreating army of 30 touches an ordinary allied army of 190? | Receiver 200; donor keeps 20 and stays retreating; retried while touching | `WM-ALLY-02` | Unity (I05 retreat transfer at capacity) |
| P10 | Two retreating allied armies of 150 each (ids 3 and 7)? | #7 donates 50 into #3 → 200 / 100; both stay retreating | `WM-ALLY-02`, `WM-CLASS-02` | Unity (I05 merge, equal sizes, partial) |
| P11 | Retreating army of 50 touches its allied castle at 280? | All 50 join (330), army removed, one 100-unit overflow army in the same step → 230 | `WM-ALLY-03`, `WM-TIME-01` | Unity (I05) |
| P12 | `TransferUnits` while the map is frozen? | Rejected ("map is frozen"); nothing moves | `WM-XFER-02` | Unity (I05 gates) |
| P13 | Units transferred into a roster: where do they go? | Back of the arrival order with new sequences; same GameObjects and handles | `WM-ROSTER-01`, `WM-XFER-01` | Unity (acceptance: same GameObjects); position Source |
| P14 | HighestLevelFirst with equal levels? | Earlier arrival first | `WM-ROSTER-01` | Unity (acceptance: ties, authored levels); live levels untested |
| P15 | `DestroyArmy` on an army holding units? | False with a warning, unless `discardUnits: true` (debug) | `WM-OWN-02`, `WM-REMOVE-01` | Source |
| P16 | Recruit while frozen? | Deferred; created on unfreeze before `FrozenChanged`, with the faction current then; deployment/overflow run on the next unfrozen step | `WM-RECRUIT-01`, `WM-FREEZE-01` | Source |
| P17 | An `ArmySpawned` handler calls `DeployArmy` for another settlement during settlement work? | Pending; runs later in the same step if that settlement has not been processed yet, otherwise next step | `WM-DEPLOY-01` | Source |

Phase 1 checks W10 and W11 now read real counts (`GarrisonCount`; `UnitCount`) instead of placeholders; W12 is unaffected. Answers are otherwise unchanged. W18 (freeze during dispatch) is now Unity-verified by I05.

## Unity-verified versus source-only

- **Unity-verified (I03–I05):** compilation and Play Mode; Phase 2 acceptance (GameObject identity through transfers, both order modes and ties, Half 7 → 3, All 250 → 200, settlement-only units stay, castle 280 + 50, blocked placement retries, allied interactions); the I05 checks (retreat transfer at and below capacity, merges smaller/equal/partial, garrison join with same-step overflow, selected transfers, gates, freeze during dispatch); the audit in every run.
- **Source only:** overflow multiples and the never-smaller rule beyond the castle case; frozen recruitment/deployment deferral; `DestroyArmy` refusal; owner-loss discard; source-settlement collision-ignore release; rollback; `Changed` batching; HighestLevelFirst with live levels; shutdown ordering.
- **Not re-run:** the final `.Contacts.cs` log-text change.

## Structural checks

- 68 source files / 58 logical entries. 8 new files form 5 new entries plus 3 controller partials. The 16 world-map entries each have exactly one card in `07`.
- 19 new `WM-*` rule definitions; 17 existing `WM-*` rules and `TARGET-01`, `CORE-02`–`CORE-04`, `ORDER-01`, `INIT-01`, `SETUP-06` edited in place. No ID renumbered or removed. Every `WM-*` ID referenced from `00`/`01`/`04`/`07` is defined in `07`.

---

# Phase 3 battle-arena validation — M03, 2026-10-18

Method: answers are derived from the attached Phase 3 source (I06–I07), which is the authority for behaviour, then checked against the I07 handoff's reported Unity results, which is the authority for verification. The handoff reports "I06 regressions" as passed without listing them, so coverage is attributed to that group only. No Unity session was run for this update.

Verification: **Unity** = a named I06/I07 result (I07 results ran in both perimeter modes); **Source** = derived from source only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| A1 | Start view size for total counts 0 / 80 / 300 / 1000 with the default curve? | 6 / 9.333 / 15 / 18 (piecewise linear, clamped to min 6 / max 18) | `ARENA-VIEW-01` | Source (curve arithmetic); no named Unity check of these values |
| A2 | Two regions contain the point with equal priority? | The earlier entry wins | `ARENA-TERRAIN-01` | Source; terrain selection covered by I06 regressions |
| A3 | Point exactly on a region's xMax? | Not contained (min inclusive, max exclusive); next candidate, fallback or generated ground | `ARENA-TERRAIN-01` | Source |
| A4 | `RequestViewSize` above the current target; below the minimum? | False, nothing changes; clamped to `CameraMinSize` | `ARENA-VIEW-01` | Source; shrink-to-min Unity |
| A5 | Create while the map is already frozen (debug freeze)? | Refused, `TotalFailedCreates` +1, nothing created | `ARENA-LIFE-02` | Source |
| A6 | Map unfrozen by something else while an arena exists? | Arena aborts with a warning: `TotalAborts` +1, world camera restored, root destroyed | `ARENA-LIFE-03` | I06 regressions (freeze ownership); detail Source |
| A7 | 20 create/remove cycles with bodies, audit one frame after the last removal? | Audit passes: one camera, one listener on the world camera, no roots, map unfrozen, no stray bodies, 0 leaks | `ARENA-LIFE-03`, `ARENA-AUDIT-01` | Unity (I07, both modes) |
| A8 | Inside bodies knocked outward at 150 u/s? | 0 leaks | `ARENA-PERIM-01`, `ARENA-BODY-02` | Unity (I07, both modes) |
| A9 | Shrink to min while bodies push outward? | 0 leaks; walls push bodies in | `ARENA-VIEW-01`, `ARENA-PERIM-01` | Unity (I07, both modes) |
| A10 | Debug "Retreat N" with N = 5 on Inside bodies? | 5 escapes, each body once | `ARENA-GATE-01` | Unity (I07; gate configuration not stated in the handoff) |
| A11 | Gate mask with a single edge? | 4 walls, 1 gate; retreating Inside bodies steer to the nearest point on that edge | `ARENA-PERIM-01`, `ARENA-GATE-01` | Unity (I07 single-gate test; the edge used was not stated) |
| A12 | Eject one Inside body? | Re-entries +1, leaks 0; it walks back in | `ARENA-BODY-02` | Unity (I07) |
| A13 | Window aspect changes so an Inside body ends up outside the view? | Re-entry, not a leak | `ARENA-BODY-02` | Unity (I07 resize) |
| A14 | Retreating body touches a gate after `EscapesAccepted` is set false? | No escape | `ARENA-GATE-01` | Source |
| A15 | `Set(point, Entry)` every step while entering: how often does `Changed` fire? | Once when the override starts; again only on a reason change or `Clear` | `ARENA-DEST-01` | Source |
| A16 | Register a body whose circle is a trigger? | Refused ("must be solid") | `ARENA-BODY-01` | Source |

## Unity-verified versus source-only

- **Unity-verified (I06–I07):** compilation; I06 regressions (create/remove, terrain selection, camera swap, freeze ownership, audit); in both perimeter modes: entry and containment, knockback 40/80/150 u/s with 0 leaks, shrink-to-min with 0 leaks, eject and walk back in, Retreat 5 → 5 escapes, single-gate test, Game-view resize, 20 cycles with bodies and a passing audit.
- **Credited only to the unlisted I06-regression group:** view size following counts; terrain selection; freeze ownership.
- **Source only:** curve and region arithmetic; known-answer values A1–A3; refusal order; throwing-`FrozenChanged` rollback (the I06 `frozenMap` fix); escape while Entering; closed gates; lost-body release; `Changed` delivery; second instance; missing rig or Wall layer.
- **Untested:** camera or rig disabled/destroyed mid-arena; scene unload with an arena; `ThrowOnFreezeTest` (not in the attached Battle files; no result reported); real units.

## Phase 3 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| Camera swaps both ways, never both or neither | I06 regressions; I07 20-cycle audit (restored state) | Passed |
| View size follows counts and never zooms out | Never zooms out: I07 shrink-to-min. Follows counts: only the unlisted I06-regression group | Passed as reported (count-following not individually named) |
| Bodies enter, cannot leave, are pushed in as it shrinks; fast knockback does not leak | I07 entry/containment, shrink, knockback (both modes) | Passed |
| Outside bodies steer back in | I07 eject | Passed |
| Repeated create/remove leaves nothing behind | I07 20 cycles, audit passing | Passed |

## Structural checks

- 79 source files / 67 logical entries. 11 new battle files form 9 entries (`BattleArena2D` has three partials); each has exactly one card in `07`, so `07` holds 25 cards.
- 15 new rule definitions: 14 `ARENA-*` in `07`, `SETUP-07` in Core. Edited in place: `WM-FREEZE-01`, `WM-BATTLE-01`, `CORE-02`–`CORE-04`, `ORDER-01`, `INIT-01`. No ID renumbered or removed. Every `ARENA-*` ID referenced from `00`, `01` and `07` is defined in `07`.
