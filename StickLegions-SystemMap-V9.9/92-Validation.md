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

Note added in M04: restructure check 15 above (UI re-enable) is superseded. Since I10, a re-enable after Start performs a full refresh (`UI-01`, Unity-verified in I10). The other 14 answers are unchanged. Phase 1 check W15 ("nothing starts") is also superseded: a FieldBattle pick now starts a battle (`WM-BATTLE-01`). In Phase 2 check P12 the refusal text is now "map is frozen (battle in progress)."; the behaviour is unchanged.

---

# Phase 4 battle-deployment validation — M04, 2026-10-22

Method: answers are derived from the attached Phase 4 source (I08–I10), which is the authority for behaviour, then checked against the reported I08, I09 and I10 results, which are the authority for verification. The I08 and I09 results come from the user's summary of those handoffs; the I09 runs used a temporary spawner that I10 deleted. No Unity session was run for this update.

Verification: **Unity** = a named I08/I09/I10 result; **Source** = derived from source only; **Source-reviewed** = listed as source-reviewed only by the I10 handoff.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| B1 | Spawn rate for starting counts 20 / 80 / 200? | 3 / 6 / 12 per second (floor(2 + N/20)) | `BATTLE-SPAWN-01` | Unity for 80 → 6/s and 20 → 3/s (I10); 200 → 12 Source |
| B2 | 80 units, never blocked: time from first to last spawn? | (80 − 1)/6 ≈ 13.17 s; the first unit spawns at commit | `BATTLE-SPAWN-01`, `BATTLE-START-03` | Unity (I10: about 13.2 s; first unit immediately) |
| B3 | 20 units, never blocked? | 19/3 ≈ 6.33 s | `BATTLE-SPAWN-01` | Source arithmetic |
| B4 | A side is held for several seconds, then released? | Credit stays at 1 while blocked; the next unit spawns at once, the rest at the normal cadence (no burst) | `BATTLE-SPAWN-01` | Unity (I10 no burst after Hold) |
| B5 | Armies of factions 3 and 5 meet? | Faction 5 is Side A (`Unit_SideA`, `Clash_SideA`), faction 3 Side B; both factions unchanged | `SIDE-01` | Source mapping; I10 units keep their faction and get the right side layers |
| B6 | Masks of a Side A unit after binding, with the I08 indices? | Targeting `1 << 16` = 65536 (`Unit_SideB`); detector and ability `1 << 12` = 4096 (`Clash_SideB`); root layer 15, Clash Range object layer 11 | `SIDE-02` | Unity (I08 binding test); numeric values from the reported indices |
| B7 | Bind a unit that is active in the hierarchy? | Refused; nothing written | `SIDE-02` | Unity (I08) |
| B8 | Rebind a unit that has already run Awake? | The live targeting filter follows the new mask | `TARGET-02`, `SIDE-02` | Unity (I08) |
| B9 | Pick positions A (0, 0), B (10, 3)? | Left/right axis; A enters Left, B Right | `BATTLE-EDGE-01` | Source |
| B10 | Pick positions A (0, 0), B (3, 3.1)? | Normalized \|x\| − \|y\| ≈ −0.023, within 0.05: the axis is random | `BATTLE-EDGE-01` | Source |
| B11 | A second FieldBattle pick in the same Play Mode session (for example after an abort)? | Refused; logged once; no counter and no retry wait | `BATTLE-START-01` | Unity (I10 only one battle per session); details Source |
| B12 | Forced preparation failure after the arena is built? | Arena removed, world camera restored, map unfrozen; `TotalPreparationFailures` +1; every start waits 1 map second; the session limit is not used up | `BATTLE-START-03` | Unity (I10 restores the map and camera); counters, wait and session limit Source |
| B13 | Sidestep while entering, or under any override? | Never: probe counters stay 0 | `SIDESTEP-GATE-02`, `SIDESTEP-LIFE-02`, `BATTLE-UNIT-03` | Unity (I09, I10) |
| B14 | Shrinking during a fight? | 0 leaks | `ARENA-PERIM-01`, `ARENA-BODY-02` | Unity (I09) |
| B15 | A deployed unit dies? | Body unregistered; ownership released as Dead at once (army count −1); `Dead` +1; GameObject tracked as a corpse until destroyed | `BATTLE-UNIT-01`, `WM-OWN-01` | Source; I10 abort audits pass after fights |
| B16 | A retreating deployed unit touches a gate? | Escaped: inactive under the parked root, still owned; never spawns again in that battle; transfers skip it | `BATTLE-UNIT-02` | Arena escape Unity (I09, with the deleted spawner, 0 lost); the manager's handling Source |
| B17 | Abort mid-battle? | Arena removed, map unfrozen, one camera; living and escaped units become parked Reserve; corpses destroyed; queued units untouched; empty armies kept | `BATTLE-ABORT-01` | Unity (I10 abort: audits pass, map unfrozen, one camera); other clauses Source |
| B18 | A transfer involving a battle army during the battle? | Refused | `WM-XFER-02` | Unity (I10); the refusing gate is not stated |
| B19 | The world-map Freeze toggle during a battle? | Disabled | `WM-FREEZE-01` | Unity (I10) |
| B20 | Stats UI re-enabled after Start? | Full refresh at once | `UI-01` | Unity (I10) |
| B21 | A unit is ejected into an entry strip? | It walks back in: re-entries +1, leaks 0 | `ARENA-BODY-02`, `MOVE-01` | Unity (I10 ejected units walk back in); counter values Source |
| B22 | A deployed unit is destroyed without dying, or deactivated outside the battle path? | Destroyed: lost, released as Discarded. Deactivated or body released: lost, re-parked as Reserve. `LostUnits` +1 either way | `BATTLE-UNIT-03` | Source-reviewed (I10); lost 0 in the I09/I10 runs |

## Unity-verified versus source-only

- **Unity-verified:** I08 layer rename and indices, binding test, side matrix report, negative asset-string check; I09 real units entering and fighting in both perimeter modes, probe 0 with Sidesteps inside, Retreat N with 0 lost, shrinking with 0 leaks, audits after deaths and arena removal (with the deleted spawner), I07 and non-battle regressions; I10 the full Phase 4 acceptance list plus abort, Freeze lock, in-battle transfer refusal and contact log text.
- **Source-reviewed only (I10 handoff):** spawn exception paths; lost-unit paths; corpses with destruction on death off; "perpendicular edges at large views".
- **Source only:** escape handling under `BattleManager2D`; edge arithmetic; refusal order and the global retry wait; spawn-rate values other than 3 and 6; drop cases; death release under the manager; Escaped → Reserve at abort; motor reset refusals; `Perimeter` handling in Sidestep and the reactive boost; live level reads after deployment.

## Phase 4 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| Enemy army contact starts a battle; one battle per Play Mode session | I10 battles start from the selected FieldBattle pick; only one battle per session | Passed |
| Units fight with the correct faction and layers | I10 faction, side layers and masks; I08 binding test; I09 real units fight | Passed |
| 80 vs 20 spawns at 6/s vs 3/s without overlaps | I10 | Passed |
| Blocked spawning resumes without a burst | I10 no burst after Hold | Passed (simulated block; a real full strip is not separately named) |
| Units don't Sidestep before entering | I10; probe 0 (I09, I10) | Passed |
| A failed preparation restores the map and camera | I10 | Passed |

Phase 4 is complete as reported.

## Structural checks

