# Cats and Kills — technical audit (2026-08-28)

Target editor: Unity 6000.5.10f1 / URP 17.5.

## Executive finding

The current project is not structured as a normal production Unity game. It is an editor/runtime-generated prototype:
- 94 C# files
- 0 committed .unity scenes
- 0 prefabs
- 0 .anim clips
- 0 Animator Controllers
- 0 Sprite Atlas assets
- 0 asmdef files
- 0 automated tests

The current visual instability is a consequence of that architecture, not one isolated renderer bug.

## P0 — stop extending the current generated visual pipeline

### 1. Scenes and prefabs are missing from source control
PrototypeSceneBuilder creates the whole level and gameplay hierarchy through code. ConceptArtPlayableBuilder and ConceptVisualPolishBuilder then mutate that generated scene.

This makes iteration destructive and difficult to inspect. The correct target is:
- Bootstrap.unity
- Level01_Facility.unity
- Player.prefab
- EnemyBase.prefab + variants
- environment prop prefabs
- real saved materials / animation assets

Builders should scaffold assets once, not recreate the production scene every iteration.

### 2. Production art is not reproducible from Git
The repository contains Assets/CatsAndKills/Art/README.md but not the ConceptAtlases folder currently used by ConceptArtIntegrator.
Assets/CatsAndKills/Generated/ is intentionally ignored.

A clean clone therefore cannot reproduce the visuals currently seen in Unity.

All production source art must be committed or imported from a stable external source.

### 3. ConceptArtIntegrator is doing image processing that belongs in the asset pipeline
ConceptArtIntegrator is ~85k characters and attempts to infer sprite bounds from generated atlas images.
This caused:
- clipped characters
- neighbouring body parts leaking into frames
- clipped walls
- oversized props
- broken pivots
- giant floor patches

Target pipeline:
- explicit individual character frames with identical canvas/pivot
- explicit modular wall/floor sprites
- Unity Sprite import metadata
- Sprite Atlas packing
- no runtime/heuristic atlas slicing for production art

### 4. Animation is not using Unity Animator
Current ThreeQuarterCharacterVisual2D switches sprites and procedurally modifies transforms in LateUpdate.
The temporary second SpriteRenderer crossfade produces ghosting rather than real animation.

Target:
- AnimationClips for idle/move/fire/reload/hurt/crawl/dead
- Animator Controller for shared state logic
- AnimatorOverrideController for player/enemy visual variants
- code sets parameters only (MoveX, MoveY, Speed, Fire, Hurt, Dead, etc.)
- feet/pivot stay fixed in every source frame

## P0 — AI

Current AI is functional prototype logic, not tactical AI.

EnemyBrain:
- one large state machine with ad-hoc branches
- role assignment is index % 4
- flank points are raw geometric offsets and are not tactically validated
- no explicit action scoring
- no reservation of tactical lanes
- no coordinated timing window for suppress/flank/push
- no proper search pattern / room clearing

CoverManager:
- only checks that a Linecast intersects something
- does not validate actual body protection
- does not validate peek/firing lane
- does not validate route quality
- adds random score noise

NavigationGrid2D:
- resets every node for every path request
- open set is a List with linear lowest-F search
- paths allocate Lists/HashSets repeatedly
- no path cache
- no dynamic obstacle cost/influence map
- no tactical cost for exposed areas

EnemyMotor2D:
- Physics2D.OverlapCircleAll every FixedUpdate allocates
- separation is reactive only
- no local steering prediction

Recommended AI architecture:
Perception -> Agent Blackboard -> Squad Blackboard -> Decision layer -> Tactical action -> Motor/Weapon.

For the decision layer either:
1. Unity Behavior package (behavior tree, reusable subgraphs, runtime graph debugging), or
2. a small custom utility selector with explicit scored actions.

Use utility scores for:
TakeCover, Suppress, Flank, Advance, Retreat, ThrowGrenade, Search, HoldAngle.
Squad controller owns intent and role reservations rather than assigning fixed roles by member index.

