# Orbital arena — Version 0.8.0

Both missions now float in outer space: a blue-violet nebula, 340 stars, a shaded distant planet, and 16 gently drifting, rotating faceted asteroids. These are decorative background objects with no collision or impact on targeting. Scenery motion pauses with gameplay and has subtle camera parallax.

Raised arena mesh is 15% larger in both ground dimensions (half-extents 9.72 x 12.42). Traversable limits were expanded to match the inset ground boundary (9.4 x 12.1), also correcting the old narrow rectangular movement restriction. The camera retains the oblique angle, zooms in roughly 30% at portrait aspect, and smoothly follows the player within a bounded central region. Outer arena edges may leave the viewport as intended for this closer view.

SpaceBackdrop owns procedural textures, sprites, asteroid meshes and cleanup. CameraRig creates the scenery once, avoiding repeat creation on mission restart. Decorative random generation is isolated from combat randomness. No external asset package required.

Verification: native and WebGL compilation, 67 runtime checks including enlarged traversable bounds and camera setup, plus screenshots from the real game. Existing tests cover both assisted full mission runs, upgrades, blast and profile persistence. Full-run tests heal the player and force the boss defeat near the end; they do not validate difficulty. Physical iPhone performance testing remains outstanding.

Next priorities: playtest outer-edge camera framing on a physical phone and tune enemy density for the expanded floor; add authored ground detail if desired.
