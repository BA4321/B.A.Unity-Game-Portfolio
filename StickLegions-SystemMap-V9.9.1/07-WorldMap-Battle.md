# Stick Legions system map — 07 World map & battle

Load with `00-Core.md` for world-map or battle work. State after Roadmap V6 **Phase 9 (complete)**. Phases 1–9 are complete, so every Roadmap V6 phase is complete. The I18 and I19 handoffs report that every one of their checks passed in Unity, and the I19 handoff that, with earlier phases' checks, they meet every Phase 9 acceptance item (`92-Validation.md`, Phase 9). Every Phase 8 acceptance item passed in Unity in I17 (`92-Validation.md`, Phase 8). Phase 6's open "dead units are skipped" item passed in the I14 Regen-tracking check, which the I15 handoff reports. Every I15 and I16 check passed. The two Phase 7 items M07 left open ("retreating units keep fighting" and "escaped units never respawn that battle") passed in the Phase 7 closing check from `92-Validation.md`, which the user reported passed after M07 (V9.7.1).
- **World map (Phases 1–2).** Armies are constant-speed bouncing marbles that own a unit roster. Settlements own a garrison and are the single recruitment entry point. Every persistent unit has exactly one owner and is kept inactive in a reserve except while deployed in a battle. Deployment, overflow, gated transfers and the three allied contact interactions take effect. Every enemy intent takes effect: FieldBattle starts a field battle and, since Phase 9, Capture captures an empty settlement and Siege starts a siege. Since Phase 5 two armies returning from a battle are briefly immune to each other, and since Phase 9 a siege attacker is briefly immune to its settlement (`WM-IMMUNE-01`).
- **Battle arena (Phase 3).** The arena can be created far from the map. It freezes the map, swaps to the battle camera, builds terrain, a perimeter, entry strips and retreat gates, shrinks its view, and tracks registered bodies through entry, leaks, re-entry and escape. Debug test bodies and, since Phase 4, real units use it.
- **Battle deployment (Phase 4).** `BattleManager2D` starts a field battle from the step's FieldBattle pick, maps the two armies to sides and edges, binds side layers and masks, and spawns each army's units in roster order onto the arena, where they fight with the existing combat. A death releases ownership; an escape returns the unit to the reserve as Escaped (inactive, still owned, not parked).
- **Battle ending and return (Phase 5).** Each manager step checks for an end (elimination or timeout). A decided result is locked: the battle's units take no damage, accept no Clash and cast nothing, motors stop, escapes and spawning stop, and battle projectiles are removed. After the end delay the normal end parks every living unit as Reserve, destroys corpses, routes the armies (retreat flags; armies left empty are removed), places the survivors apart with pair immunity, releases the lock and removes the arena. An abort does the same without a result or retreat flags. Battles repeat, one at a time. An incomplete teardown, quarantined units and an orphaned arena are retried each step, and a post-battle audit runs one frame after each end or abort.
- **Regeneration (Phase 6).** A shared tick owned by the world-map controller runs every 2 s of scaled time, frozen or locked. Each tick heals every activated living unit in the reserve registry, parked or deployed, by its definition's `healthRegenFraction` of its effective maximum. Never-activated units are skipped. It is the only gameplay healing path (`WM-REGEN-01`, `WM-REGEN-02`, `HEALTH-05`).
- **Unit retreat (Phase 7, I15).** At each battle's commit every queued unit rolls once for retreat eligibility (40%); settlement-only units never qualify. An eligible field unit whose HP drops below 30% is latched, and a later step of the battle manager's FixedUpdate (the same step for a unit latched at deployment) makes it Escaping (still on the field and owned), steers it to the nearest gate, requests the retreat modifier once and raises `UnitRetreated`. Healing never cancels it. Touching a gate makes it Escaped as before. A shared delivery path applies battle modifiers once the unit has started and drops them when it leaves, at the lock, the end and an abort (`BATTLE-RETREAT-01`–`BATTLE-RETREAT-04`, `BATTLE-DELIVER-01`, `BATTLE-DELIVER-02`).
- **Retreating armies (Phase 7, I16).** An army flagged retreating by routing recovers and becomes ordinary 20 map seconds after the flag was set, which after a battle means 20 s after the unfreeze. Merges and transfers keep each army's own timer, and an army absorbed into a garrison or emptied is removed and never recovers (`WM-RETREAT-01`).
- **Battle-start modifiers (Phase 8, I17).** At each battle's commit the configured lists are copied onto the sides: a field battle gives the field list to both sides, and since Phase 9 a siege gives the town or castle defender list to the defender only. Each unit's spawn call requests its side's list once, before any retreat request. The shared delivery path applies it once the unit has started, with the unit as its own caster at its current level and the entries in list order. Parking or death removes it (`BATTLE-DELIVER-01`–`BATTLE-DELIVER-03`).
- **Conquest and sieges (Phase 9, I18–I19).** An ordinary army touching an empty enemy settlement captures it: the settlement changes faction, the army's units become its garrison and the army is removed (`WM-CAPTURE-01`). Touching a defended enemy settlement starts a siege. The whole garrison defends as Side A, on the settlement's authored terrain and edges, spawning at twice the normal rate by default, with retreat gates only on the attacker's edge (`BATTLE-SIEGE-01`). The normal end applies the roadmap's outcome table. An attacker win captures the settlement inside the end's routing: settlement-only defenders still in the garrison die with it, and mobile defenders still in it move to a pending retreat roster, the third owner kind. Any other outcome makes a surviving attacker retreat. Every surviving attacker that does not capture is placed away from the settlement with army–settlement immunity (`BATTLE-SIEGE-02`). Pending retreat rosters place their units as retreating armies of at most 200, retrying while blocked (`WM-PENDING-01`).

Not implemented (`CORE-02`): the shrink trigger. Pending retreat rosters and capacity splits, deferred to Phase 9 by I16, were implemented in I19 (`WM-PENDING-01`). The Phase 9 known gaps (untested paths and source facts the handoffs list) are recorded in the rules they touch and in `90-Evidence.md`, Phase 9 basis. Evidence tags `[WM1]`–`[WM9-S]` are defined in `90-Evidence.md` and summarised in `CORE-04`. Phase 5 Unity results come from three chats: I11 and I12 ran on earlier source (part of I12 before its review fixes and file split), I13 on the final source; each bracket names the run. Phase 6 results come from I14, on the final source, plus the Regen-tracking check reported with I15. Phase 7 results come from I15 (unit retreat) and I16 (retreating armies), plus the Phase 7 closing check reported after M07; each bracket names its run. Phase 8 results come from I17; the handoff reports them per acceptance item, and brackets quote what it observed. Phase 9 results come from I18 (capture and siege start, on the I18 source, with an interim siege return that I19 replaced) and I19 (siege outcomes and pending retreat rosters, on the final source); each bracket names its run, and I18 results are credited only to paths I19 did not change (`90-Evidence.md`, Phase 9 basis).

## Scope

**WM-SCOPE-01 — What takes effect.** Classification produces a `WorldMapContactIntent` plus diagnostics.
- Allied intents RetreatTransfer, RetreatMerge and JoinGarrison are applied after dispatch (`WM-ALLY-01`–`WM-ALLY-03`).
- BounceTransfersAvailable has no automatic effect; selected transfers go through the gated API (`WM-XFER-02`).
- FieldBattle takes effect: the step's pick starts a field battle (`WM-BATTLE-01`, `BATTLE-START-01`).
- Siege takes effect (I18): the step's pick starts a siege (`WM-BATTLE-01`, `BATTLE-SIEGE-01`).
- Capture takes effect (I18): it is applied after allied interactions, on every touching step, from live state (`WM-CAPTURE-01`). It is never a battle candidate.
- Bounce intents have no effect beyond the bounce Army2D computes independently of classification (`WM-MOVE-02`).
- `Army2D.IsRetreating` changes only through `SetRetreating`. The normal end's field routing (since Phase 5) sets it on a losing army, and on both armies after a BothLose, when they still have units (`BATTLE-ROUTE-01`); siege routing (since I19) sets it on a surviving attacker after any outcome but a capture (`BATTLE-SIEGE-02`); an abort sets no flag. Since I19 an escaped-defender army is created already retreating (`WM-PENDING-01`). Since Phase 7 setting it starts a map-time recovery timer, and the controller clears it when the timer runs out (`WM-RETREAT-01`). Routing never clears a flag; the debug panel can set or clear it.

[WM2: allied interactions. WM4: I10 battles start from the selected FieldBattle pick. WM5: I13 test 2 (winner stays ordinary), test 3 (timeout armies show the white flag), test 5 (abort: no white flag). WM7: I16 tests 2–3 (flagged armies recover after 20 map s). WM9: I18 check 1 (capture) and checks 2–7 (sieges start); I19 tests 2–4 (a surviving attacker retreats), test 6 (escaped defenders placed as a retreating army), test 9 (siege abort: no flags)]

## Lifecycle and time

**WM-LIFE-01 — Controller readiness.** One scene `WorldMapController2D` sets `Instance` in Awake; a second one logs an error and disables itself. A missing rules asset is replaced by a non-saved default instance, which is destroyed in OnDestroy. Unresolved WM layers disable the controller, so `IsReady` stays false.

Awake then, in order:
1. Reports matrix problems (`SETUP-06`) and warns on non-identity rotation or scale.
2. Creates the `Armies` child root and the runtime bounds.
3. Creates the unit reserve (`WM-UNIT-01`).
4. Creates the regeneration service over that reserve (null when there is no reserve) (`WM-REGEN-01`).
5. Sets `IsReady`.

Spawning, freezing, recruitment, deployment, transfers and contact intake require `IsReady`. OnApplicationQuit and OnDestroy enter shutdown. OnDestroy also destroys the bounds and the fallback rules and clears `Instance`. During shutdown, army and settlement destruction raises no events, leaves registries untouched and skips owner-loss handling (`WM-OWN-02`). [WM1: fallback-rules cleanup; shutdown ordering WM2-S, untested. Step 4 WM6: the I14 ticks ran]

**WM-LIFE-02 — Bounds.** `MapBounds` is a world rectangle. Awake builds a separate, unparented `WM_Bounds (runtime)` object with a closed, frictionless EdgeCollider2D loop from it; later edits to the serialized rectangle do not rebuild it. Bounds contacts bounce armies but are never reported (`WM-CONTACT-01`). [WM1]

**WM-TIME-01 — Step order and map time.** The controller's FixedUpdate runs at order −1000, before Army2D at the default order. Each ready step does the following, in order:
1. Raise `SettlementCaptured` for siege captures queued since the last step, frozen or not (I19, `WM-CAPTURE-01`).
2. Classify and dispatch contacts gathered during the previous simulation (`WM-CONTACT-03`, `WM-CONTACT-05`).
3. If unfrozen, apply allied interactions (`WM-ALLY-01`).
4. If unfrozen, apply captures of empty enemy settlements (I18, `WM-CAPTURE-01`).
5. Increment `PhysicsStep`.
6. If unfrozen, add `Time.fixedDeltaTime` to `MapTime`.
7. If unfrozen, recover retreating armies whose timer has run out (`WM-RETREAT-01`).
8. If unfrozen, run settlement work: pending deployments, then overflow, per settlement in registry order (`WM-DEPLOY-01`, `WM-OVERFLOW-01`); then, since I19, the pending retreat rosters (`WM-PENDING-01`). An `ArmySpawned` handler that freezes the map stops the rest of that step's settlement work.
9. Step regeneration with `Time.fixedDeltaTime`, frozen or not (`WM-REGEN-01`).

Consequences:
- A garrison join is overflow-checked in the same step.
- A garrison join in step 3 makes its settlement defended, so a capture of it in step 4 of the same step is refused by the live recheck. A capture is overflow-checked for the new owner in the same step's settlement work.
- A siege's normal end runs in the battle manager's FixedUpdate (−12000), before this controller's, and unfreezes the map there (`BATTLE-END-04`). So in the same physics step, this controller raises the queued `SettlementCaptured` (after `BattleEnded`) and then runs its first unfrozen step: the new owner's overflow and the escaped defenders' first placement attempt can happen in the step the siege ends (source-derived).
- Retry timers and recovery deadlines compare against the already-advanced MapTime.
- Allied interactions run before recovery, so an army due to recover this step still takes part in that step's garrison join, retreat transfer or merge first; one that is removed by them never recovers. Contacts were classified at step 1, so a recovered army is classified from its cleared flag from the next step on.
- `PhysicsStep` counts every ready FixedUpdate, frozen or not. `MapTime` advances only while unfrozen and is the clock for all world-map timers, independent of GameplayTime and Time.time.
- Regeneration is not a map timer: it accumulates scaled fixed time on every ready step, so it keeps ticking while the map is frozen.
- Callbacks are stamped with the values current during the simulation that produced them.

