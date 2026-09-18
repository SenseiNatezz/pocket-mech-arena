# Smooth turns and arm movement — v0.10.0

The Gundam now turns through interpolated 2D views instead of snapping between eight directions. Rotation accelerates and settles smoothly, including across the 360-degree boundary. Flight banking responds to turning, and releasing movement preserves the travel heading.

The arm raise was re-authored with the shoulder leading, followed by elbow and wrist. It eases into the final pose over 0.8 seconds. Pose transitions and animation frames blend continuously, including the return to relaxed hover. Muzzle and booster sockets use the same blend as the character.

The original model and 2D sprite presentation are preserved. 7,648 authored frames cover 32 directions in 30 atlases. Atlas frames are 128 pixels to keep memory close to the earlier 28-atlas mobile build. Intermediate views and animation frames are blended at the game's rendering frame rate.

[Watch the 60 fps gameplay close-up](fluid-preview.html) · [Play the update](./?v=0.10.0)

Validation: 101 gameplay checks, including all directions, turn speed and wrap-around, frame interpolation, shader support, aiming, firing, muzzle alignment, beam damage, pause, reset, and video capture. Physical iPhone testing remains unverified.

Source project: `outputs/PocketMechFlight` in the September 18 Codex workspace. Editable animation: `outputs/Gundam_FluidMovement.blend`. New runtime helper and shader: `GundamPoseBlend.cs` and `Resources/Shaders/GundamPoseBlend.shader`.
