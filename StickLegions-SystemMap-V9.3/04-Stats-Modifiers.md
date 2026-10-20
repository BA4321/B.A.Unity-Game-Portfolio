# Stat modifiers & modifier framework

Load with Core. Clash owns paired transaction order; Health owns damage/XP semantics; Abilities owns trigger qualification/cancellation/commitment. This file owns contribution and instance lifetime.

## Effective stats and controller boundaries

- **STAT-01 — Evaluation.** Every effective getter reconciles controller expiry first. Ordinary reads may publish ModifiersChanged but never execute owed damage/children. Aggregate is `(pre-value + sum(adds)) × product(factors)`; dirty aggregates rebuild from surviving entries without division, preserving zero factors. Final effective outputs are nonnegative; Dodge uses `CALC-02`. No second authoritative cache of level/Vulnerable/modifier state exists in runtime stats.
- **STAT-02 — Input layers.** Health/movement/Dodge use current level ≥1, otherwise configured starting level clamped to ≥1. Maximum health uses level-scaled definition only. Vulnerable movement multiplier applies before stat contributions. Acceleration/base Clash damage/turn/size use definition bases; strength multiplier starts at one. Raw Dodge scaling precedes contributions/cap, missing definition yields zero. Missing controller passes through pre-values; present disabled controller remains usable. Movement reaction is `MOVE-01`/`MOVE-02`.
- **STAT-03 — Entry/bundle mutation and notifications.** Require finite entries and positive finite timed durations. Foreign/stale identities cannot change another owner's entries. Bundle install/removal requires a live notification scope; failed install changes neither contributions nor output slots. Successful installation retains ownership before callbacks. Exact removal tolerates already-removed subordinate entries. Nested scopes coalesce completed changes, not one event per entry/application; statless effects need not notify. Final close drains; subscriber mutation queues another pass, each pass snapshots subscribers/isolates exceptions. Scope acquisition may publish overdue legacy expiry: revalidate afterward and close in finally.
- **STAT-04 — Protected barrier.** Settle before acquisition (`CLASH-08`). Pending/dispatching notifications, open scopes, overdue contributions or existing protected read block acquisition. Only settled acquisition is callback-free; mutation under protection throws. Do not generalize ordinary getter callbacks to this interval.
- **STAT-05 — Legacy timed entries.** Only legacy timed entries require controller Update; idle disable is not cleanup. Queries also reconcile. Timed-only refresh rejects invalid/foreign handles/durations, processes overdue entries before lookup, preserves identity/source/amount/target/operation and replaces expiry with ApplicationTime + duration. Duration-only refresh is silent except for overdue removals. Framework bundles are untimed here; runtime owns their expiration.

## Application pipeline

- **MOD-APPLY-01 — Context and recipient.** Caller supplies original caster, captured caster level, origin and application time. Convenience capture reconciles expiry then captures recipient level once; missing/wrong-root progression yields zero and resolver rejects. Explicit contexts use supplied valid levels. Null/destroyed caster is permitted with a valid captured level; no caster-liveness or delivery-object gate. Recipient requires active/enabled runtime and active root, same-root combatant, active living health, death with HasDied false and stat controller. Faction/targetability/motor state and combatant/death/controller enablement are not additional runtime gates. Selection/Dodge/direct-hit survival belong to caller.
- **MOD-APPLY-02 — Independent commitment.** Each application creates a separate runtime-scoped instance; repeated assets never refresh/replace and have no automatic stacking limit. Freeze numeric values/settings for lifetime, retaining child/prefab/ability identities without deep copy. Applied means commitment even if callbacks remove it immediately. ApplyOrdered uses one frozen context in array order with per-entry eligibility rechecks; transactions are independent, not an atomic batch. Caller arrays must stay stable through callbacks.
- **MOD-APPLY-03 — Contributions.** Map at most six nonneutral entries into one atomic untimed bundle: Dodge Add, acceleration Multiply, speed Multiply, turn Add/Multiply and base Clash damage Add. Statless effects need no bundle. Runtime owns exact bundle and instance together. UnitSize/ClashStrengthMultiplier remain controller targets but have no SO field; maximum health, regeneration, Vulnerable duration and radius also have no SO field.
- **MOD-VALID-01 — Resolution.** Require valid captured levels, positive resolved duration, finite/representable arithmetic and supported per-field scaling. Clamp resolved magnitude nonnegative before applying direction; scaling cannot invert buff/debuff sign. Percent magnitude uses fractional units and reductions cannot exceed full reduction; Dodge changes are probability points. Formula details belong to resolver source. Meaningful effect requires damage, nonneutral stats, enabled children or trigger; visual/removal policy alone is inert. An effect can be meaningful even if duration ends before first scheduled action.
- **MOD-VALID-02 — Authoring support.** Use the following table. Enabled child links require a child, positive finite interval and acyclic enabled chain without an authored depth cap. Non-finite intervals are rejected even on disabled links. OnValidate covers trigger/child authoring; full values validate at resolution. Trigger-specific contract is `TRIGGER-01`.

