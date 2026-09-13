# v0.3 — fluid gameplay pass

This revision keeps the illustrated v0.2 artwork and changes the actual controls and combat behavior toward the reference video.

- Movement responds over a short 45 ms smoothing time constant and brakes over 28 ms. There is no long glide after releasing the stick. Blocked-axis momentum is cleared at walls.
- Aiming rotates rapidly toward the nearest target instead of snapping instantly. Rifle shots follow the weapon's current aim and wait for alignment, so the visual firing direction matches the projectile direction.
- Dash uses integrated ease-out displacement: 3.3 world units in 0.24 seconds at base movement speed. It reads current input when pressed, retains brief invulnerability, and leaves fading mech afterimages. Movement upgrades scale distance proportionally.
- The Beam Rifle fires every 0.3 seconds for 60 damage, preserving the previous 200 base damage per second while increasing projectile cadence. Recoil and a small movement bob make the mech less static.
- Enemies steer and separate with damping instead of changing velocity abruptly. Non-boss hits add small recoil/knockback. Shooter drones begin firing sooner and shoot every 1.8 seconds for 45 damage, giving more readable red projectiles without simply increasing each hit's damage.
- XP attraction stops at the player rather than overshooting.

## Play

Launch the adjacent `PocketMechArena-Windows/PocketMechArena.exe`. WASD/arrows or the joystick move; Space or the blue button dashes. The old v0.2 ZIP remains available for comparison.

## Verification

See adjacent `Verification-v03/smoke-results.txt`. The harness checks motion equations at 30, 60 and 120 FPS; prompt braking; actual dash travel; base weapon DPS; and the existing mission, upgrade, reward and restart flows. This is not a substitute for measured performance or real touch testing on a phone.

The motion preview is captured from the actual Unity player at 24 frames per simulated second. Input is automated, hull is restored, Triple Shot is pre-equipped, and the boss encounter is staged for the short demonstration. It is not an AI-generated gameplay clip or an unassisted run. Recording is opt-in via `-pmaMotion` and never runs during ordinary play.

The characters still use modular 2D artwork. Full directional walk/boost animation and 3D articulation are not implemented, and exact animation parity with the concept video has not been claimed.
