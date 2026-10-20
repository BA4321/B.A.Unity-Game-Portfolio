# Stick Legions system map — 07 World map & battle

Load with `00-Core.md` for world-map or battle-arena work. State after Roadmap V6 **Phase 3 (complete)**:
- **World map (Phases 1–2).** Armies are constant-speed bouncing marbles that own a unit roster. Settlements own a garrison and are the single recruitment entry point. Every persistent unit is parked (inactive, never awakened) in a reserve and has exactly one owner. Deployment, overflow, gated transfers and the three allied contact interactions take effect. Enemy intents (FieldBattle, Capture, Siege) are still reports only.
- **Battle arena (Phase 3).** A debug-driven arena can be created far from the map. It freezes the map, swaps to the battle camera, builds terrain, a perimeter, entry strips and retreat gates, shrinks its view, and tracks registered bodies through entry, leaks, re-entry and escape. Only debug test bodies use it.

Nothing activates a unit or registers one with the arena. No battle starts from a contact, and no result, lock, routing, capture, retreat timer or recovery exists (`CORE-02`). Evidence tags `[WM1]`–`[WM3-S]` are defined in `90-Evidence.md` and summarised in `CORE-04`.

## Scope

**WM-SCOPE-01 — What takes effect.** Classification produces a `WorldMapContactIntent` plus diagnostics.
- Allied intents RetreatTransfer, RetreatMerge and JoinGarrison are applied after dispatch (`WM-ALLY-01`–`WM-ALLY-03`).
- BounceTransfersAvailable has no automatic effect; selected transfers go through the gated API (`WM-XFER-02`).
- FieldBattle, Capture and Siege remain reports only. Battle intents are selected (`WM-BATTLE-01`) but never started.
- Bounce intents have no effect beyond the bounce Army2D computes independently of classification (`WM-MOVE-02`).
- `Army2D.IsRetreating` is set only through `SetRetreating`. Nothing sets or clears it automatically yet: no loser routing and no 20 s recovery.

[WM2: allied interactions. Enemy report-only: source, with Phase 1 WM1 coverage]

## Lifecycle and time

**WM-LIFE-01 — Controller readiness.** One scene `WorldMapController2D` sets `Instance` in Awake; a second one logs an error and disables itself. A missing rules asset is replaced by a non-saved default instance, which is destroyed in OnDestroy. Unresolved WM layers disable the controller, so `IsReady` stays false.

Awake then, in order:
1. Reports matrix problems (`SETUP-06`) and warns on non-identity rotation or scale.
2. Creates the `Armies` child root and the runtime bounds.
3. Creates the unit reserve (`WM-UNIT-01`).
4. Sets `IsReady`.

Spawning, freezing, recruitment, deployment, transfers and contact intake require `IsReady`. OnApplicationQuit and OnDestroy enter shutdown. OnDestroy also destroys the bounds and the fallback rules and clears `Instance`. During shutdown, army and settlement destruction raises no events, leaves registries untouched and skips owner-loss handling (`WM-OWN-02`). [WM1: fallback-rules cleanup; shutdown ordering WM2-S, untested]

**WM-LIFE-02 — Bounds.** `MapBounds` is a world rectangle. Awake builds a separate, unparented `WM_Bounds (runtime)` object with a closed, frictionless EdgeCollider2D loop from it; later edits to the serialized rectangle do not rebuild it. Bounds contacts bounce armies but are never reported (`WM-CONTACT-01`). [WM1]

**WM-TIME-01 — Step order and map time.** The controller's FixedUpdate runs at order −1000, before Army2D at the default order. Each ready step does the following, in order:
1. Classify and dispatch contacts gathered during the previous simulation (`WM-CONTACT-03`, `WM-CONTACT-05`).
2. If unfrozen, apply allied interactions (`WM-ALLY-01`).
3. Increment `PhysicsStep`.
4. If unfrozen, add `Time.fixedDeltaTime` to `MapTime`.
5. If unfrozen, run settlement work: pending deployments, then overflow, per settlement in registry order (`WM-DEPLOY-01`, `WM-OVERFLOW-01`). An `ArmySpawned` handler that freezes the map stops the rest of that step's settlement work.

Consequences:
- A garrison join is overflow-checked in the same step.
- Retry timers compare against the already-advanced MapTime.
- `PhysicsStep` counts every ready FixedUpdate, frozen or not. `MapTime` advances only while unfrozen and is the clock for all world-map timers, independent of GameplayTime and Time.time.
- Callbacks are stamped with the values current during the simulation that produced them.

Army2D, Settlement2D and the debug panel declare no execution order. [WM2: same-step join + overflow; MapTime arithmetic source]

**WM-FREEZE-01 — Freezing.** `SetFrozen(bool)` is a no-op when the controller is not ready or the value is unchanged. Otherwise it:
1. Sets `IsFrozen`.
2. On freeze, clears in-flight contacts and episode history.
3. For every registered, non-removed army, sets Rigidbody `simulated = !frozen` and discards its pending bounce. On unfreeze it also resets speed measurement.
4. On unfreeze, flushes deferred recruitment in every settlement (`WM-RECRUIT-01`).
5. Raises `FrozenChanged`.

`Time.timeScale` is never touched. While frozen:
- MapTime stops, and armies neither move nor bounce.
- Contact intake and classification discard everything, and allied interactions and settlement work do not run.
- Recruitment is queued, and `DeployArmy` stores a pending request.
- Gated transfers are refused, and army creation is refused.

Pending deployments and overflow run on the first unfrozen controller step after unfreeze. `DestroyArmy`, `SetRetreating`, `Settlement2D.SetFaction` and roster `Order` are not blocked by the API; only the debug panel disables them. Pairs still touching after unfreeze start new episodes.

**Arena ownership.** An arena can be created only while the map is unfrozen, and it then owns the freeze (`ARENA-LIFE-02`). Unfreezing the map by any other path while an arena exists aborts the arena (`ARENA-LIFE-03`). The world-map debug panel's Freeze toggle is not yet gated by `BattleArena2D.IsAnyArenaActive`; Phase 4 adds that gate.

[WM1: freeze with many armies; WM2: gates refuse while frozen; recruitment/deployment deferral WM2-S; arena freeze ownership WM3: I06 regressions]

## Persistent units and ownership

**WM-UNIT-01 — Reserve and parking.**
- **Storage.** `UnitReserve2D` is a plain C# object created by the controller. It is not a component: a component on an inactive root would never initialize. It owns one root, `ParkedUnits (inactive)`, a child of the controller with identity local transform (world origin and scale 1 when the controller is placed per `SETUP-06`), made inactive before any unit exists. `CreateInto` refuses to create units if this root is missing or active.
- **Recruiting.** Each recruit is instantiated directly under that inactive root, so Awake, OnEnable and Start never run. It is then set inactive itself, so a later move under an active parent cannot wake it.
- **Location.** Every owned unit stays under this root for its whole life in Phase 2. Ownership is logical, not hierarchical: transfers never reparent, and armies never carry unit transforms.
- **Activation.** Nothing in Phase 2 activates a unit. Only the future battle spawn path may (Roadmap §2.2), and it must set `UnitHandle.HasEverActivated`.

[WM2: audit "never awakened" passed every run]

**WM-UNIT-02 — Handle identity and safe reads.** Each unit has one `UnitHandle` for life.
- **Identity.** `Id` is sequential from 1 per reserve. The handle keeps the same `GameObject` through every transfer.
- **Captured at creation.** `Definition` is taken from the clone's `UnitRuntimeStats2D.Definition`, a serialized field. `Progression` may be null.
- **Wake-safe reads.** Every handle read is safe on a parked, never-awakened unit:
  - `Faction` reads the combatant's serialized field.
  - `IsSettlementOnly` reads `Definition.SettlementOnly`.
  - `CurrentLevel` returns live progression only when `Progression.didAwake` is true and its level is ≥ 1. Otherwise it returns the authored starting level, clamped to ≥ 1.
  - `IsParked` means status Reserve and the GameObject is not `activeSelf`.

These reads never initialize the unit (`CORE-03`). Phase 2 statuses are only Reserve and Discarded. [WM2: units keep GameObjects through transfers; live-level path untested]

**WM-OWN-01 — One owner, one mutator.** Every living unit is in exactly one roster, and `UnitHandle.Owner` names it. Only `UnitReserve2D` changes ownership, through four operations:
- `CreateInto` (recruitment): each handle is registered and appended in the same step, so no handle ever exists without an owner.
- `Transfer` and `TransferFirst` (`WM-XFER-01`).
- `DiscardAll`: sets status Discarded, unregisters the handle, destroys the GameObject and closes nothing itself.

Rosters, lists and selections held elsewhere are indexes, not owners. [WM2: audit passed every run]

**WM-OWN-02 — Owner loss and removal.**
- **Refusal.** `DestroyArmy` refuses an army that still owns units and logs a warning, unless `discardUnits` is true. The discard path is debug only and logs the count.
- **Lost owner.** If an army is Unity-destroyed without `DestroyArmy`, or a registered settlement is destroyed, the controller discards the orphaned units loudly with an error log and closes the roster. Units are never silently lost or left unowned. This handling is skipped during shutdown.
- **Closed rosters** accept no recruitment, transfer or deployment.

[DestroyArmy refusal WM2-S; owner destroyed outside the API untested]

**WM-ROSTER-01 — Storage, order and capacity.**
- **Storage.** Arrival order, by ascending `ArrivalSequence`. One counter per reserve, so every arrival (recruit or transfer) gets a new, later sequence. Arrivals go to the back.
- **Order.** `Order` (`Fifo` or `HighestLevelFirst`) changes only how `GetUnitsInOrder` reads the roster. HighestLevelFirst sorts by `CurrentLevel` descending, then by arrival. This is a total order, so the result is deterministic. Changing `Order` raises no `Changed` and takes effect at the next read.
- **Order defaults.** Armies take `DefaultRosterOrder` at Initialize. Garrisons are created FIFO in Awake and take `DefaultRosterOrder` in Start on registration.
- **Capacity.** Armies use `MaxUnitsPerArmy`, read once at Initialize. Garrisons are unbounded (`int.MaxValue`): the garrison limit never blocks arrivals.
- **Counts.** `EligibleCount = Count − SettlementOnlyCount`. Only garrisons accept settlement-only units.
- **Faction.** Fixed for armies. A garrison changes faction only through `WM-FACTION-01`.

