# Health, death, XP & Vulnerable lifetime

Load with Core. Producers own damage calculation/selection: Clash, modifier DoT or ability delivery. This file owns generalized health, event and attribution semantics.

## Damage and death pipeline

- **HEALTH-01 — Acceptance.** Producer supplies positive finite finalized damage/context. Health requires active component/root, living state and usable combatant/runtime-stats/death. Freeze context, refresh effective maximum, recheck eligibility, then compute loss from current health. Health adds no Dodge roll, Vulnerable multiplier or origin formula. CausedDeath means positive health became zero; damage cannot process after health/death is dead.
- **HEALTH-02 — Lethal event order.** Commit health and authoritative result → internal LethalDamageAccepted → HealthChanged → death lifecycle → DamageReceived → source DamageDealt. Lethal attribution therefore captures before public callbacks can change bounty or disable XP. Nonlethal accepted loss also produces factual damage events.
- **HEALTH-03 — Permissions and reentrancy.** Positive actual loss emits factual events even when CanTriggerDamageEffects is false; secondary-effect subscribers must inspect the copied flag. CanAwardKillExperience separately gates attribution. Snapshot subscribers, isolate exceptions, keep nested delivery synchronous. Disable does not revoke a committed hit. Destruction stops remaining target delivery, while existing source health can still receive DamageDealt; source death/disable alone does not exclude it. Health/death/motor emitter delivery stops after emitter destruction; retained Clash records are the explicit exception in `RESULT-03`.
- **HEALTH-04 — Maximum changes.** Initialize to effective maximum; refresh on enable, health FixedUpdate and before accepted damage. Increase does not heal; decrease clamps; maximum-only changes notify HealthChanged. Current health remains within zero/effective maximum. Death from a clamp or ForceDeath is unattributed, not a damage/kill-XP transaction. There is no runtime maximum-health modifier target.
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

### UnitHealth2D [F,B]
Owns: current health, observed maximum, damage acceptance and authoritative result construction.  
API: ApplyDamage; CurrentHealth, MaximumHealth, IsDead; internal forced-death zeroing.  
Events: LethalDamageAccepted → kill XP; HealthChanged → modifier runtime/UI; DamageReceived and source DamageDealt have no established production subscriber.  
Invariants: `HEALTH-01`–`HEALTH-04`.  
Never owns: maximum calculation, death lifecycle, XP or Clash effects.

### UnitDeath2D [F,B]
Owns: HasDied, one-time cleanup scheduling and destruction settings.  
API: ForceDeath; internal ExecuteDeathFromHealth.  
Events: Died → modifier runtime cleanup.  
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