## P1 — rendering / level art

Project GraphicsSettings currently use default transparency sorting and only the Default sorting layer.

For 3/4/isometric presentation use:
- dedicated sorting layers
- ground-contact pivot
- SortingGroup for characters/compound props
- custom-axis or controlled Y-depth sorting
- Isometric Tilemap / Tilemap for repeatable floors and walls
- Sprite Atlas for batching
- authored 3/4 wall modules, not stretched/cropped arbitrary images

URP 2D lighting should be applied after the base scene composition is stable.

## P1 — UI

The runtime UI currently relies heavily on OnGUI:
- ConceptHUD
- CrosshairUI
- PrototypeHUD
- RadioDialogueSystem
- RuntimeGameMenu
- WorldCalloutSystem
- PlayerDeathController

This is acceptable for debug UI but not the production HUD.

Target: UI Toolkit in Unity 6 (or uGUI if specific world-space requirements demand it).
Keep IMGUI only for internal debug overlays.

## P1 — runtime allocations / performance

Examples found:
- EnemyMotor2D: OverlapCircleAll every FixedUpdate
- CharacterCombatGeometry2D: RaycastAll
- grenade logic: overlap-all queries
- FXService creates many GameObjects/components at runtime
- TracerFX2D creates a new Material per tracer
- RuntimeCharacterVisualBootstrap calls FindObjectsByType repeatedly during gameplay
- several UI/world scripts perform Find* from frame loops

Target:
- Physics2D non-alloc/reusable buffers where applicable
- cache component/service references
- pool tracers, casings, blood, smoke, explosions, callouts
- shared materials
- no FindObjectsByType in normal frame loops
- use Unity Profiler and inspect GC.Alloc

## P1 — project organization

Missing:
- assembly definitions
- EditMode tests
- PlayMode smoke tests
- committed scenes/prefabs
- animation assets
- sprite atlases
- production material assets

ProjectSettings still contain prototype defaults such as productName=Sandbox and default company/application identifiers.

## Correct rebuild order

1. Freeze current main as prototype reference.
2. Work on refactor/vertical-slice-v2.
3. Commit/recreate canonical art sources.
4. Create persistent production folder structure and assembly definitions.
5. Create Player and Enemy prefabs.
6. Replace runtime sprite switching with Animator assets.
7. Create Level01_Facility as a real saved scene.
8. Build floor/walls with Tilemap + modular prop prefabs.
9. Establish sorting layers/groups/pivots.
10. Rebuild combat FX with pooling.
11. Replace AI decision layer and navigation hot paths.
12. Replace debug OnGUI HUD with UI Toolkit/uGUI.
13. Add EditMode + PlayMode smoke tests.
14. Profile CPU/GC/rendering before expanding content.

## Unity references reviewed

- Animator Controller / state machines:
  https://docs.unity3d.com/6000.0/Documentation/Manual/class-AnimatorController.html
- Animator Override Controller:
  https://docs.unity3d.com/6000.0/Documentation/Manual/AnimatorOverrideController.html
- Prefabs:
  https://docs.unity3d.com/6000.0/Documentation/Manual/prefabs-introduction.html
- Isometric Tilemap:
  https://docs.unity3d.com/6000.0/Documentation/Manual/tilemaps/work-with-tilemaps/isometric-tilemaps/create-isometric-tilemap.html
- Sorting Groups:
  https://docs.unity3d.com/6000.0/Documentation/Manual/sprite/sorting-group/sorting-group-reference.html
- Unity Behavior:
  https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.behavior.html
- AI Navigation:
  https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ai.navigation.html
- Profiler / GC allocations:
  https://docs.unity3d.com/6000.0/Documentation/Manual/performance-track-garbage-collection.html
- Runtime UI:
  https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-runtime-examples.html
- Unity 2D best practices:
  https://unity.com/how-to/optimize-performance-2d-games-unity-tilemap
  https://unity.com/how-to/use-2d-lights-unity-set-mood
