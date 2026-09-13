using System.Collections.Generic;
using UnityEngine;

namespace PocketMech
{
    public sealed class Projectile : MonoBehaviour
    {
        static readonly Stack<Projectile> pool = new Stack<Projectile>();
        readonly HashSet<Enemy> hit = new HashSet<Enemy>();
        Vector2 direction;
        float speed, damage, life, age;
        bool friendly, missile, critical;
        int piercing;
        Enemy target;
        public static int ActiveCount { get; private set; }
        public static void ResetPool() { pool.Clear(); ActiveCount = 0; }
        public static Projectile Launch(Vector2 p, Vector2 d, float velocity, float power, bool player, int pierce = 0, bool homing = false, Enemy victim = null, bool crit = false)
        {
            Projectile b = null;
            while (pool.Count > 0 && b == null) b = pool.Pop();
            if (b == null) b = Instantiate(Resources.Load<GameObject>("Prefabs/Projectile"), Game.Instance.World).GetComponent<Projectile>();
            b.transform.position = p; b.direction = d.normalized; b.speed = velocity; b.damage = power; b.friendly = player; b.piercing = pierce; b.missile = homing; b.target = victim; b.critical = crit; b.life = player ? 1.7f : 5; b.age = 0; b.hit.Clear();
            b.GetComponent<SpriteRenderer>().sprite = Visuals.Disc;
            b.GetComponent<SpriteRenderer>().color = player ? new Color(.7f, .97f, 1) : new Color(1, .6f, .5f);
            var glow = b.transform.Find("Glow");
            if (glow == null) { glow = new GameObject("Glow", typeof(SpriteRenderer)).transform; glow.SetParent(b.transform, false); glow.localScale = new Vector3(4, 2, 1); glow.GetComponent<SpriteRenderer>().sprite = Visuals.Glow; glow.GetComponent<SpriteRenderer>().sortingOrder = 5; }
            glow.GetComponent<SpriteRenderer>().color = player ? Visuals.Blue : Visuals.Red;
            b.transform.localScale = homing ? new Vector3(.24f, .55f, 1) : player ? new Vector3(.12f, .62f, 1) : new Vector3(.28f, .28f, 1);
            Visuals.Aim(b.transform, d); b.gameObject.SetActive(true); ActiveCount++;
            if (player) Game.Instance.ShotsFired++; else Game.Instance.EnemyShotsFired++;
            return b;
        }
        void Update()
        {
            var g = Game.Instance; if (g.State != RunState.Playing) return;
            float dt = Time.deltaTime; age += dt; life -= dt;
            if (life <= 0) { Recycle(); return; }
            if (missile)
            {
                // Enemy missiles stop steering after one second so a dash can reliably evade them.
                if (friendly && (target == null || !target.Alive)) target = g.Nearest(transform.position);
                if (friendly && target != null) direction = Vector2.Lerp(direction, ((Vector2)target.transform.position - (Vector2)transform.position).normalized, dt * 7).normalized;
                else if (!friendly && age < 1) direction = Vector2.Lerp(direction, ((Vector2)g.Player.transform.position - (Vector2)transform.position).normalized, dt * 2).normalized;
            }
            Vector2 from = transform.position, to = from + direction * speed * dt;
            transform.position = to; Visuals.Aim(transform, direction);
            if (friendly)
            {
                // Segment checks prevent fast beam bolts from tunneling through targets.
                for (int i = g.Enemies.Count - 1; i >= 0; i--)
                {
                    if (i >= g.Enemies.Count) continue;
                    var e = g.Enemies[i];
                    if (!e.Alive || hit.Contains(e) || DistanceToSegment(e.transform.position, from, to) > e.Radius + .1f) continue;
                    hit.Add(e);
                    if (missile)
                    {
                        var victims = g.Enemies.ToArray();
                        foreach (var v in victims) if (v.Alive && Vector2.Distance(v.transform.position, to) < 2.4f) v.Hurt(damage, direction, critical);
                        Visuals.Burst(to, Visuals.Blue, 12); Recycle(); return;
                    }
                    e.Hurt(damage, direction, critical);
                    if (piercing-- <= 0) { Recycle(); return; }
                }
            }
            else if (DistanceToSegment(g.Player.transform.position, from, to) < .56f) { g.Player.Hurt(damage); Recycle(); }
        }
        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a; float t = ab.sqrMagnitude < .0001f ? 0 : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }
        void Recycle() { gameObject.SetActive(false); ActiveCount = Mathf.Max(0, ActiveCount - 1); pool.Push(this); }
    }
}
