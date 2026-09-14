using UnityEngine;
namespace PocketMech
{
    public sealed class CameraRig : MonoBehaviour
    {
        public static readonly Quaternion ViewRotation = Quaternion.LookRotation(new Vector3(0, 32, 24), Vector3.up);
        Vector3 focus, velocity;
        void Start() { if (GetComponent<SpaceBackdrop>() == null) gameObject.AddComponent<SpaceBackdrop>(); }
        void LateUpdate()
        {
            var c = GetComponent<Camera>(); c.orthographic = true;
            c.orthographicSize = Mathf.Max(11.4f, 7.4f / Mathf.Max(.3f, c.aspect));
            c.backgroundColor = new Color(.018f, .025f, .07f); c.clearFlags = CameraClearFlags.SolidColor;
            var g = Game.Instance;
            Vector3 target = Vector3.zero;
            if (g != null && g.Player != null && g.State != RunState.Briefing)
                target = new Vector3(Mathf.Clamp(g.Player.transform.position.x * .65f, -4, 4), Mathf.Clamp(g.Player.transform.position.y * .55f, -3, 3), 0);
            if (g == null || g.State == RunState.Playing || g.State == RunState.Briefing)
                focus = Vector3.SmoothDamp(focus, target, ref velocity, .25f);
            transform.SetPositionAndRotation(new Vector3(0, -32, -24) + focus, ViewRotation);
        }
    }
}
