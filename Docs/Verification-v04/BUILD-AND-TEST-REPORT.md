# Version 0.4 build and verification

Unity 6000.6.0f1 / Windows x64 development player. Final build succeeded: 173,048,004 bytes. No C# compiler errors or warnings found in the build log.

44 runtime checks passed in the compiled player; see smoke-results.txt. Coverage includes mission/loadout navigation, joystick input, braking, frame-rate-independent dash math at 30/60/120 FPS, live dash displacement and invulnerability, independent automatic combat, all upgrade stat effects, visible weapon attachments, shield flanking, XP overflow choices, Assault Striker and mission phases, the boss, five-minute victory, failure, reward-claim protection, base navigation, purchase/tuning validation, equipment effects, duplicate loot, profile serialization and Veteran modifiers.

The full-run harness restores hull and forces any remaining boss defeat at 04:58. It is a progression regression, not a difficulty or phone-performance test. Verification profiles never write real player saves.

Screenshots are captured from the real Unity camera/UI at 540x960. Combat is staged for review. Mission, loadout, garage, reward and base screens use their real handlers. Captures were reviewed for layout and readability.

No Android/iOS player was built or tested. Those platform modules are not installed. Highest priorities: physical-device build/testing, human combat/economy playtests, and final directional/modular character animations.
