# Movement & Sidestep

Load with `00-Core.md`. For effective values read Modifiers; for paired preparation order read Clash. Setup and execution attributes live in Core.

Battle-arena note (Phase 3): `UnitDestinationOverride2D` exists (`ARENA-DEST-01` in `07`), and the arena exposes `ArenaBody.IsFullyInside` (`ARENA-BODY-02`). No movement component reads either yet; `MOVE-01` and every Sidestep rule below are unchanged until Phase 4 wires the override into the motor and Sidestep.

## Targeting and physical movement

- **TARGET-01 — Identity and eligibility.** Enemy means a different non-null combatant with a different faction. Faction is a serialized field, readable without Awake. It changes only through `SetFaction`, which succeeds only while the GameObject is inactive in the hierarchy; an active unit logs an error, keeps its faction and gets false. Enemy relations therefore cannot change while a unit can fight (world-map callers: `WM-RECRUIT-01`, `WM-FACTION-01`). Targetability requires active root/combatant/health, living health and active motor outside Disabled/Dead. Dead/Disabled owners clear their target; Knockback keeps target maintenance.
- **TARGET-02 — Selection.** Query configured enemy layers within authored radius using reusable storage; resolve combatant locally or in parents and choose nearest squared distance among returned valid enemies. Searches are interval-based, staggered on enable; invalidating an assigned target triggers immediate replacement search. Unlike Area collection, no grow-and-retry completeness guarantee is established for this targeting buffer.
- **MOVE-01 — Momentum and steering.** Each moving fixed step clamps clean forward speed to effective maximum, then advances toward it using effective acceleration. A lower limit applies even with zero acceleration; raising the limit grants no instant speed. Explicit speed set/multiply also clamps. Ordinary movement faces the live target and applies forward velocity. Knockback suppresses normal steering but continues rebuilding forward momentum and combines forward/Knockback velocities; expiration returns Seeking.
- **MOVE-02 — Snapshot reconciliation.** TryReconcileForwardSpeedLimit clamps without advancing momentum or decaying Knockback, rebuilding velocity from facing × (clean speed + stored Sidestep bonus), plus separate Knockback while in that state. Stat changes do not use modifier events to immediately rewrite velocity.
- **MOVE-03 — State and intent.** Disabled/Dead stop Rigidbody motion; SetDisabled cannot exit Dead. MarkDead stops motion before and after state callbacks. CanReadMovement requires enabled motor, usable dependencies and neither Disabled nor Dead. Motor disable/Disabled/Dead clears its bonus channel. After momentum/Knockback handling and before ordinary Seeking, motor asks Sidestep for intent using applied speed and fixed delta; returned velocity drives the body, while effective turn speed × returned multiplier faces the live target. Sidestep never reads runtime stats/controller directly.

## Sidestep lifecycle pipeline

- **SIDESTEP-GATE-01 — Continuous band eligibility.** Every fixed step inside the inclusive squared min/max distance band is an opportunity; no band crossing is needed and rejected opportunities are not queued. Target loss/replacement discards observed identity but preserves the unit-wide retry cooldown.
- **SIDESTEP-GATE-02 — Ordered gates and retry.** Enabled configuration → expired retry cooldown → no active/pending/ending lifecycle → enabled Seeking motor → still-current target, enabled targeting and targetable target → chance roll. Pre-roll rejection starts no cooldown. At most one roll per step. Chance failure starts cooldown; every started lifecycle completion/cancellation, including path or bonus failure, starts cooldown through cleanup. Positive retry cooldown spaces repeated opportunities; zero allows a roll each eligible step, making eventual success increasingly likely for nonzero chance.
- **SIDESTEP-PATH-01 — Geometry and Wall preflight.** Snapshot start and target endpoint once as opposite ends of a fixed half-circle; reject non-finite/unusable geometry. Randomly prefer CW/CCW, try the opposite if blocked. Clearance covers the full body with start overlap and conservative chord circle casts including curvature/numeric padding; queries exclude triggers and reuse buffers. Later target movement never rebuilds the arc. Exact subdivision count/padding belongs to source.
- **SIDESTEP-PATH-02 — Intent and progress.** Progress uses supplied applied speed and fixed delta against the reference arc, not realized Rigidbody displacement; displacement does not steer back onto that arc. Deliver at most one movement interval per fixed step and the final interval exactly once. Live target remains the facing target.
- **SIDESTEP-LIFE-01 — Start/end.** Become active only after valid path, Wall clearance and motor bonus application. Compute/store a fixed bonus from clean speed once at start. Centralized reentrancy-safe cleanup marks inactive, removes the exact stored bonus if applied, invalidates path, records end reason and starts cooldown. The motor owns the applied channel; lifecycle owns bookkeeping.
- **SIDESTEP-LIFE-02 — Interruptions.** Cancel on Knockback/Disabled/Dead StateChanged, unavailable/disabled targeting or motor, lost/replaced/untargetable target, non-Seeking state, invalid path, component disable or live Wall enter/stay. OnDisable unsubscribes, cancels and discards observed identity. Sidestep subscribes to motor state, not directly to health/death.
- **SIDESTEP-CLASH-01 — Accepted preparation.** Participant delegates PrepareForAcceptedClash to same-root Sidestep. Active session cancels normally, removing bonus and requesting invalidation/cooldown, then motor rebuilds clean velocity from current facing. Inactive session is a no-op. Director's paired ordering is `CLASH-07`–`CLASH-09`.
- **SIDESTEP-ORDER-01 — Fixed coordinator.** Validate non-Clash interruptions → finish prior delivered final interval → reconcile observed target/in-band status → evaluate one opportunity → reset per-step intent/cache for later motor consumption. All Unity messages are on UnitSidestep2D.cs.

