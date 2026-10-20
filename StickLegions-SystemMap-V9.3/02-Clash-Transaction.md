# Clash transaction

Load with Core. Modifier barriers are defined in Modifiers, lethal/XP internals in Health, and trigger matching/request lifecycle in Abilities. The numbered IDs below are stable identifiers, not section positions.

## Ordered pair transaction — authoritative sequence

- **CLASH-01 — Detection.** Clash Range trigger enter/stay filters its own collider, excluded layers, non-detectors, self-detector and same-owner cases; obtains participant owners and submits a canonical ClashRequest. Detector does not validate factions or calculate effects.
- **CLASH-02 — Canonical identity.** Pair keys and requests sort participant instance IDs; valid requests require two existing distinct participants.
- **CLASH-03 — Batch close.** Submit records the collecting physics-step ID. On next close, director swaps reusable write/processing buffers and processes that closed batch.
- **CLASH-04 — Deduplication.** A–B/B–A is one pair per batch. First report determines request position; aborted pairs remain deduplicated.
- **CLASH-05 — Acceptance.** Require distinct enemies, active participants, targetable combatants, expired per-unit global cooldowns and active kill-XP components. Retain both XP owners.
- **CLASH-06 — Storage ownership.** Reserve both one-Clash ID lists and trigger capture storage before protection; lists/buffers are distinct. Retain runtime owners separately from numerical calculator inputs.
- **CLASH-07 — Prepare both.** Prepare first then second through participant/Sidestep; revalidate after each. Both finish before either fresh snapshot. Movement's `SIDESTEP-CLASH-01` owns cancellation details.
- **CLASH-08 — Settle then protect both.** Reconcile both recipients before acquiring either barrier; revalidate. Both protected reads must be acquired before either snapshot; failed acquisition aborts. Release both afterward. See `STAT-04` for barrier eligibility.
- **CLASH-09 — Protected capture.** Each motor reconciles its ordinary speed limit; capture numerical values and exact eligible one-Clash IDs at GameplayTime, then copy trigger candidates into reserved reusable buffers. Numerical input includes movement position/facing/velocity/clean speed, weapon rating, effective size/base Clash damage/strength multiplier/Base Dodge Chance, level and transaction-time Vulnerable state. It excludes ID lists. Single/pair snapshot APIs do not themselves prepare or consume.
- **CLASH-10 — Freeze and calculate before effects.** Final pair validation, then freeze both source combatants, same-root ability holders and fallback opponent positions. Always draw first/second Dodge samples then first/second Knockback-angle samples; calculate the complete resolution. Start both cooldowns before either result callback. Cooldown extension cannot move expiry earlier.
- **CLASH-11 — Both result attempts under XP deferral.** Begin both deferrals. Attempt first result, then the original second result if its receiver exists; entry guards may reject it. Source death does not cancel the second attempt. Record WasProcessed and Dodged before notification Dispatch resets the record.
- **CLASH-12 — Exact paired consumption.** After both attempts, while XP remains deferred, consume captured unexpired/unconsumed RemoveAfterOneClash identities (including statless effects). Consumption follows capture regardless of Dodge, damage success or survival; missing IDs are harmless; callback-created instances cannot join. Open callback-free controller barriers and commit both removal sets before releasing removal notifications/visual callbacks. Pending-trigger behavior is `TRIGGER-06`.
- **CLASH-13 — Finalize and react.** End both deferrals, finalize first then second retained victim awards, dispatch first then second retained ClashResultApplied records. Nested finally blocks preserve cleanup/finalization; subscriber exceptions are isolated. Health/damage/death events already happened during effects and are not delayed to this boundary.
- **CLASH-14 — Trigger boundary.** Only normal completion of calculation/results/consumption/finalization/reactions reaches synchronous trigger dispatch, before the next pair. Reactive loss boosts have already run; later pairs see triggered effects, deaths and level changes.
- **CLASH-15 — Every-exit cleanup.** Outer finally reports matching opportunities a faulted transaction never reached, then releases both trigger buffers and registrations. Aborted preparation/settlement/capture/final validation produces no samples, cooldowns, results, consumption, casts or trigger warnings. Prior preparation/reconciliation is not rolled back. Director disable stops later requests, preserving current finalization. This is ordered capture/commit, not general rollback.

## Participant result pipeline

