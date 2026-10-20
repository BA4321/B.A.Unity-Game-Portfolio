# Stick Legions system map — V9 modular core

Always attach this file. Attach only the subsystem files relevant to the task. This map describes autonomous top-down 2D combat: targeting and movement, Clash transactions, damage/death/XP, independent modifiers, and normal/triggered abilities; plus the world map of bouncing armies, settlements, persistent parked units with rosters and garrisons, deployment, overflow, allied interactions and contact classification; plus the battle arena (camera swap, terrain, perimeter, entry strips, retreat gates), so far exercised only with debug test bodies. It is an integration contract, not a replacement for source code.

## Rules most likely to be broken — read first

These are navigation pointers; the cited IDs hold the complete rules in their owning files.

| Fragile boundary | Read before changing it |
| --- | --- |
| Both participants prepare before either snapshot; both controllers settle before both read barriers | Clash: `CLASH-07`, `CLASH-08`, `CLASH-09`; Modifiers: `STAT-04` |
| Both results → captured consumption → both XP awards → both reactions → triggered casts | Clash: `CLASH-10`, `CLASH-11`, `CLASH-12`, `CLASH-13`, `CLASH-14` |
| Ordinary stat reads can notify; protected reads cannot mutate | Modifiers: `STAT-01`, `STAT-04` |
| Nested damage finalizers must respect victim-side XP deferral | Health: `XP-02`, `XP-03` |
| One-Clash consumption preserves pending triggers; explicit removal cancels them | Abilities: `TRIGGER-06` |
| Pre-Awake/inactive grants are supported; initialized progression is still a casting prerequisite | Abilities: `ABILITY-01`, `ABILITY-02`; Core: `CORE-03` |
| Result lock and gates closing at battle lock are not established in this map | `CORE-02` |
| A target staying in band can retry Sidestep after cooldown | Movement: `SIDESTEP-GATE-01`, `SIDESTEP-GATE-02` |
| Payload selection freezes at cast; modifier values freeze at application | Abilities: `ABILITY-03`, `DELIVERY-03` |
| Bounce is Army2D-owned and independent of classification; classification reads live state one step later | World map: `WM-MOVE-02`, `WM-CONTACT-04` |
| Contact handlers may freeze mid-dispatch; removed armies must be ignored | World map: `WM-CONTACT-05`, `WM-REMOVE-01` |
| Only `UnitReserve2D` changes unit ownership; parked units never run Awake; reads use serialized data or `didAwake` | World map: `WM-OWN-01`, `WM-UNIT-01`, `WM-UNIT-02`; Core: `CORE-03` |
| Populated armies are announced only after being filled; owners holding units are never destroyed silently | World map: `WM-DEPLOY-02`, `WM-OWN-02`, `WM-REMOVE-01` |
| Allied effects run after dispatch, reclassify from live state, and stop if anything freezes the map | World map: `WM-ALLY-01`, `WM-TIME-01` |
| The arena owns the map freeze; an unfreeze from elsewhere aborts it; teardown order is fixed | Battle: `ARENA-LIFE-02`, `ARENA-LIFE-03`; World map: `WM-FREEZE-01` |
| The arena never owns body GameObjects: never parent real units under its Root, and unregister a body before deactivating or destroying it | Battle: `ARENA-BODY-01` |
| A centre outside the view is a leak unless announced (`ForceReenter`) or caused by an aspect change; Sidestep may start only when `IsFullyInside` | Battle: `ARENA-BODY-02` |

## Reading and maintenance contract

- **CORE-01 — One home per rule.** Bold ID definitions are authoritative within this map. Tables/cards elsewhere are indexes and pointers, not additional rule definitions. Keep an ID stable when moving or refining its rule; never reuse it for unrelated behavior. New behavior receives a new ID. When editing code, cite the IDs relied on and identify any whose behavior changed. Source is needed for exact signatures, numeric tuning, validation implementation and lifecycle callbacks touched by the task.
- **CORE-02 — Documented scope.** The inventory is 79 source files / 67 logical entries: seven Sidestep partials form one entry/component, five WorldMapController2D partials form one entry/component, and three BattleArena2D partials form one entry/component.
  - **Established by Roadmap V6 Phases 1–2** (`07-WorldMap-Battle.md`):
    - world-map armies and settlements;
    - persistent units parked in a reserve, with one owner each;
    - army rosters and settlement garrisons, with FIFO/HighestLevelFirst order;
    - recruitment, gated transfers, manual deployment into the world map, overflow and placement;
    - the three allied contact interactions (retreat transfer, retreat merge, garrison join).
  - **Established by Roadmap V6 Phase 3** (`07-WorldMap-Battle.md`), driven by debug tools:
    - arena creation and teardown far from the map, owning the map freeze, with a world/battle camera swap;
    - battlefield selection by map region, with fallback and generated ground;
    - a shrink-only battle view sized from unit counts;
    - a perimeter in two modes, entry strips and entry-point search;
    - entry tracking of registered bodies (Entering/Inside, leaks, re-entries);
    - retreat gates with escape of registered retreating bodies;
    - the `UnitDestinationOverride2D` value component;
    - debug test bodies and an arena audit.
  - **Not implemented:**
    - unit activation, registration of real units with the arena, battle deployment or battle start from a contact (no `BattleManager2D`);
    - side layers: unit body layers are still the faction layers (`FactionABody`/`FactionBBody`); masks are not rebound per deployment;
    - motor or Sidestep reading the destination override (`MOVE-01` and the Sidestep rules are unchanged);
    - capture, sieges, retreat eligibility of real units, army retreat timers/recovery;
    - automatic setting of the army retreat flag (it is set only by `SetRetreating`);
    - battle results, battle-result locking and post-lock gates (`BattleArena2D.EscapesAccepted` is the hook; only teardown closes it);
    - regeneration.
  - `IsRosterInBattle` always returns false. `UnitStatus` values other than Reserve and Discarded are declared but never set.
  - Neither post-lock immunity nor post-lock death is established. Consult the relevant roadmap/source before adding those rules. Summon is a damage-origin enum value without an established delivery system. No production ability-removal API is established.
