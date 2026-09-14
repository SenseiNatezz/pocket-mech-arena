using UnityEngine;

namespace PocketMech
{
    // Independent special weapon; movement and primary fire remain responsive while charging.
    public sealed class MagnumBlast : MonoBehaviour
    {
        public const float Cooldown = 12, ChargeDuration = .35f, Range = 12, HalfWidth = .65f;
        public float Remaining { get; private set; }
        public bool Charging => charge > 0;
        public int Shots { get; private set; }
        float charge;
        Vector2 direction = Vector2.up;
        GameObject chargingGlow;
        PlayerMech player;
        public void ResetWeapon()
        {
            player = GetComponent<PlayerMech>(); Remaining = charge = 0; Shots = 0;
            if (chargingGlow != null) Destroy(chargingGlow);
        }
        public bool TryActivate()
        {
            var g = Game.Instance;
            if (g.State != RunState.Playing || Remaining > 0 || Charging) return false;
            var target = g.Nearest(transform.position);
            direction = target != null ? ((Vector2)(target.transform.position - transform.position)).normalized : (Vector2)transform.Find("Turret").up;
            if (direction.sqrMagnitude < .01f) direction = Vector2.up;
            Remaining = Cooldown; charge = ChargeDuration;
            if (!g.Headless)
            {
                chargingGlow = Visuals.Shape("Magnum charging core", g.World, transform.position, Vector2.one, Visuals.Blue, 310, true);
                chargingGlow.GetComponent<SpriteRenderer>().sprite = Visuals.Glow;
            }
            SoundBank.Play(5);
            return true;
        }
        void Update()
        {
            if (Game.Instance.State != RunState.Playing) return;
            if (Input.GetKeyDown(KeyCode.E)) TryActivate();
            Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
            if (!Charging) return;
            charge -= Time.deltaTime;
            if (chargingGlow != null)
            {
                chargingGlow.transform.position = transform.position + (Vector3)direction * .8f;
                chargingGlow.transform.localScale = Vector3.one * Mathf.Lerp(2, .6f, 1 - charge / ChargeDuration);
            }
            if (charge <= 0) Fire();
        }
        public static bool Intersects(Vector2 relative, Vector2 aim, float enemyRadius)
        {
            float along = Vector2.Dot(relative, aim);
            Vector2 closest = aim * Mathf.Clamp(along, 0, Range);
            return (relative - closest).sqrMagnitude <= (HalfWidth + enemyRadius) * (HalfWidth + enemyRadius);
        }
        void Fire()
        {
            if (chargingGlow != null) Destroy(chargingGlow);
            Shots++;
            Vector2 origin = (Vector2)transform.position + direction * .8f;
            // Copy because kills remove enemies from the live collection immediately.
            foreach (var enemy in Game.Instance.Enemies.ToArray())
            {
                if (enemy == null || !enemy.Alive || !Intersects((Vector2)enemy.transform.position - origin, direction, enemy.Radius)) continue;
                Vector2 impact = enemy.transform.position;
                enemy.Hurt(player.Damage * 8, direction, true);
                Visuals.Burst(impact, new Color(.05f, 1f, .9f), 12);
            }
            SoundBank.Play(4);
            if (!Game.Instance.Headless)
            {
                var pulse = new GameObject("Magnum beam discharge"); pulse.transform.SetParent(Game.Instance.World);
                pulse.AddComponent<MagnumBeamEffect>().Init(origin, direction);
            }
        }
    }

