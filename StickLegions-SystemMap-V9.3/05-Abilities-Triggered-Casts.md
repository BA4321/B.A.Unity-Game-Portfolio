# Abilities & triggered casts

Load with Core. Clash owns the paired transaction boundary; Modifiers owns instance/scheduler behavior; Health owns damage/event/XP semantics. The rules here own casting, delivery and pending-trigger semantics.

## Normal casting pipeline

- **ABILITY-01 — Ownership and activation.** Hold abilities by asset identity. Grant works before Awake/while inactive; duplicates change neither timers nor selection. Acquisition starts a full resolved own cooldown plus independent extra delay, deferring setup until initialized progression if needed. Grant non-null startingAbilities once in Start via normal Grant. Disable preserves ownership; next activation resets held and universal timers once progression initializes, without regranting starting list. Starting configuration belongs to holder, not UnitDefinition2D.
- **ABILITY-02 — Caster and selection.** In Update, cheap caster/universal/own-time gates precede categories. Require active same-root holder/combatant, initialized progression, living health/death; pause blocks casting. Motor Disabled/Knockback/Sidestep are not independent exclusions. Randomly choose one currently eligible held asset, no reserved future order. Normal Projectile needs active same-root targeting with current targetable enemy; Area needs an eligible recipient; Self Buff needs a nonempty authored modifier list.
- **ABILITY-03 — Prepare, recheck, commit.** Freeze resolved ability values, caster identity/level/faction, masks, origin, aim where relevant, cast time and an owned copy of ordered modifier references. Recheck invocation gates; commit own/universal deadlines before effects. Shared normal/triggered guard prevents nested execution. No refund for missed delivery, rejected modifiers, lost recipients or visual failure. Area/Self are synchronous; Projectile launch has no synchronous recipient result. Snapshot asset/prefab identities are not deep-frozen contents.
- **ABILITY-04 — Time and resolution.** Ability timers/casts use scaled Time.time. Level changes affect later resolution, not running deadlines/frozen casts. Own cooldown resolves with a positive floor plus sampled extra delay; each normal cast also starts the universal delay. Pure resolver validates categories/levels/finite representable values without scene/clock/random access. Numeric formulas/minima/defaults are source-owned. Inspector range attributes alone do not establish validity; prefab validation belongs to preparation.

## Category and receiving pipeline