- **CORE-03 — Initialization is not eligibility.** The initialization responsibilities below do not establish an extra cross-component Awake order. Do not infer initialized progression from component presence, or generalized inactive-unit behavior from the holder's supported pre-Awake `Grant`. The detailed behavior of other components before first activation is not established here. World-map recruitment creates units that have never run Awake and are kept parked (`WM-UNIT-01`). Reads through `UnitHandle` use serialized fields or a `didAwake` check (`WM-UNIT-02`), and `UnitCombatant2D.SetFaction` is safe on such units. `UnitDestinationOverride2D` has no Unity messages and is safe on parked units (`ARENA-DEST-01`). This establishes parking only; behaviour on first activation remains unestablished until the battle spawn path exists.
- **CORE-04 — Evidence tags.** `[V8]`, `[V7]`, `[SID]`, `[V6]`, `[F]`, `[B]`, `[K]` identify inherited evidence, not fresh source or runtime verification. The V9 restructure inspected only the supplied map. World-map and battle tags record the basis of each Roadmap V6 rule: `[WM1]`, `[WM2]`, `[WM3]` — written from that phase's source and covered by a named Unity result the user reported for that phase; `[WM1-SR]` — source-reviewed only at the time; `[WM2-S]`, `[WM3-S]` — that phase's source only. A bracket such as `[WM3: I07 Retreat 5]` names the covering result; clauses outside it are source-derived. `90-Evidence.md` defines every tag. Configuration tables state requirements; they do not claim actual prefab/scene compliance.

### Load routing

| Task | Files in addition to this core |
| --- | --- |
| Targeting, movement, Sidestep | `01-Movement-Sidestep.md`; add Modifiers if changing effective stats |
| Clash resolution or ordering | `02-Clash-Transaction.md`, `03-Health-Death-XP.md`, `04-Stats-Modifiers.md`; add Abilities when touching trigger capture/dispatch |
| Damage, death, kill XP | `03-Health-Death-XP.md`; add the relevant producer: Clash, Modifiers or Abilities |
| Modifier scaling, lifetime, scheduled work | `04-Stats-Modifiers.md`; add Health for DoT or Abilities for triggers |
| Abilities and triggered casts | `05-Abilities-Triggered-Casts.md`; add Clash for transaction boundaries, Modifiers for request lifetime, Health for damage/XP |
| Health/level display | `06-UI.md`; add Health if changing emitted events |
| World map, armies, settlements, contacts, rosters, deployment | `07-WorldMap-Battle.md`; add combat files only when a task crosses into battle or touches a unit integration point (faction setter: Movement; `settlementOnly`: Core; definition access: Modifiers) |
| Battle arena, camera swap, perimeter, entry and retreat steering, battle layers | `07-WorldMap-Battle.md`; add `01-Movement-Sidestep.md` when wiring the destination override into motor or Sidestep; add the combat file owning any mask being rebound (targeting: Movement; detector: Clash; abilities: Abilities) |
| Audit provenance or restructure | `90-Evidence.md`, `91-ChangeLog.md`, `92-Validation.md` — omit from implementation chats |

Follow cross-file IDs only when that boundary is affected. A broad transaction change may legitimately require more than three subsystem files. Request exact source filenames from the inventory rather than attaching the entire project.

## Authoritative ownership