| Authored field | Supported modes |
| --- | --- |
| Duration | None, CasterLevel, LevelDifference |
| DamagePerTick; DodgeChanceChange | None, LevelDifference |
| ForwardAccelerationChange; MaximumForwardSpeedChange | None, CasterLevel, LevelDifference |
| TurnRateChange (flat or percent); BaseClashDamageChange | None, CasterLevel |

## Scheduled work and lifetime

- **MOD-TIME-01 — Timestamp validity.** Use Core's GameplayTime/ApplicationTime definitions. Supplied time must be finite, nonnegative, not future and not earlier than executing work. Nothing inserts behind current scheduled order. Do not globally convert Vulnerable/ability timers to modifier time.
- **MOD-TIME-02 — Scheduler order.** Timestamp → global instance creation sequence → kind. Damage precedes ChildEmission precedes Expiration only within the same instance/time. Catch up to fixed horizon without work cap, preserving scheduled cadence. Queue successors before callbacks so removal can cancel them. No work while paused. At most one damage and one child channel per instance; public scheduling rejects duplicates and authored channels reserve slots when due work exists.
- **MOD-TICK-01 — Damage cadence.** First damage at application +0.5 seconds, then every 0.5 seconds, including a tick exactly at expiry; no immediate or fractional final tick. Use frozen damage/original caster with DamageOverTime origin, effect permission false and XP permission true; add no Dodge/Vulnerable multiplier. Retain/finalize victim XP in finally under `XP-03`.
- **MOD-CHILD-01 — Child cadence and resolution.** First emission after fixed interval; repeat strictly before parent expiry. Keep original caster/captured caster level/origin; capture recipient level anew and resolve then-current child asset at scheduled emission time. Committed children survive parent removal. Revalidate enabled chains, including frozen parent edge against later edits; rejection leaves parent intact.
- **MOD-EXPIRY-01 — Stats versus owed work.** Reconciliation removes contributions at ExpiresAt ≤ ApplicationTime without executing periodic work. Keep instance identity/values/schedules/visual until scheduled expiry; Contains/TryGetInstance/ActiveInstanceCount can therefore include expired instances awaiting owed work. Catch-up reconciliation uses event time, preserving historically valid lifetimes. Explicit removal cancels owed work.
- **MOD-REMOVE-01 — Removal/cleanup.** Explicit removal cancels remaining channels, removes exact bundle/identity and releases visual. ClearAll removes only runtime-owned instances, rejects new applications during cleanup, and cancels pending requests per `TRIGGER-06`. Runtime disable/destruction, health ≤0 and Died clean up; scheduled work clears ineligible recipients. Dependency loss is checked at application/visual/scheduled boundaries, not continuously polled. Scheduler destruction clears queued owners. One-Clash consumption is `CLASH-12`.
- **MOD-VISUAL-01 — Optional leases.** Acquire after gameplay commit/notification completion, one independent prefab lease under recipient root at authored local transform. Reuse resets hierarchy/transforms, rendering/sprite state, particles/trails. Custom scripts reset private state on enable/disable, avoid scheduled Destroy and shared-material mutation. Stale generations cannot release reused clones. Visual failure never rejects gameplay. ClearAll/paired consumption defer visual callbacks until full removal set commits. Pool shutdown retires clones without cancelling modifiers. Runtime/pool have no recurring loop.

## Reactive timed-handle exception

- **REACTIVE-01 — Filters.** Require successful initialization, active/enabled reactive component and active living health; revalidate after scope acquisition. Loss/OverwhelmingLoss reactions qualify including dodged losses, but lethal/inactive recipients cannot add/refresh. Exact-Wall solid enter/stay uses contacted collider layer cached in Awake. No queries, contact collection, collision-exit handler or recurring loop.
- **REACTIVE-02 — Refresh then replace.** Each source has one independent timed TurnSpeed/Add handle. Nonpositive amount/duration prevents new add/refresh without removing existing effect. Open scope → revalidate → refresh existing → if failed add current authored effect → retain handle before scope closes in cleanup. Same-source repeats refresh, two sources coexist. Refresh preserves old amount; simultaneous Walls/re-entry share one Wall handle. After callbacks cease it expires from last successful refresh; exit does not remove it.
- **REACTIVE-03 — Lifecycle.** OnEnable subscribes after valid initialization once; OnDisable unsubscribes and retains handles without duration reset/removal. Re-enable only restores subscription. OnDestroy unsubscribes, removes owned handles if controller exists, then clears them. This is deliberately a legacy controller-handle path, not independent SO instances. Effects reach motor through runtime stats, without direct Sidestep/motor dependency.

- **MOD-DEBUG-01 — Debug scope.** Editor/development implementation requires living initialized caster and supported origin, stricter than runtime Apply; release component is empty. Debug restrictions do not redefine `MOD-APPLY-01`.

## Contract cards

### CombatModifierTypes2D [F,B]
Owns: seven stat-target enum values, Add/Multiply entries, controller handles/bundles/scopes/aggregates and embedded ModifierStatBundleMapper2D.  
API: value contracts; TryWrite mapper.  
Events: none.  
Invariants: `STAT-01`, `STAT-03`, `MOD-APPLY-03`.  
Never owns: active contributions, instance lifetime or final effective values.

