# Magnum Blast — Version 0.7.0

Tap BLAST above Dash (or press E on desktop). The weapon locks the nearest enemy's direction, charges for 0.35 seconds, then fires from the moving mech. Without a target it fires along the turret's facing direction. Movement remains available throughout.

The beam travels 12 world units, has a 1.3-unit collision width plus enemy body radius, and hits every intersecting enemy once for 8 times current rifle damage. Shield enemies retain their frontal protection. Recharges in 12 seconds from activation. Both missions support it. Pausing or choosing upgrades freezes charge, visual effects and cooldown; restarting resets the weapon.

Visuals: contracting charge glow, white beam core, cyan sheath, violet corona, branching electrical arcs, expanding muzzle ring, hit bursts and procedural charge/discharge audio. Inspired by the supplied Beam Magnum firing reference, with original procedural effects.

Implementation: MagnumBlast.cs is an independent runtime component attached by PlayerMech.Init. MagnumBeamEffect owns its short-lived graphics and material. ArenaUI supplies the touch button. SoundBank supplies synthesized audio. No additional art packages are required.

Validation: Unity 6000.6 Windows and WebGL builds; 64 automated checks including two full assisted mission runs and six blast checks. Captured real game rendering in Docs/Verification-v07/11-magnum-blast.png. Assisted mission checks heal the player and finish the boss at 04:58; these verify progression, not difficulty. Physical iPhone testing remains outstanding.

Next priorities: balance beam damage/cooldown against each boss on a physical phone; add directional aiming feedback during charge; replace procedural beam layers with authored VFX if desired.
