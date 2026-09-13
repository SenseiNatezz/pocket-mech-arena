# Raised-view verification — v0.6.0

Unity 6000.6.0f1 compiled the Windows and browser players successfully, with no C# compile errors. The Windows player passed all 58 checks in `smoke-results.txt`: the new mesh/camera presentation, existing joystick and dash, upgrades, both assisted five-minute missions, enemy attack execution, boss defeat, reward rules and compatible saves.

`02-combat.png` and `10-frostline-combat.png` are actual Unity camera/UI captures. They show the new raised platform and existing machines in each mission. The fixed camera is approximately 53 degrees away from the former straight-down view, using an orthographic projection. The full island is fitted to the mobile portrait layout.

Visual review corrected far-side cliff faces drawing over the top surface. Movement boundaries are inset to the diamond-shaped platform, preventing actors from running off its visible edges. Machine artwork remains illustrated sprites facing the camera; this is a 2.5D presentation rather than new 3D character models.

Full-run tests restore hull and force any surviving boss defeat at 04:58. Enemy-attack tests temporarily disable player firing so the equipped railgun cannot destroy test machines before their attacks execute. These are system checks, not human difficulty or device-performance tests. Physical iPhone testing remains outstanding.