- **AREA-01 — Complete selection.** Overlap enemy Clash layers, resolve local detector Owner, deduplicate roots, apply inclusive root distance from frozen origin. Collider overlap only prefilters; large collider overlap cannot bypass range. Grow/retry saturated buffers rather than truncate; reuse storage and separate nested collections. Recollect for execution and recheck each recipient; release collection afterward. One delivery per eligible enemy/cast; no Wall obstruction or Dodge. Ready empty normal Areas may query each eligible Update, and selection/preparation/execution may each query; no measured performance guarantee.
- **PROJECTILE-01 — Flight.** Launch fixed directions at cast-time aim with existing alternating spread; each projectile owns independent encounter memory but shares frozen cast. Initialize before activation. Maintain velocity/check expiry in FixedUpdate; trigger entry checks expiry before accepting. Lifetime deadline or straight-line distance from frozen origin ends flight; zero limits expire immediately. Caster loss does not cancel launched flight. Launch rotation follows direction; collision radius is independent of visual-child scale. Disable terminates; projectiles are instantiated/destroyed, not pooled.
- **PROJECTILE-02 — Encounters.** Filter captured enemy Clash/Wall masks, resolve combatant in collider hierarchy and apply receiving gates. Each eligible enemy encounter counts once per projectile, including Dodge. Read live EffectiveBaseDodgeChance and draw one sample via `CALC-02`, without Clash strength/outcome adjustment. Dodge skips damage/modifiers but consumes encounter. Non-piercing closes further contacts before callbacks on first accepted enemy or Wall, allowing accepted delivery to finish. Piercing can hit multiple enemies and pass Walls. Exclude allies/original caster.
- **DELIVERY-01 — Payload-wide recipient gates.** Area/Projectile eligibility is separate from CanBeTargeted: living movement-Disabled enemies can receive. Require active recipient root/combatant and active living same-root health. Positive direct damage also needs active same-root runtime stats/death. Any modifiers require eligible same-root modifier runtime and initialized recipient progression (didAwake, level ≥1). Missing modifier prerequisites excludes the whole hit before damage; no damage-only fallback. Individual invalid modifier assets may still fail after an eligible hit. Projectile additionally requires active recipient runtime stats even at zero damage for Dodge. Compare current recipient faction with cast-time caster faction. Self Buff delivers to original caster with zero direct damage and nonempty list, whose entries may individually reject.
- **DELIVERY-02 — Damage then XP then modifiers.** Send positive resolved direct damage with Projectile or Ability (Area) origin and both permissions true; no added Vulnerable multiplier. Retain victim XP before damage, finalize in finally, obey nested deferral (`XP-02`). Recheck survival/receiving eligibility before modifiers; direct lethal damage prevents their delivery. Reusable scratch frames protect nested delivery.
- **DELIVERY-03 — Modifier boundary.** Per-recipient context uses frozen caster level/original caster/origin, recipient level captured at application and scheduler ApplicationTime, never backdated cast time. Freeze list selection at cast; resolve each asset's current numerical contents at application, then freeze instance values. Recheck gates between independent ordered entries. Valid captured level permits modifier application after caster destruction; it does not guarantee kill XP after source destruction. Child behavior is `MOD-CHILD-01`.
- **ABILITY-VISUAL-01 — Presentation.** Area art may follow caster or stay at cast position; follow never relocates/repeats gameplay. Projectile caster effect uses scene service; projectile art belongs to prefab. Optional failure never rejects committed gameplay. Service instantiates/retires visuals, separate from modifier pool; Update expires scaled-time records, LateUpdate maintains requested world scale under parents. Parent deactivation does not stop cleanup; destruction removes children, unparented visuals can outlive caster. Shutdown retires tracked visuals. Area art uses radius one before scaling; caster effects preserve authored size at world scale one.
- **ABILITY-DEBUG-01 — Diagnostics.** Normal Last* state belongs to holder; LastDeliveryResult is Self recipient or last Area delivery, never future Projectile impact. Debug grants use normal Grant, support inactive holders, provide no force-cast. Selected/all grants and state listing are Editor/development; ownership clear/reset is Editor-only. Release shell is empty. Holder adds no gameplay event subscriptions.

## Modifier-triggered casts