## Contract cards

### UnitCombatant2D [V7, WM2]
Owns: faction identity; targetability facade.  
API: Faction, Health, WorldPosition, MajorState, CanBeTargeted, IsEnemyOf; `SetFaction(FactionId) → bool` (inactive-in-hierarchy only; safe before Awake).  
Events: none documented.  
Invariants: `TARGET-01`.  
Never owns: target selection, health, movement state, Sidestep or progression.

### UnitTargeting2D [B]
Owns: current target, search deadline, reusable query/filter state.  
API: CurrentTarget, NextSearchTime.  
Events: none documented.  
Invariants: `TARGET-01`, `TARGET-02`.  
Never owns: faction, movement, Sidestep eligibility or damage.

### UnitMotor2D [F,B]
Owns: major state, clean speed, applied bonus, Knockback, Rigidbody writes.  
API: BeginKnockback, Set/MultiplyForwardSpeed, TryApply/RemoveSidestepSpeedBonus, RebuildCleanForwardVelocityFromCurrentFacing, TryReconcileForwardSpeedLimit, SetDisabled, MarkDead, CanReadMovement.  
Events: StateChanged → enabled Sidestep interruption handler.  
Invariants: `MOVE-01`, `MOVE-02`, `MOVE-03`.  
Never owns: targeting, effective stats, Sidestep path/lifecycle/cooldown, health or death lifecycle.

### UnitSidestep2D [SID]
Owns: one session, observed target, unit-wide cooldown, path/Wall state and bonus bookkeeping.  
API: TryConsumeSidestepMovementIntent; PrepareForAcceptedClash.  
Events: none emitted; subscribes to motor StateChanged while enabled.  
Invariants: `SIDESTEP-GATE-01`, `SIDESTEP-GATE-02`, `SIDESTEP-PATH-01`, `SIDESTEP-PATH-02`, `SIDESTEP-LIFE-01`, `SIDESTEP-LIFE-02`, `SIDESTEP-CLASH-01`, `SIDESTEP-ORDER-01`.  
Never owns: target selection, major state, Rigidbody execution, effective stats or paired transaction order.

| Partial source | Exclusive state/responsibility within the one compiled type |
| --- | --- |
| UnitSidestep2D.cs | Serialized references, initialization/validation and all Unity entry points; only file declaring MonoBehaviour/component/order attributes |
| UnitSidestep2D.Eligibility.cs | observedTarget and retry expiration; continuous band checks/gates |
| UnitSidestep2D.Lifecycle.cs | Pending/active/ending flags, stored bonus/applied flag, end reason; lifecycle mutation |
| UnitSidestep2D.Path.cs | Snapshots, center/radius/angle, directions, progress, path state, cached intent and delivery flags; path mutation |
| UnitSidestep2D.WallClearance.cs | Layer/mask/filter/query arrays; requests cancellation through lifecycle |
| UnitSidestep2D.Interruptions.cs | Motor subscription flag and validity checks; requests cancellation |
| UnitSidestep2D.Clash.cs | Stateless accepted-Clash bridge |
