using UnityEngine;

namespace PocketMech
{
    public sealed class CameraRig : MonoBehaviour
    {
        public static readonly Quaternion ViewRotation = Quaternion.LookRotation(new Vector3(0, 32, 24), Vector3.up);
        void LateUpdate()
        {
            var c = GetComponent<Camera>(); c.orthographic = true;
            c.orthographicSize = Mathf.Max(13.8f, 9.6f / Mathf.Max(.3f, c.aspect));
            c.backgroundColor = new Color(.035f, .04f, .055f); c.clearFlags = CameraClearFlags.SolidColor;
            transform.SetPositionAndRotation(new Vector3(0, -32, -24), ViewRotation);
        }
    }
}