- **TRIGGER-01 — Authoring/freeze.** Condition None (default), OnClash or OnClashDodge and ability reference freeze with modifier at application. Enabled condition requires ability; trigger alone makes effect meaningful and can coexist with other channels. Validate only supported condition/reference then; ability/caster validation occurs at execution. Ability values/list resolve anew for every triggered cast. Modifier→ability→modifier cycles are not validated like child chains; later-instance capture boundary prevents same-Clash recursion.
- **TRIGGER-02 — Capture.** Protected pair capture copies every instance with non-None condition, unexpired stats and ExpiresAt > GameplayTime, in creation order, with fresh request ID and saved diagnostic labels. Later applications cannot join. Each captured instance, including duplicate assets, has one opportunity per processed Clash, with no trigger cooldown. Internal trigger-aware pair overload leases both buffers on success, silently aborts both on false/throw; ordinary snapshot overloads do not register trigger requests.
- **TRIGGER-03 — Match/order/caster.** Match only holder's processed result; OnClashDodge also requires its explicit Dodged. Lethal/zero-damage accepted results count as processed, but cancellation/caster gates may prevent casting. Rejected receiver, aborted pair, contact alone and Projectile Dodge do not qualify. If both groups have matches, draw one Random.Range(0,2) to choose first group; otherwise no order sample. Within each group use creation order, revalidate every request, continue after failure. Holder (not original modifier caster) supplies current position/faction/level/attribution through same-root ability holder; it need not own the asset.
- **TRIGGER-04 — Invocation and commitment.** TryExecuteTriggered checks pending status, pause, initialized progression and CanActAsCaster; prepare Triggered, recheck both, commit runtime request before effects. Read level at execution, after Clash XP/earlier triggers. No reads/writes to held entries, own/universal timers, activation-reset state or normal Last* diagnostics; no cooldown/extra-delay resolution or delay random sample. Keep category/prefab/finite-position/projectile/effect-deadline validation, timer payload fields zero. Shared guard rejects nested cast as ReentrantExecution but allows sequential casts. Committed cast never refunds/retries. IsPending also rejects ineligible or protected runtime.
- **TRIGGER-05 — Category differences.** Projectile aims at Clash opponent saved before damage; refresh before each Projectile request while opponent remains targetable enemy, otherwise use saved position. No target search; non-finite aim rejects, coincident position keeps existing zero-direction launch. Target death does not cancel survivor's later requests. Triggered Area executes and shows visual even empty; normal Area rejects NoRecipients. Self Buff unchanged.
- **TRIGGER-06 — Cancellation versus consumption.** Normal one-Clash consumption preserves pending request despite removed stats/schedules/visual. Remove(id) cancels that instance's pending requests even after consumption; its bool still reports only active-instance removal. ClearAll, death, health ≤0 and runtime disable/destruction cancel all, including caster's remaining requests; re-enable never restores. Reapplying same asset creates unrelated identity. Records disappear on transaction release; no cancellation history persists.
- **TRIGGER-07 — Sole diagnostic owner.** Dispatcher reports one warning per matching opportunity that fails to commit, naming holder, modifier asset/instance, ability, condition and reason/field. Unexpected exception uses one Debug.LogException instead, recording committed/uncertain status. A transaction fault before dispatch reports UnexpectedException (ClashTransactionFaulted) per matching opportunity not reached by normal dispatch. Unmatched/unprocessed/aborted-pair candidates are silent. Misses, Dodge, outgoing modifier rejection and empty triggered Area are delivery outcomes, not cast failures. Cursors advance before attempts to avoid repeat reports. Dispatch timing/outer cleanup are `CLASH-14`, `CLASH-15`.
- **TRIGGER-08 — Request/buffer protocol.** Capture stores copied opportunity data, never pooled runtime instance. Reuse while leased throws; Release returns all request records to runtime and clears refs, Abort is silent alias. Release is managed-only and safe after destruction. Request-owner IsPending/TryCommit are synchronous/callback-free; false guarantees no mutation; TryCommit throw means possible commitment. Triggered result returns commitment, MayHaveCommitted, exception and failure without overwriting normal diagnostics. Dispatcher is direct-call driven, not event subscriber and never checks active modifier membership or grants assets.

## Contract cards

### AbilityDefinition2D [V7]
Owns: category, scaling/cooldown/delay, ordered modifiers and gameplay/art settings; embedded scaled/Projectile/Area settings types.  
API: authored data for resolver/execution and triggering modifiers.  
Events: none.  
Invariants: `ABILITY-01`, `ABILITY-04`, `DELIVERY-01`.  
Never owns: unit ownership, starting-list placement or running timers.

### AbilityFrameworkTypes2D [V8]
Owns: category/invocation/failure enums, resolved/context/snapshot/launch/result contracts and request-owner interface.  
API: AbilityCastSnapshot2D, IAbilityTriggeredRequestOwner2D, execution/delivery/triggered result types.  
Events: none.  
Invariants: `ABILITY-03`, `DELIVERY-03`, `TRIGGER-08`.  
Never owns: component state or execution; delivery result is one recipient, not total future damage.

### AbilityValueResolver2D [V8]
Owns: pure validation/scaling and owned frozen payload construction.  
API: TryResolve, TryResolveCooldown, TryCaptureCast; invocation-aware overloads.  
Events: none.  
Invariants: `ABILITY-03`, `ABILITY-04`, `TRIGGER-04`.  
Never owns: scene/clock/random access, effects, prefab physics or commitment.

