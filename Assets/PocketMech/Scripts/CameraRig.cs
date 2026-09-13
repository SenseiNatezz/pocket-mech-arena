using UnityEngine;

namespace PocketMech
{
    public sealed class CameraRig : MonoBehaviour
    {
        void LateUpdate()
        {
            var g = Game.Instance; if (g == null || g.Player == null) return;
            var c = GetComponent<Camera>(); c.orthographicSize = 11.55f;
            transform.position = new Vector3(0, 0, -10);
        }
    }
}