[WM2: both order modes and ties with authored levels; HighestLevelFirst with live levels untested]

**WM-ROSTER-02 — Changed event.** `UnitRoster.Changed` fires once per completed mutation batch per roster: recruitment, transfer, discard, or a garrison relabel that changes faction with units present. A transfer raises it on the source, then the destination. Subscribers are snapshotted and each exception is isolated and logged. Army and settlement labels refresh from it. [WM2-S]

## Recruitment and faction

**WM-RECRUIT-01 — Recruitment.** `Settlement2D.Recruit(prefab, count)` is the single entry point.

It is rejected, with nothing created or queued, when:
- `count ≤ 0`;
- the settlement is unregistered, or the controller or reserve is not ready;
- the garrison is closed;
- the prefab fails `UnitReserve2D.ValidatePrefab`. That check reads the prefab asset only and requires a non-scene prefab whose root has `UnitCombatant2D` and `UnitRuntimeStats2D` with a definition.

While frozen, the request is queued and returns Deferred. It is re-issued on unfreeze, before `FrozenChanged`, so it uses the settlement's faction at that time; a re-issued request that is rejected or creates fewer units logs a warning and is dropped. Otherwise units are created at once. Each clone is named `<prefab> U<id>`, labelled with the garrison's faction through `UnitCombatant2D.SetFaction`, then owned (`WM-OWN-01`). Creation may stop part-way; the result reports `Created < Requested` with the reason. `Changed` fires once if any unit was created. [WM2: recruitment; frozen deferral WM2-S]

**WM-FACTION-01 — Settlement faction change.** `Settlement2D.SetFaction` relabels the garrison all-or-nothing.
- **Refusal.** If any garrisoned unit is not parked (status not Reserve, or `activeInHierarchy`), it changes nothing, logs a warning and returns false.
- **Relabel.** Otherwise every unit's combatant faction is set, then the roster's, then the settlement's.
- **Pending deployment.** An actual faction change cancels the pending deployment.
- **Deferred recruits** are not relabelled. They take the faction current at unfreeze (`WM-RECRUIT-01`).
- **Unit-level guard.** `UnitCombatant2D.SetFaction` refuses while its GameObject is active in the hierarchy (`TARGET-01`).

[WM2-S]

## Transfers

**WM-XFER-01 — Transfer primitive (ungated).** `UnitReserve2D.Transfer(from, to, selection)`:
- **Rejected with no change** if either roster is null, both are the same, either is closed, or the factions differ.
- **Copies the selection first**, so a live roster list may be passed.
- **Per unit, in selection order, skipped** when:
  - invalid: not owned by the source, destroyed, status not Reserve, or `activeSelf`;
  - settlement-only into an army;
  - over capacity.
- A unit listed twice is skipped as invalid the second time.
- **Moved units** go to the back of the destination with a new arrival sequence.

The result counts Requested, Moved and each kind of skip. `TransferFirst(from, to, maxCount)` takes the front of the source's current order. For an army destination it excludes settlement-only units, then truncates to `maxCount`; `maxCount ≤ 0` is rejected. These primitives are public but are internal building blocks with no in-battle or frozen gate. Their source comment "In-battle blocking is I05's" is outdated. [WM2: selected transfers; skip accounting source]

**WM-XFER-02 — Gated transfers.** Gameplay and debug code use `TransferUnits(from, to, selection, out donorRemoved, removeEmptyDonor = true)` and `TransferFirstUnits(from, to, count, out donorRemoved, removeEmptyDonor = true)`.
- **Gate.** Both call `CanTransfer`, which refuses when the controller or reserve is not ready, the map is frozen, or `IsRosterInBattle(from or to)`. A refusal returns a rejected result with nothing moved.
- **Battle hook.** `IsRosterInBattle` returns false until Phase 4/5. While a battle runs, the freeze is the effective gate.
- **Empty donor.** An army emptied by a move is removed through `DestroyArmy` unless `removeEmptyDonor` is false.

[WM2: selected transfers, gates]

## Deployment, placement and overflow

**WM-DEPLOY-01 — Manual deployment requests.** `DeployArmy(settlement, Half | All)`, also reachable through `Settlement2D.RequestDeployment`:
- **Rejection.** Rejected if the controller is not ready, the settlement is unregistered, its garrison is closed, or it has no eligible units. A rejected call leaves any existing pending request untouched.
- **Storage.** Otherwise it stores one pending request per settlement, replacing any older one. Each request gets a new serial and its retry timer is reset.
- **When it runs.**
  - Frozen: returns Pending and runs after unfreeze.
  - Called while a deployment or settlement work is already running (for example from an `ArmySpawned` handler): returns Pending and runs from settlement work: later in the same step if that settlement has not been processed yet, otherwise on the next step.
  - Otherwise: runs immediately.
- **Count.** Computed when the request runs: `ComputeDeployCount(eligible, amount, MaxUnitsPerArmy)`. All = eligible; Half = max(1, ⌊eligible/2⌋); both capped. Units are taken in roster order, army-eligible only.
- **Outcome.**
  - Blocked placement keeps the request and retries after `PlacementRetrySeconds` of map time.
  - Success clears only the request that ran, so a newer one stored by a handler survives.
  - Failure, or no eligible units remaining, drops the request with a warning; a closed garrison drops it silently.
- `CancelPendingDeployment` removes the request.

[WM2: Half of 7 → 3; All of 250 → 200; settlement-only units stay; blocked placement retries]

**WM-DEPLOY-02 — Army creation pipeline.** All populated armies come from `TryDeployFromRoster`, which is internal and roster-generic so Phase 9 can reuse it. Steps:
1. **Validate.** Not ready → Failed; frozen → Blocked; source null or closed → Failed; count outside 1..MaxUnitsPerArmy → Failed; fewer eligible units than count → Failed.
2. **Place** (`WM-PLACE-01`). No spot → Blocked.
3. **Create the army unannounced.** No `ArmySpawned` yet; a failed creation → Blocked.
4. **Fill** with exactly `count` units through `TransferFirst`.
5. **Ignore the source settlement's collider** (`WM-IGNORE-01`).
6. **Update counters,** then raise `ArmySpawned(army, Deploy | Overflow)`.

Blocked and Failed change nothing. A partial fill is logged as an error and rolled back: units return to the source (at the back of its arrival order) and the army is removed silently with no `ArmyDestroyed`. If any unit cannot be returned, the army is announced instead, so nothing is lost. An empty populated army is never announced. Debug `TrySpawnArmy` / `TrySpawnArmyAtRandom` create and announce empty armies with whatever reason the caller passes; the debug panel always passes `Debug`. [WM2; rollback path untested]

**WM-PLACE-01 — Placement.** `FindArmyPlacements(center, sourceRadius, count, results)` searches `PlacementRings` rings of `PlacementSlotsPerRing` slots.
- **Ring distances.** The first ring is at `sourceRadius + (ArmyRadius + SpawnMargin) + 0.05`. Each further ring adds `2·ArmyRadius + SpawnMargin`.
- **Slot angles.** Start at a random angle; odd rings are offset by half a slot.
- **Acceptance.** A slot is accepted if `IsPlacementFree` passes and it does not overlap a placement already found in this batch. Its direction points outward from the source, nudged away from the axes.
- **Read-only.** It reserves nothing, so callers create armies immediately. Creation rechecks the spot, and the registry check in `IsPlacementFree` covers armies created earlier in the same frame.
- `GetPlacementOuterDistance(sourceRadius)` gives the outermost ring distance.
- **Not established:** overlapping rings between nearby settlements.

[WM2: blocked placement; overlap untested]

**WM-IGNORE-01 — Source-settlement collision ignore.** A deployed army calls `Physics2D.IgnoreCollision` for its own collider and its source settlement's collider only. The ignore is released in Army2D FixedUpdate (unfrozen, not removed) once both hold:
- map time ≥ deploy time + `SettlementCollisionIgnoreSeconds`;
- centre distance > army radius + settlement `WorldRadius` + 0.02.

While ignored, the pair neither bounces nor reports contacts. A destroyed settlement clears the ignore without restoring it. A new ignore replaces the old one. `Army2D.IgnoredSettlement` exposes it. [WM2-S; an army near an edge bouncing back through its ignored source is untested]

**WM-OVERFLOW-01 — Overflow.** Overflow is state-based, checked for every registered, open settlement in each unfrozen settlement-work step. It is never run inside roster callbacks. Each check:
1. Stop if the garrison total (including settlement-only units) is below `GetGarrisonLimit` (town 200, castle 300).
2. Stop if eligible units are fewer than `OverflowArmySize` (100, clamped to `MaxUnitsPerArmy`).
3. Stop if the overflow retry timer has not elapsed.
4. Deploy exactly `OverflowArmySize` units as an `Overflow` army, then check again.

It never sends a smaller army. A blocked or failed attempt sets the retry timer; a failure also warns. At most 16 overflow armies per settlement per step; the rest continue next step. The controller's `GetGarrisonLimit(settlement)` and `Settlement2D.GarrisonLimit` expose the limit (0 when unregistered). [WM2: castle 280 + 50 → 230 + one 100-unit army, including same-step after a garrison join]

## Allied interactions

