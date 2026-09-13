# Browser playtest — v0.4.1

Play: https://senseinatezz.github.io/pocket-mech-arena/

Open the link directly in Safari on iPhone (or a current desktop browser), tap Load Game, then Play → Start Mission → Confirm & Deploy. No installation or Apple Developer account is required for browser play. The first download is about 15 MB; leave the page open while it loads. This is a hosted Unity Web build, not an installed iOS app.

Move with the left joystick, dash with the right button, choose a card when you level up. Desktop also supports WASD/arrows and Space. Keep the phone upright. Opening another app/tab pauses the run; return and tap Resume. Progress is stored locally in that browser and may be removed when site data is cleared; private browsing may not retain it.

## Build and hosting

Use Unity 6000.6.0f1 with Web Build Support. The editor entry point `PocketMech.Editor.WebBuild.Build` creates `../PocketMechArena-Web`. It uses gzip with Unity's decompression fallback for static hosting, 128 MB initial Wasm memory, a 1 GB growth limit, and a device pixel ratio cap of 1.5. The custom template is `Assets/WebGLTemplates/PocketMech/index.html`.

The published files are `index.html`, `.nojekyll`, and `Build/` at the repository root. GitHub Pages publishes the main branch root. The .nojekyll marker bypasses Jekyll. Unity-generated .unityweb files are decompressed by the loader and do not require custom server compression headers.

## Validation

Unity Web compilation succeeded without C# compiler errors. Browser checks covered loading into the actual Unity game, home → mission → loadout → combat, automatic firing and kills, XP drops, dragging the joystick, dash displacement/cooldown, and 390×844 portrait layout (390×693 game canvas centered within the phone viewport). An unsupported browser orientation-lock call was removed; CSS handles the portrait layout instead.

These checks use the desktop browser with a phone-sized viewport, not a physical iPhone or Safari engine. Real iPhone multitouch, audio, memory limits, heating, saved progress and longer-run performance still need device testing. Please report your iPhone model, iOS version, the action taken and any screenshot when reporting a problem.

The underlying v0.4 Windows combat build passed 44 runtime regression checks. Those are separate from the browser checks and should not be read as 44 iPhone checks.