Army2D, Settlement2D and the debug panel declare no execution order. [WM2: same-step join + overflow; MapTime arithmetic source. Step 7 WM7: I16 tests 2–6 (recovery after 20 map s, paused while frozen; absorbed or emptied armies never recover); its position after allied interactions WM7-S. Step 9 WM6: I14 test 3 (ticks continue while the map is frozen). Step 4 WM9: I18 check 1 (capture, then overflow 200 → town sends out 100). Step 8's pending rosters WM9: I19 tests 6–7 (pending rosters are placed). Step 1 WM9-S: no I19 test names the siege capture's `SettlementCaptured`. The position of step 4, the refused capture after a same-step join, and same-step placement after a siege's end WM9-S]

**WM-FREEZE-01 — Freezing.** `SetFrozen(bool)` is a no-op when the controller is not ready or the value is unchanged. Otherwise it:
1. Sets `IsFrozen`.
2. On freeze, clears in-flight contacts and episode history.
3. For every registered, non-removed army, sets Rigidbody `simulated = !frozen` and discards its pending bounce. On unfreeze it also resets speed measurement.
4. On unfreeze, flushes deferred recruitment in every settlement (`WM-RECRUIT-01`).
5. Raises `FrozenChanged`.

`Time.timeScale` is never touched. While frozen:
- MapTime stops, and armies neither move nor bounce.
- Contact intake and classification discard everything, and allied interactions, captures, retreat recovery and settlement work (pending retreat rosters included) do not run. Recovery timers count map time, so they pause (`WM-RETREAT-01`). Queued siege-capture events are still raised (`WM-TIME-01` step 1).
- Recruitment is queued, and `DeployArmy` stores a pending request.
- Gated transfers are refused, and army creation is refused.
- Regeneration keeps ticking on scaled time (`WM-REGEN-01`).

Pending deployments and overflow run on the first unfrozen controller step after unfreeze. `DestroyArmy`, `SetRetreating`, `Settlement2D.SetFaction` and roster `Order` are not blocked by the freeze; only the debug panel disables them. Since I18 `SetFaction` is refused while its garrison is a side of the running battle (`WM-FACTION-01`), which is the case for the whole of a siege. Pairs still touching after unfreeze start new episodes.

**Arena ownership.** An arena can be created only while the map is unfrozen, and it then owns the freeze (`ARENA-LIFE-02`). Unfreezing the map by any other path while an arena exists aborts the arena (`ARENA-LIFE-03`) and with it the battle (`BATTLE-ABORT-01`). A battle keeps the map frozen for its whole duration. The world-map debug panel's Freeze toggle is disabled while `BattleArena2D.IsAnyArenaActive`; code calling `SetFrozen(false)` is not blocked.

[WM1: freeze with many armies; WM2: gates refuse while frozen; recruitment/deployment deferral WM2-S; arena freeze ownership WM3: I06 regressions; WM4: I10 Freeze lock during a battle; WM6: I14 test 3, regeneration ticks while frozen; WM7: I16 test 6 (freezing pauses the recovery timer)]

## Persistent units and ownership

**WM-UNIT-01 — Reserve, parking and deployment status.**
- **Storage.** `UnitReserve2D` is a plain C# object created by the controller. It is not a component: a component on an inactive root would never initialize. It owns one root, `ParkedUnits (inactive)`, a child of the controller with identity local transform (world origin and scale 1 when the controller is placed per `SETUP-06`), made inactive before any unit exists. `CreateInto` refuses to create units if this root is missing or active.
- **Recruiting.** Each recruit is instantiated directly under that inactive root, so Awake, OnEnable and Start never run. It is then set inactive itself, so a later move under an active parent cannot wake it.
- **Location.** A Reserve or Escaped unit is an inactive child of this root. A Deployed or Escaping unit is an active child of the scene root `Battle Units (active)` (`BATTLE-SPAWN-02`). Ownership is logical, not hierarchical: transfers never reparent, and armies never carry unit transforms.
- **Activation.** Only `BattleManager2D`'s spawn path activates a parked unit (Roadmap §2.2). Immediately before activation, in the same call, `TryMarkDeploying(unit, out error)` requires an open owner, an existing GameObject, status Reserve and an inactive GameObject, then sets status Deployed and `HasEverActivated`. Ownership does not change.
- **Escaping (I15).** `TryMarkEscaping(unit, out error)` is the only writer of Escaping. It requires an open owner, an existing GameObject, status Deployed and a GameObject active in the hierarchy, then sets status Escaping. It is callback-free. The unit stays active, on the field and owned; only the status changes (`BATTLE-RETREAT-03`).
- **Return to the reserve.** `ReturnToReserve(unit, Reserve | Escaped)` accepts only a Deployed, Escaping or Escaped unit that still has an owner and a GameObject. It deactivates the unit if active (OnDisable runs under the battle parent), reparents it under this root keeping its world position, and sets the status. Ownership does not change. The caller must unregister the unit's arena body first (`ARENA-BODY-01`).

[WM2: audit "never awakened" passed every run. WM4: I10 deployment (units deploy and fight) and abort (audits pass afterwards). The Escaped return (`ReturnToReserve(Escaped)`) WM4-S: no I10 escape result is named. WM7: I15 natural retreat and gate escape (Escaping → Escaped), abort while units are retreating (Escaping → Reserve). The Escaping refusal path WM7-S]

**WM-UNIT-02 — Handle identity and safe reads.** Each unit has one `UnitHandle` for life.
- **Identity.** `Id` is sequential from 1 per reserve. The handle keeps the same `GameObject` through every transfer.
- **Captured at creation.** `Definition` is taken from the clone's `UnitRuntimeStats2D.Definition`, a serialized field. `Progression` may be null.
- **Wake-safe reads.** Every handle read is safe on a parked, never-awakened unit:
  - `Faction` reads the combatant's serialized field.
  - `IsSettlementOnly` reads `Definition.SettlementOnly`.
  - `CurrentLevel` returns live progression only when `Progression.didAwake` is true and its level is ≥ 1. Otherwise it returns the authored starting level, clamped to ≥ 1.
  - `IsParked` means status Reserve and the GameObject is not `activeSelf`.
  - `Health` (I14) resolves the root `UnitHealth2D` with GetComponent on first use and caches the result, null included; GetComponent does not wake the unit. It may be null.
- **Activation flag.** `HasEverActivated` is set at the unit's first deployment (`TryMarkDeploying`, `WM-UNIT-01`) and never reset. Before it is true the unit's health has not initialized: `CurrentHealth` is 0, so `IsDead` reads true. Code acting on a unit's health checks `HasEverActivated`, not `IsDead`; regeneration skips such units (`WM-REGEN-02`).

These reads never initialize the unit (`CORE-03`). Every status is set: Reserve, Deployed, Escaping (since I15), Escaped, Dead, Discarded. `IsParked` is false for an Escaping or Escaped unit (status not Reserve), so transfers and the spawn path skip it. After a unit's first deployment `Progression.didAwake` is true, so `CurrentLevel` reads live progression from then on. [WM2: units keep GameObjects through transfers. Live-level reads after deployment: WM4-S, not separately tested. WM6: `Health` and `HasEverActivated` as the regeneration filter, I14 tests 2–4]

**WM-OWN-01 — One owner, one mutator.** Every living unit is in exactly one roster, and `UnitHandle.Owner` names it. Deployed, Escaping and Escaped units stay owned by their side's roster: their army or, in a siege, the besieged garrison (Roadmap §2.1). There are three roster kinds (`UnitRosterKind`): Garrison, Army and, since I19, PendingRetreat (`WM-PENDING-01`), the roadmap's third owner kind. Only `UnitReserve2D` changes ownership, through five operations:
- `CreateInto` (recruitment): each handle is registered and appended in the same step, so no handle ever exists without an owner.
- `Transfer` and `TransferFirst` (`WM-XFER-01`).
- `DiscardAll`: sets status Discarded, unregisters the handle, destroys the GameObject and closes nothing itself.
- `ReleaseOwnership(unit, Dead | Discarded)`: removes the unit from its owner (raising that roster's `Changed`) and from the registry, and sets the final status. It never destroys the GameObject, and it refuses any other final status or a unit already Dead or Discarded; any other current status, Escaping included, is accepted. Callers: a battle death (`BATTLE-UNIT-01`); lost-unit reports for a deployed, escaping, escaped or quarantined unit destroyed without dying (`BATTLE-UNIT-03`, `BATTLE-UNIT-04`); since I19, a settlement-only defender still owned by a garrison whose settlement is captured after a siege (`WM-CAPTURE-01`). `ReleaseOwnership` itself does not write `UnitHandle.Owner`, but `UnitRoster.RemoveInternal`, which it calls, sets `Owner` to null. This settles the question open since M06 (`UnitRoster.cs` read at M09). A handle with no owner when released is still given its final status and unregistered.

`TryMarkDeploying`, `TryMarkEscaping` and `ReturnToReserve` change status and location only (`WM-UNIT-01`). Rosters, battle queues, field lists, pending-roster lists and selections held elsewhere are indexes, not owners. [WM2: audit passed every run. WM4: I10 abort audits pass. WM5: the I13 post-battle audits, which include `AuditUnits` in the Editor, passed after battles with deaths (tests 2, 4) and 0 failed in the 20-battle run; a Dead unit still owned would fail that audit (`WM-AUDIT-01`), so death release is covered indirectly. WM9: the I19 handoff reports a post-battle audit PASSED with each test, and in test 7a the unit audit listed the held pending roster. The settlement-only release WM9-S; `RemoveInternal` clearing `Owner` is source]

**WM-OWN-02 — Owner loss and removal.**
- **Refusal.** `DestroyArmy` refuses an army that still owns units and logs a warning, unless `discardUnits` is true. The discard path is debug only and logs the count.
- **Lost owner.** If an army is Unity-destroyed without `DestroyArmy`, or a registered settlement is destroyed, the controller discards the orphaned units loudly with an error log and closes the roster. Units are never silently lost or left unowned. This handling is skipped during shutdown.
- **Settlement destroyed mid-siege** (known gap, I18). Owner-loss handling discards the whole garrison, including deployed defenders; the battle's sweep then reports those as lost (`BATTLE-UNIT-03`). Not handled further; untested.
- **Pending retreat rosters** are plain objects owned by the controller. They have no destruction path other than being emptied and closed (`WM-PENDING-01`).
- **Closed rosters** accept no recruitment, transfer or deployment.

[DestroyArmy refusal WM2-S; owner destroyed outside the API untested. The mid-siege case is from the I18 handoff, untested]

**WM-ROSTER-01 — Storage, order and capacity.**
- **Storage.** Arrival order, by ascending `ArrivalSequence`. One counter per reserve, so every arrival (recruit or transfer) gets a new, later sequence. Arrivals go to the back.
- **Order.** `Order` (`Fifo` or `HighestLevelFirst`) changes only how `GetUnitsInOrder` reads the roster. HighestLevelFirst sorts by `CurrentLevel` descending, then by arrival. This is a total order, so the result is deterministic. Changing `Order` raises no `Changed` and takes effect at the next read.
- **Order defaults.** Armies take `DefaultRosterOrder` at Initialize. Garrisons are created FIFO in Awake and take `DefaultRosterOrder` in Start on registration.
- **Capacity.** Armies use `MaxUnitsPerArmy`, read once at Initialize. Garrisons and (I19) pending retreat rosters are unbounded (`int.MaxValue`): the garrison limit never blocks arrivals.
- **Counts.** `EligibleCount = Count − SettlementOnlyCount`. Only garrisons accept settlement-only units (`AcceptsSettlementOnly`).
- **Faction.** Fixed for armies and pending retreat rosters. A garrison changes faction only through `WM-FACTION-01`; `TryRelabelFaction` refuses any other kind.
- **Owner references.** `Settlement` (garrison), `Army` (army) and, since I19, `PendingRetreat` (pending retreat roster) name the owning object; the other two are null. `OwnerLabel` is `S<id>`, `#<id>` or `P<id>`.

[WM2: both order modes and ties with authored levels; HighestLevelFirst with live levels untested. Pending-roster kind WM9: I19 tests 6–7]

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
- **In-battle refusal (I18).** While `IsRosterInBattle(garrison)` is true, that is while the garrison is the defender side of the running battle, it logs a warning and returns false, even if every garrisoned unit is parked (queued). The battle froze the side's faction at its start (Roadmap §2.3), and the guard lasts until `Current` clears (`BATTLE-LIFE-01`).
- **Siege bypass (I19).** Internal `SetFactionAfterSiege` skips only that guard. Only the siege capture calls it, during the siege's normal end, after it has emptied the garrison (`WM-CAPTURE-01`).
- **Refusal.** If any garrisoned unit is not parked (no GameObject or combatant, status not Reserve, or `activeInHierarchy`), it changes nothing, logs a warning and returns false.
- **Relabel.** Otherwise every unit's combatant faction is set, then the roster's (raising `Changed` only when the faction changed and units are present), then the settlement's.
- **Pending deployment.** An actual faction change cancels the pending deployment.
- **Deferred recruits** are not relabelled. They take the faction current at unfreeze (`WM-RECRUIT-01`). A siege capture drops them instead (I19, `DropDeferredRecruitment`, with a warning), so a defender's queued request never becomes the captor's units.
- **Unit-level guard.** `UnitCombatant2D.SetFaction` refuses while its GameObject is active in the hierarchy (`TARGET-01`).

[WM2-S. WM9: I18 check 9 ("SetFaction is refused mid-siege"); I18 check 1 and I19 test 5 (the capture's faction change). The bypass's caller check and the deferred-recruit drop WM9-S]

## Transfers

**WM-XFER-01 — Transfer primitive (ungated).** `UnitReserve2D.Transfer(from, to, selection)`:
- **Rejected with no change** if either roster is null, both are the same, either is closed, the factions differ, or (I19) both are pending retreat rosters ("pending retreat rosters do not exchange units").
- **Copies the selection first**, so a live roster list may be passed.
- **Per unit, in selection order, skipped** when:
  - invalid: not owned by the source, destroyed, status not Reserve (so Deployed, Escaping and Escaped units are skipped), or `activeSelf`;
  - settlement-only into an army;
  - over capacity.
- A unit listed twice is skipped as invalid the second time.
- **Moved units** go to the back of the destination with a new arrival sequence.

The result counts Requested, Moved and each kind of skip. `TransferFirst(from, to, maxCount)` takes the front of the source's current order. For an army destination it excludes settlement-only units, then truncates to `maxCount`; `maxCount ≤ 0` is rejected. These primitives are public but are internal building blocks with no in-battle or frozen gate; their source comment now says so (the controller's `CanTransfer` gates gameplay callers). A pending retreat roster accepts no settlement-only unit (`AcceptsSettlementOnly` is false), so `TransferFirst` into one skips them like an army destination. [WM2: selected transfers; skip accounting source. The pending↔pending refusal WM9-S]

**WM-XFER-02 — Gated transfers.** Gameplay and debug code use `TransferUnits(from, to, selection, out donorRemoved, removeEmptyDonor = true)` and `TransferFirstUnits(from, to, count, out donorRemoved, removeEmptyDonor = true)`.
- **Gate.** Both call `CanTransfer`, which refuses when the controller or reserve is not ready, the map is frozen, or `IsRosterInBattle(from or to)`. A refusal returns a rejected result with nothing moved.
- **Pending rosters (known gap, I19).** `CanTransfer` does not refuse a pending retreat roster as source or destination. Nothing calls the gated transfers with one today; only the pending↔pending pair is refused, by the primitive (`WM-XFER-01`).
- **Battle hook.** `IsRosterInBattle(roster)` asks `BattleManager2D.Instance` (false with no manager): true while the roster is either side's roster of the current battle (an army, or since I18 a besieged garrison), including while its result is locked (`BATTLE-LIFE-01`). The source comments in both files still say "army"; the manager compares each side's `Roster`. A battle keeps the map frozen, so the freeze refuses first; the battle check is a second gate for a map unfrozen while the battle is still current. That happens twice:
  - at the normal end, the arena's removal unfreezes the map (and runs `FrozenChanged` handlers) before `Current` is cleared (`BATTLE-END-04`);
  - in an abort through `ArenaRemoved`, the teardown unfreezes the map before the abort clears the battle (`BATTLE-ABORT-01`).

  Any other unfreeze aborts the battle. While a pending end is retried no battle is current, so the hook is false (`BATTLE-RECOVER-01`).
- **Empty donor.** An army emptied by a move is removed through `DestroyArmy` unless `removeEmptyDonor` is false.

[WM2: selected transfers, gates. WM4: I10 in-battle transfer refusal (the handoff does not say which gate refused). The garrison side of the hook WM9: I18 check 9 (SetFaction refused mid-siege; a deployment queued during a siege's end waits); a transfer involving a besieged garrison WM9-S]

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
  - Blocked placement keeps the request and retries after `PlacementRetrySeconds` of map time. Since I18 a besieged garrison is Blocked the same way (`WM-DEPLOY-02`), so a request made during a siege waits until the battle is no longer current.
  - Success clears only the request that ran, so a newer one stored by a handler survives.
  - Failure, or no eligible units remaining, drops the request with a warning; a closed garrison drops it silently.
- `CancelPendingDeployment` removes the request.

[WM2: Half of 7 → 3; All of 250 → 200; settlement-only units stay; blocked placement retries. WM9: I18 check 9 (a deployment queued during a siege's end waits and runs later)]

**WM-DEPLOY-02 — Army creation pipeline.** All populated armies come from `TryDeployFromRoster`, which is internal and roster-generic: manual deployment and overflow use a garrison as the source, and since I19 escaped defenders use a pending retreat roster (`WM-PENDING-01`). Two overloads: one searches a placement around an origin and radius, the other takes an explicit placement. Steps:
1. **Validate.** Not ready → Failed; frozen → Blocked; source null or closed → Failed; (I18) the source roster is a side of the running battle (`IsRosterInBattle`) → Blocked, which covers the moment at a siege's end or abort when the map is already unfrozen but the battle is still current; count outside 1..MaxUnitsPerArmy → Failed; fewer eligible units than count → Failed.
2. **Place** (`WM-PLACE-01`). No spot → Blocked.
3. **Create the army unannounced.** No `ArmySpawned` yet; a failed creation → Blocked.
4. **Fill** with exactly `count` units through `TransferFirst`.
5. **Ignore the source settlement's collider** (`WM-IGNORE-01`), when one is given.
6. **Retreating (I19).** With `startRetreating`, `SetRetreating(true)`, which starts the recovery timer at the current MapTime (`WM-RETREAT-01`).
7. **Update counters** (`TotalOverflowArmies`, `TotalEscapedDefenderArmies` (I19) or `TotalDeployedArmies`), then raise `ArmySpawned(army, Deploy | Overflow | EscapedDefenders)`.

Blocked and Failed change nothing. A partial fill is logged as an error and rolled back: units return to the source (at the back of its arrival order) and the army is removed silently with no `ArmyDestroyed`. If any unit cannot be returned, the army is announced instead, retreating first when the deployment asked for it, so nothing is lost. An empty populated army is never announced. `ArmySpawned` handlers are not isolated here (known gap, from before I19; `WM-REMOVE-01`). Debug `TrySpawnArmy` / `TrySpawnArmyAtRandom` create and announce empty armies with whatever reason the caller passes; the debug panel always passes `Debug`. [WM2; rollback path untested. WM9: I19 tests 6–7 (escaped-defender armies placed retreating). The in-battle Blocked WM9: I18 check 9. The retreating rollback WM9-S]

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

It never sends a smaller army. A blocked or failed attempt sets the retry timer; a failure also warns. A garrison that is a side of the running battle is Blocked (I18, `WM-DEPLOY-02`), so a besieged garrison above its limit waits. Overflow is state-based, so a capture that leaves the new garrison at or above the limit sends an army in that step's settlement work (`WM-CAPTURE-01`). At most 16 overflow armies per settlement per step; the rest continue next step. The controller's `GetGarrisonLimit(settlement)` and `Settlement2D.GarrisonLimit` expose the limit (0 when unregistered). [WM2: castle 280 + 50 → 230 + one 100-unit army, including same-step after a garrison join. WM9: I18 check 1 (a 200-unit capture of a town sends out 100). The in-battle block WM9-S]

## Allied interactions

**WM-ALLY-01 — Scheduling.** Allied interactions run from FixedUpdate after contact dispatch, never inside a `ContactReported` handler, and only if the map is unfrozen.
- **Which pairs.** A copy of `LastStepContacts` filtered to RetreatTransfer, RetreatMerge and JoinGarrison.
- **Every step.** They are re-applied on every step a pair touches, not only on a new episode, so capacity that opens later gets filled.
- **Live recheck.** Each pair is skipped unless both participants are live (initialized, not removed, roster open; settlement registered with an open garrison). It is then reclassified from live state, and army roles are recomputed with `SecondArmyFirst` from live counts. Earlier effects in the same step are therefore reflected.
- **In-battle hook.** A pair is skipped if `IsRosterInBattle` is true for either side. The hook is live since Phase 4, but while a battle runs the map is frozen, so allied interactions do not run anyway.
- **Freezing.** Processing stops as soon as any callback freezes the map.
- **Record.** Each applied interaction (units moved or donor removed) increments its counter and appends an `AlliedInteractionRecord` (the last 24, oldest first, optionally logged). It then raises `AlliedInteractionApplied` after rosters, removal and counters are consistent. The whole multicast runs inside one try/catch, so a throwing subscriber's exception is logged and later subscribers in that invocation are skipped.

[WM2: allied interactions; ordering, record and event delivery source; handlers that freeze or redeploy untested]

**WM-ALLY-02 — Retreat transfer and merge.**
- **Roles.** For RetreatTransfer the retreating army is the donor. For RetreatMerge the smaller army donates; at equal size the higher id donates into the lower (older) id.
- **Move.** `TransferFirst(donor → receiver, donor.UnitCount)` up to the receiver's free capacity.
- **Donor left empty:** removed through `DestroyArmy`.
- **Donor left with units:** keeps them and stays retreating.
- **Receiver full:** nothing changes, nothing is recorded, and the pair retries while touching.
- **Flags and timers.** Neither interaction calls `SetRetreating`, so retreat flags are unchanged and every army keeps its own recovery timer (`WM-RETREAT-01`): a merge receiver keeps its timer, a donor left with units keeps its flag and timer, and a donor emptied is removed with its timer. A RetreatTransfer receiver is ordinary and stays ordinary.
- **Record (I16).** `AlliedInteractionRecord` also carries `DonorRecoveryRemaining` and `ReceiverRecoveryRemaining`: each army's map seconds left until recovery right after the interaction, 0 when it is not retreating or was removed. `ToString` appends them when positive. A garrison join records 0 for both, and so does the older 10-argument constructor, which is kept.

[WM2: transfer at and below capacity; merges smaller-into-larger, equal sizes, partial. WM7: I16 test 4 (a merge receiver keeps its own timer, including a partial merge where the donor keeps its leftover units and its own timer), test 5 (a donor emptied into an ordinary ally never recovers). The record's timer fields and the 10-argument constructor WM7-S]

**WM-ALLY-03 — Garrison join.** A retreating army touching an allied settlement transfers all its units into the garrison (unbounded) and is removed through `DestroyArmy`. If units remain, which source treats as a bug, it logs an error, keeps the army, which keeps owning them, and retries next touching step. The join is overflow-checked later in the same step (`WM-TIME-01`). An absorbed army is removed with its timer, so it never recovers (`WM-RETREAT-01`). [WM2: castle 280 + 50 join → 230 + 100. WM7: I16 test 5 (an absorbed army never recovers)]

**WM-AUDIT-01 — Ownership audit.** `AuditUnits(out report)` is development only and read-only. It audits all open garrisons, the rosters of non-removed armies, including the sides of a running battle, and since I19 every open pending retreat roster.

Reported problems:
- **Reserve root:** destroyed or active.
- **Roster level:**
  - over capacity;
  - arrival order not strictly increasing;
  - settlement-only counter mismatch.
- **Unit level:**
  - listed by more than one roster, or Owner mismatch;
  - not registered;
  - settlement-only unit in a roster that does not accept one (an army or a pending retreat roster);
  - destroyed while owned;
  - Reserve or Escaped: active, or not under the parked root;
  - Deployed or Escaping (I15, checked like Deployed): never marked activated, inactive, or still under the parked root;
  - any other status (Dead, Discarded) while owned;
  - faction ≠ roster faction;
  - never activated but any component has `didAwake`.
- **Registry level:** registered units without an audited owner.

The summary counts deployed, escaping and escaped units, and garrisons, armies and (I19) pending retreat rosters. The report lists at most 40 problems. Since Phase 5 the post-battle audit runs it one frame after every end or abort (`BATTLE-AUDIT-01`). [WM2: passed every run. WM4: passed after I10 aborts, when every living unit is Reserve again. WM5: passed in every I13 post-battle audit the tests name, 0 failures in the 20-battle run. The Deployed, Escaping and Escaped checks during a running battle WM4-S/WM7-S: no named audit ran mid-battle. WM6: I14 test 4, the audit still shows 0 awakened components with regeneration running. The Escaping branch: no I15 or I16 result names an audit. WM9: I19 test 7a (the audit lists the held pending roster) and the post-battle audit PASSED with every I19 test; I18 check 1 (unit audit passes after a capture)]

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

Handlers must still ignore `IsRemoved` armies. A direct Unity Destroy that bypasses `DestroyArmy` raises `ArmyDestroyed` from OnDestroy and triggers owner-loss handling, but never sets `IsRemoved`. Always use `DestroyArmy`.

Battle callers (Phase 5): the normal end and the abort remove an army left with no unit through `DestroyArmy(army)`, never with `discardUnits` (`BATTLE-RETURN-01`, and for sieges `BATTLE-SIEGE-02`). At the normal end this runs while the map is still frozen; in an abort through `ArenaRemoved` it runs after the unfreeze. For an abort caused by an outside unfreeze it runs inside that unfreeze's `FrozenChanged` dispatch. `WorldMapController2D.cs`, read at M09, raises `FrozenChanged` after its army loop and the deferred-recruitment flush, so a removal inside a handler does not disturb that loop; the path is still untested [WM5-SR]. Capture callers (Phase 9): an ordinary capture and a siege capture remove the army once all its units have joined the garrison (`WM-CAPTURE-01`).

**Handlers not isolated** (known gap, from before I19). `DestroyArmy` raises `ArmyDestroyed` without isolating its handlers, and the deployment pipeline raises `ArmySpawned` the same way (`WM-DEPLOY-02`). The siege capture wraps its own `DestroyArmy` call, so a throwing handler cannot skip its bookkeeping; the army is already marked removed when the event is raised.

[WM1 for DestroyArmy; immediate simulation stop and refusal WM2-S; direct-destroy path untested. WM5: I13 test 2 (losing army removed), test 4 (empty armies removed). WM9: I18 check 1 and I19 test 5 (the capturing army is removed). The handler-isolation gap is from the I19 handoff]

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

`WorldRadius` = `Radius` × the largest absolute X/Y scale; keep scale 1. Source warns only on non-uniform X/Y. `Position` is the transform position. The label shows the kind prefix and `GarrisonCount`, with `*` while a deployment is pending; it refreshes on garrison `Changed`. Kind, faction and radius are serialized. There is no movement API and no count setter.

**Siege setup (I18).** Each settlement authors `siegeBattlefieldPrefab`, `siegeAttackerEdge` (default Left) and `siegeDefenderEdge` (default Right), used for every siege of it regardless of the contact direction (Roadmap §1.7). `TryGetSiegeSetup(out prefab, out attackerEdge, out defenderEdge, out error)` is read-only and checks the authored fields only: a prefab is assigned, it is an asset rather than a scene object, both edges are defined `ArenaEdge` values, and they differ. The prefab's contents are checked by `BattleArena2D.ValidateBattlefieldPrefab` (`BATTLE-SIEGE-01`). OnValidate warns when the two edges are equal. [WM1: CircleCollider2D retest. Siege setup WM9: I18 checks 4 and 7 (authored edges used whatever the contact direction; an invalid setup starts nothing)]

**WM-SETTLE-02 — Registration and identity.** In Start a settlement uses its assigned controller or `Instance`, registers, receives `SettlementId` (sequential from 1 per controller, never reassigned) and sets the garrison's order to `DefaultRosterOrder`.
- `0` means unregistered. Its contacts are ignored, and recruitment and deployment are rejected.
- Start order among scene settlements, and therefore initial id order and settlement-work order, is not guaranteed.
- OnDestroy unregisters and triggers owner-loss handling, except during controller shutdown.
- No settlement events exist; `SettlementCaptured` is a controller event (`WM-CAPTURE-01`).

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

Handlers must ignore removed armies (`WM-REMOVE-01`). `BattleManager2D` starts battles (field battles and, since I18, sieges) from inside this dispatch; a committed start leaves the map frozen, so the reports after the starting one are not delivered that step. If preparation fails after the arena was built, `Remove()` unfreezes the map within the same call and dispatch continues (`BATTLE-START-01`). The I10 change to this file (comments and log text only) was checked in Unity (I10 "new contact log text"). [WM2: freeze during dispatch; ordering source. WM4: I10 battles start from the selected pick]

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

**WM-BATTLE-01 — Battle selection.** Each step, every touching pair whose intent starts a battle, new or continuing, is a candidate. One is chosen uniformly with `UnityEngine.Random` and stored in `LastSelectedBattle` with `LastBattleSelectionCandidates`. Steps with ≥ 2 candidates are counted; a Console line is written only when a candidate is new.
- **Starting.** `BattleManager2D` handles `ContactReported` and acts only on the report that matches the pick (`BATTLE-START-01`). A FieldBattle pick starts a field battle; since I18 a Siege pick starts a siege (`BATTLE-SIEGE-01`). Field battles and sieges compete in the same uniform draw. A Capture contact is never a candidate; it is applied after allied interactions (`WM-CAPTURE-01`).
- **Unselected pairs** keep touching and compete again next step. Once a battle starts the map is frozen, which ends every touch episode (`WM-FREEZE-01`).
- **Immune pairs (I13, I18).** An army pair under pair immunity, or since I18 an army–settlement pair under siege immunity (`WM-IMMUNE-01`), whose intent starts a battle is still reported, classified, logged on a new contact (with a note) and dispatched, but it is not a candidate. Each skipped candidate-step increments `ImmuneBattleCandidatesSkipped` (reset by `ResetContactStats`). A step with no candidates leaves `LastSelectedBattle` holding an earlier step's pick; the battle manager compares the step, so that pick starts nothing.
- The arena debug panel can still copy the pick's contact point and counts into its create fields.

[WM1: Converge 4. WM4: I10 battles start from the selected FieldBattle pick. WM5: I13 test 6 (an immune pair bounces instead of battling; each army can still fight a third army). The stale-pick case WM5-S. WM9: sieges start from a Siege pick (I18 checks 2–8, I19 tests 2–6, 8 and 9; test 7 used the debug evacuation, not a siege); I18 check 8 and I19 test 4 (no immediate re-siege after the return). A siege and a field battle competing in one step WM9-S]

## Pair immunity (Phase 5) and army–settlement immunity (Phase 9)

**WM-IMMUNE-01 — Army pair and army–settlement immunity.** `WorldMapController2D.Separation.cs` keeps one expiry in map time per army pair (keyed by the ordered ids) and, since I18, one per army–settlement pair (keyed by `WorldMapContactPairKey.ForSettlement(armyId, settlementId)`). The army-pair clauses come first; the army–settlement clauses follow.
- **Grant.** `GrantArmyPairImmunity(a, b, mapSeconds)` refuses when the controller is not ready, either army is null or uninitialized, both are the same army, or the duration is not positive and finite. Otherwise the expiry is `MapTime` + seconds; an existing later expiry is kept (extend, never shorten). `TotalImmunityGrants` counts every accepted call.
- **Effect.** `IsArmyPairImmune` is true while `MapTime` < expiry. An immune pair is not a battle candidate (`WM-BATTLE-01`), and `TryStartFieldBattle` refuses it (`BATTLE-START-01`). Nothing else changes: the pair is still reported and classified, still bounces, and each army can still fight other armies. Settlement contacts are unaffected by army-pair immunity.
- **Clock.** MapTime stops while frozen (`WM-TIME-01`), so a grant made at a battle's end starts counting at the unfreeze. Expired grants are pruned in each unfrozen contact step. Army ids are never reused, so grants for removed armies simply expire.
- **Callers.** The battle manager grants `BattleRules2D.PostBattleImmunitySeconds` (default 1; 1 with no arena or rules) after a normal end or an abort while both armies remain, and when a start is refused for lack of return room (`BATTLE-RETURN-01`, `BATTLE-START-03`). Roadmap §1.3 "Separation".
- **Diagnostics.** `ImmunePairCount`, `ImmuneBattleCandidatesSkipped` (candidate-steps, not episodes), `TotalImmunityGrants`, `GetArmyPairImmunityRemaining(a, b)`.
- **Where it matters today** (source-derived). When a normal end leaves both armies, routing has made at least one of them retreating (`BATTLE-ROUTE-01`), so their contact classifies as EnemyArmiesWithRetreating → Bounce (`WM-CLASS-01`) whether or not they are immune. The flag clears only at recovery, 20 map s after the unfreeze by default (`WM-RETREAT-01`), long after a 1 s grant expires. So with the defaults the immunity decides the outcome only after an abort (no retreat flags) and after a no-return-room refusal. It also matters after a normal end when `retreatRecoverySeconds` is shorter than `PostBattleImmunitySeconds`: with 0, the flag clears in the step the map unfreezes, and the 1 s grant then keeps the pair from fighting at once.
- **Army–settlement immunity (I18).** `GrantArmySettlementImmunity(army, settlement, mapSeconds)` refuses when the controller is not ready, the army is null or uninitialized, the settlement is null or unregistered (id 0), or the duration is not positive and finite. Otherwise it extends, never shortens, the pair's expiry to `MapTime` + seconds, and `TotalArmySettlementImmunityGrants` +1. `IsArmySettlementImmune(army, settlement)` and `(armyId, settlementId)` are true while `MapTime` < expiry.
- **Its effect.** An immune army–settlement pair is not a battle candidate (`WM-BATTLE-01`), and `TryStartSiege` refuses it (`BATTLE-SIEGE-01`). It does not block a capture: an empty settlement is never a battle candidate (`WM-CAPTURE-01`). The pair is still reported and classified, still bounces, and the army can still fight other armies and besiege other settlements. Same clock as the pair immunity, and both tables are pruned together in each unfrozen contact step. Settlement ids are never reused either.
- **Its callers.** The battle manager grants `PostBattleImmunitySeconds` after every siege end or abort that leaves the attacking army alive, except a capture (`BATTLE-SIEGE-02`), and when a siege is refused for lack of a return spot (`BATTLE-SIEGE-01`).
- **Where it matters** (source-derived). After a defender win, a BothLose or a timeout the surviving attacker is retreating, so its contact with the enemy settlement classifies as RetreatingArmyEnemySettlement → Bounce anyway (`WM-CLASS-01`). The siege immunity therefore decides the outcome only after an abort, after a refused capture (the attacker stays ordinary), after a no-return-room refusal, and when `retreatRecoverySeconds` is shorter than the grant.
- **Diagnostics.** `ImmuneArmySettlementCount`, `TotalArmySettlementImmunityGrants`, `GetArmySettlementImmunityRemaining(army, settlement)`; skipped candidate-steps count in the shared `ImmuneBattleCandidatesSkipped`.

[WM5: I13 test 6 (about 1 s of map time; the pair bounces instead; each army can still fight a third army); tests 3 and 5 (no new battle between the pair). Extend-not-shorten, pruning and refusals WM5-S. Army–settlement immunity WM9: I18 check 8 ("no immediate re-siege" after the interim siege end and abort); I19 test 4 (timeout: "placed away with army–settlement immunity"). Its refusals, extension, the no-return-room grant and the refused-capture case WM9-S]

## Regeneration (Phase 6)

Implemented in I14 (`UnitRegeneration2D.cs`, `WorldMapController2D.Regeneration.cs`; heal path in `UnitHealth2D`, `HEALTH-05`). Roadmap §1.4. Settings: `WorldMapRules2D.RegenerationIntervalSeconds` (2) and per unit `UnitDefinition2D.healthRegenFraction` (0.02). Departure kept from I14 (Roadmap §5 notes are guidance): the suggested name `UnitRegeneration2D` is a plain C# object owned by the controller, not a component, because a component on a parked unit never runs.

**WM-REGEN-01 — Tick owner and timing.**
- **Owner.** The controller creates one `UnitRegeneration2D` in Awake, right after the reserve, and keeps it in `Regeneration` (null when there is no reserve, in which case nothing ticks) (`WM-LIFE-01`). It is the owner because it owns the reserve, which lists every unit to heal.
- **Step.** At the end of every ready controller FixedUpdate, after settlement work, frozen or not (`WM-TIME-01`), the controller calls `Step(Time.fixedDeltaTime, RegenerationIntervalSeconds)`. The interval is the rules value (minimum 0.1), or 2 with no rules.
- **Clock.** Scaled fixed time, never MapTime: ticks continue while the map is frozen for a battle, through the result lock and the end delay. The service never reads `CombatLock2D` or `MapTime` (Roadmap §1.4).
- **Accumulation.** A nonpositive, NaN or infinite delta or interval is ignored. Elapsed time accumulates in double. A tick runs when elapsed + 1e-6 ≥ interval (tolerance for float sums such as 100 × 0.02 s); one interval is then subtracted, and a remainder that is negative or still at least one interval is reset to 0. At most one tick runs per step, so a long step never causes a burst. Because the controller passes `Time.fixedDeltaTime` and reads the interval every step, the reset to 0 drops time only when a single fixed step is longer than the interval, or when the interval is lowered at runtime below the time already accumulated. With a 0.02 s step and the default interval, a tick runs on every 100th ready step (`92-Validation.md` D4).
- **Position in the frame.** The step runs at the controller's order (−1000), after the battle manager, scheduler and Clash director have finished the step's work, before the arena (−900) and the units' own health FixedUpdate (−200), and outside any Clash transaction (`ORDER-01`).
- **Logging.** With `logRegenerationTicks` on (off by default), each tick logs its report and the frozen flag. In the Editor, a tracked unit also forces the log and appends that unit's HP (card).

[WM6: I14 test 2 (ticks about 2 s apart), test 3 (ticks continue while the map is frozen and during the locked delay), the extra check (a deployed unit reached 100% at tick 25 inside a long locked delay). The accumulation arithmetic and the leftover reset WM6-S]

**WM-REGEN-02 — Who heals, and the tick report.** Each tick copies `UnitReserve2D.Units` (every owned living unit: Reserve, Deployed, Escaped, and quarantined units, whose status is unchanged; since I19 also the parked units of pending retreat rosters, which are registered like any owned unit) into a reused buffer. It walks the copy, because health callbacks may change the registry. Per unit, in order:
1. null → Ineligible;
2. `HasEverActivated` false → NeverActivated, skipped before its health is touched. Its health has not initialized (`WM-UNIT-02`), and it starts full at first activation;
3. owner or GameObject gone, or status Dead or Discarded (released since the snapshot, for example by a death earlier in the tick) → Ineligible;
4. no root `UnitHealth2D` (`UnitHandle.Health`), or a fraction not above 0 (including a missing definition) → Ineligible;
5. `UnitHealth2D.ApplyRegeneration(Definition.HealthRegenFraction)` (`HEALTH-05`) → Healed, AlreadyFull or Ineligible. Healed is split into active and inactive using `activeInHierarchy` read before the call. An exception is logged with the unit and counted as Failed, and the tick continues.

Consequences:
- **Dead units.** A death releases ownership and removes the unit from the registry at once (`BATTLE-UNIT-01`, `WM-OWN-01`), so a corpse is never in the snapshot; the heal also refuses a died or zero-health unit.
- **No activation.** The tick never activates or wakes a unit (`CORE-03`): an inactive unit is healed while inactive.
- **Status.** Only Dead and Discarded are checked, so an Escaping (retreating) unit heals like a Deployed one. Its heal raises `HealthChanged`, which the battle's retreat latch also hears, but a heal never cancels a retreat (`BATTLE-RETREAT-02`).
- **Parked HP.** Parking keeps HP (`BATTLE-UNIT-04`); regeneration then raises it while the unit is parked, so it may return to battle with more HP than it left with.

**Report.** `LastTick` (`UnitRegenerationTickReport`: Tick, Healed, HealedActive, HealedInactive, AlreadyFull, NeverActivated, Ineligible, Failed), `TickCount`, `TotalHeals`, `ElapsedSeconds`. The service raises no event; displays update through each heal's HealthChanged (`UI-01` on redeployment).

[WM6: I14 test 2 (a parked unit healed and stays inactive), test 3 (deployed units heal), test 4 (never-activated units skipped; the audit still shows 0 awakened components), test 5 (display after redeployment); dead units skipped: the I14 Regen-tracking check, reported passed with the I15 run (`92-Validation.md` D6). Escaped and quarantined units healing by name, the Failed path and a zero fraction WM6-S. WM7: I15 "healing never cancels a retreat"; the handoff does not say whether that run healed through regeneration or the debug heal. Pending-roster units healing WM9-S: it follows from the registry walk (`UnitRegeneration2D.cs` was not re-attached at M09; I19 did not change it), and no I19 test names it]

## Retreating armies (Phase 7)

Implemented in I16 (`WorldMapController2D.Retreat.cs`, with the timer on `Army2D`). Roadmap §1.6. Setting: `WorldMapRules2D.RetreatRecoverySeconds` (20). Retreating armies otherwise behave as before: they bounce off enemies without fighting and follow the allied interaction table (`WM-CLASS-01`, `WM-ALLY-02`, `WM-ALLY-03`).

**Deferred to Phase 9 (I16 decision), implemented in I19.** I16 deferred pending retreat rosters and capacity splits, because field routing never moves units, so no field army can exceed 200. I19 added them with a third roster kind and the audit, transfer-rule and placement changes the deferral named (Roadmap §1.7, §2.1; `WM-PENDING-01`). Field routing still never moves units.

**WM-RETREAT-01 — Recovery timer and recovery.**
- **Timer on the army.** `Army2D` keeps the deadline next to the flag, so it disappears when the army is removed. `SetRetreating(true)` on an ordinary army records `MapTime + RetreatRecoverySeconds`; the rules value is clamped to ≥ 0, and 20 is used without rules. `SetRetreating(false)` drops the deadline. Setting the flag the army already has changes nothing, so no caller can restart a running timer by setting it again.
- **Readouts.** `RetreatRecoverAtMapTime` (+∞ when not retreating) and `RetreatRecoveryRemaining` (map seconds left, never negative; 0 when not retreating or due). `DebugSetRetreatRecoveryRemaining(seconds)` moves the deadline to `MapTime` + max(0, seconds) without touching the flag; it returns false when the army is not retreating, uninitialized or removed, or the value is not finite. Only the debug panel calls it.
- **Who sets the flag.** Routing at the normal end, after the end delay and while the map is still frozen: field routing (`BATTLE-ROUTE-01`) and, since I19, siege routing on a surviving attacker (`BATTLE-SIEGE-02`). MapTime is stopped then, so the timer starts counting at the unfreeze. An abort sets no flag. Since I19 the deployment pipeline also sets it on an escaped-defender army before announcing it (`WM-DEPLOY-02`, `WM-PENDING-01`); the map is unfrozen then, so that timer counts at once. Allied transfers and merges never call `SetRetreating` (`WM-ALLY-02`). The debug panel's Flag+, Flag- and test spawns call it as well.
- **Recovery** is step 5 of `WM-TIME-01`: each unfrozen controller step, after allied interactions and the MapTime advance, before settlement work. It collects every live retreating army (initialized, not removed) whose recovery is due, meaning `MapTime + 1e-4 ≥ RetreatRecoverAtMapTime`. Then, for each in registry order:
  1. stop if a callback has frozen the map; the rest recover on the next unfrozen step;
  2. skip it unless it is still live, retreating and due;
  3. skip it while its roster is in the current battle (a guard only: a retreating army cannot start a battle, and the map is frozen during one);
  4. `SetRetreating(false)`: the army becomes ordinary, and its tint and white flag clear;
  5. `TotalRetreatRecoveries` +1, and a line goes into `RecentRetreatRecoveries` (the last 16, oldest first) and, with `LogRetreatRecovery` on (the default), the Console.

  Recovery never moves units or changes rosters, and it raises no event.
- **Tolerance.** MapTime sums the float `Time.fixedDeltaTime` into a double. 0.02f is slightly below 0.02, so after 1000 steps MapTime is about 4.5e-7 s short of 20 s. The 1e-4 margin absorbs that rounding; without it recovery would land one step late.
- **Consequences.**
  - A Decisive loser, or both armies after a BothLose, recover 20 map seconds after the end unfreezes the map. Both BothLose flags are set in one routing call at the same MapTime, so both recover in the same step.
  - A merge receiver keeps its own timer, and so does a donor left with units. An army absorbed into a garrison, or emptied into another army, is removed with its timer and never recovers (`WM-ALLY-02`, `WM-ALLY-03`).
  - A recovered army is classified from its cleared flag from the next step on (`WM-CONTACT-04`), so touching an ordinary enemy army makes it a FieldBattle candidate again, unless the pair is immune (`WM-IMMUNE-01`).
  - While a normal end's teardown is retried the map stays frozen, so nothing recovers (`BATTLE-RECOVER-01`).
  - With `retreatRecoverySeconds` 0 the deadline is the MapTime at which the flag was set, so a routed army recovers in the first unfrozen step.
- **Diagnostics.** `RetreatingArmyCount`, `TotalRetreatRecoveries`, `RecentRetreatRecoveries`, `RetreatRecoverySeconds`, `LogRetreatRecovery`. The battle return summary reports each retreating army's time left (`BATTLE-ROUTE-01`).

[WM7: I16 test 2 (a Decisive loser recovers 20 map s after the unfreeze; the handoff also reports that the timer pauses during the end delay, see `90-Evidence.md`), test 3 (after a timeout both armies recover in the same step), test 4 (merge timers kept, including a partial merge), test 5 (an army absorbed by a garrison, or emptied into an ordinary ally, never recovers), test 6 (freezing pauses the timer; Flag-, Flag+ and T work), test 7 (a recovered army can battle again). WM7-S: the in-battle guard, the stop-on-freeze break, `retreatRecoverySeconds` 0, recovery while an end's teardown is pending, the tolerance arithmetic. The siege and escaped-defender callers WM9: I19 tests 2–4 and 6 (flag set); their recovery after 20 s is not named by I19 (WM9-S)]

## Capture (Phase 9)

Implemented in I18 (`WorldMapController2D.Capture.cs`; the faction change in `Settlement2D`) and extended in I19 with the siege capture (`WorldMapController2D.Siege.cs`). Roadmap §1.7 "Capture". The siege's outcomes, which decide when a siege capture runs, are `BATTLE-SIEGE-02`.

**WM-CAPTURE-01 — Capture: ordinary and after a siege.**
- **Ordinary capture: when (I18).** Step 4 of `WM-TIME-01`: each unfrozen controller step, after allied interactions and before settlement work, never inside a `ContactReported` handler. It walks a copy of `LastStepContacts` filtered to intent Capture, so it is retried on every touching step. It stops as soon as a callback freezes the map. A refused capture is logged only on a new contact, and only with `LogCaptures` on (the default).
- **Ordinary capture: checks, in order.** Public `TryCaptureSettlement(army, settlement, out error)` returns false with nothing changed when:
  1. the controller or reserve is not ready, or the map is frozen;
  2. the army is not live (initialized, not removed, roster open);
  3. the settlement is unregistered, or its garrison is closed;
  4. a live reclassification is no longer OrdinaryArmyEmptyEnemySettlement (`WM-CLASS-01`), for example because a garrison join earlier in the step made the settlement defended;
  5. either roster is in battle (`IsRosterInBattle`);
  6. the army has no units (an empty debug marble never captures);
  7. any army unit is not a parked unit of the army (status Reserve, inactive, `Owner` the army), or has a faction other than the army's.
- **Ordinary capture: effects, in order.**
  1. `Settlement2D.SetFaction(army faction)` relabels the empty garrison (`WM-FACTION-01`), which also cancels the old owner's pending deployment. If the garrison's label then differs from the army's faction, the faction change is undone and the capture refused with an error log.
  2. `CancelPendingDeployment`, explicitly.
  3. `UnitReserve2D.TransferFirst(army → garrison, every unit)`, in the army's roster order. If nothing moved, the faction is restored and the capture refused with an error log.
  4. If units remain (a bug path: every unit was checked and garrisons are unbounded), `TotalCaptureFailures` +1 with an error log, and the army keeps them. It is now an enemy of the settlement, so it besieges it next.
  5. `DestroyArmy(army)` once it is empty, never with discard (`WM-REMOVE-01`).
  6. `TotalCaptures` +1, the record goes into `RecentCaptures` (the last 16, oldest first) and the log, and `SettlementCaptured(record)` is raised at once, each handler isolated.
- **Ordinary capture: consequences.** Army–settlement immunity does not block a capture (`WM-IMMUNE-01`). Overflow for the new owner is state-based and runs in the same step's settlement work (`WM-OVERFLOW-01`), so a 200-unit army capturing a town (limit 200) sends 100 out. The map is unfrozen, so there are no deferred recruits to drop.
- **Siege capture: caller (I19).** Internal `TryApplySiegeCapture(army, settlement, battleId, out record, out error)`. Only the siege routing of the attacker-win outcome calls it, during the normal end, while the map is still frozen and the battle current (`BATTLE-SIEGE-02`). It refuses unless the manager's `Current` is a siege of this settlement, its attacker side is this army, and the manager `IsEnding`. By then every living unit has been parked with its own owner: attackers in the army; defenders, escaped ones included, in the garrison.
- **Siege capture: checks first, all before any change.** The controller is ready; the army is live; the settlement is registered with an open garrison; the army's and the settlement's factions differ and the garrison's label equals the settlement's faction; the army roster is open and not empty; every attacker is a parked unit of the army with the army's faction; and every mobile (non-settlement-only) defender is a parked unit of the garrison. Settlement-only defenders are not checked: they die with the settlement whatever their status.
- **Siege capture: steps.**
  1. **Settlement-only defenders still owned by the garrison** (dropped at spawn or quarantined; the field ones already died) die with the settlement (I18 handoff decision). Each is released through `ReleaseOwnership(Dead)` (`WM-OWN-01`), deactivated if active and destroyed. This bypasses `UnitHealth2D` and `UnitDeath2D`: no damage, no `Died`, no kill XP, and it is not counted in the battle side's `Dead` (source-derived). One log line names up to 10. A unit that cannot be released is logged and stays in the garrison, which makes step 2 fail and undo. This step cannot be undone, which is why every check runs first.
  2. **Mobile defenders still owned by the garrison** (escapees and dropped units) move, in garrison order, to a new pending retreat roster of the defenders' faction, placed at the settlement (`WM-PENDING-01`). The roster is registered as soon as it holds units, so a later failure or exception cannot leave them unaudited or unplaced (§2.7). The garrison is now empty. This includes dropped units, not only escapees: a departure from §1.7's wording, because they can neither join the captor nor stay.
  3. **`Settlement2D.SetFactionAfterSiege(army faction)`** relabels the empty garrison and cancels the pending deployment (`WM-FACTION-01`); `CancelPendingDeployment` is also called explicitly.
  4. **Every attacker moves into the garrison** (`TransferFirst`, army roster order), then `DestroyArmy(army)`, never with discard. That call is wrapped, so a throwing `ArmyDestroyed` handler cannot skip the rest (`WM-REMOVE-01`). If units remain (a bug path), `TotalCaptureFailures` +1 and the army keeps them; it is now allied with the settlement.
  5. **Bookkeeping.** Recruits queued while the map was frozen are dropped with a warning (`WM-FACTION-01`). `TotalCaptures`, `TotalSiegeCaptures` and `TotalSettlementOnlyDefendersKilled` are updated, and the record goes into `RecentCaptures` and the log. `SettlementCaptured` is queued and raised at the start of this controller's next FixedUpdate, frozen or not (`WM-TIME-01` step 1). After a normal end that is the same physics step, after `BattleEnded`.
- **Siege capture: undo.** Steps 2–4 undo themselves if a later one fails. The faction is restored, and the pending roster's units return to the garrison. If any cannot return, the pending roster stays registered, so they stay owned and are placed later. Each undo logs an error and returns false. Step 1 is not undone.
- **Event timing after an incomplete end** (source-derived, untested). If the siege's normal end becomes a pending end (`BATTLE-RECOVER-01`), `BattleEnded` is raised in a later manager step. The queued `SettlementCaptured` is raised at the controller's next step regardless, so it can come before the delayed `BattleEnded`.
- **Record.** `SettlementCaptureRecord`: `Step`, `MapTime`, `SettlementId`, `Kind`, `PreviousFaction`, `NewFaction`, `ArmyId`, `Moved`, `GarrisonCount`, `ArmyRemoved`; since I19 `BattleId` (0 for an ordinary capture), `EscapedDefenders`, `SettlementOnlyKilled`, `PendingRetreatId` (0 when none was needed) and `IsSiegeCapture`. The 10-argument constructor is kept and passes 0 for the siege fields.

[WM9: I18 check 1 (faction change; units join the garrison; army removed; `SettlementCaptured` fired; a pending deployment is cancelled; overflow 200 → town sends out 100; unit audit passes), on the I18 source. I19 test 1 (an I18 empty-settlement capture behaves as before) is the ordinary capture on the final source. Siege capture: I19 test 5 (settlement captured, attackers now garrison it, army removed) and test 6 (escaped defenders moved to pending retreat roster P1, then placed as a retreating army of the old faction), each with a post-battle audit PASSED. WM9-S: settlement-only defenders dying with the settlement (no tool can set up a dropped or quarantined settlement-only defender); every refusal and undo path (I18: the label mismatch and nothing moved; I19: an unparkable unit and a throw part-way); the bug path that keeps part of the army; the deferred-recruit drop; the event's timing relative to `BattleEnded`]

## Pending retreat rosters (Phase 9)

Implemented in I19 (`WorldMapController2D.Siege.cs`, with the roster kind in `UnitRosterTypes2D`, `UnitRoster` and `UnitReserve2D`). Roadmap §1.7 "Escaped defenders", §2.1, §2.7.

**WM-PENDING-01 — Pending retreat rosters: ownership and placement.**
- **What it is.** `PendingRetreatRoster2D` holds one `UnitRoster` of kind PendingRetreat: the third owner kind (`WM-OWN-01`). It is owned by the controller and listed in `PendingRetreats`, oldest first. It is created by the siege capture (`WM-CAPTURE-01` step 2) or the debug evacuation below, and drained only by this rule. Its units move only through `UnitReserve2D` transfers.
- **Contents.** Every unit in it is owned by it, parked (status Reserve, inactive under the reserve root) and labelled with the defenders' faction, which never changes. Capacity is unbounded. It never accepts settlement-only units, never fights, is never a battle side and never exchanges units with another pending roster (`WM-XFER-01`).
- **Where it places.** `Origin` and `OriginRadius` are the settlement's position and `WorldRadius` at the capture. The settlement reference is used only for the post-deployment collision ignore and may be destroyed later. It also keeps `Id` (`P<n>`), `BattleId` (0 for the debug evacuation), the creation map time and step, and counters (`InitialCount`, `ArmiesDeployed`, `UnitsDeployed`, `PlacementBlocks`).
- **When.** In settlement work, after every settlement's pending deployment and overflow, inside the same reentrancy guard, only while unfrozen (`WM-TIME-01` step 8). It stops if an `ArmySpawned` handler freezes the map.
- **Placement, per roster** whose retry time has passed and whose debug hold is off. Repeatedly, at most 16 armies per roster per step:
  1. take count = min(`EligibleCount`, `MaxUnitsPerArmy`);
  2. call `TryDeployFromRoster(roster, count, origin, originRadius, source settlement, EscapedDefenders, startRetreating: true)` (`WM-DEPLOY-02`): one placement around the settlement, the collision ignore with it, `SetRetreating(true)`, then `ArmySpawned(army, EscapedDefenders)`;
  3. on success, update the counters, clear the block log and continue. So 450 units become 200 + 200 + 50, each army taking the next units in roster order.
- **Blocked or failed.** At the first blocked or failed attempt, `PlacementBlocks` and `TotalPendingRetreatBlocks` +1 and the retry time is set to `MapTime + PlacementRetrySeconds`. Every remaining unit stays owned. Retries continue indefinitely, and a failed attempt is retried too, unlike a manual deployment. Nothing freezes the map. A blocked episode logs one "no room" line, reset after the next success; a failure warns every time.
- **End of a roster.** A roster that is emptied is closed, logged and dropped from the list. A null or closed entry is dropped too.
- **Meanwhile.** Its units heal through the shared regeneration tick, which walks the reserve registry (`WM-REGEN-02`). They are audited (`WM-AUDIT-01`). They never join the captor, disappear or block the map (Roadmap §1.7).
- **Placed armies.** Each is a retreating army of the defenders' faction (`WM-RETREAT-01`): it bounces off enemies, including the captured settlement, follows the allied table and recovers after `RetreatRecoverySeconds` of map time.
- **Spawn reason.** `ArmySpawnReason.EscapedDefenders`. The planned `BattleReturn` reason was dropped: returning battle armies are moved, never spawned.
- **Debug.**
  - Settlement context menu "I19: Evacuate mobile garrison as escaped defenders (debug)" → `DebugEvacuateToPendingRetreat`. It moves every army-eligible garrisoned unit, in garrison order, into a new pending roster at that settlement, with no capture and no faction change. It is refused while frozen, for an unregistered or closed garrison, while the garrison is in battle, or with no eligible units. Because the settlement keeps its faction, the retreating armies rejoin its garrison if they touch it again (`WM-ALLY-03`).
  - Controller context menus "I19: Log pending retreat rosters and siege totals" (`DescribePendingRetreats`) and "I19: Toggle pending-retreat placement hold (debug)". While `DebugHoldPendingRetreats` is set, no placement is attempted and every unit waits.
  - The battle manager's "I19: Log siege totals and pending retreat rosters".
- **Known gaps (I19).** `CanTransfer` does not refuse pending rosters (`WM-XFER-02`). `WorldMapDebugPanel2D` does not show them; the context menus do (handoff statement; the panel source was not read at M09).

[WM9: I19 test 6 (escaped defenders to P1, then a retreating army of the old faction); test 7a (450 evacuated units became 200 + 200 + 50 in one step; the hold kept all 450 waiting; the audit lists the pending roster); test 7b (with blockers, one "no room" line, blocked attempts rose, nothing lost; once the blockers were removed the armies were placed). 7a and 7b used the debug evacuation, not a siege; a siege capture fills the roster through the same placement path. WM9-S: healing while pending; the failed-attempt path; the 16-armies cap; the hold persisting; a destroyed source settlement; the debug evacuation's refusals]

## Battle arena (Phase 3)

Since Phase 4 the arena hosts real units deployed by `BattleManager2D` (`BATTLE-SPAWN-02`); the debug panel and test bodies remain. Roadmap departures kept from I06–I07 (Roadmap §5 notes are guidance): both perimeter modes are kept, entry strips are pure geometry with no `SpawnBox2D` component, and the "fewer than 30 units" shrink trigger is not implemented: Phase 5 did not add it, so shrinking is still debug-only.

**ARENA-LIFE-01 — Service and readiness.** One scene `BattleArena2D` (order −900) sets `Instance` in Awake; a second logs an error and disables itself.
- A missing `BattleRules2D` is replaced by a non-saved runtime default (warning), destroyed in OnDestroy.
- Awake resolves the battle layers from the rules' layer names (`SETUP-07`). If they resolve it reports matrix problems, otherwise it logs an error. Resolution happens once.
- An unassigned camera rig is found in the scene; none logs an error. The world map is the assigned controller, else `WorldMapController2D.Instance`.
- `IsReady` = component active and enabled, `Instance` is this, rules present, camera rig present and ready, battle layers resolved.
- `IsAnyArenaActive` = `Instance` exists and has an arena.

[WM3: every I06/I07 run; second-instance and missing-rig paths WM3-S]

**ARENA-LIFE-02 — Creation and refusals.** `TryCreate(mapPoint, countA, countB, gateEdges, out error)`; the overload without `gateEdges` uses all four edges (field battles). `mapPoint` selects the terrain; the counts set the starting view size. Since I18, `TryCreateWithBattlefield(mapPoint, countA, countB, gateEdges, battlefieldPrefab, label, out error)` (sieges) does the same with an explicit terrain prefab instead of the region selection (`ARENA-TERRAIN-01`). Both share one creation path.

Refused, with nothing changed and `TotalFailedCreates` incremented, when (checked in this order):
1. the lifecycle is busy; there is no camera rig, or it is not ready; battle layers are unresolved; the arena is not ready; an arena already exists; the map is missing or not ready; **the map is already frozen** (the arena must own the freeze);
2. a count is negative or the point is not finite;
3. (`TryCreateWithBattlefield` only) no battlefield prefab is given;
4. the largest possible view (`CameraMaxSize`, aspect = max(live aspect, `ClearanceAspect`)) at `ArenaCenter`, expanded by `GeneratedGroundMargin + ArenaClearance`, overlaps `MapBounds`.

Gate edges are masked to the four edges. Then, with the lifecycle busy:
1. Create the root `BattleArena (runtime)` as a scene root at `ArenaCenter`, identity rotation, world scale 1.
2. Build terrain (`ARENA-TERRAIN-01`). Fail if the root's renderer and collider bounds overlap `MapBounds`.
3. Build the perimeter and gates for the view about to be shown, in the next perimeter mode (debug override, else rules) (`ARENA-PERIM-01`).
4. Record the map as the one this arena froze, call `SetFrozen(true)`, fail if it is not frozen, then subscribe to `FrozenChanged`. Recording first means a throwing `FrozenChanged` subscriber cannot leave the map frozen after rollback; subscribing after means the arena does not observe its own freeze.
5. Swap cameras at the computed size (`ARENA-CAM-01`).
6. Set `Center`, start/current/target view sizes, `MapPoint`, starting counts, `Battlefield`, `EscapesAccepted = true`, then `HasArena`.

Any failure or exception rolls back through teardown (`ARENA-LIFE-03`), counts a failed create and warns. Success increments `TotalCreated` and raises `ArenaCreated`.

[WM3: I06 regressions (create/remove, freeze ownership); I07 20 cycles. Throwing-subscriber rollback WM3-S: the I06 fix was not separately reported. `TryCreateWithBattlefield` WM9: every I18 and I19 siege ran through it; its missing-prefab refusal WM9-S]

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

A selected prefab is validated from the asset only by public static `ValidateBattlefieldPrefab(prefab, out error)` (public since I18, so the siege pre-arena checks can refuse before the arena exists). It is refused if it is missing, contains a Camera or an AudioListener, or has any object on a `WM_*` layer (checked only when `WorldMapLayers2D` is resolved) or on `Perimeter`/`RetreatingPlace`. A root scale other than 1 only warns. A valid prefab is instantiated under the root at the arena centre with identity rotation. Generated ground is a sprite (sorting order −100) covering the largest view at the clearance aspect plus `GeneratedGroundMargin`.
- **Sieges (I18).** `TryCreateWithBattlefield` skips `SelectBattlefield`: the selection is `BattlefieldSource.Siege` with the given prefab and label (the manager passes "siege of S<id> → <prefab>"). The arena validates it again when it builds the terrain. A siege's terrain therefore never depends on the contact point (Roadmap §1.7).

[WM3: I06 regressions (terrain selection); tie and boundary arithmetic source. Siege terrain WM9: I18 check 4 (authored setup used whatever the contact direction), check 7 (an invalid setup starts nothing)]

**ARENA-VIEW-01 — View size, shrink-only.**
- **Start size** = `ComputeViewSize(countA + countB)`: the curve value (NaN or infinite → min) clamped to [`CameraMinSize`, `CameraMaxSize`]. The default curve is piecewise linear through (0, 6), (40, 8), (100, 10), (200, 13), (400, 17), (600, 18) and constant beyond its ends.
- **Sizes.** `StartViewSize` is the creation size, `ViewSize` the live camera size, `TargetViewSize` the shrink target. Invariant: Target ≤ ViewSize ≤ Start.
- **Requests.** `RequestViewSize(size)` only lowers the target, clamped to `CameraMinSize`; it returns false with no arena, while busy, for non-finite input, or when the result is not lower. `RequestViewSizeForCount(n)` requests the curve size for `n`.
- **Stepping.** Each arena FixedUpdate moves `ViewSize` toward the target at `ShrinkSpeed` units per second, committed only when the camera accepts the size. Nothing raises it.
- **Trigger.** Not implemented. The roadmap's trigger (both queues empty and fewer than 30 units on the field, §1.3) was not built in Phase 5; the I13 handoff lists it for a later phase. Only the debug panel requests shrinking.
- **Search reach.** The I10 handoff reports that the enemy search reach was raised to 24 in the editor (an asset value; the map does not inspect assets). 24 exceeds the maximum orthographic size 18, which is the view's half-height; the half-width at 16:9 is 32, so the reach still does not span the view across. A unit with no target keeps walking along its facing (`MOVE-01`), which points inward after entry. The I10 handoff records this item as closed (24 ≥ 18); the half-width remark is a source-side observation, not a reported problem.

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
- **Real units (Phase 4).** `BattleManager2D` registers each deployed unit after binding its layers and activating it. Before deactivating a unit or releasing its ownership it unregisters the body, unless the arena has already released it (escape, teardown) (`BATTLE-SPAWN-02`, `BATTLE-UNIT-01`–`BATTLE-UNIT-04`, `BATTLE-ABORT-01`, `BATTLE-END-04`). Their GameObjects live under `Battle Units (active)`, never under Root.

[WM3: every I07 body test; lost-body path source. WM4: real units registered and tracked (I09, I10); the I09/I10 known answers report lost 0]

**ARENA-BODY-02 — Entry states, leaks and re-entry.** Each arena FixedUpdate, after shrinking and layout, every registered body is tracked against the live view:
- **Entering → Inside** when its centre lies inside the view inset by (radius + `EntryMargin`). It then collides with walls again, loses its Entry override and, if retreating, gets a Retreat override.
- **While Entering** the Entry override points to the nearest point of the view inset by (radius + `EntryMargin` + 0.05): a straight perpendicular walk from an entry strip (the view centre if the inset is empty).
- **Inside → Entering** when its centre is outside the view. This is counted as a **leak** (warning; first 20 logged) unless the view's width/height ratio changed since the previous pass, which counts as a **re-entry** (window resize). Shrinking keeps the ratio, so a body left outside by a shrinking wall is a leak.
- **`ForceReenter(body)`** (Inside only) announces a legitimate push outside: it counts a re-entry and returns the body to Entering.

The gap between the two thresholds is hysteresis. Real units get "no Sidestep before fully inside" without reading `IsFullyInside` (state Inside): the Entry override lasts until Entering → Inside, and Sidestep never starts or continues under any override (`SIDESTEP-GATE-02`, `SIDESTEP-LIFE-02`). The battle probe checks the Entering state (`BATTLE-UNIT-03`). Counters reset only through `ResetBodyCounters`; teardown does not reset them, so leak and lost counts carry across arenas and keep failing the audit until reset.

[WM3: I07 entry/containment, knockback 0 leaks, shrink 0 leaks, eject → re-entries +1 and leaks 0, resize; both modes. WM4: probe "while entering" 0 (I10); shrinking during a fight 0 leaks (I09); ejected units walk back in (I10)]

**ARENA-GATE-01 — Retreat steering, gates and escape.**
- **Flag.** `SetBodyRetreating(body, bool)` sets or clears the flag on a registered body; an Inside body is re-steered at once. Clearing is still allowed here. "Stays retreating even if healed" is Phase 7 policy held by the battle manager, which only ever sets the flag on real units (`BATTLE-RETREAT-03`); unregistration and teardown clear it.
- **Steering.** A retreating Inside body gets a Retreat override toward the closest point on the nearest gate edge of the view. With no gate edges it gets none.
- **Gates.** One trigger box per gate edge on layer `RetreatingPlace`, spanning the view edge just inside it, on a kinematic body, moved with the walls. Depth = min(`GateDepth`, max(0.05, ¼ of the smaller view dimension at creation)). Any edge mask may be passed; field battles use all four, and since I18 sieges pass only the attacker's edge, for both sides (`BATTLE-SIEGE-01`).
- **Reporting.** A gate reports every OnTriggerEnter2D and OnTriggerStay2D contact; Stay covers a body that starts retreating while already overlapping. Escapes therefore happen in the physics trigger phase, not in FixedUpdate.
- **Escape** requires an arena, not busy, `EscapesAccepted`, and a registered retreating body — in any state, Entering included. It records `EscapeEdge`, releases the body as Escaped, increments `TotalEscapes`, calls the owner's `EscapedCallback`, then raises `BodyEscaped`; each callback is isolated. A body escapes at most once because it is no longer registered.
- **Closing.** `EscapesAccepted` is set true at creation and false at teardown, and the battle manager sets it false when it locks a result (`BATTLE-LOCK-01`). It has a public setter. Teardown raises no escape events.

Real units: `BATTLE-UNIT-02`, `BATTLE-RETREAT-03`. [WM3: I07 Retreat 5 → 5 escapes, single-gate test. Escape while Entering and the closed-gate path WM3-S. WM4: I09 Retreat N, escapes with 0 lost. WM5: I13 20-battle totals report escapes after lock 0. WM7: I15 natural retreat and gate escape of real units; "no escape after the lock". Clearing the flag on a real unit has no gameplay caller. WM9: I18 check 4 (in a siege, retreat leaves only through the attacker's edge)]

**ARENA-DEST-01 — Destination override contract.** `UnitDestinationOverride2D` holds plain state and declares no Unity messages, so it is safe to read or write on a parked, never-awakened unit (`CORE-03`).
- **Reasons:** None, Entry, Retreat, Debug.
- **`Set(point, reason)`.** None → `Clear()`. A non-finite point logs an error and is ignored. Otherwise the destination is always updated; if the reason changed, `Activations` increments and `Changed` fires.
- **`Clear()`** is a no-op with no override; otherwise it resets and raises `Changed`.
- **`Changed`** fires only when an override starts, ends or changes reason, never for destination-only updates (every step while entering). Handlers are isolated.
- **`TryGetDirection(from, arriveDistance, out direction)`** returns false with no override, and true with a zero direction within the arrive distance.
- **Users.** The arena writes it (Entry, Retreat). `UnitMotor2D` and `UnitSidestep2D` read it through an optional same-root reference (`MOVE-01`–`MOVE-03`, `SIDESTEP-GATE-02`, `SIDESTEP-LIFE-02`); absent, neither is ever overridden. `BattleTestBody2D` reads its own. Battle deployment drops a unit whose root lacks one (`BATTLE-SPAWN-02`), so every deployable unit prefab must carry it. The I09 handoff reports a comment-only update to its header; the file was not re-attached for M04.

[WM3: used by every I07 test body; `Changed` delivery and the Debug reason source. WM4: real units steer by it (I09, I10); probe "Sidestep under override" 0]

**ARENA-TEST-01 — Test bodies are debug only.** `BattleTestBody2D` is created only by the arena's `SpawnTestBodies`, under Root/Test Bodies, with its own `UnitDestinationOverride2D`, on the side's body layer (`BattleLayers2D.BodyLayer`). It is a Dynamic, continuous, interpolated body with a frictionless circle. It steers toward its override, else by behaviour (Wander, PushOutward, SeekCenter, Hold); knockback sets a raw velocity and suspends steering. On escape it deactivates and destroys itself; its OnDestroy unregisters it. Test bodies die with the root. `ArenaSide` is an arena-local physics label, not a faction (Roadmap §2.3). No gameplay code may depend on test bodies; real units are registered by `BATTLE-SPAWN-02`. [WM3]

**ARENA-AUDIT-01 — Arena audit.** `AuditArena(out report)` is development only and read-only. Run it at least one frame after a removal. Reported problems:
- **Lifecycle:** arena roots alive ≠ (1 with an arena, else 0); an active arena whose root is missing or inactive; a `FrozenChanged` subscription left with no arena.
- **Cameras:** enabled cameras ≠ 1; world/battle/rig state not matching `HasArena`; enabled AudioListeners ≠ 1, or the single one not on the world camera.
- **Map:** when ready, `IsFrozen` ≠ `HasArena`.
- **Perimeter:** layers unresolved; walls alive ≠ 4 or off `Perimeter`; gates alive ≠ the gate-edge count; camera size ≠ `ViewSize`; size order broken.
- **Bodies:** bodies registered with no arena; index mismatch; a registered body destroyed; ignore flag or physics ignore state not matching mode and state; an Inside body with an Entry override; stray test bodies; any leak or lost body since the last counter reset.

[WM3: passed after the I07 20 cycles in both modes]

## Side layers (Phase 4)

**SIDE-01 — Side mapping and side layers.** Faction is identity (`TARGET-01`); `ArenaSide` A/B only selects physics layers and masks (Roadmap §2.3).
- **Field mapping.** `BattleSides2D.TryGetFieldBattleSides(first, second, ...)` puts the higher `FactionId.Value` on Side A and refuses equal values.
- **Siege mapping (I18).** The constants `SiegeDefenderSide` (A) and `SiegeAttackerSide` (B) are used: the defending garrison is always Side A and the attacking army Side B, whatever their factions (Roadmap §1.3 "in sieges the defender is Side A"; `BATTLE-SIEGE-01`). `BattleContext.Defender` and `.Attacker` read them, and the siege outcome classification compares the winner with `SiegeAttackerSide` (`BATTLE-SIEGE-02`).
- **Resolution.** `BattleSideLayers2D.TryCreate(rules, ...)` resolves the four names in `BattleRules2D` (defaults `Unit_SideA`, `Unit_SideB`, `Clash_SideA`, `Clash_SideB`). It refuses when any is missing, when they are not four distinct layers, or when its body layers differ from an already resolved `BattleLayers2D`. The arena's own `BattleLayers2D` resolution stays separate. `default(BattleSideLayers2D)` is invalid.
- **Masks.** `EnemyBodyMask(side)` is the opposite side's body layer only (one collider per enemy; the targeting buffer has no grow-and-retry, `TARGET-02`). `EnemyClashMask(side)` is the opposite side's Clash layer (detector, Area and projectiles).
- **When.** Resolved once per battle during the pre-arena checks (`BATTLE-START-02`) and kept in `BattleContext.Layers`.
- **Matrix report.** `LogCollisionMatrixProblems` only reports: Clash A↔B must interact (error); same-side Clash pairs and body↔Clash pairs should not (warning); Clash against any `WM_*` layer is an error when WM layers are resolved. Body↔body and `Perimeter`/`RetreatingPlace` rows belong to `BattleLayers2D` (`SETUP-07`). In the attached source only `UnitSideBindingTest2D` calls it; the battle path does not.

[WM4: I08 renamed layers keep their indices (11 `Clash_SideA`, 12 `Clash_SideB`, 15 `Unit_SideA`, 16 `Unit_SideB`), side matrix report clean, negative asset-string check as expected; I10 units keep their faction and get the right side layers. Field mapping arithmetic source. WM9: I18 check 2 (the defender is Side A for both faction orders)]

**SIDE-02 — Binding contract.** `UnitSideBinder2D.TryBind(unitRoot, side, layers, out error)` binds a parked unit to a side.
- **Refusals** (nothing written): null root; unknown side; invalid layers; a root active in the hierarchy (bind while parked); no root `UnitTargeting2D` or root `UnitClashParticipant2D`; no `UnitClashDetector2D` among the children (inactive included); a detector on the root itself; a detector whose parent participant belongs to another unit.
- **All-or-nothing.** Every check runs before the first write.
- **Writes, and only these:** the root layer → own side's body layer; each detector's GameObject layer → own side's Clash layer, and its opposing mask → the enemy Clash mask; the targeting mask → the enemy body mask (live filter refreshed if already awakened, `TARGET-02`); the optional same-root ability holder's enemy Clash mask → the enemy Clash mask. The detector setter's result is not checked; it can fail only on an empty mask, which valid layers rule out.
- **Never touched:** faction, other children's layers (visuals, UI), the holder's Wall mask, and casts or projectiles already made, which keep their frozen masks (`ABILITY-03`, `PROJECTILE-01`).
- **Wake safety.** Only GetComponent and plain field writes; a never-awakened unit stays unawakened (`CORE-03`).
- **Authority.** The prefab's saved layers and masks are not authoritative in battle: every deployment binds (`BATTLE-SPAWN-02`). A unit dropped after binding keeps that side's layers until its next deployment rebinds it.

[WM4: I08 `UnitSideBindingTest2D` (never-awakened clone bound A → B → A; root and Clash layers; targeting, detector and ability masks; other layers untouched; faction unchanged; nothing woke; active unit refused; rebind after Awake refreshes the live targeting filter); I10 units get the right side layers and masks]

## Battle deployment (Phase 4)

`BattleManager2D` (nine partials since Phase 7) orchestrates battles. Phase 4 ran at most one battle per Play Mode session and had no ending; Phase 5 adds endings and repeatable battles, one at a time ("Battle ending, cleanup and return (Phase 5)" below); Phase 7 adds unit retreat and the modifier delivery path ("Unit retreat (Phase 7)" below); Phase 8 adds battle-start modifier lists ("Battle-start modifiers (Phase 8)" below); Phase 9 adds sieges, whose sides are an army and a garrison ("Sieges (Phase 9)" below). Roadmap departures kept from I10 (Roadmap §5 notes are guidance): spawn spacing uses `EntrySpawnGap` plus same-step reservations instead of a `minSpawnSpacing` setting; a blocked side's spawn credit is held at 1; a unit that cannot be deployed stays parked and owned.

**BATTLE-LIFE-01 — Manager service, step order and subscriptions.**
- **Instance.** One scene `BattleManager2D` (order −12000) sets `Instance` in Awake; a second logs an error and disables itself. OnDestroy clears `Instance`.
- **Dependencies.** Arena and map are the assigned ones, else `BattleArena2D.Instance` and `WorldMapController2D.Instance`, read live.
- **Subscriptions.** `ContactReported` on the map and `ArenaRemoved` on the arena. Made lazily in Start, and re-checked at the start of every FixedUpdate: the manager's FixedUpdate runs before the map's, so the subscription exists before the first dispatch. A changed instance is resubscribed; OnDisable unsubscribes.
- **Step.** Each FixedUpdate: re-check subscriptions → if any unit is quarantined, prune and retry it (`BATTLE-UNIT-04`) → then:
  - **with a battle:** prune destroyed corpses → sweep field units and run the probe (`BATTLE-UNIT-03`) → if the result is locked, tick the end delay and, once it has elapsed, run the normal end, and nothing else (`BATTLE-LOCK-02`, `BATTLE-END-04`); otherwise run the end check (`BATTLE-END-01`–`BATTLE-END-03`) and then, each only while the battle is still current and unlocked: tick spawning with `Time.fixedDeltaTime` (`BATTLE-SPAWN-01`; since I17 each spawn queues its battle-start request, `BATTLE-DELIVER-03`) → start latched retreats (`BATTLE-RETREAT-03`; after spawning, so a unit deployed in this step and latched at deployment starts retreating in this step) → apply pending modifier deliveries (`BATTLE-DELIVER-01`);
  - **without a battle (idle):** complete a pending end → remove an orphan arena → run a scheduled post-battle audit (`BATTLE-RECOVER-01`, `BATTLE-AUDIT-01`).
- **State.** `Current` is the running `BattleContext`; it clears at the normal end (after the arena's removal) or at the start of an abort. `IsBattleActive`, `IsResultLocked` and `IsEnding` (true only inside the normal end's cleanup) describe it. `IsRosterInBattle(roster)` is true for either side's roster while `Current` exists, including after the lock: an army roster, or since I18 a besieged garrison (it compares each side's `Roster`). The world map's transfer gate, deployment, capture and settlement faction change call it (`WM-XFER-02`, `WM-DEPLOY-02`, `WM-CAPTURE-01`, `WM-FACTION-01`). Since I13 `HasStartedBattleThisSession` is diagnostic only; an unused `sessionRefusalLogged` field is kept so older tools still compile.
- **Disable.** OnDisable during a battle (not while quitting) warns and aborts it (`BATTLE-ABORT-01`). It then completes any pending end or orphan arena once (`BATTLE-RECOVER-01`) and unsubscribes.
- **Events,** each handler isolated: `BattleStarted` and `UnitSpawned` (Phase 4), `UnitRetreated(side, handle)` (`BATTLE-RETREAT-04`), `ResultLocked(context, result)` (`BATTLE-LOCK-01`), `EndDelayElapsed(context)` (`BATTLE-LOCK-02`), `BattleEnded(context, result)` (`BATTLE-END-04`).

[WM4: the I10 battle checks ran through this component. WM5: I13 tests 2–8 ran through it on the final source. WM7: the I15 and I16 checks ran through it. Second instance and disable-abort WM4-S; the OnDisable recovery WM5-SR; the position of the retreat and delivery ticks WM7-S]

**BATTLE-START-01 — Trigger and refusals.** The `ContactReported` handler acts only when all hold: intent FieldBattle or, since I18, Siege; not preparing; no running battle; no pending end or orphan arena (`BATTLE-RECOVER-01`); map ready and unfrozen; the report is the step's `LastSelectedBattle` (same step, settlement flag, both army ids and settlement id). It then calls `TryStartFieldBattle(report, out error)` or `TryStartSiege(report, out error)` (both public for tools). Each refuses, in order, with nothing changed, no counter and no wait:
1. a battle running or being prepared;
2. an earlier battle's teardown still being recovered (pending end or orphan arena);
3. the map missing, not ready or frozen;
4. map time before the retry time set by an earlier refusal (`BATTLE-START-03`);
5. field: an army pair that is immune after a battle; siege (I18): an army–settlement pair that is immune after a siege (`WM-IMMUNE-01`). Battle selection already skips such pairs, so this covers tools calling the methods directly.

Refusals 1–4 are one shared check (`CanAttemptStart`). There is no session limit since I13: battles repeat, one at a time. A field battle then builds its context (`BATTLE-START-02`), a siege its siege context (`BATTLE-SIEGE-01`), and both create the arena and commit through one shared path (`BATTLE-START-03`). Because this runs inside the map's dispatch, a committed start leaves the map frozen: delivery of the step's remaining reports stops (`WM-CONTACT-05`), and that step's allied interactions and settlement work skip (`WM-TIME-01`). A preparation failure unfreezes the map again in the same call, so dispatch and the rest of the step continue.

[WM4: I10 battles start from the selected FieldBattle pick. WM5: I13 test 8 (20 consecutive battles, ended + aborted = 20). Refusal order and the recovery refusal WM5-S. WM9: sieges start from the Siege pick (I18 checks 2–8; I19 tests 2–6, 8 and 9); I19 test 8 (about 5 field battles and 5 sieges, refused 0). The siege immunity refusal WM9-S]

**BATTLE-START-02 — Pre-arena checks and context (field battles).** Sieges have their own checks (`BATTLE-SIEGE-01`). Nothing changes before these pass. A refusal here increments `TotalStartsRefused` and sets the retry wait, except a missing return room (`BATTLE-START-03`). In order: an arena exists and `CanCreate`; the report is an army FieldBattle report; both armies usable (initialized, not removed, open roster); neither retreating; both `UnitCount` > 0; different factions (`SIDE-01`); side layers resolve (`SIDE-01`); the map has a reserve. Then:
- **Edges** per `BATTLE-EDGE-01`.
- **Queues.** Each side's queue is a snapshot list of its army's army-eligible units in the current roster order. `StartingCount` is its length; the spawn rate is `ComputeSpawnRate(StartingCount)`; the credit cap is 1 + rate × `SpawnCatchUpSeconds`; credit starts at 1. An empty queue on either side refuses.
- **Return placements, last** (I13; the most expensive check, read-only, reserving nothing): `TryFindReturnPlacements(first, second)` must find two separated return spots (`BATTLE-RETURN-01`). If none exist the start is refused with pair immunity instead of the retry wait.
- **Context.** `BattleContext` (kind Field) keeps the pick report, side layers, axis, both sides and the two return placements, mapped to Side A and B.

[WM4-S; I10 units keep their faction and get the right sides. The return-room refusal WM5-SR: not set up in I13]

**BATTLE-EDGE-01 — Axis and edges (field battles).** A siege ignores the contact direction and uses the settlement's authored edges (`BATTLE-SIEGE-01`). From the pick's `PositionB − PositionA` (callback-time army positions after role ordering, `WM-CONTACT-04`, `WM-CLASS-02`):
- zero separation → random axis;
- otherwise, with the normalized separation d: left/right when |dₓ| − |d_y| > `SideAxisTieEpsilon` (default 0.05), bottom/top when < −epsilon, and a random axis within ±epsilon;
- on the chosen axis the army with the lower coordinate enters from Left or Bottom; an exact tie on that axis is random.

`HorizontalAxis` and `AxisChosenRandomly` are recorded on the context. [WM4-S. The I10 handoff also lists "perpendicular edges at large views" as source-reviewed only (WM4-SR) without naming the code path; it is recorded here and in `90-Evidence.md` as named, not interpreted]

**BATTLE-START-03 — Arena, final checks, commit and refusal wait.** Shared by field battles and, since I18, sieges.
- **Arena.** With `preparing` set (so its own rollback is not treated as an abort), call `BattleArena2D.TryCreate(MapPoint, StartingCountA, StartingCountB, all gate edges)`; a siege calls `TryCreateWithBattlefield` with its settlement's prefab and its gate mask (`BATTLE-SIEGE-01`). The arena owns the freeze and camera swap and rolls itself back on failure (`ARENA-LIFE-02`).
- **Final checks.** One-shot `DebugFailNextPreparation`; arena present and rig in battle view; battle-unit container created. A failure, or an exception while the arena exists, calls `Remove()`, restoring the world camera and unfreezing the map (`ARENA-LIFE-03`).
- **Counting.** A refused `TryCreate` and every final-check failure or exception increment `TotalPreparationFailures`, not `TotalStartsRefused`.
- **Refusal wait.** A refusal from the context step on, or any preparation failure, sets the next allowed start to map time + `failedStartRetrySeconds` (1). The wait is global: it applies to every pair. Refusals before the context step set no wait.
- **No return room** (I13, Roadmap §1.3 "Separation"). `TotalStartsRefused` and `TotalNoReturnRoomRefusals` +1. The pair gets `PostBattleImmunitySeconds` of pair immunity (`WM-IMMUNE-01`) and no global wait is set, so other pairs can still start. If the grant is refused (for example with immunity 0), the global wait is set instead.
- **Commit.** Assign the id (sequential from 1) and `StartTime` (`Time.fixedTimeAsDouble`); roll retreat eligibility and freeze the retreat settings on the context (I15, `BATTLE-RETREAT-01`), so a refused or failed start rolls nothing; freeze the battle-start modifier lists onto the sides (I17, `BATTLE-DELIVER-03`), with exactly one freeze per context: the siege freeze for a siege, the field freeze otherwise, so rules edits during the battle change nothing; record the context as `LastCommittedBattle` (diagnostics, I17); set `Current` and `HasStartedBattleThisSession`; `TotalBattlesStarted` +1; clear `DebugForceTimeoutNextCheck`. A post-battle audit still waiting for a battle-free frame is cancelled (`PostBattleAuditsSkipped` +1, `BATTLE-AUDIT-01`). Raise `BattleStarted` (handlers isolated). If a handler aborted the battle, stop; otherwise tick spawning with zero time, so each side's first unit spawns inside the start call.

- **Inside the preparation's exception handling** (I18 fix; rewritten from the M08 note). The commit settings after the arena's creation (id, start time, retreat rolls, battle-start freeze) now run inside the same try as the arena's creation and the final checks. An exception there is logged, removes the arena (restoring the world camera and unfreezing the map) and counts as a preparation failure, with the global retry wait; `Current` is never set. At M08 these steps ran outside that handling, and an exception would have left the arena built and the map frozen with no `Current`. Source-derived residue: whatever the commit had already done before throwing is not rolled back (a battle id consumed, retreat-roll counters raised). The retreat roll (`.Retreat.cs`, read at M09) throws nothing explicitly. A second freeze still throws by design (`BATTLE-DELIVER-03`), but exactly one freeze is called. A throw during commit is untested (I18).

[WM4: I10 a failed preparation restores the map and camera; first unit immediately. Counters and the global wait WM4-S; the no-return-room path WM5-SR. WM8: I17 "editing the rules mid-battle changed nothing in that battle" (freeze at commit). The commit inside the exception handling WM9-S]

**BATTLE-SPAWN-01 — Cadence and blocking.** Each tick handles side A then side B, sharing one reservation list that is cleared every tick.
- An empty queue sets credit 0.
- Credit += rate × dt, capped at `CreditCap`. While credit ≥ 1 and units remain queued, the front unit is tried: Spawned uses one credit; Dropped consumes the unit and keeps the credit; Blocked (or `DebugHold`) holds credit at min(credit, 1), counts the block and ends that side's tick.
- **Result.** The first unit at commit, then floor(2 + N/20) per second, where N is the side's starting count (defaults: 20 → 3/s, 80 → 6/s, 200 → 12/s). A siege defender uses `ComputeSiegeDefenderSpawnRate(N)` instead, twice that by default (`BATTLE-SIEGE-01`); each side's credit cap uses its own rate. After a block the next unit comes as soon as room appears and the rest follow at the normal cadence (no burst). Credit above 1 is limited to `SpawnCatchUpSeconds` worth of the rate.
- **Spacing.** `TryFindEntryPoint` plus the same-step reservations of both sides (`ARENA-ENTRY-01`).
- **Diagnostics** per side: spawned, dead, escaped, dropped; blocked flag, steps and episodes; maximum per step and per sliding second; first and last spawn time; a log line when a side finishes.

[WM4: I10 80 vs 20 at 6/s vs 3/s, first unit immediately, no overlaps, no burst after Hold; about 13.2 s for 80 units. The real "entry strip full" block uses the same credit handling as Hold but was not separately named. WM9: I18 check 3 (a siege of 80 vs 80 spawns the attacker at 6/s and the defender at 12/s)]

**BATTLE-SPAWN-02 — Deploying one unit, in one call.** For the front queued handle:
1. **Validate, changing nothing.** First, Blocked when there is no arena (or it has none) or no reserve. Then Dropped when: the handle is destroyed; it is no longer owned by this side's roster (the army, or since I18 the besieged garrison); it is not parked (`IsParked` false or active in the hierarchy); it is settlement-only on an army side (since I18 a garrison side deploys settlement-only defenders, Roadmap §1.2 "defend in sieges and can die there"); its faction differs from the side's; the root lacks `UnitMotor2D`, `Rigidbody2D`, `CircleCollider2D`, `UnitHealth2D` or `UnitDeath2D`; the circle is a trigger; the root lacks `UnitDestinationOverride2D`; the world radius is not positive and finite. Last, Blocked when `TryFindEntryPoint(edge, radius, reservations)` finds no point.
2. **Prepare while parked:** `UnitSideBinder2D.TryBind` (`SIDE-02`) → `UnitMotor2D.TryPrepareForDeployment(point, the edge's inward normal)` (`MOVE-04`) → `UnitReserve2D.TryMarkDeploying` (status Deployed, `HasEverActivated`). A failure drops the unit; it stays parked and owned, and the writes already made (side layers and masks, and after a reset its pose and pending reset) remain until its next deployment overwrites them.
3. **Reparent** it, still inactive and keeping its world position, under `Battle Units (active)`: a scene root created lazily at world scale 1, kept after the battle, never the arena Root (`ARENA-BODY-01`).
4. **Activate.** Awake and OnEnable run now; Start runs later, before the unit's first update callback, not inside this call.
5. **Register** with `TryRegisterBody(rigidbody, circle, destination, edge)`; the body starts Entering with an Entry override. A refusal re-parks the unit (`ReturnToReserve(Reserve)`) and drops it.
6. **Track.** Create the `BattleUnit` record (since I12 it also keeps the optional `UnitReactiveTurnBoost2D`, for removal, `BATTLE-UNIT-04`; since I15 its `ActivationFrame`, `Time.frameCount` at this activation, `BATTLE-DELIVER-01`), subscribe to its `Died`, set the body's escape callback, copy its side's retreat-eligibility roll and, if eligible, subscribe to its `HealthChanged` (`BATTLE-RETREAT-02`), then add it to the side's field list.

Steps 3–6 run in one call, so no FixedUpdate sees an active unit without its Entry override. Then the point is reserved and the spawn time recorded. A unit already dead is removed at once (not expected). Otherwise two things run, in this order. First, its side's frozen battle-start list is requested (I17, `BATTLE-DELIVER-03`; callback-free, nothing when the list is empty). Then the deployment retreat check runs (`BATTLE-RETREAT-02`). Both run after the unit is listed, and the battle-start request is queued before any retreat request. Then `UnitSpawned` is raised (handlers isolated).
- **Exceptions.** An exception in steps 5–6 detaches the unit (which also ends its retreat tracking) or unregisters the body, and re-parks the unit before rethrowing. The caller logs it, re-parks a unit still Deployed that is not on the field, and counts it Dropped.
- **Dropped units** log a warning and the queue moves on; a dropped unit is not retried this battle. One that still exists and is owned by its side's roster stays there, parked as Reserve (this path never changes ownership). A dropped siege defender is still in the garrison at the end, so a capture moves it to the pending roster, or kills it if it is settlement-only (`WM-CAPTURE-01`).

[WM4: I10 units deploy and fight with the right faction, layers and masks; I09 units walk in. Exception paths WM4-SR; other drop cases WM4-S. WM7: I15 "the deployment check" (Roadmap §1.5 "including at deployment"); the retreat subscription and activation frame are otherwise covered only through the I15 retreat and modifier checks. WM8: I17 battle-start lists were requested and delivered to deployed units (requested = delivered + dropped per side; the Battle-start audit, which reports any field unit with a nonempty list that was never requested, reported OK). That the battle-start request precedes the retreat request WM8-S. WM9: I18 check 5 (settlement-only defenders deploy); the owner and faction checks against a garrison side ran in every I18 and I19 siege]

**BATTLE-UNIT-01 — Death.** `UnitDeath2D.Died` for a deployed (or Escaping) unit of the current battle → remove it from the field → detach (`BATTLE-UNIT-04` step 1: unsubscribe `Died`, end retreat tracking, drop its pending modifier deliveries, clear the escape callback, unregister the body) → `ReleaseOwnership(Dead)`: the roster loses the unit at once, raises `Changed`, and the army's count drops → the side's `Dead` +1 and its open death batch +1 (`BATTLE-END-01`) → track the GameObject as a corpse.
- `Died` is raised synchronously in the damage path, possibly inside a Clash transaction or XP deferral (`HEALTH-02`, `DEATH-01`). This handler only unregisters, releases ownership and records the batch death; the end check decides at the next manager step.
- A death found after the lock (should not happen: damage is gated, `BATTLE-LOCK-01`) is handled the same way, increments `DeathsAfterLock` and logs an error. The result is unchanged.
- `UnitDeath2D` destroys the GameObject per its settings, after `Died`; the corpse list drops destroyed objects each step. The manager destroys any remaining corpses at the normal end and at an abort (deactivate, then Destroy; harmless if destruction is already scheduled). The sweep also catches a dead unit whose `Died` was missed (`BATTLE-UNIT-03`).
- With destruction on death turned off, a corpse stays in the scene, dead and unregistered, until the battle ends or aborts; until then it may block entry strips (not handled). [WM4-SR]

[Death release WM4-S, covered indirectly by the I13 post-battle audits after battles with deaths (tests 2, 4). WM5: corpse cleanup at the end, I12 mutual elimination with corpse cleanup (before the I12 review fixes) and I13 test 4]

**BATTLE-UNIT-02 — Escape.** A retreating registered unit touching a gate is released by the arena as Escaped (`ARENA-GATE-01`), which calls the body's escape callback (physics trigger phase). Since I15 such a unit is Escaping (`BATTLE-RETREAT-03`). For a unit of the current battle: remove it from the field → shared removal with status Escaped (`BATTLE-UNIT-04`): detach (the body is already released; retreat tracking ends and pending deliveries drop), clear its reactive turn-boost handles, `ReturnToReserve(Escaped)` (Escaping → Escaped: deactivated, moved under the parked-unit root, still owned by its side's roster, an army or in a siege the garrison; not parked, since `IsParked` needs Reserve) → `Escaped` +1 and added to the side's escaped list. If parking fails, the unit is quarantined instead and is neither counted nor listed.
- **Counts at once.** Leaving the field list lowers the side's `FieldCount` and `RemainingCount` in the same callback, before the next end check; the army's roster count is unchanged because the unit stays owned.
- Escapes close at the lock (`BATTLE-LOCK-01`). One reaching the manager after the lock (should not happen) increments `EscapesAfterLock`, logs an error and is handled the same way.
- Escaped units are never queued again (the queue is a start snapshot), and transfers skip them, so they never respawn in that battle. At the normal end and at an abort they are parked as Reserve (`BATTLE-END-04`, `BATTLE-ABORT-01`). The normal end keeps the escaped list as an index for routing; the abort clears it.
- **Retreat sources.** Gameplay retreat comes from the latched low-health trigger (`BATTLE-RETREAT-02`, `BATTLE-RETREAT-03`). `DebugSetRetreating(n, true)` forces retreats through the same start path, ignoring eligibility. Nothing un-retreats a real unit.

[Arena escape of real units WM4: I09 Retreat N, 0 lost (run with the deleted spawner). WM5: I12 escape cleanup (before the I12 review fixes and file split); I13 20-battle totals report escapes after lock 0. WM7: I15 natural retreat, gate escape and escape counts; "no escape after the lock". "Never respawn": the Phase 7 closing check, step 4 (`92-Validation.md` E12; reported passed after M07)]

**BATTLE-UNIT-03 — Sweep, lost units and the Sidestep probe.** Each manager FixedUpdate with a battle, locked or not, checks every field unit before the arena's tracking pass (−900):
- GameObject destroyed → removed and detached, reported lost: `LostUnits` +1, error, `ReleaseOwnership(Discarded)`.
- Health missing or dead → `BATTLE-UNIT-01`.
- Inactive in the hierarchy, or body missing or no longer registered (for example released as lost by the arena) → removed from the field, shared removal as Reserve (`BATTLE-UNIT-04`), `LostUnits` +1, warning. Then, if the unit is now inactive, its combat-lock entry (if any) is released. Parking comes first, so a unit of a locked battle is never active and unlocked at the same time; a unit still active because both parking and quarantine failed keeps its lock until the battle releases it.
- Otherwise the probe runs on the state left by the previous step. It counts Sidestep starts, Sidestep active under any override (`SidestepUnderOverride`, must stay 0) and Sidestep active while the body is Entering (`SidestepWhileEntering`, must stay 0).

[WM4: probe 0 and lost 0 (I09, I10). WM5: lost 0 in the I13 20-battle run. Lost-unit paths WM4-SR]

**BATTLE-ABORT-01 — Abort.** Ends a battle with no result, whether it is running or locked (Roadmap §1.3 "Failure or abort").
- **Triggers:** `AbortBattle()`; the arena removed by anything else (arena panel, or an external unfreeze that aborts the arena, `ARENA-LIFE-03`); the manager disabled during a battle. `AbortBattle` returns false with no battle or during the normal end. It removes the arena if present, and `ArenaRemoved` runs the abort. If the battle is still current afterwards (no arena, or `Remove()` refused because the arena was busy), it runs the abort directly. Arena removal during preparation, during the normal end (`IsEnding`), or with no current battle (recovery, `BATTLE-RECOVER-01`) is not an abort.
- **Order.** `ArenaRemoved` is raised after teardown has released every body and unfrozen the map, so the abort runs after the map unfreezes, in the same call. The abort then:
  1. clears `Current`, marks the context `WasAborted`, `TotalAborts` +1, clears `DebugForceTimeoutNextCheck`, drops every pending modifier delivery (I15, `BATTLE-DELIVER-02`);
  2. removes flying battle projectiles (`PROJECTILE-01`);
  3. per side, empties the field through the shared removal (living units, Escaping ones included → parked Reserve; dead → `BATTLE-UNIT-01`; destroyed → lost), parks escaped units as Reserve, then clears the escaped list (`BATTLE-UNIT-04`);
  4. destroys tracked corpses;
  5. returns the armies (`BATTLE-RETURN-01`): armies left with no unit are removed, and when both armies remain they are placed apart and get pair immunity; no retreat flag is set and there is no routing. For a siege (I19) the outcome is set to Aborted and the siege return runs instead: an empty attacker is removed, a surviving one is placed away from the settlement with army–settlement immunity, and there is no capture (`BATTLE-SIEGE-02`). Every living defender, escaped ones included, was parked back in the garrison in step 3;
  6. releases the combat lock last, on every exit.
- **Afterwards.** If the arena still exists (it was busy), it is an orphan arena, removed at the next idle manager step (`BATTLE-RECOVER-01`). A post-battle audit is scheduled (`BATTLE-AUDIT-01`). There is no result and no `BattleEnded`. Queued units were never touched and stay Reserve. Per the source comment, a motor Disabled by the lock stays Disabled while parked and `MOVE-04` resets it to Seeking at the next deployment.
- **Beyond the roadmap.** Placing the survivors apart with immunity is not in the roadmap's abort notes (§1.3). The I13 handoff records it as a decision: without it the still-touching pair would start a new battle on the next step.

[WM4: I10 abort (audits pass, map unfrozen, one camera). WM5: I12 abort while fighting and during the locked delay (before and after the I12 split); I13 test 5 (abort right after the start, while fighting and during the locked delay: units returned, map unfrozen, one camera, armies placed apart with no white flag, no immediate new battle, audit passing). An abort caused by an outside unfreeze, and the orphan arena, WM5-SR. WM7: I15 abort while units are retreating. WM8: I17 pending battle-start requests dropped at an abort (step 1), with the delivery hold on. WM9: I19 test 9 (siege Aborted, no flags, everyone back with their own side, post-battle audit PASSED); I18 check 8 (siege abort running and locked: the attacker is placed away, no immediate re-siege; on the I18 interim return, which used the same placement and immunity calls)]

## Battle ending, cleanup and return (Phase 5)

Implemented in three chats: I11 (end check, timeout, result lock, end delay, `CombatLock2D`, projectile registry), I12 (normal end, shared removal, quarantine, `BattleEnded`, the file split) and I13 (routing, separation, pair immunity, repeatable battles, recovery, post-battle audit). The rules live in `BattleManager2D.Ending.cs`, `.Units.cs` and `.Routing.cs`. Tuning is in `BattleRules2D`: `maxBattleSeconds` 0 (off), `endDelaySeconds` 1, `postBattleImmunitySeconds` 1.

**BATTLE-END-01 — Death batches.** A death batch is every death recorded between two consecutive end checks. `RemoveDeadUnit` adds one to the side's `BatchDeaths` (`BATTLE-UNIT-01`). Each end check reads both sides' batch counts and resets them, then decides from field and queue counts read once. The manager runs at −12000, before the scheduler and director (`ORDER-01`), so:
- Clash and modifier DoT deaths (later in the same FixedUpdate), projectile hits (physics callbacks) and Update-driven deaths (normal casts, debug or editor actions) all land in the batch the next check closes;
- an Update death therefore belongs to the most recently completed physics step, whatever the frame rate;
- deaths found by the sweep earlier in the same step join the batch that step's check closes;
- deaths of one batch are simultaneous, so mutual last kills give BothLose (Roadmap §1.3).

The check never runs inside a Clash transaction or XP finalization (Roadmap §2.6).

[WM5: I12 mutual elimination with corpse cleanup (before the I12 review fixes); I13 test 4 (mutual last kills → BothLose). Where Update-driven deaths land WM5-S]

**BATTLE-END-02 — Decision.** Each running step adds one to `CheckedSteps`, then:
- no decision while both sides have remaining units (`RemainingCount` = field + queued; escaped and dropped units are not remaining);
- no decision while both fields are empty and either side still has queued units: spawning continues;
- otherwise the side with more units on the field wins (`Decisive`, one per unit), and equal fields give `BothLose`. The reason is `Elimination`.

A side with no remaining units has an empty field, so a Decisive loser always has field 0 and queue 0, and an elimination BothLose has both fields and both queues empty. The immutable `BattleResult` records outcome, reason, winner (meaningful only when Decisive), both field and queued counts and the final batch's deaths, `LockStep` (= `CheckedSteps`), `LockTime` (`Time.fixedTimeAsDouble`) and `ElapsedSeconds` since `StartTime`. Spawning follows the check, so a unit spawned in the same step counts from the next check.

[WM5: I13 test 2 (80 vs 20 decisive), test 4 (BothLose); I12 normal 80 vs 20 end. The derived consequences WM5-S]

**BATTLE-END-03 — Timeout.** Checked only when no elimination was found: `maxBattleSeconds` > 0 and `Time.fixedTimeAsDouble − StartTime ≥ maxBattleSeconds` give `BothLose` with reason `Timeout` (the winner field is unused). Battles run on scaled fixed time. 0 (the default) or a non-finite value turns the timeout off.
- The one-shot `DebugForceTimeoutNextCheck` forces it at the next check unless an elimination is found first. It is cleared at every commit, abort and normal end, so it never carries into another battle.
- Nobody dies for a timeout and no kill XP is given: the lock and the normal end only park units, and queued units were never activated and stay parked.
- Roadmap §6: test releases with a nonzero timeout, since a blocked spawn or unreachable gates could otherwise stall a battle.

[WM5: I12 forced timeout with units still queued; I13 test 3 (nobody dies; both armies keep every unit). I11: a blocked spawn stalled until a configured timeout and then cleaned up normally (I11 source)]

**BATTLE-LOCK-01 — Result lock and its effects.** Locking runs inside the end check, in the manager's FixedUpdate before the scheduler and director, never inside a transaction (Roadmap §2.5). In order (closing escapes, each motor and the projectile sweep are each wrapped in their own exception handling, so one failure cannot skip the rest; storing the result and the callback-free lock writes are not wrapped):
1. Store the result on the context (`LockResult` throws if one already exists, so the result never changes) and add one to `TotalResultsLocked`.
2. Close escapes: `BattleArena2D.EscapesAccepted = false` (`ARENA-GATE-01`).
3. Lock every field unit's root GameObject in `CombatLock2D`; the manager tracks the roots it locked. This is callback-free and closes three gates:
   - `UnitHealth2D.ApplyDamage` rejects the damage like an ineligible application (`HEALTH-01`);
   - `ClashDirector2D` accepts no Clash involving a locked unit (`CLASH-05`);
   - `UnitAbilityHolder2D.CanActAsCaster` is false, so no normal or triggered cast (`ABILITY-02`, `TRIGGER-04`).
4. `SetDisabled(true)` on the motor of every living field unit. Movement stops; per the manager's source comment, Sidestep cancels through its own lifecycle and the motor's bonus channel clears (`MOVE-03`, `SIDESTEP-LIFE-02`).
5. Remove flying projectiles whose frozen enemy Clash mask includes either side's Clash layer (`PROJECTILE-01`), counted in `ProjectilesRemovedAtLock`.
6. Drop every pending modifier delivery (I15, `BATTLE-DELIVER-02`). Modifier application is not lock-gated, so this drop is what keeps the retreat modifier and the battle-start lists (I17) out of a locked battle. A request made after this point is refused, because requests need an unlocked battle (`BATTLE-DELIVER-01`).
7. Raise `ResultLocked(context, result)`.

Spawning stops (no spawn tick once locked) and nothing further changes the result. No retreat starts after the lock: the retreat tick runs only while unlocked, the low-health latch ignores a locked battle, and the debug retreat refuses (`BATTLE-RETREAT-02`, `BATTLE-RETREAT-03`). A unit already Escaping stays Escaping, cannot escape (step 2) and is parked as Reserve at the normal end.
- **Not gated:** `UnitDeath2D.ForceDeath` and maximum-health clamp deaths (`HEALTH-04`); modifier application and other scheduled modifier work, whose DoT damage the health gate rejects; regeneration healing, by design (`HEALTH-05`, `WM-REGEN-01`, Roadmap §1.4). The battle debug tools refuse to kill (ForceDeath bypasses the gate) or eject a unit after the lock.
- **Diagnostics:** `DeathsAfterLock` and `EscapesAfterLock` (both must stay 0), `CombatLock2D.LockedCount` and `BlockedDamageCount`.
- **Only `BattleManager2D` locks and unlocks** `CombatLock2D` entries. Every path releases the lock after parking (`BATTLE-END-04`, `BATTLE-ABORT-01`, `BATTLE-UNIT-03`, `BATTLE-UNIT-04`).

[WM5: I11 result-lock test (no deaths, Clashes, casts or trigger warnings; no projectiles in flight) and the I11 DoT gate after the lock (blocked damage rose, no deaths after the lock), both on the I11 source and not rerun on the final source; I13 20-battle totals, deaths/escapes after lock 0/0. Effect order and the ungated paths WM5-S. Healing after the lock WM6: I14 extra check (a deployed unit reached 100% inside a long locked delay). WM7: I15 "no escape after the lock". The delivery drop at the lock WM8: I17 pending battle-start requests dropped at the lock, with the delivery hold on (WM7-S at M07)]

**BATTLE-LOCK-02 — End delay.** Once a result is locked, each later manager step compares `Time.fixedTimeAsDouble − LockTime` (plus 1e-6) with `endDelaySeconds` (1; non-finite → 1). The final locked view stays on screen meanwhile, and the sweep keeps running. When the delay is reached, the context's `EndDelayElapsed` is set and `EndDelayElapsed(context)` is raised while the battle is still current and locked. The normal end then runs in the same step, unless a handler aborted the battle (`BATTLE-END-04`).

[WM5: I12 and I13 aborts during the locked delay. The delay arithmetic WM5-S]

**BATTLE-END-04 — Normal end.** Runs in one manager FixedUpdate, never inside a transaction, with `IsEnding` set so the arena's removal is not an abort. It first drops any pending modifier delivery (I15; normally none since the lock), then, in order:
1. remove flying battle projectiles again (`ProjectilesRemovedAtEnd`; nothing should have launched since the lock);
2. per side, every field unit through the shared removal as Reserve (`BATTLE-UNIT-04`): living units, Escaping ones included, are parked, a dead one is handled as a death, a destroyed one is reported lost;
3. then that side's escaped units are parked as Reserve, and the escaped list is kept as the routing index;
4. destroy tracked corpses;
5. routing, separation and pair immunity, while the map is still frozen (`BATTLE-ROUTE-01`, `BATTLE-RETURN-01`); for a siege, the outcome table, including a capture on an attacker win (`BATTLE-SIEGE-02`);
6. release the combat lock;
7. remove the arena (world camera back, map unfrozen; `ArenaRemoved` is ignored);
8. clear `Current`.

Steps 6–8 run on every exit. Queued units were never activated and stay parked as they were. HP, XP, level and abilities are untouched (`BATTLE-UNIT-04`).
- **Completeness check.** The end counts only when no arena exists and the camera rig is not in battle view. Then the context is marked ended (`IsEnded`, `EndTime`), `TotalBattlesEnded` +1, `BattleEnded(context, result)` is raised with the locked result, and a post-battle audit is scheduled (`BATTLE-AUDIT-01`).
- **Incomplete teardown.** Otherwise `TotalEndFailures` and `CleanupProblems` +1, and the end becomes pending, with no `BattleEnded` yet. Units are already parked, the armies returned and the lock released (`BATTLE-RECOVER-01`).
- **Diagnostics.** On the context: `UnitsParkedAtEnd`, `EscapedParkedAtEnd`, `CorpsesRemovedAtEnd`, `ProjectilesRemovedAtEnd`. On the manager: `TotalBattlesEnded`, `ProjectilesRemovedAtCleanup`, `CleanupProblems` (must stay 0), `TotalEndFailures`. `BattleEnded` is never raised for an abort.

[WM5: I13 tests 2–4 and 8 (`BattleEnded` fires once, map unfrozen, one camera, audit passing, cleanup problems 0); I12 normal 80 vs 20 end and cleanup (before and after the split); I13 test 9 (no visible hitch at the end, with the audit off)]

**BATTLE-UNIT-04 — Shared removal and quarantine.** The normal end, the abort, escapes and the sweep remove a unit through one path:
1. Detach: unsubscribe `Died`, end retreat tracking (unsubscribe `HealthChanged`, I15), drop the unit's pending modifier deliveries (I15, `BATTLE-DELIVER-02`), clear the escape callback, and unregister the body unless the arena already released it (`ARENA-BODY-01`). Every way off the field (death, escape, sweep, end, abort) detaches.
2. A unit no longer Deployed, Escaping (I15) or Escaped is left alone, so removing twice is harmless. A destroyed one is reported lost (`ReleaseOwnership(Discarded)`).
3. Clear battle-only effects that survive deactivation: the reactive turn-boost handles (`ClearOwnedModifiers`, `REACTIVE-03`), normally while the unit is still active. Framework modifiers, including a delivered retreat modifier and, since I17, a delivered battle-start list, and pending triggered casts are cleaned up when parking disables the modifier runtime (`MOD-REMOVE-01`, `TRIGGER-06`); no `ClearAll` is called. Vulnerable has no clear API and expires.
4. `UnitReserve2D.ReturnToReserve(unit, Reserve | Escaped)`: deactivate, move under the parked root, set the status. HP, XP, level, abilities and ownership are untouched (Roadmap §2.1); from then on regeneration raises HP while the unit is inactive (`WM-REGEN-02`).

Emptying a field (end and abort) copies the list and isolates each unit, so one failure cannot leave the rest on the field; leftover records are logged and dropped.
- **Quarantine.** When parking fails (no reserve, `ReturnToReserve` refused or threw, or the one-shot `DebugFailNextPark`), the unit is deactivated in place and listed in `QuarantinedUnits`, and `CleanupProblems` +1. Its ownership and status do not change, so the unit audit reports it. Since I13, an escaped unit that cannot be parked at the end or abort is quarantined the same way.
- **Retry (I13).** Every manager FixedUpdate, before the battle branch, the quarantine list is pruned:
  - entries resolved elsewhere (parked again, Dead or Discarded) drop out;
  - a destroyed entry still owned is reported lost (Discarded), never silently;
  - every other entry (still owned and Deployed, Escaping or Escaped) is parked again as Reserve, unless retries are paused (`DebugPauseQuarantineRetry`), there is no reserve, the normal end is running, or the unit is on the current battle's field. On success the entry drops out, `QuarantineRecoveries` +1, the unit's combat-lock entry is released if it is inactive, and the recovery is logged. Failed retries are silent; the quarantine itself was logged.

[WM5: I13 test 7 ("Fail next unit park": the next step parks the unit on retry and `QuarantinedCount` is 0); quarantined 0 and cleanup problems 0 in the 20-battle run; HP, XP and level kept (I12 survivor identity check, I13 test 2). The reactive clear and the destroyed-entry paths WM5-S. WM7: I15 retreat modifier "removed when the unit leaves". WM8: I17 parked units carried 0 modifier instances after a battle. A quarantined Escaping unit WM7-S]

**BATTLE-ROUTE-01 — Field routing.** Run by the normal end only, after parking and corpse removal, while the map is still frozen. A siege's normal end runs the siege routing instead (`BATTLE-SIEGE-02`). In field routing units never change roster: every living unit (field, queued, escaped, dropped or quarantined) is still owned by its army, and the dead were released at death. Per side, for an army that still exists:
- `UnitsAfterBattle` = its unit count;
- **Decisive:** the loser becomes retreating if it still has units; the winner's flag is left as it is;
- **BothLose** (elimination or timeout): every army that still has units becomes retreating;
- `RetreatingAfterBattle` records the flag.

Then the armies are returned (`BATTLE-RETURN-01`), which removes armies with no unit left, and `IsRouted` is set. With the decision rules (`BATTLE-END-02`) this matches the roadmap's routing table: a Decisive loser and an elimination BothLose side keep only units that never reached the field or left it alive (escapees, dropped and quarantined units), and a timeout keeps every living unit. Field routing never clears a flag and never moves units, so no field army ever holds more than it did before the battle. The siege capture is the only routing path that moves units (`WM-CAPTURE-01`).
- **Recovery timer (I16).** `SetRetreating(true)` starts the army's recovery timer at the current MapTime (`WM-RETREAT-01`). The map is frozen here, so the 20 s count from the unfreeze. The return summary in the log reports each retreating army's time left.

[WM5: I13 test 2 (loser removed; winner stays ordinary and keeps every unit, including queued ones), test 3 (timeout: both keep every unit and show the white flag), test 4 (empty armies removed). A loser left with escapees or dropped units WM5-S. WM7: I16 test 2 (the loser recovers 20 map s after the unfreeze), test 3 (both timeout armies recover in the same step)]

**BATTLE-RETURN-01 — Return spots and separation.** For field battles; a siege searches one return spot for its attacker away from the settlement and returns it through the siege return (`BATTLE-SIEGE-01`, `BATTLE-SIEGE-02`); `ReturnArmies` branches to it for a siege abort.
- **Search.** `TryFindReturnPlacements(first, second)` is read-only and reserves nothing. It tries spots on a line through the two armies' midpoint: along the line between them first, then rotated by 20°, −20°, 40°, −40°, 60°, −60°, 90° and −90°, at four growing distances; every angle is tried at one distance before the next distance. The first half-distance is `ArmyRadius + SpawnMargin/2 + 0.05` and each further step adds `ArmyRadius + SpawnMargin`, so the two centres start at least 2·radius + margin + 0.1 apart. Coincident armies use a random axis. Each army's placement points away from the other, nudged off the axes.
- **Free spot.** `IsReturnSpotFree` requires: inside the bounds by radius + margin; no WM collider (triggers included) within that clearance except the two armies themselves; no other registered army closer than clearance + radius, checked from the registry because frozen armies are not simulated.
- **Before the start** the search is the last pre-arena check. No spots → no battle, and the pair gets immunity (`BATTLE-START-02`, `BATTLE-START-03`).
- **At the end or abort** (each step isolated):
  1. each live army with no unit left is removed through `DestroyArmy`, never with discard (`WM-REMOVE-01`), counted in `TotalArmiesRemovedAfterBattle`;
  2. `ApplyBattleReturn`: with one or no surviving army nothing moves; otherwise the armies take the planned spots if both are still free, else a new search's spots. With no room both stay in place pointing apart (`ReturnSeparated` false, `TotalReturnsWithoutSeparation` +1, warning; physics separates them and the immunity covers the overlap). `Army2D.PlaceAt` moves them; it works while frozen and leaves the retreat flag and roster alone;
  3. if both armies remain, grant pair immunity for `PostBattleImmunitySeconds` (`WM-IMMUNE-01`; `ImmunityGranted`).

  Problems count as cleanup problems only at the normal end. With no ready map nothing is returned.
- **Diagnostics.** Context: `HasReturnPlacements`, `ReturnPlacementA`/`B`, `ReturnSeparated`, `ImmunityGranted`, `ReturnNote`, `WasAborted`, `IsRouted`. Side: `UnitsAfterBattle` (−1 until the end or abort), `RetreatingAfterBattle`, `ArmyRemovedAfterBattle`. Map: `TotalBattleReturns`, `TotalReturnsWithoutRoom`.

[WM5: I13 test 2 (the lone winner reappears just apart from the battle spot, consistent with a lone survivor left in place), tests 3 and 5 (two surviving armies placed apart, no new battle between them). The pre-start refusal WM5-SR (not set up); the search geometry and the end-time no-room fallback WM5-S]

**BATTLE-RECOVER-01 — Recovery.** Each idle manager FixedUpdate (no current battle):
- **Pending end.** A normal end whose teardown did not complete is retried: remove the arena if it exists; with no arena, exit a battle view left behind; with no arena, unfreeze a map still frozen, with a warning (the arena owned the freeze, so a leftover freeze is this battle's). Once torn down: `TotalEndRecoveries` +1, the context is marked ended, `TotalBattlesEnded` +1, `BattleEnded` is raised late, and the audit is scheduled. Retries log on the first attempt and every 50th. While an end is pending, nothing else in the idle step runs.
- **Orphan arena.** An arena that an abort could not remove (busy) is removed; this is not an abort, since no battle is current. The I13 handoff notes that from source this case cannot actually arise.
- **Quarantine retry:** `BATTLE-UNIT-04`.
- **Starts blocked.** No battle starts while a pending end or orphan arena exists (`BATTLE-START-01`).
- **Sieges (I19, source-derived).** Routing already ran, so a capture, the retreat flag and the siege return are already applied when a siege's end becomes pending; the retry only completes the teardown. The map stays frozen meanwhile, so the new owner's overflow and the escaped defenders' placement wait. The queued `SettlementCaptured` is raised at the controller's next step anyway, so it can precede the delayed `BattleEnded` (`WM-CAPTURE-01`).
- **OnDisable** completes a pending end and an orphan arena once, because FixedUpdate stops while the manager is disabled.

[WM5: I13 test 7 ("Skip arena removal at next end": the next step recovers and `BattleEnded` fires once). Orphan-arena removal, the OnDisable recovery and a real teardown refusal or exception WM5-SR]

**BATTLE-AUDIT-01 — Post-battle audit.** With `auditAfterEachBattle` (default on), it runs one rendered frame after each end or abort, in an idle manager step with no pending end or orphan arena. It is read-only. Reported problems:
- `AuditArena` fails (`ARENA-AUDIT-01`), or there is no arena component;
- no ready map, the map still frozen, or `AuditUnits` fails (Editor and development builds only, `WM-AUDIT-01`);
- a combat-lock entry left (`CombatLock2D.LockedCount`, or roots the manager still tracks);
- any projectile in flight (`AbilityProjectile2D.InFlightCount`, which counts every projectile, not only battle ones);
- tracked corpses still present, quarantined units, or children left under `Battle Units (active)`.

Results go to `PostBattleAudits`, `PostBattleAuditFailures`, `LastPostBattleAudit` and a log line with the totals; since I19 the totals also give the manager's siege captures and refusals and the map's pending-retreat unit count, and `AuditUnits` covers pending retreat rosters (`WM-AUDIT-01`). A battle that starts first cancels a scheduled audit (`PostBattleAuditsSkipped` +1). The audit is heavy, so turn it off when measuring hitches. The I13 context menu "Audit after next frame" schedules one by hand.

[WM5: passed in I13 tests 2–5 and 0 failures in the 20-battle run; test 9 ran with it off. WM9: the I19 handoff reports it PASSED with each I19 test; I18 check 8]

## Unit retreat (Phase 7)

Implemented in I15 (`BattleManager2D.Retreat.cs`, `BattleManager2D.Delivery.cs`; Escaping in `UnitReserve2D`). Roadmap §1.5. Settings in `BattleRules2D`: `escapeChance` 0.4, `escapeHealthFraction` 0.3, `retreatModifier` (empty = none); each is clamped to 0–1 and a non-finite value falls back to its default. The manager's `logRetreats` (off by default) logs each retreat start and delivery; rejected modifier entries are always logged. Roadmap §5 suggests one delivery path for the retreat modifier and Phase 8's battle-start lists; I15 built it and I17 uses it for those lists (`BATTLE-DELIVER-03`).

**BATTLE-RETREAT-01 — Eligibility, rolled at commit.**
- **When.** At commit, after every pre-arena check, the arena's creation and the final checks, after the id and `StartTime` are set and before `Current`, `BattleStarted` and the first spawn (`BATTLE-START-03`). A refused or failed start rolls nothing and changes nothing; the one exception is an exception thrown later in the commit, which leaves the roll counters raised (`BATTLE-START-03`).
- **Frozen settings.** `EscapeChance`, `EscapeHealthFraction` and `RetreatModifier` are copied onto the context (`RetreatChance`, `RetreatHealthFraction`, `RetreatModifier`; `RetreatRolled` true). Rules edits during a battle take effect in the next one. Without rules the defaults 0.4, 0.3 and none apply.
- **Who rolls.** Every handle in each side's spawn queue, once. The queue holds every participant at that point (all are still queued). An army side's queue is army-eligible only; since I18 a siege defender's queue is the whole garrison, so the explicit settlement-only skip now matters: settlement-only defenders never roll and are never eligible (Roadmap §1.5).
- **Roll.** Chance ≥ 1 → always eligible; 0 → never; otherwise `UnityEngine.Random.value < chance`.
- **Storage.** Eligible handles go into the side's `RetreatEligibleUnits`, an index keyed by handle, not an owner. A dropped or never-spawned unit keeps its roll unused. Every battle rolls anew; nothing carries into the next one. Counters: `TotalRetreatRolls`, `TotalRetreatEligible`; per side `RetreatEligibleCount`.

[WM7: I15 30% check ("ineligible units don't" retreat) and natural retreat. The roll arithmetic and the frozen settings WM7-S. The settlement-only skip WM9: I18 check 5 (settlement-only defenders never retreat)]

**BATTLE-RETREAT-02 — Low-health latch.**
- **Subscription.** At spawn step 6 (`BATTLE-SPAWN-02`) the `BattleUnit` copies its side's roll into `RetreatEligible`. Only an eligible unit subscribes to its `UnitHealth2D.HealthChanged`. Detach unsubscribes (`BATTLE-UNIT-04`).
- **Handler, record only.** `HealthChanged` is synchronous and can run inside a Clash transaction, a DoT tick, a projectile hit, a regeneration heal or a debug health change; maximum-only changes raise it too (`HEALTH-02`, `HEALTH-04`, `HEALTH-05`). The handler sets `RetreatTriggered` when the unit is eligible, not latched and not retreating, its battle is current and unlocked, and the event's own values give 0 < current < fraction × maximum, with the maximum positive and finite. It reads no stats, so it raises no notification inside the callback, and it changes nothing else.
- **Deployment check.** In the spawn call, after the unit is listed and only if it is alive, the manager reads `MaximumHealth` (which can notify, `STAT-01`), revalidates (still alive, battle current and unlocked, still on the field) and latches by the same comparison with `CurrentHealth`. It runs inside the spawn call because the first spawn of a battle runs inside the map's FixedUpdate and the regeneration tick runs later in that step, so a check deferred to the next manager step could miss a unit deployed below the threshold (source comment).
- **Strict threshold.** Below, not at: with 0.3, health at exactly 30% of the maximum does not latch.
- **Without damage** (source-derived). A maximum-only change raises `HealthChanged` too (`HEALTH-04`), and the handler compares the event's values. So an eligible unit whose maximum rises in battle, for example on a level-up from kill XP, latches once its unchanged health falls below 30% of the new maximum. Untested.
- **Never cleared.** Nothing resets `RetreatTriggered`, and nothing reads health again once it is set, so a later heal does not cancel the retreat (Roadmap §1.5 "stays retreating even if healed"). An ineligible unit never subscribes and never latches.

[WM7: I15 30% threshold (31% does not retreat, 20% does, ineligible units don't), "healing never cancels a retreat", "the deployment check". Exactly 30% and latches set from inside a transaction WM7-S]

**BATTLE-RETREAT-03 — Retreat start.**
- **When.** In the manager's FixedUpdate, after spawning, only while the battle is current and unlocked and the arena exists (`BATTLE-LIFE-01`); never inside a Clash transaction. Per side it lists only latched units that are not retreating and have not failed to start, so the per-step cost is proportional to the field, not field × field. A unit that left the field through an earlier callback is skipped.
- **Timing** (source-derived). A latch set by damage in step N (Clash and DoT run after the manager in that step, projectile hits in physics callbacks, normal casts in Update) starts in step N + 1, after that step's end check. If that check locks the result, no retreat starts. A latch from a deployment in the manager's step starts in the same step; one from the commit's first spawn, inside the map's dispatch, starts in the next manager step.
- **`TryStartRetreat`, in order:**
  1. Refuse with no running, unlocked battle, an already retreating unit, or no arena or an unregistered body (not marked failed; the sweep removes such a unit, `BATTLE-UNIT-03`). Since I18, refuse a settlement-only unit, even when the start is forced by the debug tool: it sets `RetreatStartFailed` (so it is not retried) without counting a start failure. Settlement-only defenders live or die with their settlement (Roadmap §1.2, §1.5; I18 decision).
  2. `UnitReserve2D.TryMarkEscaping` (Deployed → Escaping, callback-free, `WM-UNIT-01`). A refusal marks `RetreatStartFailed`, adds one to `TotalRetreatStartFailures` and warns; the unit is never retried.
  3. Latch `IsRetreating` (and `RetreatTriggered`) and record `RetreatStartTime` (`Time.fixedTimeAsDouble`) before any callback, so a reentrant path cannot start it twice. Side `Retreated` +1, `TotalRetreatsStarted` +1 (and `TotalDebugRetreats` when forced).
  4. Steering: `BattleArena2D.SetBodyRetreating(body, true)` (`ARENA-GATE-01`). An Inside body gets its Retreat override toward the nearest gate edge at once, which raises `Destination.Changed`, so an active Sidestep cancels through its own lifecycle (`SIDESTEP-LIFE-02`). An Entering body keeps its Entry override and gets the Retreat override on reaching Inside (`ARENA-BODY-02`). A refusal or exception is logged; the unit stays Escaping without steering, and with no override nothing keeps it from Sidestepping (source; untested).
  5. Request the retreat modifier once (`BATTLE-DELIVER-01`), if the context has one and the unit is still on the field.
  6. Raise `UnitRetreated(side, handle)` if the battle is still current (`BATTLE-RETREAT-04`).
- **Debug.** `DebugSetRetreating(maxCount, true)` forces up to `maxCount` field units with registered bodies through this same path, ignoring eligibility (but not the settlement-only refusal in step 1); it refuses after the lock. `DebugSetRetreating(_, false)` logs a warning and does nothing: the retreat is latched.

[WM7: I15 natural retreat and gate escape; the retreat modifier applied once. The refusal paths (Escaping refused, steering refused) and the start timing WM7-S. WM9: I18 check 5 (settlement-only defenders never retreat, even when "Retreat N" forces it)]

**BATTLE-RETREAT-04 — Retreating units, escape and `UnitRetreated`.**
- **Still fighting.** Nothing in the retreat path touches Clash acceptance, Dodge, casting, targeting or targetability (Roadmap §1.5): an Escaping unit Clashes, Dodges, casts and is targeted like any field unit. It does not Sidestep, because Sidestep neither starts nor continues under its Entry or Retreat override (`SIDESTEP-GATE-02`, `SIDESTEP-LIFE-02`), and once Inside its motor steers to the gate instead of the target (`MOVE-01`).
- **Latched for the battle.** No code clears `IsRetreating`; the arena's flag is cleared only by unregistration and teardown. Regeneration heals an Escaping unit like any other (`WM-REGEN-02`) without cancelling the retreat.
- **Escape.** A retreating body touching a gate escapes in any arena state, Entering included (`ARENA-GATE-01`), and becomes Escaped (`BATTLE-UNIT-02`). Source-derived consequence: a unit latched at deployment starts retreating while still Entering and crosses the gate strip just inside its own edge on the way in, so it may escape almost at once (Roadmap §5 "Expected behaviour").
- **Counts.** An Escaping unit on the field counts as remaining (`BATTLE-END-02`); once escaped it does not.
- **End, lock and abort.** After the lock it cannot escape and no new retreat starts (`BATTLE-LOCK-01`). At the normal end and at an abort an Escaping unit is parked as Reserve with the other field units (`BATTLE-END-04`, `BATTLE-ABORT-01`). An Escaping unit that dies is handled as a death (`BATTLE-UNIT-01`).
- **`UnitRetreated(BattleSide, UnitHandle)`** fires once per unit per battle, when it starts retreating: after its status is Escaping, its body steers to a gate and its modifier request is queued (the modifier itself comes later). It is raised from the manager's FixedUpdate, never inside a Clash transaction, with handlers isolated. It never fires at an escape: an escape changes the side's `Escaped` count and escaped list at once instead. A handler that aborts the battle stops the rest of that step's retreat starts.
- **Diagnostics.** Unit: `RetreatEligible`, `RetreatTriggered`, `IsRetreating`, `RetreatStartTime`, `RetreatModifierApplied`. Side: `RetreatEligibleCount`, `Retreated`, `RetreatModifiersApplied`. Manager: the retreat counters (card) and a retreat line per side in `DescribeStatus`.

[WM7: I15 natural retreat, gate escape and escape counts; "healing never cancels a retreat"; "no escape after the lock, and an abort while units are retreating". "Retreating units keep fighting" and no Sidestep while retreating: the Phase 7 closing check, step 3 (`92-Validation.md` E11; reported passed after M07). `UnitRetreated` timing and the escape-while-Entering consequence WM7-S]

**BATTLE-DELIVER-01 — Modifier delivery: request, wait, apply.**
- **Kinds.** `BattleModifierDeliveryKind {Retreat = 0, BattleStart = 1}`, declared in `BattleManager2D.Delivery.cs`. Retreat is requested by the retreat start (`BATTLE-RETREAT-03`). Since I17, BattleStart is requested by the spawn call with the side's frozen list (`BATTLE-SPAWN-02`, `BATTLE-DELIVER-03`). The two kinds are independent, so one unit can receive both.
- **Request.** Internal `RequestModifierDelivery(unit, kind, definition)` or `(unit, kind, IReadOnlyList<ModifierDefinition2D>)`. It returns false and queues nothing when: the unit is not of the current, unlocked battle; the list is null; this kind was already requested for this `BattleUnit` (one per kind per unit per battle, since a unit deploys at most once per battle); the unit is not on its side's field; or no entry is non-null. Otherwise it copies the non-null entries in order (later edits to the caller's list have no effect), records `Time.frameCount`, marks the kind and adds one to `ModifierDeliveriesRequested`.
- **Wait until started.** Each manager step, after retreat starts, a request waits until `Time.frameCount` > the unit's activation frame (`BATTLE-SPAWN-02`), its GameObject is active and, when its root has an active and enabled `UnitAbilityHolder2D`, that holder's `didStart` is true (the holder grants its starting abilities in Start, `ABILITY-01`). The frame test is what matters for a redeployed unit, which runs no Start again. A request whose unit is no longer on the field, active and alive is dropped (`BATTLE-DELIVER-02`). Requests are processed in queue order, so when two requests for one unit become ready in the same step, the earlier one applies first.
- **Debug hold (I17).** While `DebugHoldModifierDeliveries` is set, the tick applies nothing and requests stay pending. The safety-net drop at the top of the tick and every other drop still run. The flag persists across battles until cleared; clearing it lets the queued requests apply as usual. It exists to test the drops with requests still pending.
- **Apply once.** In the manager's FixedUpdate, before the scheduler and the director, never inside a Clash transaction or protected capture (`STAT-04`). The request is removed before any callback, so it applies at most once. The root must have a `UnitModifierRuntime2D` whose `CanReceiveModifiers` is true; otherwise the request is dropped with a warning. Then `ApplyOrdered(entries, 0, n, unit's combatant, handle.CurrentLevel, DamageOrigin2D.Ability, ModifierScheduler2D.ApplicationTime, results, 0)`: the unit is its own caster at its current, live level (`WM-UNIT-02`), entries apply in list order, and each rechecks eligibility (`MOD-APPLY-01`, `MOD-APPLY-02`). The runtime's convenience `ApplyOrdered` validates the ranges, captures the application context once (reconciling expiry and capturing the recipient level once), then applies entry by entry. If an entry throws, the loop stops and the unreached entries keep their default result, NotProcessed. The delivery logs the exception and counts those entries as rejected ("not processed").
- **Results.** Once `ApplyOrdered` has been called, the request counts as completed even if it threw (`ModifierDeliveriesCompleted` +1); a request dropped for a missing runtime counts as dropped instead. Each rejected entry is logged with its reason and field and never retried (`ModifierEntriesRejected`); applied ones count in `ModifierEntriesApplied`. A Retreat delivery with at least one applied entry sets `RetreatModifierApplied` and adds one to the side's `RetreatModifiersApplied`. A BattleStart delivery (I17) counts as delivered even when every entry was rejected. It records on the unit `BattleStartDelivered`, `BattleStartEntriesApplied`, and the delivery's level, frame, fixed time and holder condition (`BattleStartDeliveryLevel`, `…Frame`, `…Time`, `BattleStartHolderStarted`). It adds to the side's `BattleStartDelivered`, `BattleStartEntriesApplied` and `BattleStartEntriesRejected` (entries − applied), and to `TotalBattleStartDeliveries`. With `logRetreats` on, each delivery logs its level, every entry marked + or − in list order, and the frames since the request and since activation.
- **Frame rate.** The wait is in frames, so the number of fixed steps between activation and delivery depends on frame rate (an open item carried by the I15 and I16 handoffs).

[WM7: I15 "the retreat modifier is applied once". WM8 (I17 battle-start lists, through this path): every delivery came at least one frame after activation and the Battle-start audit reported OK, which checks the frame, the holder's `didStart` and a level ≥ 1 on each delivered field unit; the level at delivery matched the unit's level; the log showed `[+A, +B]` (list order); each unit had exactly its list's instances, plus the retreat modifier when it retreated. The wait with a disabled holder, the missing-runtime drop, an exception, rejected entries and the destroyed-asset log name WM8-S (listed untested by I17). The `ApplyOrdered` behaviour is from the runtime source attached to M08. Frame-rate dependence of the wait is untested]

**BATTLE-DELIVER-02 — Drops and removal.**
- **Drops.** A unit's pending requests are dropped when it leaves the field by any path, through detach (death, escape, sweep, normal end, abort; `BATTLE-UNIT-04`). Every pending request is dropped at the lock (`BATTLE-LOCK-01`), at the start of the normal end (`BATTLE-END-04`) and at an abort (`BATTLE-ABORT-01`); as a safety net the delivery tick also drops them all if its battle is no longer current and unlocked. Drops count in `ModifierDeliveriesDropped`; `PendingModifierDeliveries` is the queue length.
- **By design,** a unit that escapes or dies before it has started never receives the retreat modifier or its battle-start list.
- **The normal end's drop** finds nothing in practice. The lock already dropped every request, and requests are refused while the battle is locked. It stays as a safety net.
- **Not lock-gated.** Modifier application has no lock gate (`BATTLE-LOCK-01`); dropping every request at the lock is what keeps deliveries out of a locked battle.
- **Removal.** A delivered modifier is an ordinary framework instance. Parking deactivates the unit, and the modifier runtime's OnDisable unsubscribes and then calls `ClearAll` (`MOD-REMOVE-01`). A death clears them through `Died`, and so does a `HealthChanged` to zero or below. So a delivered modifier never reaches the map or another battle, and the manager calls no `ClearAll` itself.

[WM7: I15 "the retreat modifier is applied once and removed when the unit leaves". WM8: with the I17 delivery hold, pending requests were dropped on death, at the lock and at an abort (this closes those clauses of the M07 open item); parked units carried 0 modifier instances after a battle, and a re-fight of the same armies delivered fresh lists. Drops at an escape, by the sweep or at the normal end with a request still pending WM8-S. The runtime's OnDisable, `Died` and zero-health cleanup are from the runtime source attached to M08]

## Battle-start modifiers (Phase 8)

Implemented in I17 (`BattleManager2D.Delivery.cs`, with the freeze call in `.Start.cs`, the request in `.Spawning.cs` and the lists in `BattleRules2D`). Roadmap §1.8. Delivery and removal are the shared path from I15, unchanged (`BATTLE-DELIVER-01`, `BATTLE-DELIVER-02`). Roadmap §6: author list durations longer than any battle, since leaving the battle removes them.

**BATTLE-DELIVER-03 — Battle-start lists: settings, freeze, request.**
- **Settings.** `BattleRules2D` has three `ModifierDefinition2D[]` lists, all empty by default: `fieldBattleModifiers`, `townDefenderModifiers` and `castleDefenderModifiers`. `FieldBattleModifiers` returns the live field list. `GetDefenderModifiers(SettlementKind)` returns the castle list for `Castle` and the town list for any other kind. Defender lists are keyed by settlement kind, not by settlement (§1.8 "the town or castle defender modifier list"). OnValidate warns about each empty entry. Callers copy the lists at commit and must not cache the live ones.
- **Freeze at commit.** In the commit step, right after the retreat rolls and before `Current`, `BattleStarted` and the first spawn (`BATTLE-START-03`), the field list is copied in order with null entries removed. The same copy goes to both sides (§1.8 "one list for both sides"). The context records `BattleStartModifiersFrozen` and `BattleStartModifierSource`: "field list", or "field list (empty)" when nothing was left. Without rules the lists are empty. Each side then holds its frozen list (`BattleStartModifiers`, `BattleStartModifierCount`). Editing the rules, or the list arrays, during a battle changes nothing in that battle. The freeze copies asset references, not their values: each entry's values resolve from the asset's contents at application (`MOD-APPLY-02`). With `logRetreats` on, the frozen lists are logged.
- **Siege freeze (called since I18).** Internal `FreezeSiegeBattleStartModifiers(ctx, rules, SettlementKind)` runs at the same commit point instead of the field freeze, for every siege, with the besieged settlement's kind (`BATTLE-SIEGE-01`). It gives the defender side, which is `BattleSides2D.SiegeDefenderSide` (A), the copied defender list for the kind (Roadmap §1.8 "sieges use the defender lists (attackers get none)"). The attacker gets an empty list. The source is "{kind} defender list", with " (empty)" appended when nothing was left. A context accepts one freeze: a second `SetBattleStartModifiers` throws `InvalidOperationException` by design. The commit calls exactly one, and a throw there is now handled as a preparation failure (`BATTLE-START-03`). `BattleKind` has `Field` and, since I18, `Siege`.
- **Request at spawn.** In the spawn call, after the unit is listed and alive, and before its deployment retreat check (`BATTLE-SPAWN-02`), the side's frozen list is requested as kind BattleStart through `RequestModifierDelivery` (`BATTLE-DELIVER-01`). The step is callback-free. Nothing is requested, and no counter changes, when the battle is not current or is locked, or when the side's list is empty. A successful request sets the unit's `BattleStartRequested` and adds one to the side's `BattleStartRequested` and to `TotalBattleStartRequests`.
- **Once per unit per battle.** A unit deploys at most once per battle and each deployment has a new `BattleUnit`, so a list is requested at most once per unit per battle. Parking removes it before the unit can deploy again. When a unit's battle-start and retreat requests are both ready, the battle-start list applies first, because it was queued first (a retreat request is queued at the retreat start, after spawning).
- **Counts.** Per side, requested = delivered + dropped + still pending; nothing is pending after the lock. A drop of a BattleStart request adds to the side's `BattleStartDropped`, and to `ModifierDeliveriesDropped` like every other kind. There is no session total for battle-start drops.
- **Diagnostics.** `DebugAuditBattleStartModifiers(out report)` is read-only. For the running battle it checks, per side: the count identity; no pending request after the lock; every field unit with a nonempty list requested; and each delivered field unit delivered in a later frame than its activation, with its holder started and at level ≥ 1. It also checks that still-queued units that were activated before carry no modifier instances. For the last committed battle, once it is no longer current (ended, aborted, or with its end still pending), it checks that every parked unit it listed (queued, deployed or escaped) carries none. It reads `UnitModifierRuntime2D.ActiveInstanceCount`, a managed count that is safe on parked units. `DebugDescribeBattleStartUnits(max)` lists field units with their activation and delivery frames, level then and now, entries applied and current instance count.

[WM8: I17 "lists are frozen at commit" (editing the rules mid-battle changed nothing in that battle); "follows list order" (`[+A, +B]`); "happens once" (each unit had exactly its list's instances, plus the retreat modifier when it retreated; per side requested equals delivered plus dropped); "never leaks into another battle" (parked units carried 0 instances after a battle; a re-fight of the same armies delivered fresh); "uses the unit's current level" (a unit that levelled up got its new level in the next battle). The siege freeze and its throw on a second freeze, the defender-kind mapping, null-entry removal, the empty-list path and the battle-start-before-retreat order WM8-S. The handoff does not say which sides' units were inspected. The siege freeze WM9: I18 check 6 (the castle defender list reaches defenders only; the Battle-start audit is OK). The town list, and the second-freeze throw, WM9-S]

## Sieges (Phase 9)

Implemented in I18 (start, setup, sides, spawning, retreat and delivery rules: `BattleManager2D.Start.cs`, `.Spawning.cs`, `.Retreat.cs`, `BattleRules2D`, `BattleArena2D`, `Settlement2D`) and I19 (outcomes: `BattleManager2D.Routing.cs`, `BattleTypes2D`, with the capture in `WorldMapController2D.Siege.cs`). Roadmap §1.7. Settings: `BattleRules2D.siegeDefenderSpawnMultiplier` (2, clamped 0.1–10; non-finite → 2), the town and castle defender lists (`BATTLE-DELIVER-03`), and each settlement's siege setup (`WM-SETTLE-01`). Ending, the lock, the end delay, cleanup and the abort are the shared field rules (`BATTLE-END-01`–`BATTLE-END-04`, `BATTLE-LOCK-01`, `BATTLE-LOCK-02`, `BATTLE-ABORT-01`); only routing and return differ.

**BATTLE-SIEGE-01 — Siege start, setup and sides.**
- **Trigger.** A Siege pick (`WM-BATTLE-01`) calls `TryStartSiege(report, out error)` under the shared trigger conditions and refusals 1–4 of `BATTLE-START-01`, then refuses an army–settlement pair that is immune (`WM-IMMUNE-01`). ArmyA is the attacker and `Settlement` the besieged settlement.
- **Pre-arena checks, nothing changed until they pass,** in order:
  1. an arena exists and `CanCreate`;
  2. the report is a settlement contact with intent Siege;
  3. the army is usable (initialized, not removed, roster open);
  4. the settlement is registered (nonzero id, in the registry);
  5. its garrison is open;
  6. a live reclassification is still OrdinaryArmyDefendedEnemySettlement (ordinary army, enemy settlement, garrison > 0; `WM-CLASS-01`), because the report holds classification-time values;
  7. the army has units;
  8. the siege setup: `Settlement2D.TryGetSiegeSetup` (`WM-SETTLE-01`), then `BattleArena2D.ValidateBattlefieldPrefab` (`ARENA-TERRAIN-01`);
  9. side layers resolve (`SIDE-01`), and the map has a reserve.
- **Defender side.** Side A (`BattleSides2D.SiegeDefenderSide`), whatever the factions (`SIDE-01`). It is a garrison side: `Army` null, `Settlement` and `IsGarrison` set, `Roster` the garrison, `OwnerLabel` "garrison S<id>". Its queue is the whole garrison in its current roster order (`armyEligibleOnly: false`), settlement-only units and units above the limit included; `StartingSettlementOnlyCount` counts them. Its rate is `ComputeSiegeDefenderSpawnRate(N)` = max(1, floor(`ComputeSpawnRate(N)` × multiplier + 1e-4)), so 20 → 6/s, 80 → 12/s, 200 → 24/s by default, and its credit cap is 1 + rate × `SpawnCatchUpSeconds`.
- **Attacker side.** Side B: the army's army-eligible units in roster order at the normal rate (`BATTLE-START-02` queues). An empty queue on either side refuses.
- **Return spot, last.** `TryFindSiegeReturnPlacement(army, settlement)` is read-only. It searches rings around the settlement: the first at `WorldRadius` + `ArmyRadius` + `SpawnMargin` + 0.05, each further one `ArmyRadius` + `SpawnMargin` out, four rings in all. The first direction is the side the army came from, then rotations of ±20°, ±40°, ±60°, ±90°, ±120°, ±150° and 180°, every angle at one distance before the next. A spot must pass `IsReturnSpotFree`, ignoring only the attacker. Its direction points away from the settlement, nudged off the axes.
- **Context.** `BattleContext` of kind Siege, with `Settlement`, `SiegeKind`, `SiegeBattlefieldPrefab`, `GateEdges` (the attacker's edge only), `HasSiegeReturnPlacement` and `SiegeReturnPlacement`; `Defender` and `Attacker` resolve the two sides. `HorizontalAxis` is true when the attacker edge is Left or Right; `AxisChosenRandomly` is false. The contact direction is never used: the authored edges and terrain apply to every siege of the settlement (Roadmap §1.7).
- **Refusals.**
  - **Setup.** A missing or invalid siege setup (step 8): `TotalStartsRefused` and `TotalSiegeSetupRefusals` +1, the start message set, a warning only on a new contact, and no global retry wait, so a misconfigured settlement cannot hold up other battles. Known gap (I18): the pair keeps touching and stays a battle candidate, so `TotalStartsRefused` grows on every step it is the selected pick, which is every touching step when it is the only candidate.
  - **No return spot.** `TotalStartsRefused` and `TotalNoReturnRoomRefusals` +1, and the army–settlement pair gets `PostBattleImmunitySeconds` of immunity (`WM-IMMUNE-01`). No global wait is set, unless the grant is refused, in which case it is.
  - **Any other refusal** sets the global retry wait (`BATTLE-START-03`).
- **Arena and commit** (`BATTLE-START-03`). `TryCreateWithBattlefield(MapPoint, StartingCountA, StartingCountB, GateEdges, the settlement's prefab, "siege of S<id> → <prefab>")` (`ARENA-LIFE-02`). The view size follows both sides' starting counts. Then the shared final checks and commit, with the siege freeze: the defender list of the settlement's kind goes to the defender side only (`BATTLE-DELIVER-03`). Defenders enter from the authored defender edge and attackers from the attacker edge.
- **During the siege.**
  - **Retreat gates.** Only on the attacker's edge, for both sides (`ARENA-GATE-01`), so escaping defenders also run to that edge.
  - **Settlement-only defenders** deploy and fight (`BATTLE-SPAWN-02`), never roll retreat eligibility and never retreat, not even when the debug tool forces it (`BATTLE-RETREAT-01`, `BATTLE-RETREAT-03`).
  - **Remaining units.** The end check counts a living settlement-only defender on the field or in the queue like any remaining unit (`BATTLE-END-02`), so the settlement cannot fall while one fights. The I18 handoff records this as a decision.
  - **The garrison is a battle side** until `Current` clears. `IsRosterInBattle` is true for it (`BATTLE-LIFE-01`), so: its faction change is refused (`WM-FACTION-01`); its deployment and overflow are Blocked (`WM-DEPLOY-02`); a capture and allied joins of it are refused; the debug evacuation is refused; gated transfers are refused (`WM-XFER-02`). Recruitment is deferred by the map freeze (`WM-RECRUIT-01`).
- **Diagnostics.** `TotalSiegeSetupRefusals`. The start log names the settlement, kind, gates and terrain, plus each side's owner, rate, edge and settlement-only count. `DescribeStatus` and the debug panel show the siege settlement, kind, gates and battle-start source, and each side's owner and settlement-only count.

[WM9, I18 on the I18 source; I19 did not change the start, spawning, retreat, rules or arena files: check 2 (the defender is Side A for both faction orders); check 3 (80 vs 80: attacker 6/s, defender 12/s); check 4 (authored edges used whatever the contact direction; retreat leaves only through the attacker's edge); check 5 (settlement-only defenders deploy and never retreat, even when forced); check 6 (the castle defender list reaches defenders only; the Battle-start audit is OK); check 7 (an invalid setup starts nothing and other battles still start). I19 test 8 (about 5 sieges, including a castle of 300 against 200 attackers; refused 0, cleanup problems 0, lost 0). WM9-S: the no-return-room refusal and its immunity (I18 untested); the setup refusal's counter growth; the town defender list; the refusal order; rates other than 80 → 12 (for example 300 → 34/s); a settlement destroyed mid-siege (`WM-OWN-02`); a settlement-only defender keeping the defender side alive]

**BATTLE-SIEGE-02 — Siege outcomes, capture and return.**
- **Cleanup first.** The normal end parks every living unit with its own owner before routing (`BATTLE-END-04`): attackers in the army; defenders in the garrison, with field, escaped, queued and dropped units as Reserve and settlement-only ones included. Corpses are destroyed; the dead were released at death. Then, still frozen, with the battle current and `IsEnding` set, the siege routing classifies the locked result (`BattleContext.ClassifySiegeOutcome`):

  | Locked result | `SiegeOutcomeKind` |
  | --- | --- |
  | Decisive, winner `SiegeAttackerSide` (B) | AttackerWins |
  | Decisive, winner the defender (A) | DefenderWins |
  | BothLose, reason Elimination | BothLose |
  | BothLose, reason Timeout | Timeout |
  | No result (abort) | Aborted |

- **Attacker wins.**
  1. Quarantined units of either side are parked again now (unless `DebugPauseQuarantineRetry` is set), because a unit that cannot be parked cannot change owner. Their lock entries are released by the end's `ReleaseLock`, after routing.
  2. `WorldMapController2D.TryApplySiegeCapture` (`WM-CAPTURE-01`): settlement-only defenders still in the garrison die with it; mobile defenders still in it move to a pending retreat roster of their faction (`WM-PENDING-01`); the settlement changes faction; every living attacker joins the garrison; the army is removed.
  3. On success the context records `SiegeCaptured`, `SiegeAttackersGarrisoned`, `SiegeEscapedDefenders`, `SiegeSettlementOnlyKilled` and `SiegePendingRetreatId`. The manager's `TotalSiegeCaptures` +1. The defender side's `UnitsAfterBattle` is the escaped-defender count, and the army gets no flag and no immunity. If part of the army could not join (a bug path), the army is kept, now allied with the settlement, and moved away with no immunity.
  4. `SettlementCaptured` follows at the controller's next step (`WM-TIME-01` step 1).
  5. Overflow for the new owner and the escaped defenders' placement run in the first unfrozen settlement work (`WM-OVERFLOW-01`, `WM-PENDING-01`).
- **A refused capture** is a bug path: every check runs first, so nobody changes owner. `SiegeCaptureError` is set, `TotalSiegeCaptureRefusals` and `CleanupProblems` +1, and an error is logged. The winning attacker stays ordinary and goes through the siege return below; the settlement and garrison are unchanged. A capture that throws is counted the same way. If it threw after the faction change, the return note says the settlement changed hands without a completed capture record. Every unit is still owned, because the pending roster is registered as soon as it holds units.
- **Defender wins, BothLose, Timeout.** A surviving attacking army with units becomes retreating (`SetRetreating(true)`; its timer counts from the unfreeze, `WM-RETREAT-01`). Every living defender is already back in the garrison, and the settlement is unchanged. Then the siege return.
- **Siege return** (`ReturnSiege`; every outcome but a capture, and the abort; each step isolated):
  - The defender side's `UnitsAfterBattle` is the garrison count.
  - An attacking army left with no unit is removed through `DestroyArmy`, never discard; `TotalArmiesRemovedAfterBattle` +1.
  - `ApplySiegeReturn(army, settlement, hasPlan, plan, out note)` moves a surviving army to its planned spot if still free, else to a new search's spot, else leaves it in place pointing away. In that last case `ReturnSeparated` is false and `TotalReturnsWithoutSeparation` and the map's `TotalSiegeReturnsWithoutRoom` +1. It works while frozen.
  - A surviving army gets `PostBattleImmunitySeconds` of army–settlement immunity (`ImmunityGranted`).
  - With no ready map nothing is returned. Problems count as cleanup problems only at the normal end.
- **Against the roadmap table** (§1.7):

  | Outcome | Attacker | Defenders | Settlement |
  | --- | --- | --- | --- |
  | Attacker wins | All living attackers become the garrison; army removed | Mobile defenders still in the garrison (escapees, and dropped or recovered quarantined units) → pending retreat roster of their faction → retreating armies split by 200; settlement-only defenders still in it die | Captured |
  | Defender wins | Escapees and dropped units stay in the army, now retreating; removed if empty; placed away with immunity | All living stay | Unchanged |
  | BothLose | Escapees and dropped units stay, retreating; removed if empty; placed away with immunity | Living escapees and dropped units are back in the garrison | Unchanged |
  | Timeout | Every living attacker, queued ones included, stays, retreating; placed away with immunity | All living stay | Unchanged |
  | Aborted | Everyone back with their own side; no flag; empty attacker removed; placed away with immunity | All living stay | Unchanged |

- **Departures** (handoff decisions). Dropped and recovered quarantined mobile defenders leave with the escapees, beyond §1.7's "escaped mobile defenders". Settlement-only defenders still in a captured garrison die. The roadmap names the separation and immunity for the timeout row only; the source applies them after every non-capture outcome and after an abort.
- **Abort.** The abort's return (`BATTLE-ABORT-01`) sets `SiegeOutcome` to Aborted and runs the siege return with no flag and no capture. It runs after the unfreeze.
- **Diagnostics.**
  - Context: `SiegeOutcome`, `SiegeCaptured`, `SiegeCaptureError`, `SiegeAttackersGarrisoned`, `SiegeEscapedDefenders`, `SiegeSettlementOnlyKilled`, `SiegePendingRetreatId`, plus the shared return fields.
  - Manager: `TotalSiegeCaptures` and `TotalSiegeCaptureRefusals` (session; refusals must stay 0).
  - The end log's return summary names the outcome, the capture, the escaped and killed defenders and the pending roster. The post-battle audit's totals include them (`BATTLE-AUDIT-01`).
  - Context menu "I19: Log siege totals and pending retreat rosters".

[WM9, I19 on the final source, each with a post-battle audit PASSED: test 2 (defender wins: settlement unchanged; attacker removed, or retreating with escapees); test 3 (BothLose: defender escapees back in the garrison; attacker removed or retreating); test 4 (timeout: the attacker keeps every living unit, retreats and is placed away with army–settlement immunity; the garrison keeps every living defender); test 5 (attacker wins: settlement captured, attackers now garrison it, army removed); test 6 (with escaped defenders: P1, then a retreating army of the old faction); test 8 (repeated and large, cleanup problems 0, lost 0); test 9 (abort: Aborted, no flags, everyone back with their own side). I18 check 8 covered the interim siege end and abort, which I19 replaced; only its placement and immunity calls carry over. WM9-S: the refused-capture and throw paths; the quarantine parking before a capture; the bug path that keeps part of the army; settlement-only defenders dying; the in-place fallback with no return spot (I18 untested); the placement and immunity after a defender win or BothLose (tests 2–3 name only the flag and removal)]

Layer names, matrix and controller placement: `SETUP-06` (world map) and `SETUP-07` (battle) in Core. Tuning values: `WorldMapRules2D` and `BattleRules2D` source.

## Script cards

### WorldMapController2D (ten partials) [WM1, WM2, WM4, WM5, WM6, WM7, WM9]
Owns: `Instance` and readiness; army/settlement registries and ids; bounds, MapTime, PhysicsStep and freezing; army creation, placement and removal; the unit reserve and owner-loss handling; deferred-recruitment flush; deployment, overflow and placement; allied interactions and gated transfers; the contact pipeline and battle selection; battle return placement and army-pair battle immunity (since I13); the regeneration service and its step (since I14); retreating-army recovery (since I16); captures, their records and `SettlementCaptured` (since I18); siege return spots and army–settlement immunity (since I18); the siege capture, the pending retreat rosters and their placement (since I19); diagnostics.  
API (core): `Instance`, `IsReady`, `IsFrozen`, `MapTime`, `PhysicsStep`, `Rules`, `Palette`, `MapBounds`, `Armies`, `Settlements`; `SetFrozen(bool)`; `TrySpawnArmy(FactionId, Vector2 position, Vector2 direction, ArmySpawnReason, out Army2D)` and `TrySpawnArmyAtRandom(FactionId, ArmySpawnReason, out Army2D)` (empty debug armies); `IsPlacementFree(Vector2)`; `DestroyArmy(Army2D, bool discardUnits = false)`; `GetFactionColor(FactionId)`.  
API (.Units): `Reserve`, `MaxUnitsPerArmy`, `DefaultRosterOrder`, `AuditUnits(out string)` (since I19 also open pending retreat rosters).  
API (.Deployment): `DeployArmy(Settlement2D, DeployAmount) → DeployResult`, static `ComputeDeployCount(int eligible, DeployAmount, int cap)`, `GetGarrisonLimit(Settlement2D)`, `GetPlacementOuterDistance(float sourceRadius)`, `FindArmyPlacements(Vector2 center, float sourceRadius, int count, List<ArmyPlacement>) → int`, `TotalDeployedArmies`, `TotalOverflowArmies`, `TotalPlacementBlocks`, `TotalEscapedDefenderArmies` (I19). Internal: `TryDeployFromRoster(UnitRoster source, int unitCount, Vector2 origin, float originRadius, Settlement2D ignoreSettlement, ArmySpawnReason reason, out Army2D army, out string error, bool startRetreating = false)` and the same with an `ArmyPlacement` instead of origin and radius → `ArmyDeployAttempt` (Blocked while frozen or while the source is in battle; announces only when full; since I19 `startRetreating`).  
API (.Interactions): `CanTransfer(UnitRoster, UnitRoster, out string)`, `TransferUnits(...)`, `TransferFirstUnits(...)`, `IsRosterInBattle(UnitRoster)` (true while the roster is either side's roster of the running battle, a besieged garrison included, via `BattleManager2D.Instance`), `RecentAlliedInteractions`, `LogAlliedInteractions`, `TotalRetreatTransfers`, `TotalRetreatMerges`, `TotalGarrisonJoins`, `TotalUnitsMovedByInteractions`; also declares `AlliedInteractionRecord` (Step, MapTime, Intent, DonorArmyId, ReceiverArmyId (0 for joins), SettlementId (0 for army interactions), Moved, DonorRemaining, ReceiverCount, DonorRemoved; since I16 `DonorRecoveryRemaining` and `ReceiverRecoveryRemaining`, map seconds, 0 when not retreating, removed or a settlement; a 12-argument constructor, with the older 10-argument one kept and passing 0 for both).  
API (.Retreat, I16): `RetreatRecoverySeconds` (rules value clamped to ≥ 0; 20 without rules), `TotalRetreatRecoveries`, `RecentRetreatRecoveries` (last 16 lines, oldest first), `RetreatingArmyCount` (non-removed retreating armies), `LogRetreatRecovery` (get/set; serialized `logRetreatRecovery`, on by default).  
API (.Contacts): `LastStepContacts`, `RecentNewContacts`, `LastSelectedBattle`, `LastBattleSelectionCandidates`, `TotalPairReports`, `DuplicateCallbacksMerged`, `MaxPairsInOneStep`, `StepsWithMultipleBattleCandidates`, `GetEpisodeCount(WorldMapContactKind)`, `ResetContactStats()` (also resets `ImmuneBattleCandidatesSkipped`), `LogNewContacts`.  
API (.Separation, I13): `IsReturnSpotFree(Vector2, Army2D ignoreA, Army2D ignoreB) → bool`; `TryFindReturnPlacements(Army2D, Army2D, out ArmyPlacement, out ArmyPlacement) → bool`; `ApplyBattleReturn(Army2D, Army2D, bool hasPlan, ArmyPlacement, ArmyPlacement, out string note) → bool` (works while frozen; false when the controller is not ready or two survivors could not be separated; a lone survivor is left in place); `GrantArmyPairImmunity(Army2D, Army2D, float mapSeconds) → bool`; `IsArmyPairImmune(Army2D, Army2D)` and `(int, int)`; `GetArmyPairImmunityRemaining(Army2D, Army2D) → double`; `ImmunePairCount`, `ImmuneBattleCandidatesSkipped`, `TotalImmunityGrants`, `TotalBattleReturns`, `TotalReturnsWithoutRoom`. Since I18: `TryFindSiegeReturnPlacement(Army2D, Settlement2D, out ArmyPlacement) → bool` (read-only); `ApplySiegeReturn(Army2D, Settlement2D, bool hasPlan, ArmyPlacement plan, out string note) → bool` (works while frozen; true with no live army; false when not ready or left in place); `GrantArmySettlementImmunity(Army2D, Settlement2D, float mapSeconds) → bool` (map time, so a grant at a battle's end counts from the unfreeze; extend, never shorten); `IsArmySettlementImmune(Army2D, Settlement2D)` and `(int armyId, int settlementId)`; `GetArmySettlementImmunityRemaining(Army2D, Settlement2D) → double`; `ImmuneArmySettlementCount`, `TotalArmySettlementImmunityGrants`, `TotalSiegeReturns`, `TotalSiegeReturnsWithoutRoom`.  
API (.Capture, I18): `TryCaptureSettlement(Army2D, Settlement2D, out string error) → bool` (unfrozen only; raises `SettlementCaptured` synchronously; gameplay captures run from the step's Capture contacts), `LogCaptures` (get/set; serialized `logCaptures`, on), `RecentCaptures` (last 16 records, oldest first), `TotalCaptures` (ordinary and siege), `TotalCaptureFailures` (started but not every unit moved; bug path); declares `SettlementCaptureRecord` (`WM-CAPTURE-01`).  
API (.Siege, I19): `PendingRetreats` (open rosters, oldest first), `PendingRetreatUnitCount`, `LogPendingRetreats` (get/set; serialized `logPendingRetreats`, on), `TotalSiegeCaptures`, `TotalSettlementOnlyDefendersKilled`, `TotalPendingRetreatRosters`, `TotalPendingRetreatBlocks`, `TotalEscapedDefenderUnitsPlaced`, `DebugHoldPendingRetreats` (get/set), `DescribePendingRetreats() → string`; internal `TryApplySiegeCapture(Army2D army, Settlement2D settlement, int battleId, out SettlementCaptureRecord record, out string error) → bool` (only from the ending siege's routing, map frozen; queues the event) and `DebugEvacuateToPendingRetreat(Settlement2D, out string) → bool`; context menus "I19: Log pending retreat rosters and siege totals", "I19: Toggle pending-retreat placement hold (debug)"; declares `PendingRetreatRoster2D` (`Id`, `Faction`, `Roster`, `SourceSettlement`, `SourceSettlementId`, `Origin`, `OriginRadius`, `BattleId`, `CreatedMapTime`, `CreatedStep`, `InitialCount`, `ArmiesDeployed`, `UnitsDeployed`, `PlacementBlocks`, `Count`, `IsClosed`, `ToString()`; internal constructor, retry time and block-log flag) (`WM-PENDING-01`).  
API (.Regeneration, I14): `Regeneration` (`UnitRegeneration2D`; null without a reserve), `RegenerationIntervalSeconds` (rules value; 2 without rules); serialized `logRegenerationTicks` (off). Editor-only context menus: "Regen - Set selected unit to 50% HP and track it" (the selection or a child of a registry unit; refuses never-activated units), "Regen - Set every activated unit to 50% HP", "Regen - Log status" (interval, ticks, time to next tick, total heals, registry breakdown with the lowest HP %, last tick, tracked unit), "Regen - Stop tracking". A tracked unit's HP is appended to every tick log until its `Owner` is null, its GameObject is destroyed or it has no health; the log then reports it as gone with its status and stops tracking. `ReleaseOwnership` clears `Owner` through `UnitRoster.RemoveInternal` (settled at M09, `WM-OWN-01`), so a dead tracked unit is reported gone at the next tick. The status and tracking readouts read `MaximumHealth`, which can notify (`STAT-01`); debug only. The 50% menus use the development-only `UnitHealth2D.DebugSetHealthFraction` (`HEALTH-05`).  
Internal: `ReportArmyContact`, `NotifyArmyDestroyed`, `RegisterSettlement`, `UnregisterSettlement`.  
Events: `ArmySpawned(Army2D, ArmySpawnReason)` (not isolated; since I19 also `EscapedDefenders`, already retreating), `ArmyDestroyed(Army2D)` (not isolated), `FrozenChanged(bool)` (subscribed by `BattleArena2D` while an arena exists), `ContactReported(WorldMapContactReport)` (subscribed by `BattleManager2D`, `BATTLE-START-01`), `AlliedInteractionApplied(AlliedInteractionRecord)`, `SettlementCaptured(SettlementCaptureRecord)` (I18; once per capture, after the faction change, the garrison fill and the army's removal; handlers isolated; an ordinary capture raises it at once, a siege capture at the start of the next controller FixedUpdate, `WM-CAPTURE-01`).  
Invariants: `WM-LIFE-01`, `WM-LIFE-02`, `WM-TIME-01`, `WM-FREEZE-01`, `WM-SPAWN-01`, `WM-SPAWN-02`, `WM-REMOVE-01`, `WM-SETTLE-02`, `WM-OWN-02`, `WM-RECRUIT-01`, `WM-XFER-02`, `WM-DEPLOY-01`, `WM-DEPLOY-02`, `WM-PLACE-01`, `WM-OVERFLOW-01`, `WM-ALLY-01`–`WM-ALLY-03`, `WM-AUDIT-01`, `WM-CONTACT-01`–`WM-CONTACT-05`, `WM-BATTLE-01`, `WM-IMMUNE-01`, `WM-REGEN-01`, `WM-REGEN-02`, `WM-RETREAT-01`, `WM-CAPTURE-01`, `WM-PENDING-01`; `BATTLE-RETURN-01`, `BATTLE-SIEGE-01`, `BATTLE-SIEGE-02`; `SETUP-06`.  
Never owns: army direction/bounce, the classification table, unit ownership mutation (reserve only), roster storage, the settlement's faction field and its relabel (`Settlement2D`), battles and their routing decisions (which armies retreat, which outcome captures), the recovery deadline itself (`Army2D`), health mutation (`UnitHealth2D`), `Time.timeScale`.

| Partial source | Responsibility |
| --- | --- |
| WorldMapController2D.cs | Serialized config, all Unity messages (Awake/FixedUpdate/OnDestroy/OnApplicationQuit), registries, freezing, creation, removal, bounds; the step order (`WM-TIME-01`), including the siege-capture event flush, the capture step, the retreat recovery step and the regeneration initialization and step |
| .Contacts.cs | Intake, deduplication, episodes, classification, battle selection (immune army pairs and, since I18, immune army–settlement pairs skipped), `ContactReported` dispatch |
| .Units.cs | Reserve creation, audit (since I19 including pending retreat rosters), deferred-recruitment flush, owner-loss handling |
| .Deployment.cs | `DeployArmy`, pending requests, placement, `TryDeployFromRoster` (I19: `startRetreating`), rollback, overflow, settlement work (I19: ending with the pending retreat rosters) |
| .Interactions.cs | `AlliedInteractionRecord`, allied interactions, `CanTransfer`, gated transfers, live `IsRosterInBattle` |
| .Separation.cs (I13) | Return spots, `ApplyBattleReturn`, army-pair immunity and its pruning; since I18 the siege return spot, `ApplySiegeReturn` and army–settlement immunity |
| .Regeneration.cs (I14) | Regeneration service creation and stepping, tick log, Editor-only test menus and unit tracking |
| .Retreat.cs (I16) | Retreating-army recovery: the due check, the recovery loop, its counter, recent lines and log |
| .Capture.cs (I18) | `SettlementCaptureRecord`, ordinary captures from Capture contacts, `TryCaptureSettlement`, the capture record list and counters, `SettlementCaptured` |
| .Siege.cs (I19) | `PendingRetreatRoster2D`, the siege capture (`TryApplySiegeCapture`), the queued capture events and their flush, pending-roster placement, the debug evacuation, hold and listing |

### Army2D [WM1, WM2, WM5, WM7, WM9]
Owns: travel direction, bounce resolution, commanded constant velocity, its Rigidbody/collider setup, retreat flag and (since I16) its recovery deadline, its `UnitRoster`, the source-settlement collision ignore, visuals (faction colour, retreat tint and flag, unit-count label), speed/bounce diagnostics.  
API: `ArmyId`, `Faction`, `IsRetreating`, `RetreatRecoverAtMapTime` (I16; +∞ when not retreating), `RetreatRecoveryRemaining` (I16; map seconds, never negative, 0 when not retreating), `Direction`, `Radius`, `TargetSpeed`, `Position`, `IsInitialized`, `IsRemoved`, `Roster`, `UnitCount`, `IgnoredSettlement`, `MeasuredSpeed`, `RecentBouncesPerSecond`, `TotalBounces`; `SetRetreating(bool)` (a change to true records `MapTime + RetreatRecoverySeconds`, a change to false drops the deadline, no change does nothing; raises no event, refreshes visuals; `WM-RETREAT-01`; since I19 also called by siege routing and by the escaped-defender deployment before `ArmySpawned`, a caller list taken from the attached callers, `Army2D.cs` not re-attached at M09); `DebugSetRetreatRecoveryRemaining(double mapSeconds) → bool` (I16, debug: retreating, initialized, non-removed army and a finite value only; clamps at 0; flag untouched). Controller-only internal: `Initialize(WorldMapController2D, int armyId, FactionId, Vector2 initialDirection)`, `MarkRemoved()`, `ApplyFrozen(bool)` (ignored once removed), `IgnoreSettlementUntilSeparated(Settlement2D, double minUntilMapTime)`, `PlaceAt(Vector2 position, Vector2 newDirection) → bool` (I13, battle return only: teleports the body and transform, works frozen or not, sets the velocity only while simulated, drops a pending bounce and restarts the speed window; a zero or non-finite direction keeps the current one; false when uninitialized, removed or the position is non-finite; retreat flag, roster and settlement ignore untouched).  
Events: none emitted; subscribes to its roster's `Changed` for visuals; OnDestroy notifies the controller.  
Invariants: `WM-MOVE-01`–`WM-MOVE-03`, `WM-SPAWN-02`, `WM-REMOVE-01`, `WM-FREEZE-01`, `WM-CONTACT-01`, `WM-IGNORE-01`, `WM-ROSTER-01`, `WM-RETREAT-01`; `BATTLE-RETURN-01`.  
Never owns: registry, ids, removal decisions, classification, contact effects, unit ownership changes, when recovery runs (controller).

### Settlement2D [WM1, WM2, WM9]
Owns: kind, faction, radius, its garrison `UnitRoster`, deferred recruits, one pending deployment request (amount, serial) and the deploy/overflow retry timers, its static collider and visuals; since I18 its authored siege setup (battlefield prefab, attacker and defender edges).  
API: `Kind`, `Faction`, `Radius`, `WorldRadius`, `Position`, `SettlementId`, `Garrison`, `GarrisonCount`, `GarrisonLimit`, `HasPendingDeployment`, `PendingDeployAmount`, `PendingRecruitCount`; `Recruit(GameObject prefab, int count) → RecruitResult`, `RequestDeployment(DeployAmount) → DeployResult`, `CancelPendingDeployment() → bool`, `SetFaction(FactionId) → bool` (since I18 refused while the garrison is in battle); since I18 `SiegeBattlefieldPrefab`, `SiegeAttackerEdge`, `SiegeDefenderEdge`, `TryGetSiegeSetup(out GameObject battlefieldPrefab, out ArenaEdge attackerEdge, out ArenaEdge defenderEdge, out string error) → bool` (read-only, authored fields only). Controller-only internal: `AssignId`, `FlushDeferredRecruitment`, `SetPendingDeployment`, `ClearPendingDeployment(serial)`, `PendingDeploymentSerial`, `NextDeployRetryMapTime`, `NextOverflowRetryMapTime`, `Footprint`; since I19 `SetFactionAfterSiege(FactionId) → bool` (siege capture only; skips only the in-battle guard) and `DropDeferredRecruitment(string reason) → int`. Context menu (I19): "I19: Evacuate mobile garrison as escaped defenders (debug)" (`WM-PENDING-01`).  
Events: none; subscribes to garrison `Changed` for visuals.  
Invariants: `WM-SETTLE-01`, `WM-SETTLE-02`, `WM-RECRUIT-01`, `WM-FACTION-01`, `WM-DEPLOY-01`, `WM-CAPTURE-01`, `BATTLE-SIEGE-01`; `SETUP-06`; OnValidate warns when the two siege edges are equal.  
Never owns: unit creation/ownership (reserve), deployment execution, overflow, when a capture happens or what it moves (controller), siege battles (manager), the siege prefab's validation of contents (arena), its own id.

### UnitReserve2D [WM2, WM4, WM7, WM9]
Owns: the parked-unit root, the registry of owned handles, unit ids, the global arrival-sequence counter, deployment status marks (Deployed, and Escaping since I15) and returns to the reserve, and every ownership change.  
API: `Root`, `Units`, `Count`; static `ValidatePrefab(GameObject, out string)`; `Transfer(UnitRoster from, UnitRoster to, IReadOnlyList<UnitHandle>) → UnitTransferResult`; `TransferFirst(UnitRoster from, UnitRoster to, int maxCount) → UnitTransferResult` (both move only parked Reserve units and never touch retreat flags; since I19 two pending retreat rosters never exchange units). Internal: constructor `(Transform parent)`, `CreateInto(garrison, prefab, count, out error) → int`, `DiscardAll(roster) → int`, `TryMarkDeploying(UnitHandle, out string) → bool`, `TryMarkEscaping(UnitHandle, out string) → bool` (I15: Deployed and active → Escaping; callback-free), `ReturnToReserve(UnitHandle, UnitStatus) → bool` (target Reserve or Escaped; accepts a Deployed, Escaping or Escaped unit), `ReleaseOwnership(UnitHandle, UnitStatus) → bool` (Dead or Discarded; clears `Owner` through the roster; never destroys the GameObject; since I19 also the siege capture's settlement-only defenders), `Audit(rosters, out report) → bool` (Escaping checked like Deployed and counted separately; since I19 pending retreat rosters counted, and a settlement-only unit flagged in any roster that does not accept one).  
Events: none; raises roster `Changed` through the rosters.  
Invariants: `WM-UNIT-01`, `WM-OWN-01`, `WM-XFER-01`, `WM-AUDIT-01`; `BATTLE-SPAWN-02`, `BATTLE-UNIT-01`–`BATTLE-UNIT-04`, `BATTLE-ABORT-01`, `BATTLE-END-04`, `BATTLE-RETREAT-03`; `CORE-03`.  
Never owns: when transfers are allowed (controller gate), deployment decisions, activation itself (the battle spawn path activates), arena bodies, combat state.

### UnitRoster [WM2, WM9]
Owns: one owner's unit list in arrival order, settlement-only count, read order, capacity, closed flag; a garrison's faction label. Since I19 one of three owners: a garrison, an army or a pending retreat roster.  
API: `Kind`, `Settlement`, `Army`, `PendingRetreat` (I19), `Faction`, `Capacity`, `Order` (get/set), `IsClosed`, `Count`, `SettlementOnlyCount`, `EligibleCount`, `FreeCapacity`, `AcceptsSettlementOnly`, `UnitsInArrivalOrder`, `OwnerLabel`, `Contains(UnitHandle)`, `GetUnitsInOrder(List<UnitHandle>, bool armyEligibleOnly = false)`. Internal: constructor (since I19 with an optional pending-retreat owner), `AppendInternal`, `RemoveInternal` (sets `Owner` null), `Close`, `TryRelabelFaction` (garrisons only), `RaiseChanged` (subscribers isolated). `OwnerLabel` is `S<id>`, `#<id>` or `P<id>`.  
Events: `Changed(UnitRoster)` (`WM-ROSTER-02`).  
Invariants: `WM-ROSTER-01`, `WM-ROSTER-02`, `WM-OWN-01`, `WM-FACTION-01`, `WM-PENDING-01`.  
Never owns: unit creation/destruction, transfer rules, its owner's lifetime.

### UnitHandle [WM2, WM4, WM6]
Owns: one unit's persistent identity and references; the lazily resolved root `UnitHealth2D` (since I14).  
API: `Id`, `GameObject`, `Combatant`, `RuntimeStats`, `Progression` (may be null), `Definition`, `Owner`, `ArrivalSequence`, `Status`, `HasEverActivated` (never reset), `Health` (I14; GetComponent on first use, cached, may be null, wake-safe), `IsSettlementOnly`, `Exists`, `IsParked`, `Faction`, `Name`, `CurrentLevel`. Setters for Owner, ArrivalSequence, Status and HasEverActivated are internal; since Phase 4 the reserve's battle operations write Status and HasEverActivated.  
Events: none.  
Invariants: `WM-UNIT-02`, `WM-OWN-01`, `WM-REGEN-02`; `CORE-03`.  
Never owns: ownership decisions, activation or health.

### UnitRosterTypes2D [WM2, WM4, WM9]
Owns: `UnitStatus {Reserve, Deployed, Escaping, Escaped, Dead, Discarded}` (all six are set; Escaping only since I15, by `UnitReserve2D.TryMarkEscaping`); `UnitRosterKind {Garrison, Army, PendingRetreat}` (PendingRetreat since I19, `WM-PENDING-01`); `UnitRosterOrder {Fifo, HighestLevelFirst}`; `UnitTransferResult`; `RecruitStatus {Recruited, Deferred, Rejected}`; `RecruitResult`.  
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

### UnitRegeneration2D [WM6]
Owns: the shared healing tick: the scaled-time accumulator, tick count, total heals, the last tick report and a reused registry snapshot buffer. Also declares `HealthRegenerationOutcome2D {Healed, AlreadyFull, Ineligible}` (the result of one `UnitHealth2D.ApplyRegeneration` call) and the `UnitRegenerationTickReport` struct. A plain C# object with no Unity callbacks, created and stepped by `WorldMapController2D.Regeneration.cs`.  
API: `TickCount`, `TotalHeals` (longs), `LastTick` (`UnitRegenerationTickReport`: `Tick`, `Healed`, `HealedActive`, `HealedInactive`, `AlreadyFull`, `NeverActivated`, `Ineligible`, `Failed`), `ElapsedSeconds` (float view of the double accumulator). Internal: constructor `(UnitReserve2D)`, `Step(float deltaSeconds, float intervalSeconds) → bool` (true when a tick ran), `Tick()`.  
Events: none; each heal raises `UnitHealth2D.HealthChanged`.  
Invariants: `WM-REGEN-01`, `WM-REGEN-02`; `HEALTH-05`; `CORE-03`.  
Never owns: health mutation (`UnitHealth2D`), unit ownership or the registry (`UnitReserve2D`), when it is stepped (controller), MapTime, `CombatLock2D`.  
Since I19 the registry it walks also holds pending-retreat units, which it heals like any parked unit (`WM-REGEN-02`; source-derived, the file was not re-attached at M09).

### WorldMapContacts2D [WM1, WM2]
Owns: `WorldMapContactKind` (10), `WorldMapContactIntent` (8), internal `WorldMapContactPairKey`, immutable `WorldMapContactReport`, pure `WorldMapContactClassifier2D` with a 33-check self-test.  
API: `ClassifyArmies(FactionId, bool, FactionId, bool)`, `ClassifySettlement(FactionId armyFaction, bool armyRetreating, FactionId settlementFaction, int garrisonCount)`, `IntentOf(kind)`, `StartsBattle(intent)`, `SecondArmyFirst(kind, idA, retreatingA, countA, idB, retreatingB, countB)`, `RunSelfTest(out string)`, `KindCount`. Report fields: `Step`, `MapTime`, `Kind`, `Intent`, `IsNewContact`, `ArmyA`, `ArmyB`, `Settlement`, `ArmyIdA`, `ArmyIdB`, `SettlementId`, `ContactPoint`, `PositionA`, `PositionB`, `FactionA`, `FactionB` (int), `RetreatingA`, `RetreatingB`, `CountA`, `CountB` (army `UnitCount` / `GarrisonCount`), `IsSettlementContact`, `ToShortString()`.  
Events: none.  
Invariants: `WM-CLASS-01`, `WM-CLASS-02`, `WM-CONTACT-02`, `WM-CONTACT-04`.  
Never owns: Unity state, contact timing, dispatch, effects.

### WorldMapTypes2D [WM2, WM9]
Owns: `SettlementKind {Town, Castle}`; `ArmySpawnReason {Debug, Deploy, Overflow, EscapedDefenders}` (EscapedDefenders since I19: an army formed from a pending retreat roster, already retreating when announced; the planned BattleReturn was dropped, because returning battle armies are moved, never spawned); `DeployAmount {Half, All}`; `DeployStatus {Deployed, Pending, Rejected}`; internal `ArmyDeployAttempt {Deployed, Blocked, Failed}` (shared by manual deployment, overflow and escaped defenders); `DeployResult`; `ArmyPlacement`.  
API: `DeployResult` fields Status, Amount, Army (set only when Deployed), UnitCount, Message, and static Deployed/Pending/Rejected; `ArmyPlacement(Vector2 position, Vector2 direction)`.  
Events: none.  
Invariants: `WM-SPAWN-01`, `WM-DEPLOY-01`, `WM-DEPLOY-02`, `WM-PLACE-01`.  
Never owns: behaviour.

### WorldMapRules2D [WM1, WM2, WM6, WM7]
Owns:
- movement (speed, jitter, axis angle);
- army body (radius, spawn margin);
- rosters: `MaxUnitsPerArmy` 200, `DefaultRosterOrder` Fifo, town/castle limits 200/300, `OverflowArmySize` 100 (clamped to army capacity, with an OnValidate warning);
- placement: `SettlementCollisionIgnoreSeconds` 0.5, `PlacementSlotsPerRing` 16, `PlacementRings` 2, `PlacementRetrySeconds` 0.25;
- regeneration (I14): `RegenerationIntervalSeconds` 2 (minimum 0.1), in scaled-time seconds, not map time; the heal per tick is per unit (`UnitDefinition2D.healthRegenFraction`);
- retreating armies (I16): `RetreatRecoverySeconds` 20 (minimum 0), in map seconds; 0 recovers on the next unfrozen step;
- presentation (retreat tint, label size, optional army sprite).

API: read-only properties; `GetGarrisonLimit(SettlementKind)`; `ArmySprite` falls back to the generated circle. Consult source for ranges.  
Events: none.  
Invariants: `WM-LIFE-01` fallback; `WM-MOVE-01` live speed vs. initialize-time radius; roster capacity read at Initialize (`WM-ROSTER-01`); regeneration interval read every step (`WM-REGEN-01`); recovery seconds read each time a flag is set, so a change affects only later flags (`WM-RETREAT-01`).  
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

### WorldMapDebugPanel2D [WM1, WM2, WM4, WM7]
Owns: optional IMGUI tooling with four tabs (Armies, Settlements, Rosters, Contacts):
- **Spawning:** empty-army spawning (single, batch, head-on pair, Converge 4, aim at settlement), with optional fill from the selected settlement; destroy empty armies; debug discard of all armies with their units.
- **Retreat (I16):** a header line with retreating armies, recoveries and the recovery time; each army row shows `rec Ns` while retreating, Flag+/Flag- (`SetRetreating`; Flag+ starts a full timer) and T (sets the time left to the "Row T" field through `DebugSetRetreatRecoveryRemaining`; retreating rows only); optional per-army recovery times for the head-on pair (A, B) and Aim (A); in Contacts, a "Log retreat recoveries" toggle and the last four recoveries. Seconds parse culture-independently ("2.5" or "2,5", ≥ 0).
- **Settlements:** recruitment from a `UnitPrefabCatalog2D`; Set faction; order toggles; D50/D100 deploy and cancel.
- **Units:** per-unit selection and selected transfers; N-unit transfers through the gated API.
- **Placement tests:** debug placement blockers (static circles on `WM_Bounds`, unreported).
- **Diagnostics:** ownership audit; frozen recruit/deploy deferral tests; freeze-on-next-battle-report test; contact and allied-interaction statistics; classifier self-test.

API: serialized controller (fallback `Instance`) and catalog; no public methods. Debug spawns use `ArmySpawnReason.Debug`.  
Events: subscribes to `ContactReported` only while the freeze test is armed.  
Invariants: mutations (the retreat buttons included) disabled while frozen, except freeze toggle, audit, blockers and the explicit deferral tests; all transfers go through `WM-XFER-02`. The freeze toggle is disabled while `BattleArena2D.IsAnyArenaActive` (I10), because unfreezing would abort the arena and the battle (`WM-FREEZE-01`, `ARENA-LIFE-03`).  
Never owns: any gameplay rule; production code must not depend on it.  
Phase 9 (from the I19 handoff; the panel was not re-attached at M09): it does not show pending retreat rosters; the controller and settlement context menus do (`WM-PENDING-01`). Whether it shows captures or siege immunity is not established.

`FactionId` (constructor from int, `.Value`, equality, serializable) is an existing combat type. Core attributes faction types to `CombatTypes2D`; its definition was not attached to this update.

### BattleArena2D (three partials) [WM3, WM4, WM9]
Owns: `Instance` and readiness; the arena lifecycle (create, remove, abort, teardown); the arena Root and everything under it; ownership of the map freeze while an arena exists; start/current/target view size; perimeter walls, gates and entry-strip geometry; registered bodies, their entry state, the Entry/Retreat overrides it sets, wall ignores and counters; test-body creation; the arena audit.  
API (core): const `RootName`; static `Instance`, `IsAnyArenaActive`; `Rules`, `CameraRig`, `Map`, `IsReady`, `HasArena`, `Root`, `Center`, `StartViewSize`, `ViewSize`, `TargetViewSize`, `MapPoint`, `StartingCountA`, `StartingCountB`, `Battlefield`, `ViewRect` (default unless in battle view), `NextPerimeterMode`, `SetNextPerimeterMode(PerimeterMode)`, `TotalCreated`, `TotalRemoved`, `TotalFailedCreates`, `TotalAborts`; `CanCreate(out string) → bool`; `TryCreate(Vector2 mapPoint, int startingCountA, int startingCountB, out string error) → bool`; `TryCreate(Vector2, int, int, ArenaEdgeMask gateEdges, out string) → bool`; since I18 `TryCreateWithBattlefield(Vector2 mapPoint, int startingCountA, int startingCountB, ArenaEdgeMask gateEdges, GameObject battlefieldPrefab, string label, out string error) → bool` (sieges; explicit terrain, `BattlefieldSource.Siege`) and public static `ValidateBattlefieldPrefab(GameObject prefab, out string error) → bool` (asset-only check; also used by the siege pre-arena checks); `Remove() → bool`; `AuditArena(out string report) → bool`.  
API (.Perimeter): `ActivePerimeterMode`, `GateEdges`, `WallThickness`, `IsShrinking`, `GetWallCollider(ArenaEdge) → Collider2D`, `GetEntryStrip(ArenaEdge) → Rect`, `RequestViewSize(float) → bool`, `RequestViewSizeForCount(int) → bool`; internal static `FrictionlessMaterial`.  
API (.Bodies): `Bodies`, `EscapesAccepted` (get/set), `TotalBodiesRegistered`, `TotalLeaks`, `TotalReentries`, `TotalEscapes`, `TotalLostBodies`, `EnteringCount`, `InsideCount`, `ResetBodyCounters()`, `TryRegisterBody(Rigidbody2D, CircleCollider2D, UnitDestinationOverride2D, ArenaEdge entryEdge, out ArenaBody, out string) → bool`, `UnregisterBody(ArenaBody) → bool`, `SetBodyRetreating(ArenaBody, bool) → bool`, `ForceReenter(ArenaBody) → bool`, `TryFindEntryPoint(ArenaEdge, float radius, IReadOnlyList<Vector2> reserved, out Vector2) → bool`. Debug: `TestBodies`, `SpawnTestBodies(ArenaSide, ArenaEdge, int, out string) → int`, `SetTestBehaviour(TestBodyBehaviour)`, `KnockbackTestBodies(float speed, float duration) → int` (Inside bodies only), `SetTestRetreating(int maxCount, bool) → int`, `EjectRandomTestBody(out string) → bool`. Internal: `HandleGateContact`.  
Events: `ArenaCreated(BattleArena2D)`, `ArenaRemoved(BattleArena2D)` (subscribed by `BattleManager2D`: an abort except during preparation or the normal end, `BATTLE-ABORT-01`); `EscapesAccepted` is also set false by the battle result lock (`BATTLE-LOCK-01`); `BodyEscaped(ArenaBody)`; registered bodies include deployed real units (`BATTLE-SPAWN-02`); subscribes to the map's `FrozenChanged` while an arena exists.  
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

### BattleRules2D [WM3, WM4, WM5, WM7, WM8, WM9]
Owns: battle tuning. Arena placement (`ArenaCenter` (1000, 0), clearance 20, clearance aspect 2.4); battlefield regions, fallback prefab, generated-ground colour and margin 6; camera min 6 / max 18, size curve, z −10, shrink speed 1.5; perimeter mode (OneWayEffector), wall thickness 1, entry strip depth 1.5, entry margin 0.3, spawn gap 0.1, spawn attempts 16; gate depth 0.4; spawning (I10): rate base 2, units per extra rate 20, catch-up 0.25 s (0–2), side-axis tie epsilon 0.05 (0–0.5); ending (I11): `maxBattleSeconds` 0 (off), `endDelaySeconds` 1; return (I13): `postBattleImmunitySeconds` 1 (0 = none); unit retreat (I15): `escapeChance` 0.4 and `escapeHealthFraction` 0.3 (both 0–1), `retreatModifier` (a `ModifierDefinition2D`; empty = none); battle-start lists (I17): `fieldBattleModifiers`, `townDefenderModifiers`, `castleDefenderModifiers` (`ModifierDefinition2D[]`, empty by default; since I18 sieges use the defender lists); sieges (I18): `siegeDefenderSpawnMultiplier` 2 (0.1–10; the siege terrain and edges are authored per settlement, not here); side layer names (I08): bodies `Unit_SideA`/`Unit_SideB`, Clash `Clash_SideA`/`Clash_SideB`, plus `Wall`; test-body radius 0.35, speed 3 and colours. Also declares `BattlefieldRegion2D` (Name, Area, Priority, BattlefieldPrefab, `Contains(Vector2)`), `BattlefieldSource {Region, Fallback, Generated, Siege}` (Siege since I18) and `BattlefieldSelection` (Source, RegionIndex, Prefab, Label).  
API: read-only properties (clamped; names in source), including `SideABodyLayerName`, `SideBBodyLayerName`, `SideAClashLayerName`, `SideBClashLayerName`, `WallLayerName`, `SpawnRateBase`, `SpawnUnitsPerExtraRate`, `SpawnCatchUpSeconds`, `SideAxisTieEpsilon`, `MaxBattleSeconds` (non-finite → 0), `EndDelaySeconds` (non-finite → 1), `PostBattleImmunitySeconds` (non-finite → 1), `EscapeChance` (non-finite → 0.4, clamped 0–1), `EscapeHealthFraction` (non-finite → 0.3, clamped 0–1), `RetreatModifier`; `FieldBattleModifiers → IReadOnlyList<ModifierDefinition2D>` and `GetDefenderModifiers(SettlementKind) → IReadOnlyList<ModifierDefinition2D>` (I17: the live lists, possibly with null entries, never null; `Castle` → castle list, any other kind → town list; callers copy at commit); `RegionCount`, `GetRegion(int)`; `ComputeSpawnRate(int startingCount) → int` (max(1, base + N / perExtra)); since I18 `SiegeDefenderSpawnMultiplier` (non-finite → 2) and `ComputeSiegeDefenderSpawnRate(int startingCount) → int` (max(1, floor(rate × multiplier + 1e-4)); 20 → 6, 80 → 12, 200 → 24); `ComputeViewSize(int) → float`; `SelectBattlefield(Vector2) → BattlefieldSelection`.  
Events: none.  
Invariants: `ARENA-TERRAIN-01`, `ARENA-VIEW-01`, `SIDE-01`, `BATTLE-SPAWN-01`, `BATTLE-EDGE-01`, `BATTLE-END-03`, `BATTLE-LOCK-02`, `WM-IMMUNE-01`, `BATTLE-RETREAT-01` (the three retreat values are copied onto each battle at commit), `BATTLE-DELIVER-03` (the lists are copied onto each battle at commit), `BATTLE-SIEGE-01` (the defender rate); OnValidate warns on max < min, an empty curve, a strip shallower than one test body, gate depth too large for the min size, any two equal side layer names, side layer names missing from Tags and Layers, each empty entry of the three battle-start lists (I17), and regions without a prefab or with zero area.  
Never owns: runtime state, resolved layer indices.

### BattleLayers2D [WM3, WM4]
Owns: battle layer names and resolved indices.  
API: consts `PerimeterLayerName` ("Perimeter"), `RetreatingPlaceLayerName` ("RetreatingPlace"); `Perimeter`, `RetreatingPlace`, `BodyA`, `BodyB`, `Wall` (−1 when missing), `IsResolved`, `BodyMask`, `SpawnBlockMask`, `BodyLayer(ArenaSide) → int`, `TryResolve(string bodyALayerName, string bodyBLayerName, string wallLayerName, out string error) → bool`, `LogCollisionMatrixProblems()`.  
Events: none.  
Invariants: `SETUP-07`; reports matrix problems, never edits project settings.  
Never owns: Clash layers or the per-battle side layers (`BattleSideLayers2D`, `SIDE-01`), WM layers, unit masks. Its body layer names come from the rules (`Unit_SideA`/`Unit_SideB` since I08). The I09 handoff reports a stale source comment ("Phase 4 renames"), comment only; the file was not re-attached for M04.

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

### UnitDestinationOverride2D [WM3, WM4]
Owns: one unit's optional destination and reason; activation count.  
API: `HasOverride`, `Reason`, `Destination`, `Activations`; `Set(Vector2, DestinationOverrideReason)`, `Clear()`, `TryGetDirection(Vector2 from, float arriveDistance, out Vector2 direction) → bool`; enum `DestinationOverrideReason {None, Entry, Retreat, Debug}`.  
Events: `Changed(UnitDestinationOverride2D)` on start, end or reason change only.  
Invariants: `ARENA-DEST-01`; `CORE-03`.  
Never owns: steering execution (motor `MOVE-01`; test bodies), Sidestep state (Sidestep only reads it), when an override is set (arena).

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

### BattleManager2D (nine partials) [WM4, WM5, WM7, WM8, WM9]
Owns: `Instance`; the running `BattleContext`, its start (field battles and, since I18, sieges), refusals and refusal wait; retreat eligibility rolls (since I15); the end check, death batches, timeout, result lock and end delay; the normal end and the abort; the combat-lock roots it locked; routing and the army-return calls; since I19 the siege outcome, the call that applies a siege capture and the siege return; recovery (pending end, orphan arena, quarantine retry); the post-battle audit; the `ContactReported` and `ArenaRemoved` subscriptions; per-side spawn ticking, entry-point reservations and the deployment sequence; the `Battle Units (active)` container; field, escaped, corpse and quarantine lists; death, escape and lost-unit handling; the low-health latch subscriptions, retreat starts and the modifier delivery queue (since I15); the battle-start list freeze and the per-unit battle-start requests (since I17); `LastCommittedBattle` (I17 diagnostics); the Sidestep probe; counters and debug tools. Also declares `BattleModifierDeliveryKind {Retreat, BattleStart}` (`.Delivery`).  
API: const `ExecutionOrder` (−12000); static `Instance` (null without a manager); `Arena`, `Map`, `Current`, `IsBattleActive`, `IsResultLocked`, `IsEnding`, `IsRosterInBattle(UnitRoster) → bool` (true for either side's roster of the current battle, an army or since I18 a besieged garrison, including while locked; false while an end is pending), `HasStartedBattleThisSession` (diagnostic since I13), `IsEndPending`, `PendingEnd`, `IsOrphanArenaPending`, `QuarantinedUnits`, `QuarantinedCount`, `BattleUnitsRoot`, `LastStartMessage`, `LastPostBattleAudit`, `PendingModifierDeliveries` (I15), `LastCommittedBattle` (I17: the most recently committed context, kept after it ends or aborts; null before the first battle; diagnostics only); `TryStartFieldBattle(WorldMapContactReport, out string) → bool`; since I18 `TryStartSiege(WorldMapContactReport, out string) → bool`; `AbortBattle() → bool` (false with no battle or during the normal end). Internal (I15): `RequestModifierDelivery(BattleUnit, BattleModifierDeliveryKind, ModifierDefinition2D) → bool` and `(BattleUnit, BattleModifierDeliveryKind, IReadOnlyList<ModifierDefinition2D>) → bool` (`BATTLE-DELIVER-01`); since I17 `FreezeSiegeBattleStartModifiers(BattleContext, BattleRules2D, SettlementKind)` (called at every siege's commit since I18; `BATTLE-DELIVER-03`).  
Counters: `TotalBattlesStarted`, `TotalStartsRefused`, `TotalNoReturnRoomRefusals`, `TotalPreparationFailures`, `TotalResultsLocked`, `TotalBattlesEnded`, `TotalAborts`, `TotalEndFailures`, `TotalEndRecoveries`, `TotalOrphanArenaRemovals`, `TotalArmiesRemovedAfterBattle`, `TotalReturnsWithoutSeparation`, `LostUnits`, `CleanupProblems`, `QuarantineRecoveries`, `DeathsAfterLock`, `EscapesAfterLock`, `ProjectilesRemovedAtLock`, `ProjectilesRemovedAtCleanup`, `PostBattleAudits`, `PostBattleAuditFailures`, `PostBattleAuditsSkipped`; since I15 `TotalRetreatRolls`, `TotalRetreatEligible`, `TotalRetreatsStarted`, `TotalRetreatStartFailures`, `TotalDebugRetreats`, `ModifierDeliveriesRequested`, `ModifierDeliveriesCompleted`, `ModifierDeliveriesDropped`, `ModifierEntriesApplied`, `ModifierEntriesRejected`; since I17 `TotalBattleStartRequests`, `TotalBattleStartDeliveries` (no battle-start drop total: drops count in `ModifierDeliveriesDropped`, per side in `BattleStartDropped`); since I18 `TotalSiegeSetupRefusals` (also in `TotalStartsRefused`); since I19 `TotalSiegeCaptures` and `TotalSiegeCaptureRefusals` (must stay 0), declared in `.Routing`. All are session totals.  
Debug: one-shot `DebugFailNextPreparation`, `DebugFailNextPark`, `DebugSkipArenaRemovalAtNextEnd`, `DebugForceTimeoutNextCheck`, and `DebugPauseQuarantineRetry` (all get/set); `DebugHoldModifierDeliveries` (I17, get/set: pending deliveries stay queued while set, every drop still runs; persists across battles until cleared, `BATTLE-DELIVER-01`); `DebugAuditBattleStartModifiers(out string) → bool` and `DebugDescribeBattleStartUnits(int maxCount) → string` (I17, read-only, `BATTLE-DELIVER-03`); `DebugSetRetreating(int maxCount, bool retreating) → int` (I15: true forces retreats through the gameplay start path, ignoring eligibility, refused after the lock; false does nothing and returns 0); `DebugSetFieldHealth(DebugRetreatHealthTarget, float fraction, int maxCount, out string) → int` and `DebugSetQueuedHealth(float fraction, out string) → int` (I15, Editor and development builds: set HP through `UnitHealth2D.DebugSetHealthFraction`; the first targets eligible, ineligible or retreating field units, the second still-queued units that were activated before); enum `DebugRetreatHealthTarget {EligibleNotRetreating, IneligibleNotRetreating, Retreating}`; `DebugEjectRandomUnit(out string) → bool` (`ForceReenter`, then a move into the nearest edge's strip; refused after the lock); `DebugKillField(bool sideA, bool sideB, out string) → int` (unattributed `ForceDeath` of field units in one call, so in one batch; refused after the lock); `ResetProbe()`, `DescribeStatus()` (with retreat lines per side and the I15 session counters; since I17 the battle-start source, a battle-start line per side and a session line that flags a held delivery queue), `SidestepStarts`, `SidestepUnderOverride`, `SidestepWhileEntering`. Context menus: status, kill both/A/B, fail next park, skip arena removal at next end, force timeout; since I15 "Retreat: Force 5 field units to retreat (debug, ignores eligibility)", "Retreat: Eligible field units → 20% HP (up to 5)", "Retreat: Ineligible field units → 20% HP (up to 5)", "Retreat: Eligible field units → 31% HP (up to 5, just above)", "Retreat: Retreating field units → 100% HP (heal must not cancel)", "Retreat: Queued activated units → 20% HP (deployment check)"; since I17 "Battle-start: Audit deliveries and leftover modifiers", "Battle-start: Log field units (up to 30)", "Battle-start: Log rule lists and the running battle's frozen lists", "Battle-start: Toggle delivery hold (debug; requests stay pending)" (`.Debug`); log return and recovery totals, toggle the quarantine-retry pause, audit after next frame, and since I19 "I19: Log siege totals and pending retreat rosters" (`.Routing`). Since I18 `DescribeStatus` shows the battle kind and, for a siege, the settlement, kind and gates, and each side's owner, rate, edge and settlement-only count.  
Events: `BattleStarted(BattleContext)` (after commit, the eligibility rolls and the battle-start freeze, before the first spawn), `UnitSpawned(BattleSide, UnitHandle)` (after activation, registration and listing), `UnitRetreated(BattleSide, UnitHandle)` (I15: once per unit per battle when it starts retreating, never at its escape, `BATTLE-RETREAT-04`), `ResultLocked(BattleContext, BattleResult)` (after every lock effect), `EndDelayElapsed(BattleContext)` (still locked and current; a handler may abort), `BattleEnded(BattleContext, BattleResult)` (normal end only, after routing, the lock release, the arena's removal and the unfreeze; possibly late after a recovered end; never on an abort); handlers isolated. Subscribes to the map's `ContactReported`, the arena's `ArenaRemoved`, each deployed unit's `UnitDeath2D.Died` and each retreat-eligible field unit's `UnitHealth2D.HealthChanged` (I15); sets each body's escape callback.  
Invariants: `BATTLE-LIFE-01`, `BATTLE-START-01`–`BATTLE-START-03`, `BATTLE-SIEGE-01`, `BATTLE-SIEGE-02`, `BATTLE-EDGE-01`, `BATTLE-SPAWN-01`, `BATTLE-SPAWN-02`, `BATTLE-UNIT-01`–`BATTLE-UNIT-04`, `BATTLE-ABORT-01`, `BATTLE-END-01`–`BATTLE-END-04`, `BATTLE-LOCK-01`, `BATTLE-LOCK-02`, `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `BATTLE-RECOVER-01`, `BATTLE-AUDIT-01`, `BATTLE-RETREAT-01`–`BATTLE-RETREAT-04`, `BATTLE-DELIVER-01`–`BATTLE-DELIVER-03`; `SIDE-01`, `SIDE-02`; `WM-OWN-01`, `WM-IMMUNE-01`, `WM-CAPTURE-01`; `ARENA-BODY-01`; `MOD-APPLY-01`; `ORDER-01`.  
Never owns: units (rosters own them; status and ownership changes, Escaping included, go through `UnitReserve2D`), faction, the map freeze or cameras (arena), arena body tracking and retreat steering (arena), pair- and army–settlement immunity storage, return-spot geometry, the capture itself, pending retreat rosters and army recovery (world map), the combat gates themselves (`CombatLock2D` and the three combat components that read it), modifier instances and their lifetime (`UnitModifierRuntime2D`).

| Partial source | Responsibility |
| --- | --- |
| BattleManager2D.cs | Serialized config (including `logRetreats`, off; since I17 it also logs battle-start deliveries and frozen lists), all state (including the I17 delivery hold and last committed context), events and public properties, all Unity messages (Awake, Start, FixedUpdate, OnDisable, OnDestroy, OnApplicationQuit) and the step order, subscriptions, event raisers; declares `[DefaultExecutionOrder(-12000)]` |
| BattleManager2D.Start.cs | `ContactReported` trigger, pre-arena checks (including return placements), sides and edges, arena creation, final checks, commit (with the eligibility rolls and the I17 battle-start freeze), refusal wait or pair immunity; since I18 the siege trigger, siege checks and context, the setup and no-return-room refusals, and the commit inside the preparation's exception handling |
| BattleManager2D.Spawning.cs | Battle-unit container, per-side credit ticking, reservations, the per-unit deployment sequence (with the activation frame, retreat subscription, the I17 battle-start request and the deployment check; since I18 owner and faction checks against the side's roster, and the settlement-only drop on army sides only) and its exception recovery |
| BattleManager2D.Ending.cs | End check and death batches, timeout, result lock (with the delivery drop), end delay, normal end (cleanup → routing → `BattleEnded`), abort |
| BattleManager2D.Units.cs | Shared unit removal (park, quarantine and its retry, escaped parking, corpses), death, escape, sweep of lost units, lost-unit reports, detach (which also ends retreat tracking and drops deliveries), probe |
| BattleManager2D.Routing.cs | Routing, army return and immunity calls; since I19 the siege outcome, the capture call with the quarantine parking, the refused-capture handling and the siege return, with the two siege counters; idle recovery (pending end, orphan arena); post-battle audit; the three I13 context menus and the I19 siege-totals menu |
| BattleManager2D.Retreat.cs (I15) | Eligibility rolls, the low-health latch and its subscription, the deployment check, retreat starts (since I18 refused for settlement-only units, even forced), the retreat counters |
| BattleManager2D.Delivery.cs (I15, I17) | `BattleModifierDeliveryKind`, the request queue, the wait for start and the debug hold, application through `ApplyOrdered`, drops, the delivery counters; since I17 the field and siege freezes, the battle-start request and its counters |
| BattleManager2D.Debug.cs | Development tools (including the I15 retreat and health tools and the I17 battle-start audit, unit listing and hold toggle), status text (since I18 with the siege shape and each side's owner) and context menus |

### BattleTypes2D [WM4, WM5, WM7, WM8, WM9]
Owns: `BattleKind {Field, Siege}` (Siege since I18); `SiegeOutcomeKind {None, AttackerWins, DefenderWins, BothLose, Timeout, Aborted}` (I19); `BattleOutcome {Decisive, BothLose}`; `BattleEndReason {Elimination, Timeout}`; the immutable `BattleResult`; `BattleContext`; `BattleSide`; `BattleUnit`. Index records only: they reference units and never own them.  
API: `BattleResult`: `Outcome`, `Reason`, `IsDecisive`, `Winner` (meaningful only when Decisive), `Loser`, `FieldCountA`/`B`, `QueuedCountA`/`B`, `FinalBatchDeathsA`/`B`, `LockStep`, `LockTime`, `ElapsedSeconds`, `ToString()`. `BattleContext`: `Id`, `Kind`, `Contact` (the pick; holds live army references), `MapPoint`, `Layers`, `HorizontalAxis`, `AxisChosenRandomly`, `SideA`, `SideB`, `StartTime`, `CheckedSteps`, `Result`, `IsResultLocked`, `LockTime`, `EndDelayElapsed`, `IsEnded`, `EndTime`, `UnitsParkedAtEnd`, `EscapedParkedAtEnd`, `CorpsesRemovedAtEnd`, `ProjectilesRemovedAtEnd`, `HasReturnPlacements`, `ReturnPlacementA`/`B`, `GetReturnPlacement(ArenaSide)`, `WasAborted`, `IsRouted`, `ReturnSeparated`, `ImmunityGranted`, `ReturnNote`, `Get(ArenaSide)`; since I15 `RetreatRolled`, `RetreatChance`, `RetreatHealthFraction`, `RetreatModifier` (frozen at commit) and `IsRetreatEligible(UnitHandle)` (either side); since I17 `BattleStartModifiersFrozen`, `BattleStartModifierSource` and internal `SetBattleStartModifiers(sideA, sideB, source)` (lists must already be null-free copies; a second call throws `InvalidOperationException`); internal `LockResult` (throws if a result already exists), `MarkEnded` (throws without a result; repeated calls do nothing), `SetReturnPlacements`, `SetRetreatSettings`; since I18 the siege fields `Settlement`, `SiegeKind`, `SiegeBattlefieldPrefab`, `GateEdges` (all four for a field battle), `IsSiege`, `Defender` and `Attacker` (null for a field battle), `HasSiegeReturnPlacement`, `SiegeReturnPlacement` and internal `SetSiegeSetup`, `SetSiegeReturnPlacement`; since I19 `SiegeOutcome`, `SiegeCaptured`, `SiegeCaptureError`, `SiegeAttackersGarrisoned`, `SiegeEscapedDefenders`, `SiegeSettlementOnlyKilled`, `SiegePendingRetreatId` and static `ClassifySiegeOutcome(BattleResult) → SiegeOutcomeKind` (null → Aborted). `BattleSide`: `Context`, `Side`, `Army` (null for a garrison side), since I18 `Settlement`, `IsGarrison`, `OwnerLabel` ("army #id" or "garrison Sid") and `StartingSettlementOnlyCount`, `Roster`, `Faction`, `Edge`, `StartingCount`, `SpawnRate`, `CreditCap`, `QueuedCount`, `UnitsOnField`, `UnitsEscaped` (kept after a normal end as the routing index; cleared by an abort), `FieldCount`, `RemainingCount` (field + queued), `Spawned`, `Dead`, `Escaped`, `Dropped`, `UnitsAfterBattle` (−1 until the end or abort), `RetreatingAfterBattle`, `ArmyRemovedAfterBattle`, `DebugHold` (get/set), `IsBlocked`, `BlockedSteps`, `BlockEpisodes`, `MaxSpawnedInOneStep`, `MaxSpawnedInOneSecond`, `FirstSpawnTime`, `LastSpawnTime`; since I15 `IsRetreatEligible(UnitHandle)`, `RetreatEligibleCount`, `Retreated`, `RetreatModifiersApplied`; since I17 `BattleStartModifiers` (a read-only view of the frozen list; empty = none), `BattleStartModifierCount`, `BattleStartRequested`, `BattleStartDelivered`, `BattleStartDropped`, `BattleStartEntriesApplied`, `BattleStartEntriesRejected` (per battle; requested = delivered + dropped + pending) and internal `StartModifiers`, `SetBattleStartModifiers`; internal queue, queue index, credit, field and escaped lists, `BatchDeaths`, `RetreatEligibleUnits` (I15; a handle set, an index). `BattleUnit`: `Side`, `Context`, `Handle`, `Body`, `Health`, `Death`, `Motor`, `Sidestep` (optional), `Targeting` (optional), `Destination`, `Rigidbody`, `ReactiveTurnBoost` (optional, I12); since I15 `RetreatEligible`, `RetreatTriggered`, `IsRetreating`, `RetreatStartTime` (NaN until it starts), `RetreatModifierApplied`, and internal `ActivationFrame`, the `HealthChanged` handler, `RetreatStartFailed` and `DeliveryKindsRequested` (one bit per delivery kind); since I17 `BattleStartRequested`, `BattleStartDelivered` (`ApplyOrdered` was called), `BattleStartEntriesApplied`, `BattleStartDeliveryLevel`, `BattleStartDeliveryFrame` (−1 until delivered), `BattleStartDeliveryTime` (NaN until delivered), `BattleStartHolderStarted` (the wait's holder condition at delivery; true without a holder). A `BattleUnit` is per deployment, and a unit deploys at most once per battle, so its retreat state is per battle. Constructors are internal: since I18 one per side kind (army; garrison, the whole garrison as its queue).  
Events: none.  
Invariants: `BATTLE-SIEGE-01`, `BATTLE-SIEGE-02`, `BATTLE-START-02`, `BATTLE-SPAWN-01`, `BATTLE-END-01`, `BATTLE-END-02`, `BATTLE-LOCK-01`, `BATTLE-ROUTE-01`, `BATTLE-RETURN-01`, `BATTLE-RETREAT-01`–`BATTLE-RETREAT-04`, `BATTLE-DELIVER-01`, `BATTLE-DELIVER-03`; `WM-OWN-01`.  
Never owns: behaviour, units or ownership.

### BattleSides2D [WM4, WM9]
Owns: static field side mapping and siege side constants (used since I18: defender A, attacker B; the file was not re-attached at M09, and its use is taken from the callers); the `BattleSideLayers2D` value type (resolved body and Clash layers per side).  
API: `BattleSides2D`: consts `SiegeDefenderSide` (A), `SiegeAttackerSide` (B); `Opposite(ArenaSide)`; `TryGetFieldBattleSides(FactionId first, FactionId second, out ArenaSide firstSide, out ArenaSide secondSide) → bool`. `BattleSideLayers2D`: `BodyA`, `BodyB`, `ClashA`, `ClashB`, `IsValid`, `BodyLayer(ArenaSide)`, `ClashLayer(ArenaSide)`, `EnemyBodyMask(ArenaSide)`, `EnemyClashMask(ArenaSide)`, static `TryCreate(BattleRules2D, out BattleSideLayers2D, out string) → bool`, `LogCollisionMatrixProblems()`, `ToString()`.  
Events: none.  
Invariants: `SIDE-01`; `SETUP-07`; reports matrix problems, never edits project settings.  
Never owns: faction, unit layers or masks (the binder writes them), `BattleLayers2D` resolution.

### UnitSideBinder2D [WM4]
Owns: nothing persistent (a reusable detector list, cleared after each call).  
API: static `TryBind(GameObject unitRoot, ArenaSide side, in BattleSideLayers2D layers, out string error) → bool`.  
Events: none.  
Invariants: `SIDE-02`; `TARGET-02`, `CLASH-01`, `SETUP-01`, `SETUP-03`; `CORE-03`.  
Never owns: the masks themselves (targeting, detector and holder own their fields), faction, Wall masks, casts already made.

### UnitSideBindingTest2D [WM4]
Owns: an optional Play Mode check (Editor and development builds only; the release class is empty). For each assigned prefab it binds a never-awakened clone A → B → A and checks layers, masks, untouched child layers, faction and `didAwake`; optionally it wakes the clone briefly far away to check a rebind after Awake and the refusal of an active unit; everything is destroyed afterwards. A second context menu logs the side matrix report.  
API: context menus "Run side binding test" and "Log side-layer matrix problems"; serialized rules (fallback: the arena's), prefabs, wake option and position, run on Start.  
Events: none.  
Invariants: `SIDE-01`, `SIDE-02`; debug only.  
Never owns: anything gameplay depends on.

### BattleManagerDebugPanel2D [WM4, WM8-S, WM9-S]
Owns: optional IMGUI window (collapsible). It shows: battle status ("#id running" or none, not distinguishing a locked battle) and whether a battle started this session; refused starts, preparation failures, aborts, lost units and the last start message. Since I18 the battle line gives the kind: for a siege the settlement, kind, gates and battle-start source, else the field axis. Per side it shows the owner label (army or garrison), faction, edge, starting count with any settlement-only units, and rate; queued and field counts (entering, inside, retreating by the arena body's flag, with a target, in Knockback); spawned, dead, escaped and dropped; the spawn span, maximum per step and per second, and blocks; since I17 a battle-start line (list size, requested, delivered, dropped, entries applied and rejected). Tools: per-side Hold (simulated block); one-shot forced preparation failure; since I17 a "Hold modifier deliveries" toggle with the pending count (`DebugHoldModifierDeliveries`); Retreat N; "Cancel retreat"; eject a random unit; abort; probe readout and reset; unit, arena and, since I17, battle-start audits, each logged to the Console with the first line shown in the window.  
API: serialized manager (fallback `Instance`) and window rect; no public methods.  
Events: none.  
Invariants: debug only; `BATTLE-DELIVER-01` (the hold), `BATTLE-DELIVER-03` (the audit).  
Never owns: any gameplay rule; production code must not depend on it.  
Re-attached for M08 and again for M09 (I18 added the siege and owner readouts). It has no readouts for the Phase 5 lock, end, quarantine or recovery counters; those are in `DescribeStatus` and the manager's context menus. "Cancel retreat" calls `DebugSetRetreating(int.MaxValue, false)`, which does nothing since I15 (retreat is latched, `BATTLE-RETREAT-03`), so it reports "0 units stopped retreating". Whether I17's tests used the panel's hold toggle or the context menu is not stated.

### CombatLock2D [WM5]
Owns: the static set of locked unit root GameObjects (reference identity, so destroyed objects never merge) and the blocked-damage counter. Lives in the combat layer, so combat components never reference battle types.  
API: `IsLocked(GameObject) → bool`; `Lock(GameObject) → bool` (called by `BattleManager2D` only); `Unlock(GameObject) → bool` (safe for destroyed objects; removing twice is harmless); `LockedCount`; `BlockedDamageCount`; `ResetCounters()`; internal `RecordBlockedDamage()`. Statics reset at subsystem registration, so nothing survives into the next Play Mode session when domain reload is off.  
Events: none.  
Invariants: `BATTLE-LOCK-01`; read by `HEALTH-01`, `CLASH-05`, `ABILITY-02` and `TRIGGER-04`.  
Never owns: when to lock or unlock (`BattleManager2D`), battle results, how each gate rejects (the three combat components).