- 86 source files / 73 logical entries (seven Sidestep, five controller, three arena and two battle-manager partials). 7 new files form 6 entries; `BattleUnitTestSpawner2D` (I09) was deleted and is not listed. Every entry has exactly one card: 2 in Core, 4 in `01`, 6 in `02`, 6 in `03`, 11 in `04`, 12 in `05`, 1 in `06` and 31 in `07`.
- 171 unique rule definitions across `00`–`07`, no duplicates; 14 new (`MOVE-04`, `SIDE-01`/`02`, 11 `BATTLE-*`). Every backticked rule ID referenced from `00`–`07` is defined.
- Edited in place, same IDs: `TARGET-02`, `MOVE-01`–`MOVE-03`, `SIDESTEP-GATE-02`, `SIDESTEP-PATH-01`, `SIDESTEP-LIFE-02`, `CLASH-01`, `REACTIVE-01`, `UI-01`, `CORE-02`–`CORE-04`, `ORDER-01`, `INIT-01`, `SETUP-01`–`SETUP-03`, `SETUP-07`, and the `WM-*`/`ARENA-*` rules listed in the changelog. No ID renumbered or removed.

---

# Phase 5 battle-ending validation — M05, 2026-10-25

Method: answers are derived from the attached Phase 5 source (I11–I13), which is the authority for behaviour, then checked against the reported I11, I12 and I13 results, which are the authority for verification. Only I13 ran on the final source; I11 and I12 results are named with their chat. No Unity session was run for this update.

