# Pocket Mech Arena v0.4 — playable storyboard pass

A real Unity game prototype following the illustrated Pocket Mech Arena concept and the first-five-minutes storyboard. Includes the illustrated arena and fluid combat from v0.3, plus connected mission selection, loadouts, garage progression, an elite encounter, later upgrades, bonus equipment and return-to-base flow.

## Play

Download the Windows ZIP from [Releases](https://github.com/SenseiNatezz/pocket-mech-arena/releases), extract it, then launch `PocketMechArena.exe` inside `PocketMechArena-Windows`. Keep the whole Windows folder together. Choose **Play → Start Mission → Confirm & Deploy**. From the rewards screen, Continue opens Back to Base with Play Again, Garage, Change Loadout and Select Another Mission.

- Move: WASD/arrows or drag the left joystick.
- Dash: Space or the right blue button; 0.24-second burst with brief invulnerability, 4-second base recharge.
- Aim/fire: automatic nearest-target tracking, independent of movement.
- Level up: collect XP and choose one of three cards; combat pauses for the choice.
- Pause: Escape or top-right pause button. Losing focus pauses normal play.

## Unity project

Use Unity **6000.6.0f1**. Add this folder in Unity Hub, open `Assets/PocketMech/Scenes/ArenaA1.unity`, select a portrait 9:16 Game view and press Play. No external asset packages are required.

One industrial arena and one playable Ranger; Scout, Shooter, Bomber, Shield, Rush, elite Assault Striker and Heavy Siege Walker; blue player projectiles, red enemy projectiles, XP pickups, damage numbers, explosions, dash trails, responsive movement and independent aiming.

Standard victory awards 1,250 credits, four parts and 12 weapon XP. Veteran uses the same arena with 30% more enemy hull and 1,750 victory credits. Both require defeating the boss and surviving to 05:00. Selection screens and pauses do not consume mission time.

The garage has five functional equipment slots, 11 items, purchases, three tuning ranks per item and four battle-mech paint choices. Equipment affects starting stats; Beam Rifle and Railgun have different firing behavior. Persistent equipment and currency are separate from temporary run upgrades. Saves use local PlayerPrefs, migrating the earlier prototype's currency totals. No account or server is needed.

## Storyboard coverage

See `STORYBOARD-v04.md` for the panel-by-panel implementation, mission timings, upgrade effects, drop probabilities, and remaining art/device work.

## Source organization

- `Assets/PocketMech/Scenes`: playable scene.
- `Assets/PocketMech/Resources/Illustrated`: replaceable illustrated sprites.
- `Assets/PocketMech/Resources/Prefabs`: player, seven enemy variants, projectile and XP pickup.
- `Assets/PocketMech/Resources/RunBalance.asset`: base combat tuning.
- `Assets/PocketMech/Scripts/Game.cs`: encounter phases and run progression.
- `Assets/PocketMech/Scripts/PilotProfile.cs`: inventory, equipment catalog, tuning, saves and rewards.
- `Assets/PocketMech/Scripts/Balance.cs`: upgrade catalog and balance types.
- `Assets/PocketMech/Scripts/ArenaUI.cs`: mobile controls and connected menus.
- `Assets/PocketMech/Editor`: build utilities.

Keep actor `Turret` and `Legs` transforms when replacing artwork. The original sketch and source atlas are retained separately with the local project.

## Verification

The final Windows player compiled successfully and passed **44 runtime checks**, including an assisted full five-minute run, joystick/dash, upgrades, phases, boss, rewards, garage costs and save serialization. Actual player screenshots and full results are under `Docs/Verification-v04`. The full-run check restores hull and defeats any surviving boss at 04:58; it verifies progression, not human difficulty.

Use **Pocket Mech → Build Windows Prototype** to build the existing scene. Do not use the old placeholder scene rebuild command: it intentionally replaces the illustrated scene. The player flag `-batchmode -nographics -pmaSmoke` runs the checks; `-pmaCapture` generates staged screenshots. Verification uses an isolated profile and cannot overwrite real progress.

## Highest-priority next work

1. Install Android build support and test on an actual phone: touch comfort, safe areas, frame pacing and thermal behavior. No APK or iOS build has been verified; iOS also needs its Unity module and Mac/Xcode.
2. Human playtests for XP pacing, elite pressure, boss difficulty and economy. Storyboard level-up times are targets, not forced interruptions.
3. Dedicated character animations, modular equipment art and victory art. Elite currently reuses enlarged/recolored Rush art. Paint affects the battle mech; the full-screen hero illustration is static.

This is a playable prototype, not a finished mobile-store release.
