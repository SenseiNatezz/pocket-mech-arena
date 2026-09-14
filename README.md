# Version 0.7.0 — Magnum Blast

Play: https://senseinatezz.github.io/pocket-mech-arena/?v=0.7.0

Tap **BLAST** above Dash, or press **E**, for a charged piercing beam. 0.35-second charge, 12-second cooldown; movement stays responsive. Available in both missions. See [blast notes](MAGNUM-BLAST-v07.md).

# Pocket Mech Arena — v0.6.0

[Play in your browser](https://senseinatezz.github.io/pocket-mech-arena/) — tap **Load Game**, then **Play**. Select **Arena A-1** or the new **Frostline** mission, choose difficulty and confirm your loadout.

Frostline Reactor adds an icy arena, three new enemy machines and the Glacier Colossus boss. See [Frostline details](FROSTLINE-v05.md) for enemy behavior, timing, rewards, artwork and next work.

The battle view now uses a raised rocky platform and angled camera. See [raised-view notes](RAISED-VIEW-v06.md). Existing machines and both missions remain available.

## Play

- Movement: left virtual joystick, WASD or arrow keys.
- Dash: right button or Space; short burst with brief invulnerability.
- Aiming and firing are automatic, independent of movement.
- Collect XP and choose one of three upgrades while combat pauses.
- Defeat the boss and survive five minutes to earn rewards.

Both arenas have Standard and Veteran difficulty. Garage equipment, tuning, paint and currency persist between runs. Old saves keep their progress and default to Arena A-1. The public browser game is the current v0.6.0 build; the older downloadable Windows GitHub release is v0.4.0. Current Windows and Unity builds are also available in the local project outputs.

Open the browser link in Safari to test on iPhone. Physical iPhone performance and simultaneous-touch testing remain outstanding; see [browser notes](WEB-PLAYTEST.md). This is a prototype, not an App Store release.

## Unity source

Use **Unity 6000.6.0f1**. Add this repository folder in Unity Hub and open `Assets/PocketMech/Scenes/ArenaA1.unity`; both missions use this scene with runtime environment selection. Use a portrait 9:16 Game view.

- `Assets/PocketMech/Scripts/Missions.cs`: mission catalog, boss and reward definitions.
- `Game.cs`: waves, progression and environment selection.
- `Enemy.cs`: enemy movement and attacks.
- `PilotProfile.cs`: compatible saves, inventory and equipment.
- `ArenaUI.cs`: connected menus and mobile controls.
- `Assets/PocketMech/Resources/Illustrated`: replaceable game art.
- `Assets/PocketMech/Resources/Prefabs`: authored actor prefabs.
- `Assets/PocketMech/Editor/FrostlinePass.cs`: new artwork import and prefab assembly.
- `Assets/PocketMech/Editor/WebBuild.cs`: browser build settings.

Build the existing scene with `PocketMech.Editor.ProjectBuilder.BuildWindows` or `PocketMech.Editor.WebBuild.Build`. The older placeholder reconstruction commands intentionally replace scene content and are not part of normal builds.

## Verification

Windows v0.5.0 compiles and passes **54 runtime checks**, including assisted five-minute runs in both areas, Glacier Colossus attack cycling and defeat, distinct mission rewards, old-save compatibility, mission switching, controls, upgrades and equipment. Full results and actual game captures are under `Docs/Verification-v05` in GitHub, or the sibling `Verification-v05` local output folder. Full-run tests restore hull and force any surviving boss defeat at 04:58; they verify progression rather than human difficulty.

Run the Windows player with `-batchmode -nographics -pmaSmoke` for isolated checks or `-pmaCapture` for staged gameplay captures. Verification profiles cannot overwrite real progress.

Next priorities: physical iPhone playtests, human balancing of the new encounters, and dedicated character animation. The new enemies currently use single illustrated sprites.
