# Frostline Reactor — v0.5.0

Play at https://senseinatezz.github.io/pocket-mech-arena/ . Choose **Play → Frostline → Standard or Veteran → Start Mission → Confirm & Deploy**. Arena A-1 remains available beside Frostline.

## New mission

An illustrated frozen reactor yard replaces the industrial courtyard during this mission. The same responsive movement, independent aim, blue player bolts, red hostile projectiles, dash, XP choices, equipment and five-minute extraction apply. Frostline is available immediately; no unlock grind is required.

| Machine | Behavior | Counterplay |
| --- | --- | --- |
| Ice Skimmer | Fast approach with a weaving lateral motion | Keep distance and move through openings |
| Rail Sentinel | Stops for a 0.85-second aim lock, then fires two fast parallel bolts | Sidestep its marked aim before it fires |
| Cryo Mortar | Places two delayed blast circles to either side of the pilot | Move out of the red zones before detonation |
| Glacier Colossus | Boss at 04:20; cycles 12-bolt rings, a three-zone reactor strike, and Skimmer summons | Slip through ring gaps, leave warning zones and clear drones |

Skimmers begin the mission; Sentinels join from 00:35 and Mortars from 01:40. Paired reactor vent strikes begin at 01:30, Sentinel reinforcements arrive at 02:40, and the boss warning appears at 03:45. The player must defeat the boss and survive until 05:00.

Frostline victory awards 1,500 credits on Standard or 2,000 on Veteran, plus four parts and 12 weapon XP. Existing bonus-drop odds apply. Veteran adds 30% enemy hull. Arena A-1 retains its original enemies and 1,250/1,750 credit rewards.

## Implementation

- `Missions.cs` defines area metadata, roster entry, boss and rewards. Area selection is separate from the existing difficulty field.
- `PilotProfile.area` persists selection without changing the save key or erasing old equipment/currency. Old saves default to Arena A-1.
- Four independent enemy prefabs use named sub-sprites from the generated `Illustrated/FrostMachines.png` atlas. Import rectangles preserve alpha and source dimensions.
- `Illustrated/FrostArena.png` supplies both the mission preview and the real battle background.
- `FrostlinePass.Build` imports the new art, creates the four prefabs and builds Windows. `WebBuild.Build` builds the browser version. Normal development uses the existing scene; the old placeholder reconstruction commands are not needed.
- A duplicate environment renderer inherited from the earlier art pass was removed from the scene.

## Art generation

Generated with the built-in image generation tool, then copied into `Assets/PocketMech/Resources/Illustrated/`. Original alpha is preserved. The atlas is split with Unity sprite import metadata; the source image is retained intact.

**Environment prompt:** Production game background for a polished illustrated top-down mobile mech shooter. Portrait 9:16, orthographic overhead, no horizon, text, UI or characters. Frozen industrial reactor yard with snowy steel tiles, ice cracks, coolant pipes and dark navy machinery framing the outside edges. Open, low-contrast blue-gray floor in the central 80%, readable under blue/red projectiles. Crisp comic outlines and restrained cyan light accents. No large center obstructions.

**Machine atlas prompt:** Transparent 2x2 production sprite atlas, complete separated overhead machines facing down, no labels or environment. Crisp illustrated ink outlines, white/navy armor, orange-red hostile cores and icy cyan accents. Top left: triangular three-fin Ice Skimmer hover drone. Top right: four-legged Rail Sentinel with parallel cannons. Bottom left: six-legged Cryo Mortar with a cylindrical launcher. Bottom right: broad armored Glacier Colossus with shoulder cannons and a huge reactor core. Distinct silhouettes readable at mobile scale.

## Next priorities

1. Physical iPhone Safari playtests for frame pacing, heat, memory and simultaneous joystick/dash touches.
2. Human difficulty testing for both Frostline difficulties, especially rail warnings and the final boss's projectile gaps.
3. Dedicated machine animations and more environment interactions. The current generated machines are single illustrated sprites.

Automated full-run checks use hull restoration and force any surviving boss defeat at 04:58. They establish progression correctness, not human balance or physical-device performance.