| Concern | Owner |
| --- | --- |
| Faction identity and enemy relation (changeable only while parked) | `UnitCombatant2D` |
| Target selection/search deadlines | `UnitTargeting2D` |
| Major state, clean forward speed, applied Sidestep bonus, Rigidbody motion and Knockback | `UnitMotor2D` |
| Sidestep observed target/retry cooldown | `UnitSidestep2D.Eligibility.cs` |
| Sidestep lifecycle/bonus bookkeeping | `UnitSidestep2D.Lifecycle.cs` |
| Sidestep geometry/progress/movement intent | `UnitSidestep2D.Path.cs` |
| Sidestep Wall queries/classification | `UnitSidestep2D.WallClearance.cs` |
| Current health and authoritative damage result | `UnitHealth2D` |
| One-time death and destruction scheduling | `UnitDeath2D` |
| Vulnerable expiration | `UnitVulnerability2D` |
| Level, XP, requirements and bounty | `UnitProgression2D` |
| Per-victim lethal attribution, pending award and deferral depth | `UnitKillExperience2D` |
| Effective stat calculation and final bounds | `UnitRuntimeStats2D` |
| Contributions, handle/bundle identity, aggregates and legacy timed expiry | `UnitStatModifierController2D` |
| Independent modifier instances, frozen values, lifetime, visual leases, pending trigger requests | `UnitModifierRuntime2D` |
| Modifier authored settings / validation and scaling | `ModifierDefinition2D` / `ModifierValueResolver2D` |
| Scheduled ordering, cancellation identities and global creation sequence | `ModifierScheduler2D` |
| Pooled modifier clones and lease generations | `ModifierVisualPool2D` |
| Reactive loss/Wall filters, subscriptions and two source handles | `UnitReactiveTurnBoost2D` |
| Unit Clash cooldown, snapshots, trigger capture, participant effects and retained reaction record | `UnitClashParticipant2D` |
| Batch/pair deduplication, preparation, paired capture/consumption, result/XP/reaction/trigger boundaries | `ClashDirector2D` |
| Two Dodge samples and two Knockback-angle samples | `ClashDirector2D` |
| Numerical Clash resolution / pure Dodge arithmetic | `ClashCalculator2D` / `DodgeCalculator2D` |
| Trigger group ordering, saved aim, per-request invocation and diagnostics | `ModifierAbilityTriggerDispatcher2D` |
| Ability ownership, normal timers/selection, triggered entry and shared execution guard | `UnitAbilityHolder2D` |
| Authored ability values / frozen cast construction | `AbilityDefinition2D` / `AbilityValueResolver2D` |
| Category preparation and committed dispatch | `AbilityExecution2D` |
| Area collection and root-distance filtering | `AbilityAreaQuery2D` |
| Ability recipient gates, damage/XP and modifier delivery | `AbilityDelivery2D` |
| Projectile flight, expiry, encounter memory and impact Dodge sampling | `AbilityProjectile2D` |
| Scene-owned ability visual lifetime/world scale | `AbilityVisuals2D` |
| Health-bar/level-sprite display | `UnitStatsUI2D` |
| Base unit configuration / shared Clash configuration | `UnitDefinition2D` / `ClashRules2D` |
| World-map registry, army/settlement ids, bounds, MapTime, PhysicsStep, freezing, army creation, army removal | `WorldMapController2D` |
| Contact intake/deduplication, touch episodes, report dispatch, battle-candidate selection | `WorldMapController2D.Contacts.cs` |
| Reserve creation, ownership audit, deferred-recruitment flush, owner-loss handling | `WorldMapController2D.Units.cs` |
| Deployment requests, placement, populated army creation/rollback, overflow | `WorldMapController2D.Deployment.cs` |
| Allied interactions, transfer gate and gated transfers | `WorldMapController2D.Interactions.cs` |
| Parked storage, unit registry/ids, arrival sequence, every unit-ownership change | `UnitReserve2D` |
| One owner's unit membership, arrival-order storage, read order, capacity, garrison faction label | `UnitRoster` |
| Persistent unit identity and wake-safe reads | `UnitHandle` |
| Contact table, intents and role order | `WorldMapContactClassifier2D` (in `WorldMapContacts2D`) |
| Army direction, bounce, constant velocity, retreat flag, army roster, source-settlement collision ignore | `Army2D` |
| Settlement kind, faction, radius, garrison roster, recruitment entry, pending deployment request and retry timers | `Settlement2D` |
| World-map tuning / faction colours / WM layer indices | `WorldMapRules2D` / `FactionPalette2D` / `WorldMapLayers2D` |
| Arena lifecycle, Root and its contents, map-freeze ownership while an arena exists, view sizes, arena audit | `BattleArena2D` |
| Perimeter walls, retreat gates, entry-strip geometry, shrink stepping | `BattleArena2D.Perimeter.cs` |
| Registered arena bodies, entry state, wall ignores, arena-set overrides, leak/re-entry/escape/lost counters | `BattleArena2D.Bodies.cs` |
| World/battle Camera enablement, battle camera pose and size | `BattleCamera2D` |
| A unit's destination override value and its `Changed` event | `UnitDestinationOverride2D` |
| Battle tuning and battlefield regions / battle layer indices | `BattleRules2D` / `BattleLayers2D` |

## Source inventory and one-line roles

All names below are `.cs` files. Card home identifies the one place with the script's contract card. Partial responsibilities live in the single Sidestep card, the single WorldMapController2D card and the single BattleArena2D card.

