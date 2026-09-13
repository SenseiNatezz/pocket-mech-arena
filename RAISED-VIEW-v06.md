# Raised battlefield — v0.6.0

The battle view now follows the supplied Grind 'Em All screenshot and gameplay reference: a broad, raised, irregular diamond platform against a dark background, seen through an oblique orthographic camera. This applies to both missions. Arena A-1 has muted rock coloring; Frostline has cool stone and an icy rim. Enemy artwork, attack patterns, upgrades, loadouts and mission progression remain the existing game systems.

`RaisedBattlefield.cs` creates a real flat mesh, faceted cliff sides and narrow rim. `CameraRig.cs` sets the angled view and fits the full platform to portrait or landscape viewports. `ActorPresentation.cs` faces existing machine sprites toward the camera, adds contact shadows and orders overlapping actors by ground position. This is a 2.5D presentation using the existing illustrated enemies, not newly modeled 3D enemies.

The simulation remains on its XY plane. Movement and dash boundaries are inset from the platform edge. Attack warnings, projectiles and pickups remain aligned to that ground plane. The original painted environment is disabled during battle; its art remains available for mission preview menus and future use.

Use Unity 6000.6.0f1 and the existing ArenaA1 scene. `RaisedViewBuild.Build` creates Windows; `WebBuild.Build` creates the browser build. The browser template uses versioned asset URLs so older cached builds are refreshed without clearing player saves.

Reference: https://www.youtube.com/watch?v=anmUEen1tjI (early combat around 01:40) and the user's supplied screenshot. No video artwork was extracted or copied into the game; platform geometry is authored in the project.

Remaining priorities: physical-phone readability and simultaneous-touch testing, human playtests with the changed viewing angle, and optional fully modeled/animated machines if desired later.