Verification: **Unity** = a named I11/I12/I13 result; **Source** = derived from source only; **Source-reviewed** = listed by the I13 handoff as untested, source review only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| C1 | The last unit of each side dies in the same step, no units queued? | Both deaths are in one batch: BothLose, reason Elimination; each side's final-batch deaths include its last unit | `BATTLE-END-01`, `BATTLE-END-02` | Unity (I12 mutual elimination; I13 test 4) |
| C2 | Side A: field 0, queued 0; side B: field 3, queued 5? | Decisive, Side B wins; B keeps all 8 living units (the 5 queued stay parked) | `BATTLE-END-02`, `BATTLE-ROUTE-01` | Unity for the 80 vs 20 decisive case (I13 test 2: winner keeps every unit, including queued ones); these counts Source |
| C3 | Both fields empty, A has 4 queued, B none? | No decision; spawning continues | `BATTLE-END-02` | Source |
| C4 | `maxBattleSeconds` 60, no elimination by 60 s, units still queued? | BothLose, reason Timeout; nobody dies; queued units stay parked; every army with units becomes retreating | `BATTLE-END-03`, `BATTLE-ROUTE-01` | Unity (I12 forced timeout; I13 test 3); the 60 s value Source |
| C5 | A unit of a locked battle receives DoT damage? | Rejected like an ineligible application: zero result, no events, `CombatLock2D.BlockedDamageCount` +1 | `HEALTH-01`, `BATTLE-LOCK-01` | Unity (I11 DoT gate, I11 source) |
| C6 | Two locked units' Clash Ranges overlap after the lock? | The director rejects the pair silently: no sample, cooldown, result or trigger warning | `CLASH-05`, `BATTLE-LOCK-01` | Unity (I11 lock test, I11 source) |
| C7 | The lock happens while a battle unit's projectile is flying? | Terminated (its enemy Clash mask includes a side's Clash layer); a projectile whose mask has neither side's Clash layer is left alone | `PROJECTILE-01`, `BATTLE-LOCK-01` | Unity for "no projectiles in flight" (I11); the non-battle case Source |
| C8 | `endDelaySeconds` 1 with a 0.02 s fixed step, locked at step N? | The end delay elapses and the normal end runs at step N + 50 | `BATTLE-LOCK-02` | Source arithmetic (the project's fixed step is not inspected) |
| C9 | Decisive 80 vs 20, the loser left with no units: what happens to the two armies? | Loser removed through `DestroyArmy`; winner keeps its flag and every unit and is left where it stood (a lone survivor is not moved); no pair immunity is granted, since only one army remains | `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `WM-IMMUNE-01` | Unity (I13 test 2: loser removed, winner ordinary with every unit, reappears just apart from the battle spot); no-immunity clause Source |
| C10 | Immunity of 1 s granted when a battle ends or aborts? | Counts from the unfreeze (MapTime stops while frozen); the pair bounces instead of battling for about 1 map second; each army can still fight a third army. After a normal end at least one surviving army is retreating, so the pair would bounce anyway; the immunity decides it only after an abort | `WM-IMMUNE-01`, `WM-BATTLE-01` | Unity (I13 test 6; the handoff does not say which end preceded it); the after-normal-end clause Source |
| C11 | A grant of 1 s for a pair with 3 s of immunity left? | Stays at 3 s: grants extend, never shorten; `TotalImmunityGrants` +1 | `WM-IMMUNE-01` | Source |
| C12 | No two free return spots exist when an enemy pair touches? | The battle does not start: `TotalStartsRefused` and `TotalNoReturnRoomRefusals` +1, pair immunity, no global retry wait. With immunity 0 the grant fails and the retry wait is set instead | `BATTLE-START-02`, `BATTLE-START-03` | Source-reviewed (I13: not set up) |
| C13 | First return-spot distance with `SpawnMargin` 0.2? | Half-distance R + 0.15 from the midpoint, so the centres start 2R + 0.3 apart; first along the line between the armies, then ±20°, ±40°, ±60°, ±90° | `BATTLE-RETURN-01` | Source (the asset's margin is not inspected; 0.2 is the roadmap default) |
| C14 | Abort during the locked delay? | No routing and no retreat flags; empty armies removed; survivors placed apart with pair immunity; lock released last; no `BattleEnded` | `BATTLE-ABORT-01` | Unity (I12; I13 test 5) |
| C15 | "Skip arena removal at next end"? | That end: `TotalEndFailures` and `CleanupProblems` +1, no `BattleEnded`. Next manager step: the arena is removed, `TotalEndRecoveries` +1, `BattleEnded` fires once | `BATTLE-END-04`, `BATTLE-RECOVER-01` | Unity (I13 test 7) |
| C16 | "Fail next unit park"? | The unit is quarantined (deactivated in place, still owned, status unchanged); the next manager step parks it as Reserve and `QuarantinedCount` is 0 | `BATTLE-UNIT-04` | Unity (I13 test 7) |
| C17 | 20 consecutive battles with mixed ends and aborts? | Ended + aborted = 20; audits failed 0, lost 0, cleanup problems 0, quarantined 0, deaths/escapes after lock 0/0 | `BATTLE-AUDIT-01`, `BATTLE-LOCK-01` | Unity (I13 test 8) |
| C18 | A FieldBattle pick while an earlier battle's end is still pending? | Ignored by the handler; `TryStartFieldBattle` refuses with no counter and no wait | `BATTLE-START-01`, `BATTLE-RECOVER-01` | Source |

Superseded earlier answers:
- V9 restructure check 2 ("Can a unit die after battle-result lock?"): now established. After the lock, `ApplyDamage` rejects damage for the battle's units; `ForceDeath` and clamp deaths are not gated, and the battle debug kill refuses after the lock (`BATTLE-LOCK-01`, `HEALTH-01`).
- Phase 4 B11: there is no session limit; a second battle starts normally once the first has ended or aborted (`BATTLE-START-01`).
- Phase 4 B12: the clause "the session limit is not used up" no longer applies; the rest is unchanged.
- Phase 4 B15: corpses are also destroyed at the normal end (`BATTLE-UNIT-01`).
- Phase 4 B16: an escaped unit is parked as Reserve at the normal end or abort; if parking fails at the escape it is quarantined (`BATTLE-UNIT-02`).
- Phase 4 B17: an abort now also sweeps battle projectiles, removes armies left empty and places the survivors apart with pair immunity; the lock is released last (`BATTLE-ABORT-01`).

## Unity-verified versus source-only

- **Unity, I13 (final source):** tests 1–9 as listed in `90-Evidence.md`.
- **Unity, I11/I12 (earlier source):** the I11 lock test (no deaths, Clashes, casts, trigger warnings or projectiles in flight); the I11 DoT gate; the I11 stalled spawn ending by timeout; the I12 normal end, forced timeout, survivor identity, mutual elimination with corpse cleanup, escape cleanup and aborts (before the I12 split), and the post-split regressions.
- **Source-reviewed only:** the no-return-room refusal; OnDisable recovery; orphan-arena removal; an abort through an outside unfreeze (`DestroyArmy` inside `FrozenChanged`); a real teardown refusal or exception.
- **Source only:** lock effect order and the ungated paths; end-delay arithmetic; where Update-driven deaths land; the stale-pick case; return-spot geometry and the end-time no-room fallback; immunity extension, pruning and refusals; the reactive clear; destroyed-quarantine handling; escape handling on the final source; `TryStartFieldBattle` refusal order.

## Phase 5 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| Results are correct for mutual last kills | I12 mutual elimination with corpse cleanup; I13 test 4 | Passed |
| Results are correct for remaining queues | I12 forced timeout with units queued; I13 test 3 (and test 2: winner keeps queued units) | Passed |
| Timeout kills nobody | I12 forced timeout; I13 test 3 | Passed |
| Nothing dies, Clashes or casts after lock, without warnings | I11 lock test (no deaths, Clashes, casts or trigger warnings; no projectiles in flight); I11 DoT gate; I13 20-battle deaths/escapes after lock 0/0 | Passed. The Clash and cast sub-items rest on the I11 test, not a rerun on the final source |
| 20 consecutive battles leave no lost units or leftovers | I13 test 8 | Passed |
| Cleanup causes no visible hitch | I13 test 9 (audit off) | Passed |
| Abort works at every stage | I10 failed preparation; I12 abort while fighting and in the locked delay; I13 test 5 (right after the start, fighting, locked delay) | Passed |
| Units keep HP/XP/level across battles | I12 survivor identity check; I13 test 2 | Passed |

Phase 5 is complete as reported: every acceptance item has a passing Unity result.

## Structural checks

- 93 source files / 74 logical entries (seven Sidestep, six controller, three arena and seven battle-manager partials). 7 new files: `CombatLock2D` (a new entry), five `BattleManager2D` partials (`.Start`, `.Ending`, `.Units`, `.Debug` in I12; `.Routing` in I13) and `WorldMapController2D.Separation` (I13). Every entry has exactly one card: 2 in Core, 4 in `01`, 6 in `02`, 6 in `03`, 11 in `04`, 12 in `05`, 1 in `06` and 32 in `07`.
- 12 new rule definitions, all in `07`: `BATTLE-END-01`–`BATTLE-END-04`, `BATTLE-LOCK-01`, `BATTLE-LOCK-02`, `BATTLE-UNIT-04`, `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `BATTLE-RECOVER-01`, `BATTLE-AUDIT-01`, `WM-IMMUNE-01`. 166 unique definitions in the attached `00`, `02`–`05` and `07`; with the 17 in `01` and `06` (M04 count; those files were not attached) the total is 183. No duplicates.
- Edited in place, same IDs: `HEALTH-01`, `CLASH-05`, `ABILITY-02`, `TRIGGER-04`, `PROJECTILE-01`, `REACTIVE-03`; `CORE-02`, `CORE-04`, `ORDER-01`, `SETUP-07`; `WM-SCOPE-01`, `WM-OWN-01`, `WM-XFER-02`, `WM-AUDIT-01`, `WM-REMOVE-01`, `WM-BATTLE-01`, `ARENA-VIEW-01`, `ARENA-GATE-01`, `ARENA-BODY-01` (pointer only), `BATTLE-LIFE-01`, `BATTLE-START-01`–`BATTLE-START-03`, `BATTLE-SPAWN-02`, `BATTLE-UNIT-01`–`BATTLE-UNIT-03`, `BATTLE-ABORT-01`. No ID renumbered or removed. The projectile registry and sweep extend `PROJECTILE-01` rather than taking a new ID.

---

# Phase 6 regeneration validation — M06, 2026-10-25

Method: answers are derived from the attached Phase 6 source (I14), which is the authority for behaviour. They were then checked against the I14 handoff's reported Unity results, which are the authority for verification. All I14 results ran on the final source. No Unity session was run for this update.

Verification: **Unity** = a named I14 result; **Source** = derived from source only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| D1 | A parked, previously activated unit is set to 50% and left unchanged: how many ticks to full, and does it wake? | 25 ticks (24 → 98%); it stays inactive throughout | `WM-REGEN-02`, `HEALTH-05` | Unity (I14 test 2) |
| D2 | The same for a deployed unit inside a long locked delay? | 25 ticks; the result lock does not block healing | `HEALTH-05`, `BATTLE-LOCK-01` | Unity (I14 extra check) |
| D3 | Scaled time from setting 50% to full, interval 2? | More than 48 s and at most 50 s: the tick clock is shared, so the first heal arrives within 2 s and 24 more follow 2 s apart | `WM-REGEN-01` | Source arithmetic; I14 test 2 reports ticks about 2 s apart |
| D4 | Fixed step 0.02 (as a float), interval 2: which steps tick? | Every 100th ready controller step. 100 × 0.02f ≈ 1.99999996 s passes the 1e-6 tolerance; the remainder (≈ −4.5e-8) resets to 0, so there is no drift | `WM-REGEN-01` | Source arithmetic (the project's fixed step is not inspected) |
| D5 | A garrison holds never-activated recruits at a tick? | Each is counted NeverActivated and skipped before its health is read; nothing wakes | `WM-REGEN-02`, `CORE-03` | Unity (I14 test 4: skipped, 0 awakened components in the audit) |
| D6 | A deployed unit dies; do later ticks heal its corpse? | No. The death removes it from the registry at once, so it is never in a later snapshot; if it were, Dead status and the heal's died/zero-health check would reject it | `WM-REGEN-02`, `BATTLE-UNIT-01`, `HEALTH-05` | Unity (the I14 Regen-tracking check, reported passed with the I15 run; updated M07). At M06 this was source only and the open Phase 6 item |
| D7 | The map is frozen for a whole battle: does regeneration pause? | No. It uses scaled fixed time on every ready step, frozen or not; MapTime stops | `WM-REGEN-01`, `WM-TIME-01`, `WM-FREEZE-01` | Unity (I14 test 3) |
| D8 | An Escaped unit at 40% while its battle is still running? | Healed by the next tick while inactive, still Escaped, never woken | `WM-REGEN-02`, `HEALTH-05` | Source (Escaped by name untested) |
| D9 | A definition with `healthRegenFraction` 0? | Never healed; counted Ineligible on every tick | `WM-REGEN-02` | Source |
| D10 | Maximum 10, current 9.9, fraction 0.02? | Heals to exactly 10 (capped); the next tick returns AlreadyFull with no HealthChanged | `HEALTH-05` | Source |
| D11 | A unit levels up and its maximum rises: does current health rise? | Not by the level-up (`HEALTH-04`); only later regeneration ticks raise it | `HEALTH-04`, `HEALTH-05` | Source (existing rule) |
| D12 | A unit healed while parked is redeployed? | Its health bar shows the healed value at once (full refresh on re-enable) | `UI-01`, `WM-REGEN-02` | Unity (I14 test 5) |

Superseded earlier answers: none. Phase 5's "HP kept across battles" (C2, C9 and the acceptance table) still holds at parking; regeneration raises HP afterwards (`BATTLE-UNIT-04`).

## Unity-verified versus source-only

- **Unity (I14):** compilation and the Regen menus; the parked 25-tick case; deployed healing; ticking while frozen and in the locked delay; the deployed 25-tick case inside a locked delay; never-activated units skipped with 0 awakened components; the health bar after redeployment; Phase 5 regressions (group, individual checks not listed).
- **Unity, reported with I15:** dead units skipped (D6), through the Regen-tracking check (updated M07).
- **Source only:** Escaped and quarantined units by name; the Failed path; a zero fraction; the `didAwake` guard; the inactive-unit refusal instead of a clamp death; the active-path clamp death; the leftover reset; the tolerance arithmetic (D3, D4); a player build without the debug setter.

## Phase 6 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| An unchanged unit at 50% HP is full after 25 ticks, parked | I14 test 2 | Passed |
| … or deployed | I14 extra check (tick 25, inside a locked delay); test 3 (deployed units heal) | Passed |
| Never-activated units are skipped | I14 test 4 | Passed |
| Dead units are skipped | The I14 Regen-tracking check, reported passed with the I15 run (M07) | Passed (at M06: not passed, untested) |
| Health display is correct after redeployment | I14 test 5 | Passed |

**Phase 6 is complete (updated M07).** At M06 it was not marked complete because the dead-units item had no Unity result. The I15 handoff reports that the closing check below passed, so every acceptance item now has a passing result. The check as proposed at M06: with tick logging on, track a deployed unit ("Regen - Set selected unit to 50% HP and track it"). Let it die while the battle continues for at least two more ticks: let it fall in combat, or kill its side while that side still has queued units. Every later tick log should report the tracked unit as gone, or at 0 HP with status Dead. "Regen - Log status" should show the registry smaller by the deaths, Failed should stay 0, and no corpse's health bar should refill.

## Structural checks

- 95 source files / 75 logical entries (seven Sidestep, seven controller, three arena and seven battle-manager partials). 2 new files: `UnitRegeneration2D` (a new entry, card in `07`) and `WorldMapController2D.Regeneration` (a seventh controller partial). Every entry has exactly one card: 2 in Core, 4 in `01`, 6 in `02`, 6 in `03`, 11 in `04`, 12 in `05`, 1 in `06` and 33 in `07`.
- 3 new rule definitions: `HEALTH-05` in `03`, `WM-REGEN-01` and `WM-REGEN-02` in `07`. 130 unique definitions in the attached `00`, `03`, `04`, `06` and `07` (127 before), with no duplicates. With `01`, `02` and `05` unchanged since M05, the total is 186.
- Edited in place, same IDs: `HEALTH-04`; `UI-01` (bracket only); `CORE-02`, `CORE-03`, `CORE-04`, `ORDER-01`, `INIT-01` (pointer); `WM-LIFE-01`, `WM-TIME-01`, `WM-FREEZE-01`, `WM-UNIT-02`, `WM-AUDIT-01` (bracket only, V9.6.1), `BATTLE-LOCK-01`, `BATTLE-UNIT-04`. No ID renumbered or removed. Every backticked rule ID in the attached files is defined in them or belongs to the unattached `01`, `02` or `05`.

---

# Phase 7 retreat validation — M07, 2026-10-26

Method: answers are derived from the attached Phase 7 source (I15 unit retreat, I16 retreating armies), which is the authority for behaviour. They were then checked against the I15 and I16 handoffs' reported Unity results, which are the authority for verification. No Unity session was run for this update.

Verification: **Unity** = a named I15 or I16 result; **Source** = derived from source only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| E1 | Eligible field units are set to 31%, then 20% HP; ineligible ones to 20%? | 31%: no retreat. 20%: each eligible unit is latched and starts retreating at the next manager step. Ineligible units never retreat | `BATTLE-RETREAT-02`, `BATTLE-RETREAT-03` | Unity (I15 30% threshold) |
| E2 | An eligible unit at exactly 30% of its maximum? | No latch: the test is strictly below `escapeHealthFraction` × maximum | `BATTLE-RETREAT-02` | Source |
| E3 | A retreating unit is healed to 100%? | Still retreating: status Escaping, still steering to a gate. Nothing clears the latch | `BATTLE-RETREAT-02`, `BATTLE-RETREAT-04` | Unity (I15 healing never cancels a retreat) |
| E4 | `escapeChance` 1, or 0? | 1: every queued unit is eligible. 0: none. In between: `Random.value < chance`, once per unit per battle at commit | `BATTLE-RETREAT-01` | Source |
| E5 | A refused or failed start (no return room, preparation failure)? | No rolls: eligibility is rolled only at commit, after every check and the arena creation | `BATTLE-RETREAT-01`, `BATTLE-START-03` | Source |
| E6 | An eligible queued unit that was parked at 20% HP deploys later in the battle? | Latched in the spawn call, after it is listed; its retreat starts in the same manager step, after spawning. For the first spawn of a battle (inside the map's dispatch) it starts at the next manager step. It may then escape almost at once through the gate just inside its own edge | `BATTLE-RETREAT-02`, `BATTLE-RETREAT-03`, `BATTLE-RETREAT-04` | Unity (I15 deployment check); timing and the early escape Source |
| E7 | Clash damage in step N takes an eligible unit to 25%? | The latch is recorded inside the transaction; the retreat starts in the manager's FixedUpdate of step N + 1, after that step's end check. If that check locks the result, it never starts | `BATTLE-RETREAT-02`, `BATTLE-RETREAT-03`, `BATTLE-LOCK-01` | Source |
| E8 | A retreat starts with a retreat modifier configured: when is it applied, by whom, how often? | Once, in the first manager step of a frame later than the unit's activation, once its ability holder (if enabled) has run Start. The unit is its own caster at its current level, origin Ability. `ModifierDeliveriesCompleted` +1, side `RetreatModifiersApplied` +1 | `BATTLE-DELIVER-01` | Unity for "applied once" (I15); timing Source. *Updated M08:* the shared wait (a later frame than activation, the holder's `didStart`) is Unity through the I17 battle-start deliveries (F1) |
| E9 | A unit holding the retreat modifier escapes, or the battle ends or aborts? | The modifier goes when parking disables its modifier runtime; nothing reaches the map or another battle | `BATTLE-DELIVER-02`, `MOD-REMOVE-01` | Unity (I15 removed when the unit leaves) |
| E10 | A unit escapes, or the result locks, before a pending delivery is applied? | The request is dropped (`ModifierDeliveriesDropped` +1); the unit never receives that modifier | `BATTLE-DELIVER-02` | Source at M07. *Updated M08:* the lock is Unity through I17 battle-start requests held pending (F6); the escape is still Source |
| E11 | A retreating unit walks past an enemy in Clash range? | It Clashes, can Dodge, casts and is targeted as before; it does not Sidestep under its Retreat override | `BATTLE-RETREAT-04`, `SIDESTEP-GATE-02` | Unity (the Phase 7 closing check, step 3, reported passed after M07). Source only at M07 |
| E12 | After several escapes on one side, with no lost or quarantined units: field + queued + dead + escaped + dropped? | Equals that side's `StartingCount`: the queue only moves forward and an escaped unit is never queued again, so it never respawns that battle | `BATTLE-UNIT-02`, `BATTLE-SPAWN-01` | Unity (the Phase 7 closing check, step 4, reported passed after M07). Source only at M07 |
| E13 | A retreating unit is one step from a gate when the result locks? | It does not escape (escapes closed), no new retreat starts, pending deliveries are dropped; at the normal end it is parked as Reserve | `BATTLE-LOCK-01`, `BATTLE-RETREAT-04` | Unity for "no escape after the lock" (I15); the rest Source |
| E14 | Abort while units are retreating? | Escaping units are parked as Reserve with the other field units, escaped ones as Reserve, no retreat flags, the lock released last | `BATTLE-ABORT-01` | Unity (I15) |
| E15 | A Decisive loser is flagged at a normal end, fixed step 0.02 s: when does it recover? | 20 map s after the unfreeze: at the 1000th unfrozen controller step, counting the step in which the end unfroze the map. 1000 × 0.02f ≈ 19.99999955 s, within the 1e-4 tolerance; step 999 is not due | `WM-RETREAT-01`, `BATTLE-ROUTE-01` | Unity for "20 map s after the unfreeze" (I16 test 2); the step count Source arithmetic (the project's fixed step is not inspected) |
| E16 | A timeout leaves both armies with units? | Both are flagged in one routing call at the same MapTime, so both recover in the same step | `WM-RETREAT-01` | Unity (I16 test 3) |
| E17 | Retreating A (8 units, 12 s left) touches allied retreating B (30 units, 5 s left), room for all? | A merges into B and is removed with its timer; B keeps 5 s. If B had room for only 3, A keeps its 5 units, stays retreating and keeps its 12 s | `WM-ALLY-02`, `WM-RETREAT-01` | Unity (I16 test 4); these numbers Source |
| E18 | A retreating army with 3 s left joins an allied garrison? | Removed with its timer; it never recovers and `TotalRetreatRecoveries` does not change | `WM-ALLY-03`, `WM-RETREAT-01` | Unity (I16 test 5) |
| E19 | A retreating army with 5 s left while the map is frozen for 30 s? | Still 5 s left at the unfreeze: MapTime stops while frozen | `WM-RETREAT-01`, `WM-FREEZE-01` | Unity (I16 test 6) |
| E20 | `retreatRecoverySeconds` 0 after a normal end that leaves both armies? | The flags clear in the controller step right after the unfreeze; the pair's 1 s immunity then keeps them from fighting each other at once | `WM-RETREAT-01`, `WM-IMMUNE-01` | Source (I16: 0 untested) |
| E21 | A recovered army touches an ordinary enemy army? | From the next step its contact classifies as EnemyOrdinaryArmies → FieldBattle candidate (unless the pair is immune) | `WM-RETREAT-01`, `WM-CLASS-01` | Unity (I16 test 7) |
| E22 | An eligible field unit at 32 / 100 HP levels up in battle and its maximum becomes 110? | It latches without taking damage: 32 < 0.3 × 110 = 33, and the maximum-only `HealthChanged` reaches the latch | `BATTLE-RETREAT-02`, `HEALTH-04` | Source (V9.7.3) |

Superseded earlier answers:
- Phase 4 B16: the unit is Escaping (not Deployed) before it reaches a gate; Escaped as before after it (`BATTLE-UNIT-02`).
- Phase 5 C10: still holds with the default 20 s recovery. With `retreatRecoverySeconds` below the immunity, the immunity also decides the outcome after a normal end (E20, `WM-IMMUNE-01`).
- Phase 6 D6: now Unity (updated in place above).

## Unity-verified versus source-only

- **Unity, I15:** compilation; natural retreat, gate escape and escape counts; the 31%/20%/ineligible threshold check; healing never cancels a retreat; the retreat modifier applied once and removed when the unit leaves; the deployment check; no escape after the lock; abort while units retreat; Phase 5 and 6 regressions (group); the I14 Regen-tracking check (Phase 6).
- **Unity, I16:** compilation; a Decisive loser recovers 20 map s after the unfreeze; both timeout armies recover in the same step; merge timers kept, including a partial merge; absorbed or emptied armies never recover; freezing pauses the timer; Flag-, Flag+ and T; a recovered army can battle again; Phase 5 and 6 regressions (group).
- **Unity, Phase 7 closing check** (reported after M07, as a group): retreating units still fight with no Sidestep (E11); escaped units never respawn (E12).
- **Source only:** the `[WM7-S]` list in `90-Evidence.md`.

## Phase 7 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| Healing never cancels retreat | I15 "healing never cancels a retreat" | Passed |
| Retreating units keep fighting | The Phase 7 closing check, step 3 (reported after M07) | Passed (at M07: not passed, untested) |
| … and receive the retreat modifier once, which is removed with them | I15 "applied once and removed when the unit leaves" | Passed |
| Escape happens once and updates counts immediately | I15 "gate escape and escape counts" | Passed |
| Escaped units never respawn that battle | The Phase 7 closing check, step 4 (reported after M07) | Passed (at M07: not passed, untested) |
| No escape after lock | I15 "no escape after the lock" | Passed |
| Losers and timeout survivors become retreating and recover after 20 s | I16 tests 2 and 3 | Passed |
| … unless absorbed | I16 test 5 | Passed |

**Phase 7 is complete (updated V9.7.1).** At M07 it was not marked complete because two acceptance items had no named Unity result. The user reported after M07 that the closing check below passed, so every acceptance item now has a passing result. The check as proposed at M07:
1. Set `escapeChance` 1 and author a visible retreat modifier, then start a battle with enough units that each side still has units queued after the first retreats, for example 80 vs 80.
2. Turn on `logRetreats` and run "Retreat: Eligible field units → 20% HP (up to 5)" a few times, or let units fall in combat.
3. **Fighting:** watch a retreating unit that walks past enemies on its way to a gate. It should still Clash and take or deal Clash damage, Dodge, and cast. Enemies should keep targeting it, and the Sidestep probe ("Sidestep under override") should stay 0.
4. **No respawn:** once several units of a side have escaped, run "Battle: Log status" while that side still has queued units, and again after its queue is empty. Each time field + queued + dead + escaped + dropped should equal that side's starting count (E12), and no escaped unit should appear on the field again.

## Structural checks

- 98 source files / 75 logical entries (seven Sidestep, eight controller, three arena and nine battle-manager partials). 3 new files, all partials: `BattleManager2D.Retreat`, `BattleManager2D.Delivery` (I15) and `WorldMapController2D.Retreat` (I16). Every entry has exactly one card: 2 in Core, 4 in `01`, 6 in `02`, 6 in `03`, 11 in `04`, 12 in `05`, 1 in `06` and 33 in `07`.
- 7 new rule definitions, all in `07`: `BATTLE-RETREAT-01`–`BATTLE-RETREAT-04`, `BATTLE-DELIVER-01`, `BATTLE-DELIVER-02`, `WM-RETREAT-01`. 168 unique definitions in the attached `00`, `01`, `03`, `04`, `05` and `07`, with no duplicates; with `02` and `06` unchanged since M05 and M06, the total is 193 (186 before).
- Edited in place, same IDs: `CORE-02`, `CORE-03`, `CORE-04`, `ORDER-01`; `HEALTH-05` (debug-setter callers); `MOD-APPLY-01` (new caller); `WM-SCOPE-01`, `WM-TIME-01`, `WM-FREEZE-01`, `WM-UNIT-01`, `WM-UNIT-02`, `WM-OWN-01`, `WM-XFER-01`, `WM-ALLY-02`, `WM-ALLY-03`, `WM-AUDIT-01`, `WM-IMMUNE-01`, `WM-REGEN-02`, `ARENA-GATE-01`, `BATTLE-LIFE-01`, `BATTLE-START-03`, `BATTLE-SPAWN-02`, `BATTLE-UNIT-01`, `BATTLE-UNIT-02`, `BATTLE-UNIT-04`, `BATTLE-ABORT-01`, `BATTLE-LOCK-01`, `BATTLE-END-04`, `BATTLE-ROUTE-01`. No ID renumbered or removed. Every backticked rule ID in the attached files is defined in them or belongs to the unattached `02` or `06`.

---

# Phase 8 battle-start modifier validation — M08, 2026-10-27

Method: answers are derived from the attached Phase 8 source (I17) and the re-attached `UnitModifierRuntime2D.cs`, which are the authority for behaviour. They were then checked against the I17 handoff's reported Unity results, which are the authority for verification. The handoff reports one result per acceptance item, with short observations, not individual test numbers. No Unity session was run for this update.

Verification: **Unity** = an I17 result (items 1–7 in `90-Evidence.md`, Phase 8 basis); **Source** = derived from source only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| F1 | A unit with an enabled ability holder is activated in frame N: when can its battle-start list be applied? | In the first manager FixedUpdate of a frame later than N in which its holder's `didStart` is true, never in frame N. The audit then shows its delivery frame > activation frame and holder started true | `BATTLE-DELIVER-01`, `BATTLE-DELIVER-03` | Unity (I17 item 1) |
| F2 | A unit levels from 3 to 4 during battle 1 after its delivery. Which level does battle 2's delivery use? | 4: the delivery reads `UnitHandle.CurrentLevel` when it applies, and the unit is its own caster at that level | `BATTLE-DELIVER-01` | Unity (I17 item 2) |
| F3 | The field list is [A, (empty), B]. What is frozen and in what order is it applied? | [A, B] on both sides, applied A then B; with `logRetreats` on, the delivery log shows `[+A, +B]` when both apply | `BATTLE-DELIVER-03`, `BATTLE-DELIVER-01` | Unity for the order (I17 item 3); the empty-entry removal and both sides Source |
| F4 | Field list [A, B], a retreat modifier R configured, `escapeChance` 1, durations longer than the battle; a unit deploys and later retreats. Which instances does it carry? | One A, one B and one retreat modifier: one BattleStart and one Retreat request, each once. Per side, requested = delivered + dropped (+ pending) | `BATTLE-DELIVER-03`, `BATTLE-DELIVER-01` | Unity (I17 item 4) |
| F5 | After a battle ends, how many modifier instances does a parked unit carry, and what does a re-fight of the same armies deliver? | 0, because parking ran the runtime's OnDisable `ClearAll`. The re-fight delivers the list again, once per unit, to new `BattleUnit` records | `BATTLE-DELIVER-02`, `MOD-REMOVE-01` | Unity (I17 item 5) |
| F6 | Delivery hold on, requests pending: a unit dies; then the result locks; separately, a battle aborts. | Death: that unit's requests drop (side `BattleStartDropped` +1, `ModifierDeliveriesDropped` +1). Lock and abort: every pending request drops and none is pending after the lock. Requested = delivered + dropped still holds | `BATTLE-DELIVER-02`, `BATTLE-LOCK-01`, `BATTLE-ABORT-01` | Unity (I17 item 6) |
| F7 | Entry C is added to the field list mid-battle. What changes? | Nothing in the running battle (the lists were copied at commit); the next battle freezes [A, B, C] | `BATTLE-DELIVER-03`, `BATTLE-START-03` | Unity for "nothing changes" (I17 item 7); the next battle Source |
| F8 | A listed asset's magnitude is edited mid-battle, with the list unchanged. | Units delivered after the edit get the new values, and earlier deliveries keep theirs. The freeze copies references; values resolve and freeze at each application | `BATTLE-DELIVER-03`, `MOD-APPLY-02` | Source |
| F9 | The field list is empty. | Source "field list (empty)"; no BattleStart request, no battle-start counter changes; the audit reports no problem | `BATTLE-DELIVER-03` | Source |
| F10 | An eligible unit deploys below 30% HP with a nonempty field list and a retreat modifier configured. Which modifier applies first? | The battle-start list. It is requested in the spawn call before the deployment latch, and the retreat request is queued later, at the retreat start. Both share the wait, and the queue applies in order | `BATTLE-DELIVER-03`, `BATTLE-SPAWN-02`, `BATTLE-RETREAT-03` | Source |
| F11 | Phase 9 calls the siege freeze for a castle. What do the sides get, and what if the field freeze also ran? | Side A (defender) gets the copied castle defender list and Side B nothing; the source reads "Castle defender list". A second freeze throws `InvalidOperationException` at commit, outside the preparation's exception handling (`BATTLE-START-03`). *Updated M09:* since I18 the commit is inside that handling, so the throw would remove the arena and count as a preparation failure; exactly one freeze is called | `BATTLE-DELIVER-03` | Source at M08 (no caller). *Updated M09:* the castle list on defenders only is Unity (I18 check 6); the throw is still Source |
| F12 | A deployed unit has no `UnitModifierRuntime2D`. | Its request is dropped with a warning when it would apply (side `BattleStartDropped` +1); it is not counted delivered | `BATTLE-DELIVER-01` | Source (I17 untested list) |
| F13 | One entry of the list is rejected at application. | The other entries still apply in order. The rejected one is logged with its reason and never retried. The delivery counts as delivered, and the side's `BattleStartEntriesRejected` rises by 1 | `BATTLE-DELIVER-01`, `MOD-APPLY-02` | Source (I17 untested list) |
| F14 | The unit's ability holder is disabled. | The wait needs only a frame later than activation; the delivery records holder started as true | `BATTLE-DELIVER-01` | Source (I17 untested list) |

Superseded or updated earlier answers:
- Phase 7 E8 and E10: verification updated in place above (the shared wait and the drop at the lock now have Unity results through I17).
- No earlier answer changes. Phase 7 E8's timing for the retreat modifier also holds for battle-start lists (same path).

## Unity-verified versus source-only

- **Unity, I17:** compilation; the seven reported items: waits until started (with the audit OK), current level (including a level-up carried into the next battle), list order, once per unit (with the retreat modifier alongside), no leak into another battle (0 instances when parked; fresh delivery on a re-fight), drops on death, at the lock and at an abort with the hold, lists frozen at commit.
- **Source only:** the `[WM8-S]` list in `90-Evidence.md`: the siege freeze and its second-freeze throw; the missing-runtime drop; rejected entries and the destroyed-asset log name; the disabled-holder wait; null-entry removal and the empty list; the defender-kind mapping; the battle-start-before-retreat order; an exception inside `ApplyOrdered`; drops at an escape, by the sweep and at the normal end; the hold persisting across battles; frame-rate dependence of the wait.

## Phase 8 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| Delivery waits until the unit has started | I17 item 1 (every delivery at least one frame after activation; the Battle-start audit reported OK) | Passed |
| … uses its current level | I17 item 2 | Passed |
| … and list order | I17 item 3 (`[+A, +B]`) | Passed |
| … happens once | I17 item 4 | Passed |
| … and never leaks into another battle | I17 item 5 | Passed |

Phase 8's goal also names "defender lists configured for Phase 9". That is not an acceptance check: the two lists exist as settings and the siege freeze exists without a caller (source only; to be exercised in Phase 9).

**Phase 8 is complete:** every acceptance item has a passing Unity result.

## Structural checks

- 98 source files / 75 logical entries, unchanged: I17 added no file. The changed files are `BattleRules2D`, `BattleTypes2D`, `BattleManagerDebugPanel2D` and five `BattleManager2D` partials (`.cs`, `.Start`, `.Spawning`, `.Delivery`, `.Debug`). Every entry has exactly one card: 2 in Core, 4 in `01`, 6 in `02`, 6 in `03`, 11 in `04`, 12 in `05`, 1 in `06` and 33 in `07`.
- 1 new rule definition, in `07`: `BATTLE-DELIVER-03`. There are 144 unique definitions in the attached `00`, `04`, `05` and `07` (143 before), with no duplicates. With `01`, `02`, `03` and `06` unchanged since M07, M05, M07 and M06, the total is 194 (193 before).
- Edited in place, same IDs: `CORE-02`, `CORE-04`, `ORDER-01` (manager row); `BATTLE-LIFE-01` (step order, V9.8.1), `BATTLE-START-03`, `BATTLE-SPAWN-02`, `BATTLE-LOCK-01` (step 6 and bracket), `BATTLE-ABORT-01` (bracket only), `BATTLE-UNIT-04` (step 3 and bracket, V9.8.1), `BATTLE-DELIVER-01`, `BATTLE-DELIVER-02`. Card-only changes in `04` (`UnitModifierRuntime2D`) and `05` (`UnitAbilityHolder2D`); no `04` or `05` rule text changed. No ID renumbered or removed. Every backticked rule ID in the attached files is defined in them or belongs to the unattached `01`, `02`, `03` or `06`.

---

# Phase 9 conquest and sieges validation — M09, 2026-10-31

Method: answers are derived from the attached Phase 9 source (I18 capture and siege start, I19 siege outcomes and pending retreat rosters), which is the authority for behaviour. They were then checked against the I18 and I19 handoffs' reported Unity results, which are the authority for verification. I18 ran on the I18 source, with an interim siege end; I19 ran on the final source. Check and test numbers are those of `90-Evidence.md`, Phase 9 basis. No Unity session was run for this update.

Verification: **Unity** = a named I18 check or I19 test; **Source** = derived from source only.

## Known-answer checks

| # | Question | Answer | Rule home | Verification |
| --- | --- | --- | --- | --- |
| G1 | An army of faction 2 besieges a settlement of faction 7, and separately an army of faction 7 besieges a settlement of faction 2. Which side is A? | The defending garrison is Side A both times, the attacker Side B; factions are unchanged | `BATTLE-SIEGE-01`, `SIDE-01` | Unity (I18 check 2) |
| G2 | Siege of 80 attackers against 80 defenders, default rules: spawn rates and the defender's spawn span? | Attacker 6/s, defender 12/s (floor(6 × 2)); first unit of each at commit; the 80 defenders take about (80 − 1)/12 ≈ 6.6 s unblocked | `BATTLE-SIEGE-01`, `BATTLE-SPAWN-01` | Unity for the rates (I18 check 3); the span Source arithmetic |
| G3 | A castle of 300 against 200 attackers: rates and start view size with the default curve? | Defender floor(17 × 2) = 34/s, attacker 12/s; view size for 500 units = 17.5 (between (400, 17) and (600, 18)) | `BATTLE-SIEGE-01`, `ARENA-VIEW-01` | Source arithmetic; the battle itself ran in I19 test 8 (cleanup problems 0, lost 0), with no rates reported |
| G4 | A garrison of 30 mobile + 10 settlement-only units is besieged by 50 attackers. Queues, rates and retreat rolls? | Defender queue 40 in roster order, settlement-only included, at floor(4 × 2) = 8/s; attacker 50 at 4/s; only the 30 mobile defenders and the 50 attackers roll retreat eligibility | `BATTLE-SIEGE-01`, `BATTLE-RETREAT-01` | Source; settlement-only defenders deploying and never retreating Unity (I18 check 5) |
| G5 | "Retreat N" forced during a siege while settlement-only defenders are on the field? | They are refused (marked failed, not counted as a start failure); mobile units of both sides retreat to the attacker's edge, the only gate | `BATTLE-RETREAT-03`, `ARENA-GATE-01` | Unity (I18 checks 4 and 5) |
| G6 | A settlement's attacker and defender edges are both Left, or its siege prefab is a scene object; an ordinary enemy army touches it while defended. | No siege: nothing changes, no global retry wait, a warning on the new contact only; `TotalSiegeSetupRefusals` and `TotalStartsRefused` +1 on every step the pair is the selected pick (every touching step when it is the only candidate); other battles still start | `BATTLE-SIEGE-01`, `WM-SETTLE-01` | Unity for "starts nothing; other battles still start" (I18 check 7); the per-step counter growth Source (I18 known gap) |
| G7 | A 200-unit army touches an empty enemy town (limit 200) that has a pending deployment. | Captured in that step: faction changes, the pending deployment is cancelled, the 200 units become the garrison, the army is removed, `SettlementCaptured` fires; the same step's settlement work sends a 100-unit overflow army if there is room, leaving 100 | `WM-CAPTURE-01`, `WM-OVERFLOW-01` | Unity (I18 check 1); the ordinary capture also I19 test 1 |
| G8 | In one step a retreating allied army joins an empty settlement and an ordinary enemy army touches it. | The join runs first (step 3), so the capture's live recheck finds a defended settlement and refuses; from the next step the contact is a Siege candidate | `WM-TIME-01`, `WM-CAPTURE-01` | Source |
| G9 | The attacker wins a siege; the garrison then holds 12 escaped mobile defenders, 1 dropped mobile defender and 2 dropped settlement-only defenders. | The 2 settlement-only defenders are released as Dead and destroyed (one log line); the 13 mobile ones move to a new pending roster P of the old faction; the settlement takes the attacker's faction; every living attacker joins the garrison and the army is removed; `SettlementCaptured` is raised at the controller's next FixedUpdate, after `BattleEnded`; P is placed as one retreating army of 13 (`EscapedDefenders`) | `WM-CAPTURE-01`, `BATTLE-SIEGE-02`, `WM-PENDING-01` | Unity for capture with escapees to P1, then a retreating army of the old faction (I19 tests 5 and 6); settlement-only and dropped clauses Source |
| G10 | A pending roster holds 450 units and there is room. | In one settlement-work step: retreating armies of 200, 200 and 50, in roster order, each already retreating when `ArmySpawned(…, EscapedDefenders)` is raised; then the roster closes | `WM-PENDING-01`, `WM-DEPLOY-02` | Unity (I19 test 7a, through the debug evacuation) |
| G11 | The same roster while every spot around the settlement is blocked. | Nothing changes and every unit stays owned; one "no room" log line; `PlacementBlocks` rises once per `PlacementRetrySeconds` (0.25) of map time; the audit lists the roster; once room appears the armies are placed | `WM-PENDING-01`, `WM-AUDIT-01` | Unity (I19 test 7b; the hold in 7a); the retry interval Source |
| G12 | A siege times out with attackers still queued. | Nobody dies; every living attacker, queued ones included, stays in the army, which becomes retreating and is placed away from the settlement with 1 s of army–settlement immunity; every living defender stays in the garrison; the settlement is unchanged | `BATTLE-SIEGE-02`, `WM-IMMUNE-01` | Unity (I19 test 4) |
| G13 | The defender wins a siege; 6 attackers had escaped. | The army keeps the 6, becomes retreating and is placed away with immunity; with no escapee or dropped unit it is removed; the settlement is unchanged | `BATTLE-SIEGE-02` | Unity for "removed, or retreating with escapees" (I19 test 2); placement and immunity Source |
| G14 | A siege ends BothLose by elimination; 4 defenders had escaped. | The 4 are back in the garrison as Reserve; the attacker army is removed, or retreating with its escapees; the settlement is unchanged | `BATTLE-SIEGE-02` | Unity (I19 test 3) |
| G15 | A siege is aborted while fighting. | Outcome Aborted; no result, no flag, no capture; every living unit back with its own side; an empty attacker removed, a surviving one placed away with army–settlement immunity; no immediate re-siege | `BATTLE-ABORT-01`, `BATTLE-SIEGE-02` | Unity (I19 test 9); placement and "no immediate re-siege" through I18 check 8 (interim return, same calls) |
| G16 | The attacker wins, but a mobile defender stays quarantined (`DebugPauseQuarantineRetry` on). | The capture is refused before any change: nobody changes owner; the attacker stays ordinary and is placed away with immunity; `TotalSiegeCaptureRefusals` and `CleanupProblems` +1, with an error log | `BATTLE-SIEGE-02`, `WM-CAPTURE-01` | Source (I19 untested list) |
| G17 | `Settlement2D.SetFaction` and `DeployArmy` for a settlement under siege. | `SetFaction` is refused with a warning; the deployment is stored and stays Blocked until the battle is no longer current, then runs | `WM-FACTION-01`, `WM-DEPLOY-02` | Unity (I18 check 9) |
| G18 | A gated or primitive transfer between two pending retreat rosters; a gated transfer between a pending roster and a garrison. | The primitive rejects pending ↔ pending; `CanTransfer` does not refuse a pending roster (known gap), so pending ↔ garrison would reach the primitive. Nothing calls it today | `WM-XFER-01`, `WM-XFER-02` | Source |
| G19 | The defender's queue is empty and its only unit left is a settlement-only defender on the field, while attackers are still fighting. | No decision: the defender side still has a remaining unit, and it can neither retreat nor escape, so the siege continues until it dies or a timeout locks the result. Once it dies with attackers on the field, the attacker wins | `BATTLE-SIEGE-01`, `BATTLE-END-02` | Source (I18 decision) |
| G20 | An empty debug army touches an empty enemy settlement. | No capture ("has no units"), logged on the new contact; no siege either, because the settlement is empty | `WM-CAPTURE-01` | Source |

Superseded or updated earlier answers:
- **Phase 1 W9:** an empty enemy settlement is now captured by an ordinary army (applied after allied interactions, `WM-CAPTURE-01`). Its classification is unchanged.
- **Phase 1 W10:** a Siege pick now starts a siege (`BATTLE-SIEGE-01`).
- **Phase 8 F11:** the siege freeze now has a caller. A second freeze would still throw, but the commit now runs inside the preparation's exception handling, so such a throw would remove the arena and count as a preparation failure (`BATTLE-START-03`). The commit calls exactly one freeze. Updated in place above.
- **Phase 4 B16** also holds in sieges, with the garrison as an escaped defender's owner. At the end it is parked in the garrison, and after an attacker win the capture moves it to a pending roster (G9).

## Unity-verified versus source-only

- **Unity, I18** (I18 source): checks 1–9 as listed in `90-Evidence.md`. Check 8 is credited only to the placement and immunity calls that I19 kept.
- **Unity, I19** (final source, post-battle audit PASSED with each): tests 1–9 as listed in `90-Evidence.md`.
- **Source only:** the `[WM9-S]` list in `90-Evidence.md`.

## Phase 9 acceptance (Roadmap §3)

| Acceptance item | Covering result | Status |
| --- | --- | --- |
| (Goal) Capture of an empty enemy settlement | I18 check 1; I19 test 1 | Passed |
| The defender is Side A for both faction orderings | I18 check 2 | Passed |
| 80 vs 80 spawns at 6/s vs 12/s | I18 check 3 | Passed |
| All four outcomes keep ownership correct | I19 tests 2 (defender wins), 3 (BothLose), 4 (timeout), 5–6 (attacker wins), each with a post-battle audit PASSED; abort, test 9 | Passed |
| Escaped defenders split by capacity | I19 test 7a (450 → 200 + 200 + 50); test 6 (a real siege's escapees placed) | Passed. The split ran through the debug evacuation, sharing the siege capture's placement path |
| … blocked placement keeps them | I19 tests 7a (hold) and 7b (blockers) | Passed |
| Repeated field/siege battles and large garrisons work | I19 test 8 (about 5 field battles and 5 sieges, a castle of 300 against 200; refused 0, cleanup problems 0, lost 0) | Passed |

**Phase 9 is complete:** every acceptance item has a passing Unity result, as the I19 handoff reports. The untested paths and known gaps in `90-Evidence.md` (Phase 9 basis) and `CORE-02` remain open. With Phase 9, every Roadmap V6 phase is complete.

## Structural checks

- 100 source files / 75 logical entries: seven Sidestep, ten controller, three arena and nine battle-manager partials. 2 new files, both controller partials: `WorldMapController2D.Capture` (I18) and `WorldMapController2D.Siege` (I19). Neither is a new entry. Every entry has exactly one card: 2 in Core, 4 in `01`, 6 in `02`, 6 in `03`, 11 in `04`, 12 in `05`, 1 in `06` and 33 in `07`.
- 4 new rule definitions, all in `07`: `WM-CAPTURE-01`, `WM-PENDING-01`, `BATTLE-SIEGE-01`, `BATTLE-SIEGE-02`. `07` now holds 93 definitions (89 before) and `00` holds 14, with no duplicates. With `01`–`06` unchanged since M08, the total is 198 (194 before).
- Edited in place, same IDs:
  - Core: `CORE-02`, `CORE-03`, `CORE-04`, `ORDER-01`, `SETUP-07`.
  - World map: `WM-SCOPE-01`, `WM-TIME-01`, `WM-FREEZE-01`, `WM-OWN-01`, `WM-OWN-02`, `WM-ROSTER-01`, `WM-FACTION-01`, `WM-XFER-01`, `WM-XFER-02`, `WM-DEPLOY-01`, `WM-DEPLOY-02`, `WM-OVERFLOW-01`, `WM-AUDIT-01`, `WM-REMOVE-01`, `WM-SETTLE-01`, `WM-SETTLE-02`, `WM-CONTACT-05`, `WM-BATTLE-01`, `WM-IMMUNE-01` (extended to army–settlement immunity), `WM-REGEN-02`, `WM-RETREAT-01`.
  - Arena and sides: `ARENA-LIFE-02`, `ARENA-TERRAIN-01`, `ARENA-GATE-01`, `SIDE-01`.
  - Battle: `BATTLE-LIFE-01`, `BATTLE-START-01`–`BATTLE-START-03`, `BATTLE-EDGE-01`, `BATTLE-SPAWN-01`, `BATTLE-SPAWN-02`, `BATTLE-UNIT-02`, `BATTLE-ABORT-01`, `BATTLE-END-04`, `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `BATTLE-RECOVER-01` (V9.9.1), `BATTLE-AUDIT-01`, `BATTLE-RETREAT-01`, `BATTLE-RETREAT-03`, `BATTLE-DELIVER-03`.
  - No ID was renumbered or removed. Every backticked rule ID in the attached files is defined in them or belongs to the unattached `01`–`06`.