| Source basename | Role | Card home |
| --- | --- | --- |
| UnitDefinition2D | Shared base unit, progression, movement, Sidestep and reactive configuration | Core |
| ClashRules2D | Shared targeting/Clash timing, calculation and Vulnerable configuration | Core |
| CombatTypes2D | Faction, major state and immutable Clash communication types | Clash |
| CombatDamageTypes2D | Generalized damage request/result types | Health |
| CombatModifierTypes2D | Stat entries, handles, bundles, scopes and embedded bundle mapper | Modifiers |
| UnitCombatant2D | Identity and targetability facade | Movement |
| UnitTargeting2D | Nearest valid enemy selection | Movement |
| UnitMotor2D | State, steering, forward momentum, Knockback and intent execution | Movement |
| UnitSidestep2D | Single component coordinator and Unity entry points | Movement |
| UnitSidestep2D.Eligibility | In-band opportunities and unit-wide retry cooldown | Movement |
| UnitSidestep2D.Lifecycle | Session start/end and exact bonus cleanup | Movement |
| UnitSidestep2D.Path | Snapshotted half-circle and once-per-step intent | Movement |
| UnitSidestep2D.WallClearance | Full-body Wall preflight and contact classification | Movement |
| UnitSidestep2D.Interruptions | Motor subscription and active-session validity | Movement |
| UnitSidestep2D.Clash | Accepted-Clash cancellation and clean velocity rebuild | Movement |
| UnitProgression2D | Level/XP advancement and bounty | Health |
| UnitVulnerability2D | Extend-only Vulnerable timer | Health |
| UnitRuntimeStats2D | Derived effective stats | Modifiers |
| UnitStatModifierController2D | Contribution storage, aggregation and notifications | Modifiers |
| UnitReactiveTurnBoost2D | Timed loss/Wall additive turn effects | Modifiers |
| UnitHealth2D | Authoritative damage and health mutation | Health |
| UnitDeath2D | One-time death lifecycle | Health |
| UnitKillExperience2D | Victim attribution and deferred award | Health |
| UnitClashDetector2D | Clash Range trigger-to-request adapter | Clash |
| ClashDirector2D | Scene batch and paired transaction owner | Clash |
| ClashCalculator2D | Pure pair calculation | Clash |
| DodgeCalculator2D | Pure shared chance/sample helper | Clash |
| UnitClashParticipant2D | Unit cooldown, capture and result orchestration | Clash |
| ModifierDefinition2D | Modifier authoring and child/trigger references | Modifiers |
| ModifierFrameworkTypes2D | Context, resolved values, instance/schedule identities | Modifiers |
| ModifierValueResolver2D | Pure modifier validation/scaling | Modifiers |
| UnitModifierRuntime2D | Independent instances and pending trigger ownership | Modifiers |
| ModifierScheduler2D | Shared fixed-time scheduled work | Modifiers |
| ModifierVisualPool2D | Persistent modifier visual pool | Modifiers |
| ModifierApplicationDebug2D | Optional manual modifier diagnostics | Modifiers |
| ModifierAbilityTriggerTypes2D | Copied opportunities and leased capture buffers | Abilities |
| ModifierAbilityTriggerDispatcher2D | Transaction-local trigger execution/diagnostics | Abilities |
| AbilityDefinition2D | Ability category, values, modifier list and art settings | Abilities |
| AbilityFrameworkTypes2D | Invocation, cast, request-owner and result contracts | Abilities |
| AbilityValueResolver2D | Pure ability resolution and cast capture | Abilities |
| UnitAbilityHolder2D | Ownership, timers, selection and cast commitment | Abilities |
| AbilityExecution2D | Prepared category dispatch | Abilities |
| AbilityAreaQuery2D | Complete deduplicated Area collection | Abilities |
| AbilityDelivery2D | Synchronous recipient bridge | Abilities |
| AbilityProjectile2D | Independent projectile physics and encounters | Abilities |
| AbilityVisuals2D | Per-scene ability presentation service | Abilities |
| AbilityDebugGrant2D | Optional grants and normal-state diagnostics | Abilities |
| UnitStatsUI2D | Event-driven health and level display | UI |
| WorldMapController2D | Single map controller: registry, bounds, map time, freezing, spawning | WorldMap |
| WorldMapController2D.Contacts | Contact intake, deduplication, episodes, classification dispatch, battle pick | WorldMap |
| WorldMapController2D.Units | Reserve, audit, deferred-recruitment flush, owner loss | WorldMap |
| WorldMapController2D.Deployment | Deployment, placement, overflow, settlement work | WorldMap |
| WorldMapController2D.Interactions | Allied interactions, gated transfers, interaction record | WorldMap |
| Army2D | Constant-speed bouncing army marble owning a unit roster | WorldMap |
| Settlement2D | Static town/castle with garrison, recruitment and pending deployment | WorldMap |
| UnitReserve2D | Parked-unit storage and the only ownership mutator | WorldMap |
| UnitRoster | Logical owner of units (garrison or army) | WorldMap |
| UnitHandle | Persistent unit identity | WorldMap |
| UnitRosterTypes2D | Unit status, roster kind/order, transfer and recruit results | WorldMap |
| UnitPrefabCatalog2D | Debug list of recruitable unit prefabs | WorldMap |
| WorldMapContacts2D | Contact kinds/intents, pair key, report and pure classifier | WorldMap |
| WorldMapTypes2D | Settlement kind, spawn reason, deploy request/result and placement types | WorldMap |
| WorldMapRules2D | Shared world-map tuning asset | WorldMap |
| FactionPalette2D | Faction colours with generated fallback | WorldMap |
| WorldMapLayers2D | WM layer resolution and matrix reporting | WorldMap |
| WorldMapMath2D | Pure rotation and axis-nudge helpers | WorldMap |
| WorldMapAssets2D | Generated placeholder sprites, material and labels | WorldMap |
| WorldMapDebugPanel2D | Optional IMGUI spawn, roster, deployment and contact diagnostics | WorldMap |
| BattleArena2D | Arena service: lifecycle, map freeze, camera swap, terrain, teardown, audit | WorldMap |
| BattleArena2D.Perimeter | Walls, retreat gates, entry strips, shrink | WorldMap |
| BattleArena2D.Bodies | Body registration, entry/leak tracking, escapes, entry points, test bodies | WorldMap |
| BattleCamera2D | World/battle camera swap and battle view size | WorldMap |
| BattleRules2D | Battle tuning, battlefield regions and selection | WorldMap |
| BattleLayers2D | Battle layer resolution and matrix reporting | WorldMap |
| BattleArenaTypes2D | Edges, sides, perimeter modes, body states, ArenaBody | WorldMap |
| BattleArenaGate2D | Retreat gate trigger reporter | WorldMap |
| UnitDestinationOverride2D | Destination that replaces target steering while active (unit integration point) | WorldMap |
| BattleTestBody2D | Debug stand-in unit body | WorldMap |
| BattleArenaDebugPanel2D | Optional IMGUI arena and test-body diagnostics | WorldMap |

