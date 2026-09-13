using UnityEngine;

namespace PocketMech
{
    public static class MotionMath
    {
        public static Vector2 Respond(Vector2 current, Vector2 target, float seconds, float dt)
            => Vector2.Lerp(current, target, 1 - Mathf.Exp(-dt / Mathf.Max(.001f, seconds)));
        // Integrated ease-out displacement, independent of the number of rendered frames.
        public static float DashProgress(float u) { u = Mathf.Clamp01(u); return 1 - (1 - u) * (1 - u); }
        public static void Turn(Transform t, Vector2 direction, float degreesPerSecond, float dt)
        {
            if (direction.sqrMagnitude < .0001f) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.Euler(0, 0, angle), degreesPerSecond * dt);
        }
    }
}
