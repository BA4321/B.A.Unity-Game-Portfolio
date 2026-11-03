# Stick Legions system map — 03 Health, death, XP & Vulnerable lifetime

Load with Core. Producers own damage calculation/selection: Clash, modifier DoT or ability delivery. This file owns generalized health, event and attribution semantics.

## Damage and death pipeline

- **HEALTH-01 — Acceptance.** Producer supplies positive finite finalized damage/context. Health requires active component/root, living state and usable combatant/runtime-stats/death. Freeze context; reject an ineligible receiver or invalid amount; then reject a receiver whose root is locked by the battle result lock (`CombatLock2D`, `BATTLE-LOCK-01`) exactly like an ineligible one (zero result, no events), adding one to `CombatLock2D.BlockedDamageCount`. Then refresh effective maximum, recheck eligibility and compute loss from current health. The lock check precedes every reconciliation callback; the lock is set only in the battle manager's FixedUpdate, never inside a damage callback, so it cannot appear between the check and the mutation. The gate covers `ApplyDamage` only: forced death and clamp deaths (`HEALTH-04`) are not gated. Health adds no Dodge roll, Vulnerable multiplier or origin formula. CausedDeath means positive health became zero; damage cannot process after health/death is dead. [Lock gate WM5: I11 DoT gate after the lock, blocked damage rose and no deaths after the lock (I11 source); I13 20-battle deaths after lock 0]
- **HEALTH-02 — Lethal event order.** Commit health and authoritative result → internal LethalDamageAccepted → HealthChanged → death lifecycle → DamageReceived → source DamageDealt. Lethal attribution therefore captures before public callbacks can change bounty or disable XP. Nonlethal accepted loss also produces factual damage events.
- **HEALTH-03 — Permissions and reentrancy.** Positive actual loss emits factual events even when CanTriggerDamageEffects is false; secondary-effect subscribers must inspect the copied flag. CanAwardKillExperience separately gates attribution. Snapshot subscribers, isolate exceptions, keep nested delivery synchronous. Disable does not revoke a committed hit. Destruction stops remaining target delivery, while existing source health can still receive DamageDealt; source death/disable alone does not exclude it. Health/death/motor emitter delivery stops after emitter destruction; retained Clash records are the explicit exception in `RESULT-03`.
- **HEALTH-04 — Maximum changes.** Initialize to effective maximum; refresh on enable, health FixedUpdate, before accepted damage and before an active unit's regeneration heal (`HEALTH-05`). Increase does not heal; decrease clamps; maximum-only changes notify HealthChanged. Current health remains within zero/effective maximum. Death from a clamp or ForceDeath is unattributed, not a damage/kill-XP transaction. There is no runtime maximum-health modifier target. Regeneration (`HEALTH-05`) is the only gameplay path that raises current health; the development-only debug setter there is not a gameplay path. The enable, FixedUpdate and damage refreshes skip an inactive component, so a parked, Escaped or quarantined unit's maximum is reconciled only at its next enable or by the heal path (and the debug setter, which shares it), which clamps it down but never to death: an inactive unit with a nonpositive maximum refuses the heal instead. [Regeneration clauses WM6: I14 tests 2 and 3. The inactive-unit refusal instead of a clamp death WM6-S]
- **HEALTH-05 — Regeneration heal (Phase 6, I14).** Internal `ApplyRegeneration(fraction)` returns Healed, AlreadyFull or Ineligible. Its only caller in the I14 source is the shared tick (`WM-REGEN-01`, `WM-REGEN-02`).
  - **Eligibility.** A positive finite fraction; the component has run Awake (`didAwake`) and is enabled; combatant, runtime stats and death are present and enabled; death has not happened; current health > 0. Component `enabled` flags are checked, not `isActiveAndEnabled`, so an inactive (parked, Escaped or quarantined) unit qualifies, while a never-activated unit (no Awake, health 0) does not, even if a caller forgets `HasEverActivated`.
  - **Maximum, active unit.** The usual reconciliation (`HEALTH-04`: clamp down, maximum-only notification, unattributed clamp death), then eligibility is rechecked; a death or loss of eligibility there makes the heal Ineligible.
  - **Maximum, inactive unit.** The effective maximum is read once; that read can notify (`STAT-01`), so eligibility is rechecked and the unit must still be inactive. A nonpositive maximum refuses the heal. Current health above the maximum is clamped down to it, and HealthChanged is raised if the maximum or health changed, followed by another recheck. The unit is never clamped to death and never woken.
  - **Amount.** At or above the maximum → AlreadyFull. Otherwise heal maximum × min(1, fraction), capped at the maximum (an amount that is not positive and finite → Ineligible); a result within 1e-4 × maximum of full snaps to exactly full, so 50% + 25 × 2% ends at the maximum. A result that does not exceed the previous health → AlreadyFull. No callback runs between the reconciliation and the commit. Then HealthChanged(previous, healed, maximum). No damage, death or XP event is raised.
  - **Not gated** by `CombatLock2D`: locked units heal during the end delay (Roadmap §1.4). Raising the maximum still never heals (`HEALTH-04`).
  - **Debug setter.** Internal `DebugSetHealthFraction(fraction)`, compiled only in the Editor and development builds, sets health to fraction × maximum for 0 < fraction ≤ 1 under the same eligibility and reconciliation, parked or deployed. It can lower or raise health, raises only HealthChanged, never kills or wakes a unit, is not lock-gated and is not a gameplay path. The controller's I14 context menus use it, and so do the battle manager's I15 retreat-threshold tools (`DebugSetFieldHealth`, `DebugSetQueuedHealth`), whose HealthChanged reaches the retreat latch like damage or a heal (`BATTLE-RETREAT-02`).

  [WM6: I14 test 2 (a parked unit at 50% is full after exactly 25 ticks and stays inactive), test 3 (deployed units heal; ticks continue while the map is frozen and in the locked delay), the extra check (a deployed unit reached 100% at tick 25 inside a long locked delay). The didAwake guard, the inactive-unit refusal, a zero fraction, the active-path clamp death, which of the cap or the 1e-4 snap ended test 2's 25th heal, and a player build without the debug setter WM6-S]
- **DEATH-01 — One-time lifecycle.** Health-driven death requires zero health. Commit HasDied, request motor Dead, emit Died, schedule configured destruction once. Missing motor cannot revoke death. ForceDeath asks health to perform zeroing; death does not directly own health mutation. Motor's callback-safe stop is `MOVE-03`.

## Attribution and finalization

- **XP-01 — Once per victim.** Kill-XP subscribes to internal lethal acceptance and resolves attribution once, including ineligible cases. Require XP permission, victim progression and a different existing enemy source with progression; self/friendly/missing sources do not prepare an award. Capture killer progression and victim bounty before public health callbacks. Later requests cannot replace attribution. Killer need not be alive/enabled.
- **XP-02 — Deferral.** Begin/EndClashAwardDeferral maintain victim-side depth. While deferred, finalization leaves pending data untouched. Otherwise clear pending fields before GainExperience to prevent duplicate/reentrant awards. Retained managed victim state may finalize after victim component destruction; killer progression must still exist. Mutual-kill award calls are supported, not guaranteed XP to a destroyed killer progression.
- **XP-03 — Caller responsibility.** Director brackets both result attempts and exact consumption, then finalizes both before reactions (`CLASH-11`–`CLASH-13`). DoT runtime and direct ability delivery retain victim XP and finalize in finally. Nested finalizers respect active victim deferral. No other delivery/finalizer is established for Summon.
- **PROGRESSION-01 — Advancement.** Start at configured level, at least one; reject nonpositive/non-finite gains. One gain may advance multiple levels; recalculate requirement/bounty and emit OnLevelChanged once per gained level. OnExperienceChanged reports within-level XP. Formulas/defaults belong to UnitDefinition2D source.
- **VULNERABLE-01 — Timer only.** ApplyOrExtend ignores nonpositive durations and only advances expiration; emit VulnerabilityRefreshed only on extension. Consequences belong to effective stats/Clash. Current movement uses Time.time; Clash snapshots query transaction time. Dodge suppresses new application without clearing an existing timer (`RESULT-02`).

## Contract cards

### CombatDamageTypes2D [F,B]
Owns: DamageOrigin2D, DamageContext2D, DamageResult2D definitions.  
API: request amount/source/origin/two permissions; result requested/actual loss, previous/remaining health, caused-death, source/target and copied permissions.  
Events: none.  
Invariants: `HEALTH-01`, `HEALTH-02`, `HEALTH-03`; Core `CORE-02`.  
Never owns: formulas, mutation, death or attribution processing.

### UnitHealth2D [F,B, WM5, WM6, WM7]
Owns: current health, observed maximum, damage acceptance, authoritative result construction and the regeneration heal (the only gameplay healing path).  
API: ApplyDamage (reads `CombatLock2D.IsLocked`, I11); CurrentHealth, MaximumHealth, IsDead (also true for a never-activated unit, whose health is still 0); internal forced-death zeroing (not lock-gated); internal `ApplyRegeneration(float) → HealthRegenerationOutcome2D` (I14; not lock-gated; called only by `UnitRegeneration2D`); internal `DebugSetHealthFraction(float) → bool` (I14; Editor and development builds only). `HealthRegenerationOutcome2D` is declared in `UnitRegeneration2D.cs`.  
Events: LethalDamageAccepted → kill XP; HealthChanged → modifier runtime/UI, and since I15 `BattleManager2D` for each retreat-eligible field unit (the retreat latch: it records the threshold crossing from the event's own values and changes nothing else, so it is safe inside a Clash transaction, `BATTLE-RETREAT-02`; unsubscribed when the unit leaves the field). HealthChanged is also raised by each heal and by the inactive-unit reconciliation; normally nothing subscribes while a unit is parked, per the source comment. DamageReceived and source DamageDealt have no established production subscriber. [Battle subscriber WM7: I15 30% threshold and "healing never cancels a retreat"]  
Invariants: `HEALTH-01`–`HEALTH-05`; `BATTLE-LOCK-01`; `WM-REGEN-02`.  
Never owns: maximum calculation, death lifecycle, XP, Clash effects, or when regeneration ticks (`UnitRegeneration2D`).

### UnitDeath2D [F,B]
Owns: HasDied, one-time cleanup scheduling and destruction settings.  
API: ForceDeath; internal ExecuteDeathFromHealth.  
Events: Died → modifier runtime cleanup; `BattleManager2D` while the unit is deployed (unregisters its arena body, releases ownership and records the death in the open death batch, `BATTLE-UNIT-01`, `BATTLE-END-01`). The battle manager's debug kill uses ForceDeath and refuses after a result lock (`BATTLE-LOCK-01`).  
Invariants: `DEATH-01`, `HEALTH-03`.  
Never owns: health mutation, motor state or killer attribution.

### UnitKillExperience2D [F,B]
Owns: attribution-resolved flag, pending killer/bounty, deferral depth and lethal subscription.  
API: FinalizePendingKillExperienceAward; internal Begin/EndClashAwardDeferral.  
Events: subscribes to LethalDamageAccepted; emits none.  
Invariants: `XP-01`, `XP-02`, `XP-03`.  
Never owns: XP totals, level, bounty formula, death or Clash sequencing.

### UnitProgression2D [B]
Owns: level, within-level XP, requirement/bounty and multi-level advancement.  
API: GainExperience; current level/experience/requirement/bounty properties.  
Events: OnLevelChanged → UI; OnExperienceChanged has no established subscriber.  
Invariants: `PROGRESSION-01`; Core `INIT-01`, `CORE-03`.  
Never owns: attribution, health, death or effective stats.

### UnitVulnerability2D [B]
Owns: Vulnerable expiration.  
API: ApplyOrExtendVulnerability, IsVulnerableAt, GetRemainingVulnerability; current/remaining properties.  
Events: VulnerabilityRefreshed; no established subscriber.  
Invariants: `VULNERABLE-01`.  
Never owns: movement/strength consequences or Clash outcome processing.