**WM-ALLY-01 — Scheduling.** Allied interactions run from FixedUpdate after contact dispatch, never inside a `ContactReported` handler, and only if the map is unfrozen.
- **Which pairs.** A copy of `LastStepContacts` filtered to RetreatTransfer, RetreatMerge and JoinGarrison.
- **Every step.** They are re-applied on every step a pair touches, not only on a new episode, so capacity that opens later gets filled.
- **Live recheck.** Each pair is skipped unless both participants are live (initialized, not removed, roster open; settlement registered with an open garrison). It is then reclassified from live state, and army roles are recomputed with `SecondArmyFirst` from live counts. Earlier effects in the same step are therefore reflected.
- **In-battle hook.** A pair is skipped if `IsRosterInBattle` is true for either side.
- **Freezing.** Processing stops as soon as any callback freezes the map.
- **Record.** Each applied interaction (units moved or donor removed) increments its counter and appends an `AlliedInteractionRecord` (the last 24, oldest first, optionally logged). It then raises `AlliedInteractionApplied` after rosters, removal and counters are consistent. The whole multicast runs inside one try/catch, so a throwing subscriber's exception is logged and later subscribers in that invocation are skipped.

[WM2: allied interactions; ordering, record and event delivery source; handlers that freeze or redeploy untested]

**WM-ALLY-02 — Retreat transfer and merge.**
- **Roles.** For RetreatTransfer the retreating army is the donor. For RetreatMerge the smaller army donates; at equal size the higher id donates into the lower (older) id.
- **Move.** `TransferFirst(donor → receiver, donor.UnitCount)` up to the receiver's free capacity.
- **Donor left empty:** removed through `DestroyArmy`.
- **Donor left with units:** keeps them and stays retreating.
- **Receiver full:** nothing changes, nothing is recorded, and the pair retries while touching.
- **Flags.** Retreat flags are unchanged by both interactions. Merge timers do not exist yet; Phase 7 must keep the receiver's own timer.

[WM2: transfer at and below capacity; merges smaller-into-larger, equal sizes, partial]

**WM-ALLY-03 — Garrison join.** A retreating army touching an allied settlement transfers all its units into the garrison (unbounded) and is removed through `DestroyArmy`. If units remain, which source treats as a bug, it logs an error, keeps the army, which keeps owning them, and retries next touching step. The join is overflow-checked later in the same step (`WM-TIME-01`). [WM2: castle 280 + 50 join → 230 + 100]

**WM-AUDIT-01 — Ownership audit.** `AuditUnits(out report)` is development only and read-only. It audits all open garrisons and the rosters of non-removed armies.

Reported problems:
- **Reserve root:** destroyed or active.
- **Roster level:**
  - over capacity;
  - arrival order not strictly increasing;
  - settlement-only counter mismatch.
- **Unit level:**
  - listed by more than one roster, or Owner mismatch;
  - not registered;
  - settlement-only unit in an army;
  - destroyed while owned;
  - status not Reserve (the Phase 2 expectation);
  - active;
  - not under the parked root;
  - faction ≠ roster faction;
  - never activated but any component has `didAwake`.
- **Registry level:** registered units without an audited owner.

The report lists at most 40 problems. [WM2: passed every run]

## Armies

**WM-SPAWN-01 — Army creation.** Only the controller creates armies, through private `TryCreateArmy`. It returns false with no side effects if the controller is not ready, the map is frozen, or `IsPlacementFree` fails. Placement requires:
- the circle inside bounds;
- no WM collider within radius + spawn margin (physics overlap);
- no registered army within the same clearance by transform position, which covers same-frame spawns not yet in physics.

On success the controller creates GameObject `Army_<id>` on `WM_Army` under the Armies root and adds `Army2D`, whose RequireComponent adds Rigidbody2D and CircleCollider2D. It calls `Initialize` synchronously with the axis-nudged direction; this creates the army's roster (`WM-ROSTER-01`). The army is then registered.
- **Debug `TrySpawnArmy` / `TrySpawnArmyAtRandom`** announce the empty army immediately with the given reason.
- **Populated armies** are announced only by `WM-DEPLOY-02`.

`ArmySpawned` means the army is registered. It is filled before the announcement only when it comes from `WM-DEPLOY-02`; source does not stop a caller passing `Deploy` or `Overflow` to `TrySpawnArmy` for an empty army, so handlers read `UnitCount` rather than infer it from the reason. [WM1; WM2 for populated path]

**WM-SPAWN-02 — Army identity.** `ArmyId` is sequential from 1 per controller and never reused. `IsInitialized` means `Initialize` has run; contact intake ignores uninitialized armies. Faction has no setter after initialization; the roster's faction is fixed to it.

**WM-REMOVE-01 — Removal.** `DestroyArmy(army, discardUnits = false)` returns false for null, already-removed or unregistered armies, and for armies that own units (`WM-OWN-02`). Otherwise, in order:
1. Unregister the army and close its roster.
2. `MarkRemoved`: set `IsRemoved` and set Rigidbody `simulated = false` immediately, so this step's simulation neither bounces off nor reports it.
3. Raise `ArmyDestroyed` once.
4. Call deferred Destroy.

Handlers must still ignore `IsRemoved` armies. A direct Unity Destroy that bypasses `DestroyArmy` raises `ArmyDestroyed` from OnDestroy and triggers owner-loss handling, but never sets `IsRemoved`. Always use `DestroyArmy`. [WM1 for DestroyArmy; immediate simulation stop and refusal WM2-S; direct-destroy path untested]

**WM-MOVE-01 — Constant speed.** Each unfrozen FixedUpdate of a non-removed army: update the settlement ignore (`WM-IGNORE-01`), resolve any bounce, then set `linearVelocity = Direction × Rules.ArmySpeed`.
- **Rigidbody setup:** Dynamic, zero gravity and damping, mass 1, FreezeRotation, interpolated, continuous, never sleeps.
- **Colliders** are frictionless with zero bounciness, so the solver only separates overlaps and speed is restored every step.
- **Live versus fixed values.** ArmySpeed is read live; radius and sprite apply once at Initialize.
- `Position` is the Rigidbody position. `MeasuredSpeed` and `RecentBouncesPerSecond` are one-second diagnostics.

[WM1]

**WM-MOVE-02 — One bounce per step.** Enter and Stay collision callbacks against any WM collider (bounds included) add the unit vectors from each contact point to the army centre, at most 8 points per callback. This is independent of Collision2D normal conventions. The next FixedUpdate resolves at most one reflection against the normalized sum; if the direction already leaves the surface (dot ≥ 0), there is no bounce. Contact reporting never affects the bounce. Removed and frozen armies accumulate nothing. [WM1]

**WM-MOVE-03 — Jitter, axis nudge, safety.** The reflection is rotated by a uniform ±`BounceJitterDegrees`. `NudgeAwayFromAxes` then keeps it at least `MinAxisAngleDegrees` (clamped below 45°) from both axes, so armies cannot settle into straight back-and-forth bounces; an exact-axis tie picks a random side. If the result leaves the surface by less than about 10° (dot < 0.17), it is replaced by the normal rotated ±60° and nudged; this fallback is not rechecked. Spawn and placement directions are nudged too. [WM1]

## Settlements

**WM-SETTLE-01 — Requirements and presentation.** A settlement is a static footprint: a same-object CircleCollider2D (required; added with a warning if missing at runtime) and no Rigidbody2D (warning only).

Awake:
- creates the garrison (`WM-ROSTER-01`);
- puts the root on `WM_Settlement`;
- forces collider radius = `Radius`, zero offset, solid, frictionless;
- disables any leftover BoxCollider2D with a warning;
- builds visuals.

`WorldRadius` = `Radius` × the largest absolute X/Y scale; keep scale 1. Source warns only on non-uniform X/Y. `Position` is the transform position. The label shows the kind prefix and `GarrisonCount`, with `*` while a deployment is pending; it refreshes on garrison `Changed`. Kind, faction and radius are serialized. There is no movement API and no count setter. [WM1: CircleCollider2D retest]

**WM-SETTLE-02 — Registration and identity.** In Start a settlement uses its assigned controller or `Instance`, registers, receives `SettlementId` (sequential from 1 per controller, never reassigned) and sets the garrison's order to `DefaultRosterOrder`.
- `0` means unregistered. Its contacts are ignored, and recruitment and deployment are rejected.
- Start order among scene settlements, and therefore initial id order and settlement-work order, is not guaranteed.
- OnDestroy unregisters and triggers owner-loss handling, except during controller shutdown.
- No settlement events exist.

## Contact pipeline (`WorldMapController2D.Contacts.cs`)

**WM-CONTACT-01 — Intake.** Army2D forwards every Enter/Stay callback with the other collider and the mean contact point. Intake is ignored unless the controller is ready and unfrozen and the reporting army exists and is not removed. The other collider's own GameObject decides:
- `WM_Army` layer with a different, initialized, non-removed Army2D on that object;
- `WM_Settlement` layer with a Settlement2D on that object whose `SettlementId` is nonzero (registry membership is not rechecked);
- anything else (bounds, debug blockers on `WM_Bounds`) is not reported.

Intake captures the contact point, both positions, step and map time.