    // Repulsor-style white core, soft turquoise halo and compact muzzle spray.
    public sealed class MagnumBeamEffect : MonoBehaviour
    {
        static readonly Color Cyan = new Color(.02f, 1f, .88f);
        float age;
        SpriteRenderer halo, sheath, core, flash;
        SpriteRenderer[] sparks;
        LineRenderer[] rays;
        Material material;
        Texture2D glowTexture;
        Sprite glowSprite;
        Vector2 origin, aim, side;
        public void Init(Vector2 start, Vector2 direction)
        {
            origin = start; aim = direction; side = new Vector2(-aim.y, aim.x);
            // Soft cross-section, but a straight continuous beam along its length.
            glowTexture = new Texture2D(64, 128, TextureFormat.RGBA32, false);
            var pixels = new Color[64 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 64; x++)
            {
                float cross = Mathf.Abs((x - 31.5f) / 31.5f);
                float end = Mathf.Clamp01((127 - y) / 8f);
                pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Pow(1 - cross, 2) * end);
            }
            glowTexture.SetPixels(pixels); glowTexture.Apply();
            glowSprite = Sprite.Create(glowTexture, new Rect(0, 0, 64, 128), new Vector2(.5f, .5f), 64);
            halo = Beam("Soft turquoise bloom", 2.2f, Cyan, 301, true);
            sheath = Beam("Turquoise beam edge", .65f, Cyan, 302, false);
            core = Beam("Continuous white core", .4f, new Color(.9f, 1f, 1f), 303, false);
            flash = Visuals.Shape("Repulsor muzzle bloom", transform, start, Vector2.one * 2.3f, Cyan, 304, true).GetComponent<SpriteRenderer>();
            flash.sprite = Visuals.Glow;
            material = new Material(Shader.Find("Sprites/Default")); rays = new LineRenderer[8];
            for (int i = 0; i < rays.Length; i++)
            {
                var go = new GameObject("Muzzle flare spike"); go.transform.SetParent(transform);
                var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.positionCount = 2;
                line.startWidth = .2f; line.endWidth = .015f; line.startColor = Color.white; line.endColor = Cyan; line.sortingOrder = 305;
                float angle = (i - 3.5f) * 11;
                Vector2 ray = Quaternion.Euler(0, 0, angle) * aim;
                line.SetPosition(0, origin); line.SetPosition(1, origin + ray * (1.2f + (i % 3) * .45f)); rays[i] = line;
            }
            sparks = new SpriteRenderer[26];
            for (int i = 0; i < sparks.Length; i++)
            {
                var go = Visuals.Shape("Turquoise energy fleck", transform, start, new Vector2(.14f + (i % 3) * .04f, .08f), Cyan, 306);
                Visuals.Aim(go.transform, aim); sparks[i] = go.GetComponent<SpriteRenderer>();
            }
            Animate(0);
        }
        SpriteRenderer Beam(string name, float width, Color color, int order, bool soft)
        {
            var go = Visuals.Shape(name, transform, origin + aim * MagnumBlast.Range / 2,
                new Vector2(width, soft ? MagnumBlast.Range / 2 : MagnumBlast.Range), color, order);
            if (soft) go.GetComponent<SpriteRenderer>().sprite = glowSprite;
            Visuals.Aim(go.transform, aim); return go.GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            if (Game.Instance.State != RunState.Playing) return;
            age += Time.deltaTime; Animate(age);
            if (age >= .55f) Destroy(gameObject);
        }
        void Animate(float time)
        {
            float fade = Mathf.Clamp01((.55f - time) / .3f), pulse = .95f + .05f * Mathf.Sin(time * 60);
            Tint(halo, fade * .85f); Tint(sheath, fade); Tint(core, fade);
            core.transform.localScale = new Vector3(.4f * pulse, MagnumBlast.Range, 1);
            flash.transform.localScale = Vector3.one * (2.5f - time * 2); Tint(flash, fade);
            for (int i = 0; i < rays.Length; i++)
            {
                var c = Cyan; c.a = fade; rays[i].endColor = c; c = Color.white; c.a = fade; rays[i].startColor = c;
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                float forward = .4f + (i % 7) * .62f + time * (2 + i % 3);
                float spread = (i % 2 == 0 ? 1 : -1) * (.35f + (i % 5) * .18f + time * .65f);
                sparks[i].transform.position = origin + aim * forward + side * spread;
                Tint(sparks[i], fade * (.5f + .5f * Mathf.Abs(Mathf.Sin(i * 3 + time * 18))));
            }
        }
        static void Tint(SpriteRenderer renderer, float alpha) { var c = renderer.color; c.a = alpha; renderer.color = c; }
        void OnDestroy()
        {
            if (material != null) Destroy(material);
            if (glowSprite != null) Destroy(glowSprite);
            if (glowTexture != null) Destroy(glowTexture);
        }
    }
}