### UnitRuntimeStats2D [F,B, WM2]
Owns: queried derivation/final bounds of health, speed, acceleration, damage, turn, size, strength and Base Dodge Chance.  
API: Effective* getters; internal controller access for capture; `Definition` (read-only serialized `UnitDefinition2D`, safe on parked never-awakened units and prefab assets; used by `UnitReserve2D.ValidatePrefab` and `UnitHandle`, `WM-UNIT-02`).  
Events: none; getters can cause controller notifications.  
Invariants: `STAT-01`, `STAT-02`, `STAT-04`.  
Never owns: level, Vulnerable timer, modifiers, current health or motion.

### UnitStatModifierController2D [F]
Owns: contributions, scoped identities, aggregates, legacy expiry, notifications and protection state.  
API: Add/RemoveModifier, AddPermanentModifier, TryRefreshModifierDuration, GetCombinedModifier, ApplyModifiers, ReconcileForRead, Begin/EndNotificationScope, permanent bundle operations; internal protected-read operations.  
Events: ModifiersChanged; no established production subscriber.  
Invariants: `STAT-01`, `STAT-03`, `STAT-04`, `STAT-05`.  
Never owns: final stats, framework instances/work/visuals, health or Clash order.

### UnitReactiveTurnBoost2D [F,B]
Owns: initialization/Wall cache, result subscription and separate loss/Wall handles.  
API: lifecycle/collision callbacks only; no public method.  
Events: subscribes to ClashResultApplied; emits none.  
Invariants: `REACTIVE-01`, `REACTIVE-02`, `REACTIVE-03`; Core `SETUP-02`.  
Never owns: contribution storage/expiry, effective stats, motion or paired ordering.

### ModifierDefinition2D [V8]
Owns: authored magnitudes/directions/scaling, removal policy, child/trigger settings and optional visual prefab.  
API: read-only settings; OnValidate delegates authoring checks.  
Events: none.  
Invariants: `MOD-VALID-01`, `MOD-VALID-02`; `TRIGGER-01`.  
Never owns: per-recipient state or resolved instance lifetime.

### ModifierFrameworkTypes2D [V8]
Owns: contexts/resolved values, scaling/direction/trigger enums, application results and runtime/schedule identities.  
API: immutable contracts; Applied/Rejected/NotProcessed; trigger validation reason/field values.  
Events: none.  
Invariants: `MOD-APPLY-01`, `MOD-APPLY-02`; `TRIGGER-01`.  
Never owns: live component state, scheduling or delivery.

### ModifierValueResolver2D [V8]
Owns: stateless definition/context validation and scaling, enabled-chain validation.  
API: resolution and TryValidateAbilityTrigger.  
Events: none.  
Invariants: `MOD-VALID-01`, `MOD-VALID-02`; `TRIGGER-01`.  
Never owns: live levels/health queries, effect application or instance IDs.

### UnitModifierRuntime2D [V8]
Owns: frozen instances/context/expiry, exact bundle, scheduled channels, lease, diagnostic labels and independent pending request records.  
API: Apply, CaptureApplicationContext, ApplyOrdered, Contains/TryGetInstance/Remove/ClearAll, ReconcileExpiredStats, TrySchedulePeriodicWork, CopyEligibleOneClashIds/ConsumeCapturedClashPair; internal trigger reserve/copy/release and IsPending/TryCommit.  
Events: subscribes to HealthChanged/Died while enabled; emits no documented custom event.  
Invariants: `MOD-APPLY-01`–`MOD-APPLY-03`, `MOD-TIME-01`, `MOD-TICK-01`, `MOD-CHILD-01`, `MOD-EXPIRY-01`, `MOD-REMOVE-01`, `MOD-VISUAL-01`; `TRIGGER-02`, `TRIGGER-04`, `TRIGGER-06`, `TRIGGER-08`.  
Never owns: selection/Dodge, final stats, health/XP mutation, trigger matching or ability casting.

### ModifierScheduler2D [F]
Owns: persistent ordered work, cancellation handles and global creation sequence.  
API: GameplayTime, ApplicationTime; scheduling contracts route work to runtime owners.  
Events: scheduled work callbacks, no documented custom event.  
Invariants: `MOD-TIME-01`, `MOD-TIME-02`, `MOD-TICK-01`, `MOD-CHILD-01`, `MOD-REMOVE-01`.  
Never owns: instances, stat aggregation, health or rendering.

### ModifierVisualPool2D [F]
Owns: prefab-keyed clones and lease generations.  
API: lease acquisition/release through runtime.  
Events: no custom event.  
Invariants: `MOD-VISUAL-01`; Core `SETUP-05`.  
Never owns: gameplay lifetime or stat state.

### ModifierApplicationDebug2D [F]
Owns: optional Play Mode application/removal and stable result history.  
API: configured caster/recipient/definitions/origin; manual commands via runtime.  
Events: none documented.  
Invariants: `MOD-DEBUG-01`; Core `SETUP-05`.  
Never owns: production delivery, general stacking or eligibility.