- **RESULT-01 — Entry and retention.** Reject missing/inactive/dead health or missing motor without a record. Living source and participant enablement are not entry gates. Accepted entry snapshots listeners before effects, including lethal/Dodge/zero-damage outcomes.
- **RESULT-02 — Survivor effects.** Dodge skips damage. Otherwise submit finalized Clash damage with effect and XP permission true. Lethal damage or failed survivor guards stops remaining effects but keeps the record. Continue only with active living health and motor.CanReadMovement; recheck between momentum → non-dodged new Vulnerable → Knockback. Existing Vulnerable is not cleared by Dodge. Zero/rejected damage is not Dodge and need not stop survivor effects.
- **RESULT-03 — Notification variants.** Director uses `CLASH-13`. Standalone ApplyClashResult dispatches immediately with neither paired consumption nor director XP boundary and never triggers casts. Retained records can outlive receiver destruction; listeners check liveness.

## Calculation boundary

- **CALC-01 — Pure pair calculation.** Calculator consumes snapshots/settings and explicit samples, reads no component/random state, computes both fully modified strengths before outcome classification and mirrors the second outcome. Strength combines Martial Might, projected relative momentum and engagement alignment, then Vulnerable and effective strength multiplier. Formula/tuning values live in source. Zero-distance direction uses first facing then Vector2.up.
- **CALC-02 — Shared Dodge semantics.** Raw chance is modified before shared capping; non-finite/negative chance becomes zero and finite chance caps at 0.95. Clash applies OverwhelmingLoss and relative-final-strength adjustments; Projectile uses live base chance without those adjustments. A finite sample within [0,1] succeeds strictly below capped chance; equality fails. Non-finite strengths invalidate Clash chance. Dodge helper never samples randomness.
- **CALC-03 — Result is not health loss.** Requested incoming damage derives from opponent's effective base damage/outcome settings, independent of opponent's own Dodge. Explicit Dodged enforces zero requested damage and no new Vulnerable, preserving outcome/momentum/Knockback and duration fields. Result stores supplied final chance without its own clamp; health returns actual loss. Knockback points away then rotates by its independent mapped angle; speed is unchanged.
- **CALC-04 — Configuration distinction.** Authored ClashRules angle clamps to 0–90 degrees; ClashCalculationSettings constructor permits 0–180 (NaN becomes zero). Preserve this distinct existing boundary; do not silently normalize it during documentation work. Other formulas/defaults/signatures must be read from source when edited.

## Contract cards

### CombatTypes2D [B,K]
Owns: definitions of faction/state, pair/request, calculation settings, snapshot, strength, result and resolution contracts.  
API: immutable value contracts; exact constructors from source.  
Events: none.  
Invariants: `CLASH-02`, `CLASH-09`, `CALC-03`, `CALC-04`.  
Never owns: runtime state, processing or random sampling.

### UnitClashDetector2D [B]
Owns: configured owner/director/collider/opposing mask references.  
API: Owner; Unity trigger enter/stay → SubmitRequest.  
Events: no custom event.  
Invariants: `CLASH-01`; Core `SETUP-01`, `SETUP-03`.  
Never owns: faction gates, deduplication, cooldown, calculation or effects.

### ClashDirector2D [V8,K]
Owns: singleton, batch buffers/IDs/keys, calculation settings, four samples, retained pair storage and one trigger dispatcher.  
API: SubmitRequest; Instance; PendingRequestCount.  
Events: no custom event; orchestrates retained participant reactions.  
Invariants: `CLASH-01`–`CLASH-15`.  
Never owns: unit health/movement/XP/modifier state, per-unit cooldown, trigger request state or ability timers.

### UnitClashParticipant2D [V8]
Owns: unit cooldown, fresh snapshots, capture delegation, participant effects and PendingClashResultNotification.  
API: CanClashAt, BeginGlobalClashCooldown, PrepareForAcceptedClash, TryCaptureClashSnapshot/Pair, ApplyClashResult; internal deferred result and AbilityHolder/KillExperience access.  
Events: ClashResultApplied → enabled reactive boost; not the trigger dispatcher.  
Invariants: `CLASH-05`–`CLASH-13`, `RESULT-01`–`RESULT-03`.  
Never owns: pair order, calculation, Sidestep state, modifier requests, casting or authoritative effect state.

### ClashCalculator2D [B,K]
Owns: pure numerical resolution formulas.  
API: Calculate with both inputs/settings and four explicit samples → ClashResolution.  
Events: none.  
Invariants: `CALC-01`, `CALC-02`, `CALC-03`.  
Never owns: component references, randomness, queries, cooldowns or effects.

### DodgeCalculator2D [B]
Owns: shared cap, Clash chance adjustment and strict sample comparison.  
API: ClampChance, CalculateClashDodgeChance, DoesDodgeSucceed.  
Events: none.  
Invariants: `CALC-02`.  
Never owns: random sampling, mutable Dodge state, authored values or delivery.
