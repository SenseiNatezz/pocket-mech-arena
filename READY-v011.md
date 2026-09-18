# Rifle ready throughout the run — v0.11.0

The Gundam keeps its rifle arm raised while idle, flying, turning, dashing, and waiting between enemies. Shots still have recoil, and beam attacks recover to the raised flight pose. Smooth body turns and animated torso motion remain active.

The supplied 18-panel image is the gameplay storyboard. Existing mechanics already cover its sequence:

| Storyboard beat | Game behavior |
| --- | --- |
| Mission and loadout | Arena A-1 selection, equipment confirmation, deployment |
| 00:00–00:20 | Movement, automatic-fire and dash tutorial prompts |
| 00:20–00:45 | Scouts and rushing drones |
| Level 2 | Triple Shot / Attack Speed / Reinforced Plating |
| Level 3 | Piercing / Faster Thrusters / Overcharged Core |
| 01:30 onward | Shield bots and marked hazards |
| Level 4 | Side Cannons / Dash Cooldown / Repair Nano-bots |
| 02:15 onward | Bombers, then elite Assault Striker |
| Level 5 | Spread Amplifier / Critical Core / Energy Barrier |
| 03:00 onward | Larger waves and combined weapons |
| Level 6 | Missile Pod / Weapon Overclock / Heavy Armor |
| 03:45 | Heavy enemy warning |
| 04:20 | Heavy Siege Walker |
| 05:00 | Victory requires boss defeat; rewards and return to base |

Upgrade times depend on collecting XP, rather than triggering automatically at the illustration's timestamps. Spread Amplifier requires Triple Shot; another valid choice replaces it if Triple Shot was not selected. Standard Arena A-1 victory awards 1,250 credits, 4 parts, and 12 weapon XP plus a chance of a bonus drop.

The space arena is retained in this pass. The industrial environment in the picture has not been recreated.

[Watch actual gameplay](ready-preview.html) · [Play v0.11.0](./?v=0.11.0)

Source: `outputs/PocketMechFlight`. Regression tests cover continuous raised-arm poses, turning, firing, beam recovery, dash, pause, restart, and all five storyboard upgrade sets. Physical iPhone performance remains unverified.
