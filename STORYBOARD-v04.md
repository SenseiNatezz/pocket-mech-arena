# Storyboard implementation — Version 0.4

The storyboard is now the playable mission and progression flow, rather than a slide shown inside the game. The visual reference remains the illustrated blue-and-white Ranger in an industrial arena, readable blue/red projectiles and portrait controls.

## The 18 panels

| Panel | Implemented behavior |
|---|---|
| 1 Mission select | Arena A-1 briefing, five-minute objective, enemy/boss information, reward preview, Standard/Veteran selector, Start Mission. Veteran is the same arena, not a second map. |
| 2 Loadout | VX-01 Ranger, actual starting hull/speed/damage/fire interval/dash/crit, five equipment slots, selected paint/mission, Confirm & Deploy. Each slot opens the garage. |
| 3 Spawn, 0:00–0:20 | Three scouts and light spawning. Timed instructions introduce movement, automatic fire and dash. |
| 4 First wave, 0:20–0:45 | Scouts plus Rush drones (the storyboard's rushing/burst-bug role). Telegraph before the rush. |
| 5 Level 2 | Triple Shot, +25% Attack Speed, Reinforced Plating. Three cards pause combat. |
| 6 Combat, 0:45–1:30 | Shooter drones join and fire readable red bolts. |
| 7 Level 3 | Piercing, Faster Thrusters, Overcharged Core. |
| 8 Midgame, 1:30–2:15 | Shield bots reduce frontal damage; flanking matters. Timed floor hazards begin at 1:45 and recur every 16 seconds until the warning phase. |
| 9 Level 4 | Side Cannons, Dash Cooldown, Repair Nano-bots. |
| 10 Escalation, 2:15–3:00 | Bombers join. Assault Striker elite appears at 2:40 with 1,800 base hull, telegraphed charge and three-shot bursts. Drops 15 XP. |
| 11 Level 5 | Spread Amplifier, Critical Core, Energy Barrier. Spread Amplifier requires Triple Shot; otherwise a valid alternative appears. |
| 12 Power, 3:00–3:45 | Larger and faster reinforcement groups. Support weapons continue firing automatically. |
| 13 Level 6 | Missile Pod, Weapon Overclock, Heavy Armor. |
| 14 Warning, 3:45–4:20 | Heavy-enemy warning; reduced reinforcement rate and floor-hazard phase ends. |
| 15 Boss, 4:20–5:00 | Heavy Siege Walker, boss hull bar, cannon fans, telegraphed ground slam, briefly homing missiles and drone summons. |
| 16 Victory | Boss explosion. Extraction at 5:00 requires both survival and boss defeat; early boss defeat gives a hold-until-extraction message. |
| 17 Rewards | Credits, parts and weapon XP, chance-based bonus equipment/materials. Rewards are granted once and saved. |
| 18 Return to base | Play Again, Go to Garage, Change Loadout, Select Another Mission, Home. |

Level-ups remain XP-based. Their sequence follows the first five storyboard selections; exact times depend on kills and collection. Later choices are random among uncapped eligible upgrades. Level-up and pause screens stop the mission clock.

## All 15 run upgrades

| Upgrade | Effect |
|---|---|
| Triple Shot | Three primary bolts, 11-degree spacing. One rank. |
| Piercing | One additional target per rank. |
| Attack Speed | Primary fire rate +25% per rank. |
| Overcharged Core (+Damage) | Weapon damage +15% per rank. |
| Faster Thrusters | Movement speed +20% per rank. |
| Dash Cooldown | Recharge time -20% per rank. |
| Side Cannons | Two support bolts every two seconds. One rank. |
| Missile Pod | Homing explosive missile every three seconds. One rank. |
| Critical Core (Crit) | +10 percentage points of double-damage chance per rank. |
| Reinforced Plating | +15% maximum hull per rank; heal the added amount. |
| Energy Barrier | 150 shield per rank, restored every 20 seconds. |
| Repair Nano-bots | Restore 3% maximum hull per rank every 10 seconds. |
| Spread Amplifier | Widen Triple Shot spacing to 18 degrees. One rank. |
| Weapon Overclock | Primary fire rate +20% per rank. |
| Heavy Armor | +20% maximum hull per rank; heal the added amount. |

All upgrades except the four one-rank weapon/spread unlocks cap at four ranks. Run upgrades reset at deployment; equipment persists.

## Garage and progression

Five slots: head, arms, armor, weapon and booster. The starter set is free. Six additional items can be purchased using credits and parts; Beam Rifle Mod and Booster B-2 can also drop after victory. The inventory contains 11 equipment items. Each item has three tuning ranks; each rank costs 300 × next rank credits plus one part. Tuning benefits are shown on the item cards. The Railgun fires at half the Beam Rifle rate with 2.1× damage and two extra pierces. Armor trades 5% speed for 200 hull. All equipped bonuses affect the next run and its loadout preview.

Four free paint schemes apply to battle sprites. Head and arm selection currently changes stats using the shared Ranger art; bespoke modular silhouettes remain art work. The missile pod and side cannons visibly attach during runs. The full-screen hero illustration does not change with equipment or paint.

Victory makes one bonus roll: 20% Booster B-2, 15% Beam Rifle Mod, 25% Armor Scrap (+2 parts), 40% no bonus. Duplicate equipment becomes two parts. Base victory rewards are displayed separately from the bonus. Defeat awards five credits per kill and one weapon XP per full survived minute, with no victory parts or bonus. Weapon XP is saved/displayed for future weapon mastery; it does not yet buy skills.

## Validation and limits

The final compiled Windows game passes 44 runtime checks. Menus have been rendered and visually reviewed at 540×960. This confirms a playable desktop build with mobile-oriented controls; physical phone input, GPU performance, thermal behavior and device deployment remain unverified. Balance still needs human playtests. The test pilot is explicitly assisted, not evidence of ordinary-player difficulty.

The implementation uses replaceable illustrated sprites and simple effects. It is not a pixel-for-pixel recreation of generated concept animation. Dedicated walk/aim animations, new equipment silhouettes, elite art, a bespoke victory pose, more maps/mechs and store-release work remain outside this one-arena prototype.