## Execution and initialization

**ORDER-01 — Declared callback order.** Lower values execute earlier within the same callback phase. These attributes do not create loops, guarantee relative order for equal values, or override unknown project settings.

| Component | DefaultExecutionOrder | Relevant phase/role |
| --- | ---: | --- |
| ModifierScheduler2D | -11000 | FixedUpdate: scheduled work before director |
| AbilityVisuals2D | -11000 | Update/LateUpdate only: presentation |
| ClashDirector2D | -10000 | FixedUpdate: close previous request batch; synchronous pair transactions |
| WorldMapController2D | -1000 | FixedUpdate: world map only. Classify/dispatch previous step's contacts → allied interactions → step/map time → settlement work, before Army2D (default 0) |
| BattleArena2D | -900 | FixedUpdate: battle arena only. Shrink step → perimeter layout on the live view → body tracking and Entry/Retreat overrides, before default-order scripts (test bodies). By declared order its Awake follows the world-map controller's, so WM layers are normally resolved for its matrix report (source; not separately tested) |
| UnitCombatant2D | -300 | Facade lifecycle order |
| UnitDeath2D | -250 | Death callbacks; no fixed loop implied |
| UnitKillExperience2D | -225 | Attribution callbacks; no fixed loop implied |
| UnitHealth2D | -200 | FixedUpdate: maximum-health reconciliation |
| UnitTargeting2D | -200 | FixedUpdate: target maintenance |
| UnitSidestep2D | -150 | FixedUpdate: eligibility/session coordination |
| UnitMotor2D | -100 | FixedUpdate: physical execution |

World-map and combat components share no transaction or call path. The −1000 value is meaningful only relative to Army2D; its numeric position among combat values carries no designed ordering. `BattleArena2D` reaches the world map only through `WorldMapController2D.Instance` (when no map is assigned), `IsReady`, `IsFrozen`, `MapBounds`, `SetFrozen`, `FrozenChanged` and read-only `WorldMapLayers2D` indices, and calls no combat component in Phase 3. Its −900 places override updates before the unit components, which Phase 4's motor will rely on; that relation is not yet used. Retreat-gate escapes are reported from physics trigger callbacks (after the step's simulation), not from FixedUpdate. `BattleCamera2D` declares no order; its camera-count monitor runs in LateUpdate. Whether world map and combat run in one scene is not established. World-map ordering detail: `WM-TIME-01`. `UnitReserve2D`, `UnitRoster` and `UnitHandle` are plain C# objects with no Unity callbacks.

**ORDER-02 — Unspecified order and direct calls.** Health versus targeting at equal order is unspecified. Participant, detector, reactive boost, UI, holder and projectile declare no separate order. Sidestep partials add no separate component/order. Accepted-Clash preparation and triggered dispatch use synchronous director calls. No relative order among different holders/projectiles or between reactive Wall handling and Sidestep Wall cancellation is specified. Normal casts run in Update; starting grants in Start; triggers run in the director's FixedUpdate before movement. `Time.time` in that fixed callback is fixed-step time. Ability visual order does not place it in a FixedUpdate gameplay transaction.

