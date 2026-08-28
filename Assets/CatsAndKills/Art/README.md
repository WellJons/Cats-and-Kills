# Cats and Kills production art

The old generated top-down sprites are not production art.

The target is the approved 2D **3/4 angled** presentation.

Run in Unity:

Tools -> Cats and Kills -> 3-4 Art -> Create Production Folders

This creates the expected production tree and a ProductionArtPack.asset.

## Recommended character export

For each character class, export transparent PNG frames at the same canvas size.

Directions:

- E
- NE
- N
- NW
- W
- SW
- S
- SE

States for the first polished combat room:

- idle
- move
- fire
- reload
- hurt
- crawl
- dead

Recommended naming:

player_idle_E.png
player_idle_NE.png
player_move_E.png
rifleman_idle_SW.png

Keep the feet / ground-contact point at the same pixel position in every frame.
That point is the world sorting anchor.

## Import settings

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single for individual frames
- Filter Mode: Point for pixel-art exports
- Compression: None for source art
- Pivot: custom ground-contact pivot, consistent across the set

## Perspective rule

Do not rotate a single sprite around 360 degrees.

Characters must use directional art. The weapon can have continuous aiming,
but the torso / legs should switch between directional poses.

## Environment rule

Walls must be authored as actual 3/4 modules with visible height.
Do not fake production walls by stretching a flat top-down texture.

Minimum environment pack:

- floor industrial
- floor office
- wall straight
- wall corner
- wall damaged
- reinforced door
- light crate
- heavy crate
- fuel drum
- terminal
- fence
- pipes
- lamp
- debris
- propaganda poster

The approved concept screenshot is the visual target for contrast, density and depth.