### UnitAbilityHolder2D [V8]
Owns: held entries/timers/selection, shared execution guard, helper/storage and normal diagnostics.  
API: Grant, Contains, TryGetState, CanActAsCaster, TryGetNormalProjectileTarget, GetNormalBlockReason, ExecutionInProgress (NormalExecutionInProgress alias), TryExecuteTriggered.  
Events: none; no gameplay event subscriptions.  
Invariants: `ABILITY-01`–`ABILITY-04`, `ABILITY-DEBUG-01`, `TRIGGER-03`, `TRIGGER-04`, `TRIGGER-08`.  
Never owns: target search, health mutation, modifier instances or trigger logging.

### AbilityExecution2D [V8]
Owns: reusable delivery/Area helpers, category preparation/dispatch and spread/effect requests.  
API: TryValidate, TryPrepare (default Normal or explicit invocation); internal ExecuteCommitted.  
Events: none.  
Invariants: `ABILITY-03`, `AREA-01`, `PROJECTILE-01`, `ABILITY-VISUAL-01`, `TRIGGER-05`.  
Never owns: target search, held entries or commitment/timers.

### AbilityAreaQuery2D [V7]
Owns: reusable collider arrays, deduplicated roots and nested collection storage.  
API: Collect, IsEligible, Release.  
Events: none.  
Invariants: `AREA-01`, `DELIVERY-01`; Core `SETUP-03`.  
Never owns: damage, Dodge, Wall obstruction or cooldowns.

### AbilityDelivery2D [V7]
Owns: synchronous payload-wide receiving gates and nested scratch frames.  
API: CanReceive, category prerequisites, Deliver.  
Events: emits through health APIs only.  
Invariants: `DELIVERY-01`, `DELIVERY-02`, `DELIVERY-03`.  
Never owns: range/contact selection, random Dodge, timers or effect lifetime.

### AbilityProjectile2D [V7]
Owns: physics execution, frozen launch, terminal state and encountered-enemy set.  
API: internal validation/spawn; FixedUpdate/trigger-entry/disable lifecycle.  
Events: no custom event.  
Invariants: `PROJECTILE-01`, `PROJECTILE-02`, `DELIVERY-01`; Core `SETUP-04`.  
Never owns: source-unit timers or modifier lifetime.

### AbilityVisuals2D [V7]
Owns: scene host, tracked visuals, scaled deadlines and world scale.  
API: TrySpawn; Update/LateUpdate cleanup/scale maintenance.  
Events: no gameplay event.  
Invariants: `ABILITY-VISUAL-01`; Core `SETUP-05`.  
Never owns: gameplay or modifier-pool leases.

### AbilityDebugGrant2D [V7]
Owns: optional configured holder/list/index and Play Mode diagnostics.  
API: selected/all grants and state listing; Editor clear/reset.  
Events: none.  
Invariants: `ABILITY-DEBUG-01`.  
Never owns: force-casting or production starting grants.

### ModifierAbilityTriggerTypes2D [V8]
Owns: copied candidate type and reusable creation-ordered capture buffer/lease.  
API: Release, Abort; candidate request/instance/sequence/condition/ability/labels.  
Events: none.  
Invariants: `TRIGGER-02`, `TRIGGER-08`.  
Never owns: request state, result filtering, logging or casting.

### ModifierAbilityTriggerDispatcher2D [V8]
Owns: one transaction's buffers, frozen holders/casters/opponents, saved aim, result flags, group cursors/order sample and diagnostics.  
API: BeginTransaction, RecordResult, Dispatch, EndTransaction, called by director.  
Events: none; not a subscriber.  
Invariants: `TRIGGER-03`, `TRIGGER-05`, `TRIGGER-07`, `TRIGGER-08`; `CLASH-14`, `CLASH-15`.  
Never owns: runtime pending state, caster gates, commitment, timers or delivery.