**INIT-01 — Initialization responsibilities.** Progression initializes configured starting level/XP requirement/bounty; runtime stats derives queried values; health starts at effective maximum; motor starts at configured speed clamped to effective maximum; targeting allocates reusable queries. Sidestep resolves peers, validates geometry/configuration and prepares Wall queries. Reactive boost caches Wall and validates peers in Awake. Runtime attaches to its controller and subscribes while enabled; scheduler exists before scene load; modifier pool and ability visual hosts are lazy. Holder waits for initialized progression when necessary. Apply `CORE-03` when working with inactive units. World-map initialization: `WM-LIFE-01`, `WM-SETTLE-02`, `WM-UNIT-01`. Battle-arena initialization: `ARENA-LIFE-01`, `ARENA-CAM-01`; `UnitDestinationOverride2D` needs none. Recruited units run none of the unit initialization above until the battle spawn path activates them.

## Scene, placement and references

**SETUP-01 — Unit root and assets.** Required same-root peers are listed below; do not infer that components are automatically installed. The four `*Definition2D`/rules assets are shared ScriptableObjects. Definition assignment is authored separately. UI may be on root or child. Clash detector belongs on a Clash Range object and finds its participant in parents. A single scene director is required with Physics 2D simulation in Fixed Update mode; detector uses an assigned director or singleton fallback.

| Component | Required peers/configuration; exceptions |
| --- | --- |
| UnitCombatant2D | Motor, health |
| UnitTargeting2D | Unit definition, Clash rules, combatant, enemy-query mask |
| UnitMotor2D | Unit definition, Rigidbody2D, targeting, runtime stats; resolves Sidestep but motor validation does not require it |
| UnitProgression2D | Unit definition |
| UnitRuntimeStats2D | Unit definition, Clash rules, progression, vulnerability; controller optional, including valid idle-disabled controller |
| UnitHealth2D | Combatant, runtime stats, death |
| UnitDeath2D | Health, motor |
| UnitKillExperience2D | Combatant, health, progression |
| UnitClashParticipant2D | Unit definition, combatant, motor, same-root Sidestep, progression, runtime stats, vulnerability, health, kill XP; optional same-root holder |
| UnitSidestep2D | Unit definition, targeting, motor, body circle and geometry in `SETUP-02` |
| UnitReactiveTurnBoost2D | Unit definition; same-root participant, health, controller; exact Wall layer; invalid initialization disables component |
| UnitStatModifierController2D | No required serialized dependency |
| UnitModifierRuntime2D | Same-root combatant, health, death, controller; progression for convenience capture; kill XP for damage integration; no RequireComponent installation |
| UnitAbilityHolder2D | Same-root combatant/progression/health/death; targeting only for normal Projectile; starting list, enemy Clash and Wall masks; repairs wrong-root/missing peer references locally |
| UnitClashDetector2D | Parent participant, local trigger circle, director, nonempty opposing Clash mask |
| UnitStatsUI2D | Health/progression on enable; optional fill Transform, renderer and sprite array |

**SETUP-02 — Body and Wall.** Sidestep requires a solid centered same-root CircleCollider2D, zero offset, positive finite world radius and approximately uniform absolute X/Y world scale. Exact layer `Wall` must exist. Sidestep excludes triggers from preflight; both Sidestep and reactive contacts inspect the contacted collider's own layer. Body/Wall collision matrix must permit solid callbacks. Invalid Sidestep configuration/geometry/references disables Sidestep; missing/wrong-root Sidestep disables the participant. Reactive validation does not independently validate collider solidity, matrix or geometry.

**SETUP-03 — Ability colliders and masks.** Area uses enemy Clash layers: a queryable enabled collider's filled volume must contain its unit root throughout gameplay, with local detector/owner mapping to the same-root combatant. Detector enablement is not an Area gate; root containment is a prefab invariant, not runtime validation. Projectile contacts resolve combatants through parents. Collider/root layers may differ, but mappings must identify the same unit. Projectile layer interactions must permit intended enemy Clash and Wall contacts; masks cannot create disabled physics interactions. Modifiers introduce no new layer requirement.

**SETUP-04 — Projectile prefab.** Inactive, unparented root at scale one; enabled AbilityProjectile2D; exactly one enabled centered trigger CircleCollider2D and one Dynamic Rigidbody2D, simulation authored off, gravity/linear damping zero, exactly FreezeRotation constraints. Visual root is a direct child; no additional colliders/Rigidbodies anywhere below it. Payload/geometry initialize before object/simulation activation. Art faces local +X or uses a rotated visual child.

**SETUP-05 — Shared hosts and optional tools.** Scheduler is persistent and auto-created before scene load; modifier visual pool is persistent/lazy; neither goes on units. AbilityVisuals2D creates one host per loaded scene, not on units/effects. Its spawn needs finite position, positive world scale, supported duration, usable parent scale and same-scene parent. Calculator/dispatcher/execution/query/delivery are plain C#; resolvers/contracts/Dodge helper are not attachable components. Debug harnesses are optional; their production release shells are empty. Visual assignments and actual scenes/assets remain outside verification.

