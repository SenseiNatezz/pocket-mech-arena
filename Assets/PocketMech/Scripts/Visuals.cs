using UnityEngine;

namespace PocketMech
{
    // Generated shapes are placeholders; prefab child renderers can be replaced with sprites.
    public static class Visuals
    {
        public static readonly Color Blue = new Color(.22f, .83f, 1f), Red = new Color(1f, .25f, .3f);
        public static Sprite Square, Disc, Glow, Ring;
        static Material textMaterial;
        public static void Initialize()
        {
            if (Square != null) return;
            Square = Resources.Load<Sprite>("Art/Square"); Disc = Resources.Load<Sprite>("Art/Disc");
            if (Square == null) Square = MakeSprite(false);
            if (Disc == null) Disc = MakeSprite(true);
            var glowTex = new Texture2D(64, 64); var glowPixels = new Color[4096];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) { float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32; glowPixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Max(0, 1 - d), 2)); }
            glowTex.SetPixels(glowPixels); glowTex.Apply(); Glow = Sprite.Create(glowTex, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64);
            var discTex = new Texture2D(256, 256); var ringTex = new Texture2D(256, 256); var dp = new Color[65536]; var rp = new Color[65536];
            for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++) { float d = Vector2.Distance(new Vector2(x, y), new Vector2(127.5f, 127.5f)); float a = Mathf.Clamp01(127 - d); dp[y * 256 + x] = new Color(1, 1, 1, a); rp[y * 256 + x] = new Color(1, 1, 1, a * Mathf.Clamp01(d - 122)); }
            discTex.SetPixels(dp); discTex.Apply(); ringTex.SetPixels(rp); ringTex.Apply(); Disc = Sprite.Create(discTex, new Rect(0, 0, 256, 256), new Vector2(.5f, .5f), 256); Ring = Sprite.Create(ringTex, new Rect(0, 0, 256, 256), new Vector2(.5f, .5f), 256);
        }
        static Sprite MakeSprite(bool circle)
        {
            var t = new Texture2D(32, 32); var p = new Color[1024];
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++) p[y * 32 + x] = !circle || Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15.5f ? Color.white : Color.clear;
            t.SetPixels(p); t.Apply(); return Sprite.Create(t, new Rect(0, 0, 32, 32), new Vector2(.5f, .5f), 32);
        }
        public static GameObject Shape(string name, Transform parent, Vector2 position, Vector2 size, Color color, int order = 0, bool circle = false)
        {
            Initialize(); var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = size;
            var r = go.AddComponent<SpriteRenderer>(); r.sprite = circle ? Disc : Square; r.color = color; r.sortingOrder = order; return go;
        }
        public static void Mech(Transform root, bool boss = false)
        {
            Color armor = boss ? new Color(.38f, .22f, .26f) : new Color(.55f, .73f, .82f);
            Color glow = boss ? Red : Blue;
            Shape("Shadow", root, new Vector2(.06f, -.15f), new Vector2(1.4f, 1.3f), new Color(0, 0, 0, .45f), 1, true);
            var legs = new GameObject("Legs").transform; legs.SetParent(root, false);
            Shape("Left Tread", legs, new Vector2(-.43f, 0), new Vector2(.32f, 1.05f), new Color(.15f, .23f, .28f), 3);
            Shape("Right Tread", legs, new Vector2(.43f, 0), new Vector2(.32f, 1.05f), new Color(.15f, .23f, .28f), 3);
            var turret = new GameObject("Turret").transform; turret.SetParent(root, false);
            Shape("Chassis", turret, Vector2.zero, new Vector2(.8f, .82f), armor, 4);
            Shape("Core", turret, new Vector2(0, .12f), new Vector2(.36f, .24f), glow, 5);
            Shape("Beam Rifle", turret, new Vector2(.29f, .61f), new Vector2(.19f, .83f), armor, 4);
            Shape("Muzzle", turret, new Vector2(.29f, 1.02f), new Vector2(.15f, .14f), glow, 5);
            Shape("Booster L", legs, new Vector2(-.35f, -.56f), new Vector2(.2f, .2f), glow, 4);
            Shape("Booster R", legs, new Vector2(.35f, -.56f), new Vector2(.2f, .2f), glow, 4);
        }
        public static void Enemy(Transform root, EnemyKind kind)
        {
            if (kind == EnemyKind.Boss) { Mech(root, true); root.localScale = Vector3.one * 2.1f; return; }
            Color c = kind == EnemyKind.Shield ? new Color(.7f, .52f, .25f) : kind == EnemyKind.Bomber ? new Color(.76f, .36f, .8f) : new Color(.72f, .28f, .32f);
            Shape("Shadow", root, new Vector2(.07f, -.1f), Vector2.one, new Color(0, 0, 0, .35f), 1, true);
            Shape("Hull", root, Vector2.zero, new Vector2(.65f, .72f), c, 3, kind == EnemyKind.Scout || kind == EnemyKind.Bomber);
            Shape("Core", root, new Vector2(0, .15f), new Vector2(.24f, .2f), Red, 4);
            Shape("Wing L", root, new Vector2(-.4f, 0), new Vector2(.18f, .5f), c, 3);
            Shape("Wing R", root, new Vector2(.4f, 0), new Vector2(.18f, .5f), c, 3);
            if (kind == EnemyKind.Shield) Shape("Frontal Shield", root, new Vector2(0, .48f), new Vector2(1.05f, .15f), new Color(1f, .75f, .25f), 4);
            if (kind == EnemyKind.Shooter) Shape("Cannon", root, new Vector2(0, .5f), new Vector2(.14f, .4f), Red, 4);
            if (kind == EnemyKind.Rush) Shape("Blade", root, new Vector2(0, .55f), new Vector2(.08f, .4f), Color.white, 4);
        }
        public static void Aim(Transform t, Vector2 dir) { if (dir.sqrMagnitude > .001f) t.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90); }
        public static void Afterimage(Transform part)
        {
            if (Game.Instance.Headless) return;
            foreach (var original in part.GetComponentsInChildren<SpriteRenderer>())
            {
                if (original.sprite == Glow) continue;
                var ghost = new GameObject("Boost afterimage", typeof(SpriteRenderer)); ghost.transform.SetParent(Game.Instance.World);
                ghost.transform.position = original.transform.position; ghost.transform.rotation = original.transform.rotation; ghost.transform.localScale = original.transform.lossyScale;
                var r = ghost.GetComponent<SpriteRenderer>(); r.sprite = original.sprite; r.sortingOrder = 2; r.color = new Color(.1f, .75f, 1, .4f); ghost.AddComponent<Transient>().Init(Vector2.zero, .16f);
            }
        }
        public static void Burst(Vector2 p, Color c, int count = 8)
        {
            if (Game.Instance != null && Game.Instance.Headless) return;
            if (count >= 8)
            {
                var impact = new GameObject("Illustrated impact", typeof(SpriteRenderer)); impact.transform.SetParent(Game.Instance.World); impact.transform.position = p;
                var ir = impact.GetComponent<SpriteRenderer>(); ir.sprite = Resources.Load<Sprite>("Illustrated/Impact"); ir.sortingOrder = 12;
                if (ir.sprite != null) impact.transform.localScale = Vector3.one * (count >= 30 ? 3.5f : 1.6f) / ir.sprite.bounds.size.x;
                impact.AddComponent<Transient>().Init(Vector2.zero, .3f);
            }
            for (int i = 0; i < count; i++)
            {
                var g = Shape("Spark", Game.Instance.World, p, Vector2.one * Random.Range(.08f, .21f), c, 9, true);
                g.AddComponent<Transient>().Init(Random.insideUnitCircle * 4, .3f + Random.value * .25f);
            }
        }
        public static void Number(Vector2 p, string value, Color c)
        {
            if (Game.Instance.Headless) return;
            var g = new GameObject("Damage"); g.transform.SetParent(Game.Instance.World); g.transform.position = p;
            var t = g.AddComponent<TextMesh>(); t.text = value; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = 32; t.characterSize = .13f; t.anchor = TextAnchor.MiddleCenter; t.color = c;
            if (textMaterial == null) textMaterial = t.font.material;
            var r = g.GetComponent<MeshRenderer>(); r.sharedMaterial = textMaterial; r.sortingOrder = 15;
            var shadow = new GameObject("Ink outline", typeof(TextMesh)); shadow.transform.SetParent(g.transform, false); shadow.transform.localPosition = new Vector3(.035f, -.035f, 0);
            var st = shadow.GetComponent<TextMesh>(); st.text = value; st.font = t.font; st.fontSize = 32; st.characterSize = .13f; st.anchor = TextAnchor.MiddleCenter; st.color = Color.black;
            var sr = shadow.GetComponent<MeshRenderer>(); sr.sharedMaterial = textMaterial; sr.sortingOrder = 14;
            g.AddComponent<Transient>().Init(new Vector2(Random.Range(-.3f, .3f), 1.5f), .65f);
        }
    }
    public sealed class Transient : MonoBehaviour
    {
        Vector2 velocity; float life, total, startingAlpha = 1;
        public void Init(Vector2 v, float seconds) { velocity = v; life = total = seconds; var r = GetComponent<SpriteRenderer>(); if (r != null) startingAlpha = r.color.a; }
        void Update()
        {
            if (Game.Instance.State != RunState.Playing) return;
            life -= Time.deltaTime; transform.position += (Vector3)(velocity * Time.deltaTime);
            var r = GetComponent<SpriteRenderer>(); if (r != null) { Color c = r.color; c.a = startingAlpha * Mathf.Clamp01(life / total); r.color = c; }
            if (life <= 0) Destroy(gameObject);
        }
    }
}
