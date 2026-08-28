# Cats and Kills — 3/4 Visual Bible

## Status

The old pure top-down presentation is now **legacy mechanics scaffolding**.
It stays in the repository only because it is useful for testing AI, combat,
damage and navigation.

The production-facing direction is a **2D three-quarter / angled presentation**
matching the approved visual mockup: industrial, post-war, dense, readable and
visually dimensional.

## Camera

- Orthographic 2D gameplay camera.
- Character and environment art is drawn in a fixed 3/4 angle.
- The world still uses normal 2D physics and navigation under the art.
- We do **not** rotate the whole character like a top-down disc.
- Player aim remains continuous, but character facing is quantized to 8 directions.
- Target gameplay framing: close enough to read weapons, limbs and cover state.
- Camera lead follows aim, but the player should remain visually dominant.

## Character presentation

Production characters are not assembled from circles and rectangles.

Each combatant needs an 8-direction presentation:

- E
- NE
- N
- NW
- W
- SW
- S
- SE

Minimum states for the first polished slice:

- idle
- move
- fire
- reload
- hurt
- crawl
- dead

The weapon can still use continuous aim internally, but the body uses the nearest
directional pose. This prevents the current "rotating paper doll" look.

## Main character silhouette

- light/white cat fur;
- dark military / civilian hybrid clothing;
- damaged collar is always readable;
- rifle is visually large enough to identify at gameplay zoom;
- silhouette should remain clear against blue-violet industrial backgrounds;
- magenta/red accent is reserved for collar / danger / propaganda motifs.

## Enemy readability

Classes must be readable before shooting:

### Pistolier
- lighter civilian equipment;
- compact silhouette;
- less armour;
- pistol and improvised grenade pouch.

### Rifleman
- regular military silhouette;
- rifle;
- standard armour / webbing.

### Machine gunner
- visibly heavier torso;
- bigger backpack / ammunition load;
- machine gun silhouette dominates the pose.

### Demolitionist
- unstable / damaged equipment;
- demolition harness;
- readable explosives;
- red warning accents, but not a glowing arcade target.

## Environment

The first production environment is a military-industrial facility.

Every wall module needs:

- visible top plane;
- visible front / side face;
- trim;
- cast or baked fake shadow;
- corner module;
- doorway module;
- damaged variant.

Required modular pieces:

- concrete / metal floor;
- wall straight;
- wall corner;
- reinforced door;
- window / observation slot;
- crate cover;
- heavy crate;
- fuel drum;
- terminal;
- pipes / cable trays;
- fencing;
- hazard markings;
- propaganda poster slots;
- light fixtures;
- debris;
- blood decals.

## Depth and sorting

World logic remains 2D.

Visual depth is achieved using:

- 3/4 art;
- Y-based sprite sorting;
- per-object height offsets;
- foreground occluders;
- soft character shadows;
- wall faces and top planes;
- controlled light pools;
- atmospheric overlays.

## Palette

Base:
- charcoal navy;
- desaturated blue;
- violet-black;
- cold steel;
- dirty concrete.

Accents:
- emergency red;
- damaged-collar magenta;
- sodium amber;
- cyan practical lights.

Blood is dark saturated crimson, not neon pink.

## Lighting

The target look should have obvious local lighting even though the game is 2D.

First-slice lighting language:

- cold overhead industrial light;
- red emergency light after alarm;
- cyan terminal spill;
- warm muzzle flashes;
- volumetric-looking smoke / haze overlays;
- deep local shadows around cover.

## Combat FX

Required for the polished room:

- muzzle flash with short local light pulse;
- tracer / bullet streak;
- sparks on metal;
- concrete dust;
- blood spray;
- persistent blood decals;
- shell casings;
- grenade flash;
- smoke cloud;
- debris;
- screen-space suppression vignette;
- damage response;
- hit confirmation.

## UI

UI should not look like a debug overlay.

Target:
- compact health / body status;
- ammo;
- grenades;
- collar state;
- objective;
- contextual interaction;
- short radio subtitles.

Visual language:
- military terminal;
- thin geometry;
- subtle scanline / damaged signal motif;
- no giant opaque debug boxes.

## First polished room

Before rebuilding the whole level, the new art pipeline must prove itself in one
small combat room.

Contents:
- player;
- 3 riflemen;
- 1 machine gunner;
- reinforced door;
- terminal;
- 3 cover objects;
- one explosive prop;
- alarm state;
- normal / alert lighting;
- grenade;
- damage / blood / smoke;
- complete HUD.

The room is considered successful only if a screenshot from actual Unity gameplay
looks recognisably close to the approved 3/4 concept image.

## Production rule

Do not spend time beautifying the old procedural top-down sprites.

Any new visual work must either:
1. support the 3/4 production pipeline, or
2. be a temporary technical fallback that can be removed without redesigning gameplay.