**SETUP-06 — World-map layers and controller.** Exact layers `WM_Army`, `WM_Settlement` and `WM_Bounds` must exist, or the controller disables itself and settlements log an error. `WM_Army` must collide with all three WM layers (error logged otherwise). Every WM layer must ignore every named non-WM layer (warning logged otherwise), so combat layers never interact with the world map. Settlement↔Settlement, Settlement↔Bounds and Bounds↔Bounds are not checked. Source only reports; it never edits the matrix. Armies and bounds receive layers in code; settlements set their own root layer in Awake. Exactly one controller, at the world origin with identity rotation and scale 1 (warning only). The world map depends on FixedUpdate collision callbacks; Physics 2D simulation mode is not validated by world-map source (`SETUP-01` already requires Fixed Update). World-map placement and overlap checks use `WorldMapLayers2D.AllMask` only. The parked-unit root is created in code under the controller with identity local transform, so with the controller at scale 1 a recruit keeps its prefab root's scale (keep it 1; Roadmap §5). Recruitable prefabs must pass `UnitReserve2D.ValidatePrefab`: a prefab asset whose root has `UnitCombatant2D` and a `UnitRuntimeStats2D` with a definition. Settlement requirements: `WM-SETTLE-01`.

**SETUP-07 — Battle camera, battle layers and arena placement.** Source reports problems; it never edits project settings.
- **Layers.** Exact layers `Perimeter` and `RetreatingPlace` must exist, and the two body layers named in `BattleRules2D` must exist and differ (today `FactionABody`/`FactionBBody`; Roadmap §4 renames them to `Unit_SideA`/`Unit_SideB` at the same indices, and the strings in the rules asset must change with them). Otherwise battle layers stay unresolved and no arena can be created. The `Wall` layer named there is optional: if missing, a warning is logged and entry points ignore terrain obstacles.
- **Matrix** (reported at arena Awake): `Perimeter` and `RetreatingPlace` must interact with both body layers (error). Body layers that do not collide with themselves or each other warn. `Perimeter` or `RetreatingPlace` interacting with any other named layer, including themselves and each other, warns; against a `WM_*` layer it is an error, but only when `WorldMapLayers2D` is already resolved (otherwise it is reported as a warning).
- **Cameras.** One `BattleCamera2D` with a world camera (assigned, else the `MainCamera`-tagged camera at Awake) and a battle camera on a different GameObject. Both GameObjects stay active; only Camera components are toggled. The battle camera renders to the screen (no target texture), should be Untagged, must not carry an enabled AudioListener (one is disabled with a warning) and is forced orthographic. The world camera should carry the enabled AudioListener (warning otherwise). Exactly one camera should be enabled at any time; other enabled scene cameras fail the monitor and the audit.
- **Arena.** Exactly one `BattleArena2D`. `ArenaCenter` must be far enough from `MapBounds` that the largest view at `ClearanceAspect`, plus ground margin and clearance, does not overlap them (checked at creation). The arena root is created at runtime as a scene root at world scale 1.
- **Battlefield prefabs.** No Camera or AudioListener; nothing on a `WM_*` layer (checked only when WM layers are resolved), `Perimeter` or `RetreatingPlace`; root scale 1 preferred.

## Glossary — defined once

