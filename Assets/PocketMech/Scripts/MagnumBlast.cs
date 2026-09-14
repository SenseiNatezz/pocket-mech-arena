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
                Visuals.Burst(impact, new Color(.7f, .65f, 1), 12);
            }
            SoundBank.Play(4);
            if (!Game.Instance.Headless)
            {
                var pulse = new GameObject("Magnum beam discharge"); pulse.transform.SetParent(Game.Instance.World);
                pulse.AddComponent<MagnumBeamEffect>().Init(origin, direction);
            }
        }
    }

    // Layered beam, muzzle shockwave and jagged electric branches; no imported anime assets.
    public sealed class MagnumBeamEffect : MonoBehaviour
    {
        float age;
        SpriteRenderer outer, middle, core, ring, flash;
        Vector2 origin, aim;
        LineRenderer[] arcs;
        Material arcMaterial;
        public void Init(Vector2 start, Vector2 direction)
        {
            origin = start; aim = direction;
            outer = Beam("Violet corona", 1.7f, new Color(.52f, .25f, 1, .55f), 301);
            middle = Beam("Cyan sheath", .85f, new Color(.2f, .8f, 1, .9f), 302);
            core = Beam("White hot core", .27f, Color.white, 303);
            var r = Visuals.Shape("Muzzle shock ring", transform, start, Vector2.one, Visuals.Blue, 304, true);
            ring = r.GetComponent<SpriteRenderer>(); ring.sprite = Visuals.Ring;
            flash = Visuals.Shape("Muzzle flash", transform, start, Vector2.one * 3, Color.white, 305, true).GetComponent<SpriteRenderer>(); flash.sprite = Visuals.Glow;
            arcMaterial = new Material(Shader.Find("Sprites/Default")); arcs = new LineRenderer[3];
            for (int a = 0; a < arcs.Length; a++)
            {
                var go = new GameObject("Electrical branch"); go.transform.SetParent(transform);
                var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = arcMaterial; line.positionCount = 13;
                line.startWidth = .055f; line.endWidth = .018f; line.startColor = line.endColor = Visuals.Blue;
                line.sortingOrder = 306; arcs[a] = line;
            }
            Animate(0);
        }
        SpriteRenderer Beam(string name, float width, Color color, int order)
        {
            var go = Visuals.Shape(name, transform, origin + aim * MagnumBlast.Range / 2,
                new Vector2(width, MagnumBlast.Range), color, order);
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
            float fade = Mathf.Clamp01(1 - time / .55f), pulse = .7f + .3f * Mathf.Sin(time * 80);
            Tint(outer, fade * .5f); Tint(middle, fade * .9f); Tint(core, fade);
            core.transform.localScale = new Vector3(.27f * pulse * fade, MagnumBlast.Range, 1);
            ring.transform.localScale = Vector3.one * (1 + time * 10); Tint(ring, fade);
            flash.transform.localScale = Vector3.one * (1 + fade * 3); Tint(flash, fade);
            Vector2 side = new Vector2(-aim.y, aim.x);
            for (int a = 0; a < arcs.Length; a++)
            {
                var color = Visuals.Blue; color.a = fade; arcs[a].startColor = arcs[a].endColor = color;
                for (int i = 0; i < 13; i++)
                {
                    float jitter = Mathf.Sin(i * 13.7f + a * 7.3f + Mathf.Floor(time * 24) * 2.4f);
                    arcs[a].SetPosition(i, origin + aim * i + side * ((a - 1) * .55f + jitter * .45f));
                }
            }
        }
        static void Tint(SpriteRenderer renderer, float alpha) { var c = renderer.color; c.a = alpha; renderer.color = c; }
        void OnDestroy() { if (arcMaterial != null) Destroy(arcMaterial); }
    }
}
