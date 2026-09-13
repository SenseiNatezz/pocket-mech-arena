# Pocket Mech Arena v0.2 — illustrated revision

This revision replaces the geometric v0.1 presentation using the original four-screen sketch and the recovered gameplay video as references. It is a playable Unity game, not a movie playing behind controls.

## What changed

- New white/royal-blue Ranger artwork with amber accents and cyan optics, separate upper-body and leg sprites, and animated thruster glow.
- Five illustrated red-core drone types and a large six-legged Siege Walker. Source-art facing is converted to gameplay facing.
- A painted industrial courtyard, metal flooring, grated drain, machinery walls, barrels and hazard stripes. The arena is now narrow and the camera is fixed to better match the video. Wall artwork sits beyond the movement bounds.
- Glowing blue/red shots, illustrated impact bursts, outlined damage numbers and translucent blast zones with warning rings.
- Illustrated home screen with prominent mech, title, statistics, yellow Play button, and working Garage/Missions/Mechs navigation.
- Green hull bar, wave/enemy telemetry, framed panels, smooth joystick and blue dash control.
- Three horizontal upgrade cards. The first choice reproduces the sketch: Triple Shot, Faster Thrusters, Attack Speed.
- Illustrated rewards screen and a Continue action back to Home. Credits, parts and weapon XP are the implemented rewards; a rare-weapon inventory is not implemented.
- Starter-loadout garage inspection with equipment information. Equipping new parts, cosmetic customization and purchasing upgrades remain future work; the screen does not pretend those features work.

## Play

Launch the adjacent `PocketMechArena-Windows/PocketMechArena.exe`, then choose Play. WASD/arrows move; Space dashes; Escape pauses. Drag the left joystick and click the dash control to try the mobile layout with a mouse.

The Unity source remains in this folder. Open `Assets/PocketMech/Scenes/ArenaA1.unity` with Unity 6000.6.0f1.

## Art pipeline

The built-in ImageGen tool generated three source images using the supplied sketch as a reference: an actor atlas, an empty arena, and home-screen hero art. Their source copies are retained in the adjacent `Art-References` folder and the project's `Resources/Illustrated` directory. `IllustratedPass.cs` imports individual alpha sprites and wires the real prefabs and scene. Runtime weapons, enemies, pickups and menus remain interactive.

Normal rebuild: **Pocket Mech → Build Windows Prototype**. Do not use the old “Rebuild Placeholder Assets and Scene” menu for this art pass: that intentionally regenerates v0.1 geometry. The illustrated importer can be rerun with the `PocketMech.Editor.IllustratedPass.Build` editor method if its source atlas is available in the adjacent `Art-References` folder.

## Validation and limits

See adjacent `Verification-v02` for the compiled-player smoke report and five actual in-game captures. Captures deliberately stage the boss encounter and reward state to inspect them; screenshot generation never grants saved currency. The sustained five-minute regression restores hull and defeats any remaining boss at 04:58, so it validates progression rather than human difficulty.

This now follows the reference's illustrated look and screen structure, but is not a pixel-identical recreation. Characters still use 2D modular sprites rather than fully rigged 3D models or directional animation sets. The reward illustration currently reuses the home hero pose. Android/iOS build modules are still absent; phone installation, real multi-touch performance, balance and touch comfort require the next device-testing pass.

Next priorities: physical phone testing and balance; directional character animation and stronger boost motion; full garage customization and inventory; distinct garage and victory poses.
