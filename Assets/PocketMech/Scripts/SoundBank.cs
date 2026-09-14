using UnityEngine;

namespace PocketMech
{
    public sealed class SoundBank : MonoBehaviour
    {
        static SoundBank instance;
        AudioSource source;
        readonly AudioClip[] clips = new AudioClip[6];
        void Awake()
        {
            instance = this; source = gameObject.AddComponent<AudioSource>(); source.volume = .16f;
            for (int j = 0; j < 6; j++)
            {
                int n = j == 0 ? 2200 : 9000; var data = new float[n];
                for (int i = 0; i < n; i++) { float t = (float)i / 22050, fade = 1f - (float)i / n; float freq = j == 0 ? 900 - 600 * (1 - fade) : j == 1 ? 110 : j == 2 ? 640 : 440 + 440 * (1 - fade); data[i] = Mathf.Sin(t * freq * Mathf.PI * 2) * fade * fade * .5f; }
                if (j >= 4) for (int i = 0; i < n; i++) { float t = (float)i / n, envelope = j == 4 ? Mathf.Pow(1 - t, 2) : Mathf.Sin(t * Mathf.PI); float phase = j == 4 ? 130 * t - 100 * t * t : 80 * t + 300 * t * t; data[i] = envelope * (Mathf.Sin(phase * 6.283f) * .65f + Mathf.Sin(phase * 17.31f) * .25f); }
                clips[j] = AudioClip.Create("Synth " + j, n, 1, 22050, false); clips[j].SetData(data, 0);
            }
        }
        public static void Play(int id) { if (instance != null && !Game.Instance.Headless) instance.source.PlayOneShot(instance.clips[id]); }
        void OnDestroy() { if (instance == this) instance = null; foreach (var clip in clips) if (clip != null) Destroy(clip); }
    }
}