**WM-CONTACT-02 — Deduplication.** The pair key is the ordered army ids, or army id + settlement id (separate key spaces). The first callback for a key in a step is kept; later ones (the other army's callback, Enter+Stay) are dropped and counted in `DuplicateCallbacksMerged`. Result: at most one report per pair per physics step. [WM1]

**WM-CONTACT-03 — Touch episodes.** At the next controller FixedUpdate each pending pair is revalidated (removed or destroyed participants drop out), marked `IsNewContact` if absent from the previous step's pair set, and classified. A pair absent for one step ends its episode; freezing ends all episodes. Only new contacts update episode counters, `RecentNewContacts` (bounded, oldest first) and Console logging. `LastStepContacts` and `ContactReported` contain every touching pair every step. [WM1]

**WM-CONTACT-04 — Snapshot timing.** Contact point and positions are callback-time values (after the step's simulation). Faction, retreat flag and the real counts (`Army2D.UnitCount`, `Settlement2D.GarrisonCount`) are read at classification time. Changes between callback and classification are therefore reflected. Reports are immutable value copies but also hold references to the live Army2D/Settlement2D, which may later change or be removed. Allied interactions reclassify from live state rather than trusting report fields (`WM-ALLY-01`). [WM2-S]

**WM-CONTACT-05 — Dispatch.** After classification, statistics and battle selection, `ContactReported` is invoked once per report in `LastStepContacts` order. This happens before allied interactions, so reports describe the state at classification time, before allied effects. If a handler freezes the map:
- dispatch stops and allied interactions do not run that step;
- undelivered pairs are not queued and, if still touching after unfreezing, are reported as new episodes;
- freezing clears pending and episode state but leaves `LastStepContacts` readable for the interrupted dispatch.

Handlers must ignore removed armies (`WM-REMOVE-01`). The final I05 change to this file (log text and comments only) was not re-run in Unity. [WM2: freeze during dispatch; ordering source]

## Classification

**WM-CLASS-01 — Interaction table.** `WorldMapContactClassifier2D` is pure. Equal `FactionId` = allied; any other = enemy.

| Kind | Condition | Intent |
| --- | --- | --- |
| AlliedOrdinaryArmies | allied, neither retreating | BounceTransfersAvailable |
| AlliedRetreatingAndOrdinaryArmy | allied, exactly one retreating | RetreatTransfer |
| AlliedRetreatingArmies | allied, both retreating | RetreatMerge |
| RetreatingArmyAlliedSettlement | retreating army, allied settlement | JoinGarrison |
| OrdinaryArmyAlliedSettlement | ordinary army, allied settlement | Bounce |
| EnemyArmiesWithRetreating | enemies, at least one retreating | Bounce |
| RetreatingArmyEnemySettlement | retreating army, enemy settlement, any count | Bounce |
| EnemyOrdinaryArmies | enemies, neither retreating | FieldBattle |
| OrdinaryArmyEmptyEnemySettlement | ordinary army, enemy settlement, garrison 0 | Capture |
| OrdinaryArmyDefendedEnemySettlement | ordinary army, enemy settlement, garrison > 0 | Siege |

"Defended" uses `Settlement2D.GarrisonCount`, the real garrison total including settlement-only units. `StartsBattle` is true only for FieldBattle and Siege. [WM1: all 10 kinds live; self-test. Real counts: WM2-S]

**WM-CLASS-02 — Role order.** Settlement reports: the army is ArmyA. Army reports use `SecondArmyFirst`:
- RetreatTransfer: the retreating donor is ArmyA.
- RetreatMerge: the smaller `UnitCount` is ArmyA (donor); at equal counts the higher id is ArmyA, so the lower (older) id receives.
- Every other kind: the lower id is ArmyA.

Positions swap with roles. [WM1: self-test; WM2: live merges]

**WM-BATTLE-01 — Battle selection.** Each step, every touching pair whose intent starts a battle, new or continuing, is a candidate. One is chosen uniformly with `UnityEngine.Random` and stored in `LastSelectedBattle` with `LastBattleSelectionCandidates`. Steps with ≥ 2 candidates are counted; a Console line is written only when a candidate is new. Nothing starts; unselected pairs keep touching and compete again next step. The arena debug panel can copy the selected pick's contact point and counts into its create fields; nothing else consumes it. [WM1: Converge 4]

## Battle arena (Phase 3)

The arena is exercised only through `BattleArenaDebugPanel2D` and debug test bodies. Roadmap departures kept from I06–I07 (Roadmap §5 notes are guidance): both perimeter modes are kept, entry strips are pure geometry with no `SpawnBox2D` component, the "fewer than 30 units" shrink trigger is left to Phase 5, and the destination override is wired into motor and Sidestep in Phase 4.

**ARENA-LIFE-01 — Service and readiness.** One scene `BattleArena2D` (order −900) sets `Instance` in Awake; a second logs an error and disables itself.
- A missing `BattleRules2D` is replaced by a non-saved runtime default (warning), destroyed in OnDestroy.
- Awake resolves the battle layers from the rules' layer names (`SETUP-07`). If they resolve it reports matrix problems, otherwise it logs an error. Resolution happens once.
- An unassigned camera rig is found in the scene; none logs an error. The world map is the assigned controller, else `WorldMapController2D.Instance`.
- `IsReady` = component active and enabled, `Instance` is this, rules present, camera rig present and ready, battle layers resolved.
- `IsAnyArenaActive` = `Instance` exists and has an arena.

[WM3: every I06/I07 run; second-instance and missing-rig paths WM3-S]

**ARENA-LIFE-02 — Creation and refusals.** `TryCreate(mapPoint, countA, countB, gateEdges, out error)`; the overload without `gateEdges` uses all four edges (field battles). `mapPoint` selects the terrain; the counts set the starting view size.

Refused, with nothing changed and `TotalFailedCreates` incremented, when (checked in this order):
1. the lifecycle is busy; there is no camera rig, or it is not ready; battle layers are unresolved; the arena is not ready; an arena already exists; the map is missing or not ready; **the map is already frozen** (the arena must own the freeze);
2. a count is negative or the point is not finite;
3. the largest possible view (`CameraMaxSize`, aspect = max(live aspect, `ClearanceAspect`)) at `ArenaCenter`, expanded by `GeneratedGroundMargin + ArenaClearance`, overlaps `MapBounds`.

Gate edges are masked to the four edges. Then, with the lifecycle busy:
1. Create the root `BattleArena (runtime)` as a scene root at `ArenaCenter`, identity rotation, world scale 1.
2. Build terrain (`ARENA-TERRAIN-01`). Fail if the root's renderer and collider bounds overlap `MapBounds`.
3. Build the perimeter and gates for the view about to be shown, in the next perimeter mode (debug override, else rules) (`ARENA-PERIM-01`).
4. Record the map as the one this arena froze, call `SetFrozen(true)`, fail if it is not frozen, then subscribe to `FrozenChanged`. Recording first means a throwing `FrozenChanged` subscriber cannot leave the map frozen after rollback; subscribing after means the arena does not observe its own freeze.
5. Swap cameras at the computed size (`ARENA-CAM-01`).
6. Set `Center`, start/current/target view sizes, `MapPoint`, starting counts, `Battlefield`, `EscapesAccepted = true`, then `HasArena`.

Any failure or exception rolls back through teardown (`ARENA-LIFE-03`), counts a failed create and warns. Success increments `TotalCreated` and raises `ArenaCreated`.

[WM3: I06 regressions (create/remove, freeze ownership); I07 20 cycles. Throwing-subscriber rollback WM3-S: the I06 fix was not separately reported]

**ARENA-LIFE-03 — Removal, teardown and abort.** `Remove()` returns false with no arena or while busy, so removing twice is harmless. Otherwise it clears `HasArena`, tears down, increments `TotalRemoved` and raises `ArenaRemoved`.

An **abort** takes the same path and also increments `TotalAborts`. It happens when:
- the world map is unfrozen by anything else while the arena exists (warning);
- the arena component is disabled or destroyed while an arena exists (warning unless quitting).

**Teardown** is shared with failed creation. Each step is isolated so one failure cannot block the rest. Order:
1. exit the battle view;
2. unsubscribe from `FrozenChanged`;
3. release every registered body silently, with no escape events (`ARENA-BODY-01`);
4. set the root inactive (stops rendering, physics and gate callbacks this frame) and Destroy it;
5. unfreeze the map if it is ready and frozen (skipped while quitting).

It then clears perimeter references, sets `EscapesAccepted = false` and zeroes the view sizes. Destroyed roots disappear at the end of the frame, so audit at least one frame later. `ArenaCreated` and `ArenaRemoved` handlers are isolated per handler.

A camera rig that leaves battle view while the arena exists does not abort the arena; the arena stops stepping and the audit reports it.

[WM3: I07 20 create/remove cycles with bodies, audit passing; I06 regressions. Camera disabled or destroyed mid-arena and scene unload with an arena are untested]

**ARENA-CAM-01 — Camera swap.** `BattleCamera2D` toggles only Camera components; both camera GameObjects stay active, so the world camera's AudioListener stays enabled (Roadmap §2.10).
- **Awake** validates (`SETUP-07`). On success it disables the battle camera and enables the world camera; on failure it logs an error and `IsReady` stays false.
- **Enter.** `EnterBattleView(center, size, z)` requires ready, not already in battle view, both cameras, and a positive finite size. It places the battle camera at (center, z) with identity rotation, sets its orthographic size, then enables the battle camera and disables the world camera in the same call, so no rendered frame shows both or neither.
- **Resize.** `SetViewSize` works only in battle view and only for positive finite sizes. The "never grow" policy belongs to the arena (`ARENA-VIEW-01`).
- **Exit.** `ExitBattleView` is idempotent and tolerates destroyed cameras. Disabling the rig while in battle view warns and exits.
- **Monitoring.** Optional: LateUpdate logs an error whenever the enabled-camera count changes to anything but 1.
- **View rect.** `Aspect` is the battle camera's aspect (16:9 if unavailable); `GetViewRect()` is centred on the battle camera with half-height = size and half-width = size × aspect.

[WM3: I06 regressions (camera swap); the I07 20-cycle audit, run one frame after the last removal, checks the restored state (one enabled camera, world camera on, single listener on it). The in-battle direction has no separately named audit. Rig disabled mid-arena untested]

**ARENA-TERRAIN-01 — Battlefield selection and terrain.** `BattleRules2D.SelectBattlefield(mapPoint)` is pure:
- Among regions with a prefab whose area contains the point (xMin ≤ x < xMax, yMin ≤ y < yMax), the highest priority wins; equal priority → the earlier entry.
- Otherwise the fallback prefab; otherwise generated ground.

A selected prefab is validated from the asset only. It is refused if it contains a Camera or an AudioListener, or any object on a `WM_*` layer (checked only when `WorldMapLayers2D` is resolved) or on `Perimeter`/`RetreatingPlace`. A root scale other than 1 only warns. A valid prefab is instantiated under the root at the arena centre with identity rotation. Generated ground is a sprite (sorting order −100) covering the largest view at the clearance aspect plus `GeneratedGroundMargin`.

[WM3: I06 regressions (terrain selection); tie and boundary arithmetic source]

**ARENA-VIEW-01 — View size, shrink-only.**
- **Start size** = `ComputeViewSize(countA + countB)`: the curve value (NaN or infinite → min) clamped to [`CameraMinSize`, `CameraMaxSize`]. The default curve is piecewise linear through (0, 6), (40, 8), (100, 10), (200, 13), (400, 17), (600, 18) and constant beyond its ends.
- **Sizes.** `StartViewSize` is the creation size, `ViewSize` the live camera size, `TargetViewSize` the shrink target. Invariant: Target ≤ ViewSize ≤ Start.
- **Requests.** `RequestViewSize(size)` only lowers the target, clamped to `CameraMinSize`; it returns false with no arena, while busy, for non-finite input, or when the result is not lower. `RequestViewSizeForCount(n)` requests the curve size for `n`.
- **Stepping.** Each arena FixedUpdate moves `ViewSize` toward the target at `ShrinkSpeed` units per second, committed only when the camera accepts the size. Nothing raises it.
- **Trigger.** Phase 5 owns when to shrink (both queues empty and fewer than 30 units on the field, Roadmap §1.3). Today only the debug panel requests it.
- **Not established:** whether enemy search reach (default 8) covers the largest view (size 18).

[WM3: I07 shrink-to-min while bodies push outward, 0 leaks, both modes. Count-following is credited only to the I06-regression group, whose individual checks were not listed; curve values and request refusals source]

**ARENA-PERIM-01 — Perimeter modes and geometry.** The mode is fixed per arena at creation: `OneWayEffector` (default) or `SolidWithEntryIgnore`. With V the live view rect and T = `WallThickness` (captured at creation):
- **Walls.** Four boxes on layer `Perimeter`, each T thick just outside its edge. Left/right are V.height + 2T tall and bottom/top V.width + 2T wide, closing the corners. Frictionless, on a kinematic Rigidbody2D (never sleeps, rotation frozen).
- **OneWayEffector.** Each wall collider is used by a `PlatformEffector2D` whose collider mask is the two body layers: one-way, 180° surface arc facing into the arena, no side friction or bounce.
- **SolidWithEntryIgnore.** Solid walls; Entering bodies ignore all four walls through `Physics2D.IgnoreCollision` and collide again once Inside (`ARENA-BODY-02`).
- **Following the view.** Each arena FixedUpdate, after the shrink step, walls and gates are laid out on the live view (shrinking and window-aspect changes). Moves use `MovePosition`, so a shrinking wall pushes bodies instead of teleporting into them; box sizes are updated in place. An unchanged rect skips layout.

Both modes passed every I07 check; `OneWayEffector` stays the default. [WM3: I07 entry/containment, knockback 40/80/150 u/s, shrink-to-min, resize; both modes]

**ARENA-ENTRY-01 — Entry strips and spawn points.** `GetEntryStrip(edge)` is a rectangle `EntryStripDepth` deep beyond that wall's outer face, spanning the view edge. It has no collider and is valid only while an arena exists.

`TryFindEntryPoint(edge, radius, reserved, out point)` is geometry plus one physics query; there is no spawn-box component. It needs an arena in battle view and a positive radius. It tries up to `EntrySpawnAttempts` random candidates:
- along the edge, inset by the radius;
- outward from the edge, between T + radius + `EntrySpawnGap` and max(that, T + `EntryStripDepth` − radius);
- rejected if a circle of radius + gap overlaps a body layer or the terrain `Wall` layer (when resolved), or lies within 2·radius + gap of a point in `reserved`.

It reserves nothing: a caller spawning several bodies in one frame passes the points already used (Roadmap §5 "Spawning room"). It returns false when no free point is found.

[WM3: I07 test-body spawning and entry; a full strip is source]

**ARENA-BODY-01 — Body registration and ownership.** The arena tracks registered bodies, steers them through their destination override and classifies them. It never owns, parents, deactivates or destroys their GameObjects. Everything the arena creates lives under its Root and is destroyed with it, so **real units must never be parented under Root**.
- **Register.** `TryRegisterBody(rigidbody, circle, destination, entryEdge, out body, out error)` refuses with no arena or while busy, a missing component, a trigger circle, or a circle already registered. A new body starts Entering and is evaluated immediately. Bodies are indexed by their circle collider. `EntryEdge` is recorded; steering does not use it.
- **Not validated:** that the three components share a GameObject, the body's layer, or the circle offset. `ArenaBody.Position` is the Rigidbody position and assumes a zero offset (`SETUP-02`); `WorldRadius` is the radius × the larger absolute lossy scale.
- **Unregister.** `UnregisterBody` is idempotent. It restores wall collisions, clears only Entry and Retreat overrides (a Debug override stays) and sets the state to Removed.
- **Lost bodies.** A registered body whose components are destroyed, or whose circle is inactive or disabled, is released at the next tracking pass and counted as lost with a warning (the audit then fails). Owners must unregister before deactivating or destroying a body.
- **Teardown** releases every body silently.

[WM3: every I07 body test; lost-body path source]

**ARENA-BODY-02 — Entry states, leaks and re-entry.** Each arena FixedUpdate, after shrinking and layout, every registered body is tracked against the live view:
- **Entering → Inside** when its centre lies inside the view inset by (radius + `EntryMargin`). It then collides with walls again, loses its Entry override and, if retreating, gets a Retreat override.
- **While Entering** the Entry override points to the nearest point of the view inset by (radius + `EntryMargin` + 0.05): a straight perpendicular walk from an entry strip (the view centre if the inset is empty).
- **Inside → Entering** when its centre is outside the view. This is counted as a **leak** (warning; first 20 logged) unless the view's width/height ratio changed since the previous pass, which counts as a **re-entry** (window resize). Shrinking keeps the ratio, so a body left outside by a shrinking wall is a leak.
- **`ForceReenter(body)`** (Inside only) announces a legitimate push outside: it counts a re-entry and returns the body to Entering.

The gap between the two thresholds is hysteresis. `ArenaBody.IsFullyInside` (state Inside) is the Phase 4 gate for "no Sidestep before fully inside". Counters reset only through `ResetBodyCounters`; teardown does not reset them, so leak and lost counts carry across arenas and keep failing the audit until reset.

[WM3: I07 entry/containment, knockback 0 leaks, shrink 0 leaks, eject → re-entries +1 and leaks 0, resize; both modes]

**ARENA-GATE-01 — Retreat steering, gates and escape.**
- **Flag.** `SetBodyRetreating(body, bool)` sets or clears the flag on a registered body; an Inside body is re-steered at once. Clearing is allowed here; "stays retreating even if healed" is Phase 7 policy.
- **Steering.** A retreating Inside body gets a Retreat override toward the closest point on the nearest gate edge of the view. With no gate edges it gets none.
- **Gates.** One trigger box per gate edge on layer `RetreatingPlace`, spanning the view edge just inside it, on a kinematic body, moved with the walls. Depth = min(`GateDepth`, max(0.05, ¼ of the smaller view dimension at creation)). Any edge mask may be passed; field battles use all four, sieges will pass the attacker's edge.
- **Reporting.** A gate reports every OnTriggerEnter2D and OnTriggerStay2D contact; Stay covers a body that starts retreating while already overlapping. Escapes therefore happen in the physics trigger phase, not in FixedUpdate.
- **Escape** requires an arena, not busy, `EscapesAccepted`, and a registered retreating body — in any state, Entering included. It records `EscapeEdge`, releases the body as Escaped, increments `TotalEscapes`, calls the owner's `EscapedCallback`, then raises `BodyEscaped`; each callback is isolated. A body escapes at most once because it is no longer registered.
- **Closing.** `EscapesAccepted` is set true at creation and false at teardown. It has a public setter; it is the hook Phase 5's result lock closes. Teardown raises no escape events.

[WM3: I07 Retreat 5 → 5 escapes, single-gate test. Escape while Entering and the closed-gate path WM3-S]

**ARENA-DEST-01 — Destination override contract.** `UnitDestinationOverride2D` holds plain state and declares no Unity messages, so it is safe to read or write on a parked, never-awakened unit (`CORE-03`).
- **Reasons:** None, Entry, Retreat, Debug.
- **`Set(point, reason)`.** None → `Clear()`. A non-finite point logs an error and is ignored. Otherwise the destination is always updated; if the reason changed, `Activations` increments and `Changed` fires.
- **`Clear()`** is a no-op with no override; otherwise it resets and raises `Changed`.
- **`Changed`** fires only when an override starts, ends or changes reason, never for destination-only updates (every step while entering). Handlers are isolated.
- **`TryGetDirection(from, arriveDistance, out direction)`** returns false with no override, and true with a zero direction within the arrive distance.
- **Phase 3 users.** Only the arena writes it (Entry, Retreat); only `BattleTestBody2D` reads it. `MOVE-01` and the Sidestep rules are unchanged; Phase 4 wires it into the motor and Sidestep. Unit prefabs are not established to carry it.

[WM3: used by every I07 test body; `Changed` delivery and the Debug reason source]

**ARENA-TEST-01 — Test bodies are debug only.** `BattleTestBody2D` is created only by the arena's `SpawnTestBodies`, under Root/Test Bodies, with its own `UnitDestinationOverride2D`, on the side's body layer (`BattleLayers2D.BodyLayer`). It is a Dynamic, continuous, interpolated body with a frictionless circle. It steers toward its override, else by behaviour (Wander, PushOutward, SeekCenter, Hold); knockback sets a raw velocity and suspends steering. On escape it deactivates and destroys itself; its OnDestroy unregisters it. Test bodies die with the root. `ArenaSide` is an arena-local physics label, not a faction (Roadmap §2.3). No gameplay code may depend on test bodies; Phase 4 registers real units instead. [WM3]

**ARENA-AUDIT-01 — Arena audit.** `AuditArena(out report)` is development only and read-only. Run it at least one frame after a removal. Reported problems:
- **Lifecycle:** arena roots alive ≠ (1 with an arena, else 0); an active arena whose root is missing or inactive; a `FrozenChanged` subscription left with no arena.
- **Cameras:** enabled cameras ≠ 1; world/battle/rig state not matching `HasArena`; enabled AudioListeners ≠ 1, or the single one not on the world camera.
- **Map:** when ready, `IsFrozen` ≠ `HasArena`.
- **Perimeter:** layers unresolved; walls alive ≠ 4 or off `Perimeter`; gates alive ≠ the gate-edge count; camera size ≠ `ViewSize`; size order broken.
- **Bodies:** bodies registered with no arena; index mismatch; a registered body destroyed; ignore flag or physics ignore state not matching mode and state; an Inside body with an Entry override; stray test bodies; any leak or lost body since the last counter reset.

[WM3: passed after the I07 20 cycles in both modes]

Layer names, matrix and controller placement: `SETUP-06` (world map) and `SETUP-07` (battle) in Core. Tuning values: `WorldMapRules2D` and `BattleRules2D` source.

## Script cards

### WorldMapController2D (five partials) [WM1, WM2]
Owns: `Instance` and readiness; army/settlement registries and ids; bounds, MapTime, PhysicsStep and freezing; army creation, placement and removal; the unit reserve and owner-loss handling; deferred-recruitment flush; deployment, overflow and placement; allied interactions and gated transfers; the contact pipeline and battle selection; diagnostics.  
API (core): `Instance`, `IsReady`, `IsFrozen`, `MapTime`, `PhysicsStep`, `Rules`, `Palette`, `MapBounds`, `Armies`, `Settlements`; `SetFrozen(bool)`; `TrySpawnArmy(FactionId, Vector2 position, Vector2 direction, ArmySpawnReason, out Army2D)` and `TrySpawnArmyAtRandom(FactionId, ArmySpawnReason, out Army2D)` (empty debug armies); `IsPlacementFree(Vector2)`; `DestroyArmy(Army2D, bool discardUnits = false)`; `GetFactionColor(FactionId)`.  
API (.Units): `Reserve`, `MaxUnitsPerArmy`, `DefaultRosterOrder`, `AuditUnits(out string)`.  
API (.Deployment): `DeployArmy(Settlement2D, DeployAmount) → DeployResult`, static `ComputeDeployCount(int eligible, DeployAmount, int cap)`, `GetGarrisonLimit(Settlement2D)`, `GetPlacementOuterDistance(float sourceRadius)`, `FindArmyPlacements(Vector2 center, float sourceRadius, int count, List<ArmyPlacement>) → int`, `TotalDeployedArmies`, `TotalOverflowArmies`, `TotalPlacementBlocks`. Internal: `TryDeployFromRoster` (search and explicit-placement overloads) → `ArmyDeployAttempt`.  
API (.Interactions): `CanTransfer(UnitRoster, UnitRoster, out string)`, `TransferUnits(...)`, `TransferFirstUnits(...)`, `IsRosterInBattle(UnitRoster)` (false until Phase 4/5), `RecentAlliedInteractions`, `LogAlliedInteractions`, `TotalRetreatTransfers`, `TotalRetreatMerges`, `TotalGarrisonJoins`, `TotalUnitsMovedByInteractions`; also declares `AlliedInteractionRecord` (Step, MapTime, Intent, DonorArmyId, ReceiverArmyId (0 for joins), SettlementId (0 for army interactions), Moved, DonorRemaining, ReceiverCount, DonorRemoved).  
API (.Contacts): `LastStepContacts`, `RecentNewContacts`, `LastSelectedBattle`, `LastBattleSelectionCandidates`, `TotalPairReports`, `DuplicateCallbacksMerged`, `MaxPairsInOneStep`, `StepsWithMultipleBattleCandidates`, `GetEpisodeCount(WorldMapContactKind)`, `ResetContactStats()`, `LogNewContacts`.  
Internal: `ReportArmyContact`, `NotifyArmyDestroyed`, `RegisterSettlement`, `UnregisterSettlement`.  
Events: `ArmySpawned(Army2D, ArmySpawnReason)`, `ArmyDestroyed(Army2D)`, `FrozenChanged(bool)` (subscribed by `BattleArena2D` while an arena exists), `ContactReported(WorldMapContactReport)`, `AlliedInteractionApplied(AlliedInteractionRecord)`.  
Invariants: `WM-LIFE-01`, `WM-LIFE-02`, `WM-TIME-01`, `WM-FREEZE-01`, `WM-SPAWN-01`, `WM-SPAWN-02`, `WM-REMOVE-01`, `WM-SETTLE-02`, `WM-OWN-02`, `WM-RECRUIT-01`, `WM-XFER-02`, `WM-DEPLOY-01`, `WM-DEPLOY-02`, `WM-PLACE-01`, `WM-OVERFLOW-01`, `WM-ALLY-01`–`WM-ALLY-03`, `WM-AUDIT-01`, `WM-CONTACT-01`–`WM-CONTACT-05`, `WM-BATTLE-01`; `SETUP-06`.  
Never owns: army direction/bounce, the classification table, unit ownership mutation (reserve only), roster storage, settlement faction, battles, `Time.timeScale`.

| Partial source | Responsibility |
| --- | --- |
| WorldMapController2D.cs | Serialized config, all Unity messages (Awake/FixedUpdate/OnDestroy/OnApplicationQuit), registries, freezing, creation, removal, bounds |
| .Contacts.cs | Intake, deduplication, episodes, classification, battle selection, `ContactReported` dispatch |
| .Units.cs | Reserve creation, audit, deferred-recruitment flush, owner-loss handling |
| .Deployment.cs | `DeployArmy`, pending requests, placement, `TryDeployFromRoster`, rollback, overflow, settlement work |
| .Interactions.cs | `AlliedInteractionRecord`, allied interactions, `CanTransfer` and gated transfers |

### Army2D [WM1, WM2]
Owns: travel direction, bounce resolution, commanded constant velocity, its Rigidbody/collider setup, retreat flag, its `UnitRoster`, the source-settlement collision ignore, visuals (faction colour, retreat tint and flag, unit-count label), speed/bounce diagnostics.  
API: `ArmyId`, `Faction`, `IsRetreating`, `Direction`, `Radius`, `TargetSpeed`, `Position`, `IsInitialized`, `IsRemoved`, `Roster`, `UnitCount`, `IgnoredSettlement`, `MeasuredSpeed`, `RecentBouncesPerSecond`, `TotalBounces`; `SetRetreating(bool)`. Controller-only internal: `Initialize(WorldMapController2D, int armyId, FactionId, Vector2 initialDirection)`, `MarkRemoved()`, `ApplyFrozen(bool)` (ignored once removed), `IgnoreSettlementUntilSeparated(Settlement2D, double minUntilMapTime)`.  
Events: none emitted; subscribes to its roster's `Changed` for visuals; OnDestroy notifies the controller.  
Invariants: `WM-MOVE-01`–`WM-MOVE-03`, `WM-SPAWN-02`, `WM-REMOVE-01`, `WM-FREEZE-01`, `WM-CONTACT-01`, `WM-IGNORE-01`, `WM-ROSTER-01`.  
Never owns: registry, ids, removal decisions, classification, contact effects, unit ownership changes.

### Settlement2D [WM1, WM2]
Owns: kind, faction, radius, its garrison `UnitRoster`, deferred recruits, one pending deployment request (amount, serial) and the deploy/overflow retry timers, its static collider and visuals.  
API: `Kind`, `Faction`, `Radius`, `WorldRadius`, `Position`, `SettlementId`, `Garrison`, `GarrisonCount`, `GarrisonLimit`, `HasPendingDeployment`, `PendingDeployAmount`, `PendingRecruitCount`; `Recruit(GameObject prefab, int count) → RecruitResult`, `RequestDeployment(DeployAmount) → DeployResult`, `CancelPendingDeployment() → bool`, `SetFaction(FactionId) → bool`. Controller-only internal: `AssignId`, `FlushDeferredRecruitment`, `SetPendingDeployment`, `ClearPendingDeployment(serial)`, `PendingDeploymentSerial`, `NextDeployRetryMapTime`, `NextOverflowRetryMapTime`, `Footprint`.  
Events: none; subscribes to garrison `Changed` for visuals.  
Invariants: `WM-SETTLE-01`, `WM-SETTLE-02`, `WM-RECRUIT-01`, `WM-FACTION-01`, `WM-DEPLOY-01`; `SETUP-06`.  
Never owns: unit creation/ownership (reserve), deployment execution, overflow, capture, sieges, its own id.

### UnitReserve2D [WM2]
Owns: the parked-unit root, the registry of owned handles, unit ids, the global arrival-sequence counter, and every ownership change.  
API: `Root`, `Units`, `Count`; static `ValidatePrefab(GameObject, out string)`; `Transfer(UnitRoster from, UnitRoster to, IReadOnlyList<UnitHandle>) → UnitTransferResult`; `TransferFirst(UnitRoster from, UnitRoster to, int maxCount) → UnitTransferResult`. Internal: constructor `(Transform parent)`, `CreateInto(garrison, prefab, count, out error) → int`, `DiscardAll(roster) → int`, `Audit(rosters, out report) → bool`.  
Events: none; raises roster `Changed` through the rosters.  
Invariants: `WM-UNIT-01`, `WM-OWN-01`, `WM-XFER-01`, `WM-AUDIT-01`; `CORE-03`.  
Never owns: when transfers are allowed (controller gate), deployment, unit activation, combat state.

### UnitRoster [WM2]
Owns: one owner's unit list in arrival order, settlement-only count, read order, capacity, closed flag; a garrison's faction label.  
API: `Kind`, `Settlement`, `Army`, `Faction`, `Capacity`, `Order` (get/set), `IsClosed`, `Count`, `SettlementOnlyCount`, `EligibleCount`, `FreeCapacity`, `AcceptsSettlementOnly`, `UnitsInArrivalOrder`, `OwnerLabel`, `Contains(UnitHandle)`, `GetUnitsInOrder(List<UnitHandle>, bool armyEligibleOnly = false)`. Internal: constructor, `AppendInternal`, `RemoveInternal`, `Close`, `TryRelabelFaction`, `RaiseChanged`.  
Events: `Changed(UnitRoster)` (`WM-ROSTER-02`).  
Invariants: `WM-ROSTER-01`, `WM-ROSTER-02`, `WM-OWN-01`, `WM-FACTION-01`.  
Never owns: unit creation/destruction, transfer rules, its owner's lifetime.

### UnitHandle [WM2]
Owns: one unit's persistent identity and references.  
API: `Id`, `GameObject`, `Combatant`, `RuntimeStats`, `Progression` (may be null), `Definition`, `Owner`, `ArrivalSequence`, `Status`, `HasEverActivated`, `IsSettlementOnly`, `Exists`, `IsParked`, `Faction`, `Name`, `CurrentLevel`. Setters for Owner, ArrivalSequence, Status and HasEverActivated are internal.  
Events: none.  
Invariants: `WM-UNIT-02`, `WM-OWN-01`; `CORE-03`.  
Never owns: ownership decisions or activation.

### UnitRosterTypes2D [WM2]
Owns: `UnitStatus {Reserve, Deployed, Escaping, Escaped, Dead, Discarded}` (Phase 2 sets only Reserve and Discarded); `UnitRosterKind {Garrison, Army}`; `UnitRosterOrder {Fifo, HighestLevelFirst}`; `UnitTransferResult`; `RecruitStatus {Recruited, Deferred, Rejected}`; `RecruitResult`.  
API: `UnitTransferResult` fields Requested, Moved, SkippedSettlementOnly, SkippedCapacity, SkippedInvalid, RejectReason, plus Rejected and Skipped. `RecruitResult` fields Status, Requested, Created, Message. Constructors and factories are internal.  
Events: none.  
Invariants: `WM-XFER-01`, `WM-RECRUIT-01`.  
Never owns: behaviour.

### UnitPrefabCatalog2D [WM2]
Owns: authored list of unit prefab assets for debug recruitment.  
API: `Count`, `Get(int)` (null when out of range).  
Events: none.  
Invariants: debug only; gameplay never depends on it.  
Never owns: prefab validation (reserve) or recruitment.

### WorldMapContacts2D [WM1, WM2]
Owns: `WorldMapContactKind` (10), `WorldMapContactIntent` (8), internal `WorldMapContactPairKey`, immutable `WorldMapContactReport`, pure `WorldMapContactClassifier2D` with a 33-check self-test.  
API: `ClassifyArmies(FactionId, bool, FactionId, bool)`, `ClassifySettlement(FactionId armyFaction, bool armyRetreating, FactionId settlementFaction, int garrisonCount)`, `IntentOf(kind)`, `StartsBattle(intent)`, `SecondArmyFirst(kind, idA, retreatingA, countA, idB, retreatingB, countB)`, `RunSelfTest(out string)`, `KindCount`. Report fields: `Step`, `MapTime`, `Kind`, `Intent`, `IsNewContact`, `ArmyA`, `ArmyB`, `Settlement`, `ArmyIdA`, `ArmyIdB`, `SettlementId`, `ContactPoint`, `PositionA`, `PositionB`, `FactionA`, `FactionB` (int), `RetreatingA`, `RetreatingB`, `CountA`, `CountB` (army `UnitCount` / `GarrisonCount`), `IsSettlementContact`, `ToShortString()`.  
Events: none.  
Invariants: `WM-CLASS-01`, `WM-CLASS-02`, `WM-CONTACT-02`, `WM-CONTACT-04`.  
Never owns: Unity state, contact timing, dispatch, effects.

### WorldMapTypes2D [WM2]
Owns: `SettlementKind {Town, Castle}`; `ArmySpawnReason {Debug, Deploy, Overflow}` (BattleReturn and EscapedDefenders are planned in comments only); `DeployAmount {Half, All}`; `DeployStatus {Deployed, Pending, Rejected}`; internal `ArmyDeployAttempt {Deployed, Blocked, Failed}`; `DeployResult`; `ArmyPlacement`.  
API: `DeployResult` fields Status, Amount, Army (set only when Deployed), UnitCount, Message, and static Deployed/Pending/Rejected; `ArmyPlacement(Vector2 position, Vector2 direction)`.  
Events: none.  
Invariants: `WM-SPAWN-01`, `WM-DEPLOY-01`, `WM-DEPLOY-02`, `WM-PLACE-01`.  
Never owns: behaviour.

### WorldMapRules2D [WM1, WM2]
Owns:
- movement (speed, jitter, axis angle);
- army body (radius, spawn margin);
- rosters: `MaxUnitsPerArmy` 200, `DefaultRosterOrder` Fifo, town/castle limits 200/300, `OverflowArmySize` 100 (clamped to army capacity, with an OnValidate warning);
- placement: `SettlementCollisionIgnoreSeconds` 0.5, `PlacementSlotsPerRing` 16, `PlacementRings` 2, `PlacementRetrySeconds` 0.25;
- presentation (retreat tint, label size, optional army sprite).

API: read-only properties; `GetGarrisonLimit(SettlementKind)`; `ArmySprite` falls back to the generated circle. Consult source for ranges.  
Events: none.  
Invariants: `WM-LIFE-01` fallback; `WM-MOVE-01` live speed vs. initialize-time radius; roster capacity read at Initialize (`WM-ROSTER-01`).  
Never owns: runtime state.

### FactionPalette2D [WM1]
Owns: authored faction→colour entries.  
API: `GetColor(FactionId)`, static `Resolve(FactionPalette2D, FactionId)`, static `Generated(FactionId)` (deterministic fallback for unlisted factions or no palette).  
Events: none.  
Invariants: presentation only.  
Never owns: faction identity or relations.

### WorldMapLayers2D [WM1]
Owns: WM layer names and resolved indices.  
API: `Army`, `Settlement`, `Bounds`, `IsResolved`, `AllMask`, `TryResolve(out string)`, `LogCollisionMatrixProblems()`. Read by `BattleLayers2D` (matrix report) and `BattleArena2D` (prefab validation).  
Events: none.  
Invariants: `SETUP-06`; reports matrix problems, never edits project settings.  
Never owns: combat or battle layers.

### WorldMapMath2D [WM1]
Owns: pure direction helpers.  
API: `Rotate(Vector2, float)`, `FromAngle(float)`, `NudgeAwayFromAxes(Vector2, float minDegrees)`, `RandomDirection(float)`.  
Events: none.  
Invariants: `WM-MOVE-03`.  
Never owns: state.

### WorldMapAssets2D [WM1]
Owns: lazily generated, process-cached `HideAndDontSave` placeholder circle/square sprites (one world unit), frictionless material, label font.  
API: `CircleSprite`, `SquareSprite`, `Frictionless`, `CreateLabel(Transform, Vector3, float characterSize, int sortingOrder) → TextMesh`, `CreateSpriteChild(Transform, string, Sprite, Vector3 localPosition, Vector2 size, int sortingOrder) → SpriteRenderer`, `ReadableTextColor(Color)`. Children copy the parent's layer. (Signatures inferred from callers; the file was unchanged and not re-attached.)  
Events: none.  
Invariants: visual children carry no colliders.  
Never owns: gameplay data.

### WorldMapDebugPanel2D [WM1, WM2]
Owns: optional IMGUI tooling with four tabs (Armies, Settlements, Rosters, Contacts):
- **Spawning:** empty-army spawning (single, batch, head-on pair, Converge 4, aim at settlement), with optional fill from the selected settlement; destroy empty armies; debug discard of all armies with their units.
- **Settlements:** recruitment from a `UnitPrefabCatalog2D`; Set faction; order toggles; D50/D100 deploy and cancel.
- **Units:** per-unit selection and selected transfers; N-unit transfers through the gated API.
- **Placement tests:** debug placement blockers (static circles on `WM_Bounds`, unreported).
- **Diagnostics:** ownership audit; frozen recruit/deploy deferral tests; freeze-on-next-battle-report test; contact and allied-interaction statistics; classifier self-test.

API: serialized controller (fallback `Instance`) and catalog; no public methods. Debug spawns use `ArmySpawnReason.Debug`.  
Events: subscribes to `ContactReported` only while the freeze test is armed.  
Invariants: mutations disabled while frozen, except freeze toggle, audit, blockers and the explicit deferral tests; all transfers go through `WM-XFER-02`. The freeze toggle is not yet gated by an active arena; unfreezing during an arena aborts it (`WM-FREEZE-01`, `ARENA-LIFE-03`).  
Never owns: any gameplay rule; production code must not depend on it.

`FactionId` (constructor from int, `.Value`, equality, serializable) is an existing combat type. Core attributes faction types to `CombatTypes2D`; its definition was not attached to this update.

### BattleArena2D (three partials) [WM3]
Owns: `Instance` and readiness; the arena lifecycle (create, remove, abort, teardown); the arena Root and everything under it; ownership of the map freeze while an arena exists; start/current/target view size; perimeter walls, gates and entry-strip geometry; registered bodies, their entry state, the Entry/Retreat overrides it sets, wall ignores and counters; test-body creation; the arena audit.  
API (core): const `RootName`; static `Instance`, `IsAnyArenaActive`; `Rules`, `CameraRig`, `Map`, `IsReady`, `HasArena`, `Root`, `Center`, `StartViewSize`, `ViewSize`, `TargetViewSize`, `MapPoint`, `StartingCountA`, `StartingCountB`, `Battlefield`, `ViewRect` (default unless in battle view), `NextPerimeterMode`, `SetNextPerimeterMode(PerimeterMode)`, `TotalCreated`, `TotalRemoved`, `TotalFailedCreates`, `TotalAborts`; `CanCreate(out string) → bool`; `TryCreate(Vector2 mapPoint, int startingCountA, int startingCountB, out string error) → bool`; `TryCreate(Vector2, int, int, ArenaEdgeMask gateEdges, out string) → bool`; `Remove() → bool`; `AuditArena(out string report) → bool`.  
API (.Perimeter): `ActivePerimeterMode`, `GateEdges`, `WallThickness`, `IsShrinking`, `GetWallCollider(ArenaEdge) → Collider2D`, `GetEntryStrip(ArenaEdge) → Rect`, `RequestViewSize(float) → bool`, `RequestViewSizeForCount(int) → bool`; internal static `FrictionlessMaterial`.  
API (.Bodies): `Bodies`, `EscapesAccepted` (get/set), `TotalBodiesRegistered`, `TotalLeaks`, `TotalReentries`, `TotalEscapes`, `TotalLostBodies`, `EnteringCount`, `InsideCount`, `ResetBodyCounters()`, `TryRegisterBody(Rigidbody2D, CircleCollider2D, UnitDestinationOverride2D, ArenaEdge entryEdge, out ArenaBody, out string) → bool`, `UnregisterBody(ArenaBody) → bool`, `SetBodyRetreating(ArenaBody, bool) → bool`, `ForceReenter(ArenaBody) → bool`, `TryFindEntryPoint(ArenaEdge, float radius, IReadOnlyList<Vector2> reserved, out Vector2) → bool`. Debug: `TestBodies`, `SpawnTestBodies(ArenaSide, ArenaEdge, int, out string) → int`, `SetTestBehaviour(TestBodyBehaviour)`, `KnockbackTestBodies(float speed, float duration) → int` (Inside bodies only), `SetTestRetreating(int maxCount, bool) → int`, `EjectRandomTestBody(out string) → bool`. Internal: `HandleGateContact`.  
Events: `ArenaCreated(BattleArena2D)`, `ArenaRemoved(BattleArena2D)`, `BodyEscaped(ArenaBody)`; subscribes to the map's `FrozenChanged` while an arena exists.  
Invariants: `ARENA-LIFE-01`–`ARENA-LIFE-03`, `ARENA-TERRAIN-01`, `ARENA-VIEW-01`, `ARENA-PERIM-01`, `ARENA-ENTRY-01`, `ARENA-BODY-01`, `ARENA-BODY-02`, `ARENA-GATE-01`, `ARENA-TEST-01`, `ARENA-AUDIT-01`; `SETUP-07`; `ORDER-01`.  
Never owns: body GameObjects, unit faction or side, Camera components (rig), world-map state beyond its freeze, battle results, `Time.timeScale`.

| Partial source | Responsibility |
| --- | --- |
| BattleArena2D.cs | Serialized config, all Unity messages (Awake, FixedUpdate, OnDisable, OnDestroy, OnApplicationQuit), creation, removal, teardown, terrain, freeze subscription, audit; declares `[DefaultExecutionOrder(-900)]` |
| BattleArena2D.Perimeter.cs | Walls, effectors, gates, entry strips, layout on the live view, shrink requests and stepping, perimeter audit |
| BattleArena2D.Bodies.cs | Registration, entry/leak/re-entry tracking, retreat steering, gate escapes, entry points, wall ignores, test bodies, body audit |

### BattleCamera2D [WM3]
Owns: enablement of the world and battle Camera components, the battle camera pose and orthographic size, camera-count monitoring.  
API: `IsReady`, `IsInBattleView`, `WorldCamera`, `BattleCamera`, `Aspect`, `ViewSize`, `ViewCenter`; `EnterBattleView(Vector2 center, float orthographicSize, float z) → bool`, `SetViewSize(float orthographicSize) → bool`, `ExitBattleView()`, `GetViewRect() → Rect`; static `CountEnabledAudioListeners(out AudioListener single) → int` (allocates).  
Events: none.  
Invariants: `ARENA-CAM-01`; `SETUP-07`.  
Never owns: when to swap or the view-size policy (arena); AudioListener placement beyond disabling one found on the battle camera.

### BattleRules2D [WM3]
Owns: battle tuning. Arena placement (`ArenaCenter` (1000, 0), clearance 20, clearance aspect 2.4); battlefield regions, fallback prefab, generated-ground colour and margin 6; camera min 6 / max 18, size curve, z −10, shrink speed 1.5; perimeter mode (OneWayEffector), wall thickness 1, entry strip depth 1.5, entry margin 0.3, spawn gap 0.1, spawn attempts 16; gate depth 0.4; layer names (`FactionABody`, `FactionBBody`, `Wall`); test-body radius 0.35, speed 3 and colours. Also declares `BattlefieldRegion2D` (Name, Area, Priority, BattlefieldPrefab, `Contains(Vector2)`), `BattlefieldSource {Region, Fallback, Generated}` and `BattlefieldSelection` (Source, RegionIndex, Prefab, Label).  
API: read-only properties (clamped; names in source), including `SideABodyLayerName`, `SideBBodyLayerName`, `WallLayerName`; `RegionCount`, `GetRegion(int)`; `ComputeViewSize(int) → float`; `SelectBattlefield(Vector2) → BattlefieldSelection`.  
Events: none.  
Invariants: `ARENA-TERRAIN-01`, `ARENA-VIEW-01`; OnValidate warns on max < min, an empty curve, a strip shallower than one test body, gate depth too large for the min size, equal body-layer names, and regions without a prefab or with zero area.  
Never owns: runtime state, resolved layer indices.

### BattleLayers2D [WM3]
Owns: battle layer names and resolved indices.  
API: consts `PerimeterLayerName` ("Perimeter"), `RetreatingPlaceLayerName` ("RetreatingPlace"); `Perimeter`, `RetreatingPlace`, `BodyA`, `BodyB`, `Wall` (−1 when missing), `IsResolved`, `BodyMask`, `SpawnBlockMask`, `BodyLayer(ArenaSide) → int`, `TryResolve(string bodyALayerName, string bodyBLayerName, string wallLayerName, out string error) → bool`, `LogCollisionMatrixProblems()`.  
Events: none.  
Invariants: `SETUP-07`; reports matrix problems, never edits project settings.  
Never owns: Clash layers, WM layers, unit masks.

### BattleArenaTypes2D [WM3]
Owns: `ArenaEdge {Left = 0, Right = 1, Bottom = 2, Top = 3}`; `[Flags] ArenaEdgeMask {None, Left, Right, Bottom, Top, All}`; `ArenaSide {A = 0, B = 1}` (physics label, not a faction); `PerimeterMode {OneWayEffector = 0, SolidWithEntryIgnore = 1}`; `ArenaBodyState {Entering, Inside, Escaped, Removed}`; static `ArenaEdges`; the `ArenaBody` tracking record.  
API: `ArenaEdges.Count`, `Names`, `ToMask`, `Contains`, `CountIn`, `InwardNormal`, `ClosestPoint(Rect, ArenaEdge, Vector2)`, `Nearest(Rect, Vector2)` (ties to the lower enum value). `ArenaBody`: `Id`, `Rigidbody`, `Circle`, `Destination`, `EntryEdge`, `State`, `IsRetreating`, `IgnoringPerimeter`, `EscapeEdge` (setters internal), `IsRegistered` (Entering or Inside), `IsFullyInside` (Inside), `Exists`, `Position`, `WorldRadius`; internal constructor and `EscapedCallback`.  
Events: none.  
Invariants: `ARENA-BODY-01`, `ARENA-BODY-02`, `ARENA-GATE-01`.  
Never owns: behaviour or faction.

### BattleArenaGate2D [WM3]
Owns: one gate's edge and references to its kinematic body and trigger box.  
API: `Edge`, `Body`, `Box`; internal `Initialize(BattleArena2D, ArenaEdge, Rigidbody2D, BoxCollider2D)`.  
Events: none; forwards OnTriggerEnter2D and OnTriggerStay2D to the arena.  
Invariants: `ARENA-GATE-01`.  
Never owns: the escape decision.

### UnitDestinationOverride2D [WM3]
Owns: one unit's optional destination and reason; activation count.  
API: `HasOverride`, `Reason`, `Destination`, `Activations`; `Set(Vector2, DestinationOverrideReason)`, `Clear()`, `TryGetDirection(Vector2 from, float arriveDistance, out Vector2 direction) → bool`; enum `DestinationOverrideReason {None, Entry, Retreat, Debug}`.  
Events: `Changed(UnitDestinationOverride2D)` on start, end or reason change only.  
Invariants: `ARENA-DEST-01`; `CORE-03`.  
Never owns: steering execution (test body today, motor from Phase 4), Sidestep state, when an override is set (arena).

### BattleTestBody2D [WM3]
Owns: one debug stand-in body (Rigidbody2D, circle, its own destination override, visual), behaviour, knockback window. Also declares `TestBodyBehaviour {Wander, PushOutward, SeekCenter, Hold}`.  
API: `Body`, `Circle`, `Destination`, `ArenaBody`, `Side`, `Behaviour` (get/set); `Knockback(Vector2 velocity, float duration)`, `Teleport(Vector2)`; internal static `Create(...)`, `Bind(ArenaBody)`.  
Events: none; escape callback deactivates and destroys it; OnDestroy unregisters it.  
Invariants: `ARENA-TEST-01`.  
Never owns: anything gameplay depends on.

### BattleArenaDebugPanel2D [WM3]
Owns: optional IMGUI tooling: arena status (readiness, cameras, listeners, map state, sizes, terrain, perimeter, gates, escapes); create/remove with counts, map point and gate toggles; next perimeter mode; size and terrain preview; "Use last battle pick"; create/remove cycles (one action per frame, optional test bodies up to 30 per side on Left/Right, audit one frame after the last removal); shrink to count or minimum; test-body spawn per edge or all four, behaviours, outward knockback, eject, Retreat N / cancel; counter reset; audit.  
API: serialized arena (fallback `Instance`); no public methods.  
Events: none.  
Invariants: actions disabled while cycling or when they do not apply; debug only.  
Never owns: any gameplay rule; production code must not depend on it.