| Term | Meaning |
| --- | --- |
| GameplayTime | Modifier scheduler's scaled fixed clock, `Time.fixedTimeAsDouble`. |
| ApplicationTime | Current scheduled event timestamp during catch-up; otherwise fixed time. |
| Time.time | Scaled Unity time used by ability timers/casts and current Vulnerable movement reads; fixed-step time inside FixedUpdate. |
| Transaction time | Time supplied to a Clash snapshot/result, including Vulnerable snapshot reads. |
| Map time | `WorldMapController2D.MapTime`: fixed-step seconds accumulated only while the map is ready and unfrozen; the clock for world-map timers, distinct from GameplayTime and Time.time. |
| Physics step | `WorldMapController2D.PhysicsStep`: count of ready controller FixedUpdates, frozen or not; stamps contact reports. |
| Touch episode | Consecutive physics steps in which one army/army or army/settlement pair is reported; ends when a step lacks it or the map freezes. |
| Contact report | `WorldMapContactReport`: one classified touching pair for one physics step. |
| Contact intent | `WorldMapContactIntent`: what a contact kind does. Since Phase 2 the allied intents take effect; FieldBattle, Capture and Siege are still reported only. |
| Parked | A persistent unit that is inactive under the reserve root with status Reserve; never awakened unless it has been activated. |
| Roster / garrison | `UnitRoster`: the single logical owner of units. A garrison is a settlement's unbounded roster; an army roster has `MaxUnitsPerArmy` capacity. |
| Settlement-only unit | Unit whose definition sets `SettlementOnly`; counts in its garrison and never enters an army. |
| Eligible units | A roster's units that may join an army: `Count − SettlementOnlyCount`. |
| Arrival sequence | Per-reserve increasing number assigned on recruitment and on every transfer; roster storage and tie-break order. |
| Garrison limit / overflow | Town 200 / castle 300 total-garrison threshold at or above which exactly `OverflowArmySize` eligible units leave as an army; it never blocks arrivals. |
| Allied interaction | Retreat transfer, retreat merge or garrison join applied by the controller to touching allied pairs. |
| Arena | The battle area `BattleArena2D` builds under its runtime Root far from the world map; it exists only while `HasArena`. |
| Battle view | The battle camera's live orthographic view rectangle (`GetViewRect`); the perimeter, gates and entry tracking follow it. |
| Entry strip | Region beyond a wall's outer face where bodies of that edge appear before walking in. |
| Perimeter | The four `Perimeter`-layer walls just outside the battle view; bodies may enter, and once Inside cannot leave. |
| Retreat gate | `RetreatingPlace` trigger strip just inside a gate edge; a registered retreating body touching it escapes. |
| Arena body | `ArenaBody`: the arena's tracking record for one registered body (test body now, real unit later); not an owner. |
| Leak / re-entry | Leak: an Inside body's centre found outside the view without announcement. Re-entry: an announced (`ForceReenter`) or aspect-change return to Entering. |
| Seeking / Knockback / Disabled / Dead | Motor major states; Disabled is gameplay state, distinct from a disabled component/root. |
| Sidestep | Subordinate locomotion session, not a UnitMajorState value. |
| Targetable | Combatant predicate for targeting/Clash; not a universal damage/modifier receiving gate. |
| Processed | Participant result accepted at entry; may be lethal, dodged or zero-damage. |
| Dodged | Explicit sampled result; never inferred from zero/rejected damage. |
| Committed | Irreversible point of a particular operation (damage, modifier application or cast); does not promise successful delivery or continued presence. |
| Pending trigger request | Runtime-owned captured opportunity that can be canceled or committed independently of active instance membership. |
| Handle | Controller-scoped identity of one stat entry; controller plus nonzero entry ID. |
| Bundle | Controller-scoped group identity for atomic framework stat installation/exact removal. |
| Instance ID | Runtime-scoped identity of one independent modifier application. |
| Schedule ID | Scheduler identity used to cancel scheduled work. |
| Lease | Pool clone plus generation identity preventing stale release of a reused clone. |
| Normal invocation | Held ability selected automatically with own/universal timer policy. |
| Triggered invocation | Pending-request-authorized cast; holder need not own the asset. |
| Captured set | Exact pre-result identities/opportunities; later callback-created instances cannot join it. |
| Settlement | Expiry reconciliation before callback-free protected capture. (World-map settlements are `Settlement2D`, not this term.) |
| Protected read | Settled controller barrier excluding mutation during paired snapshot reads. |
| Notification scope | Nestable controller notification deferral/coalescing; acquisition can itself settle overdue entries. |
| Requested damage / actual loss | Producer amount versus authoritative health loss reported by DamageResult2D. |
| Clean forward speed | Motor momentum excluding its separately stored Sidestep bonus and Knockback velocity. |
| Reserve / Deployed / Escaping / Escaped / Dead / Discarded | `UnitStatus` values. Reserve means parked, including any future queued-to-spawn unit. Discarded means explicitly destroyed by debug or owner-loss code and no longer owned. Phase 2 sets only these two; Deployed, Escaping, Escaped and Dead have no transitions yet (`CORE-02`). `Army2D.IsRetreating` is an army classification/visual flag, not Escaping. `ArenaBodyState.Escaped` is arena bookkeeping, not this status. |
| Result lock | Proposed battle boundary not defined by this inventory; not a synonym for Clash commitment or world-map battle selection. `BattleArena2D.EscapesAccepted` is the hook it will close. |

## Shared configuration cards

### UnitDefinition2D [B, WM2]
Owns: authored unit/progression/movement/Sidestep/reactive values, raw level scaling, and the world-map `settlementOnly` flag.  
API: level-value getters and read-only configuration properties, including `StartingUnitLevel` and `SettlementOnly`; consult source for formulas/defaults.  
Events: none documented.  
Invariants: `INIT-01`; `CORE-01`; consuming subsystem rules; `SettlementOnly` is read through `UnitHandle` and rosters (`WM-UNIT-02`, `WM-ROSTER-01`).  
Never owns: runtime health/XP, movement, timers, effective stats or transaction state.

### ClashRules2D [B,K]
Owns: targeting interval, global Clash cooldown setting, calculation parameters, Vulnerable multipliers and outcome effects.  
API: CreateCalculationSettings; read-only configuration.  
Events: none documented.  
Invariants: `SETUP-01`; `CALC-01`, `CALC-04` in Clash.  
Never owns: current target, per-unit cooldown/Vulnerable expiry or transaction execution.
