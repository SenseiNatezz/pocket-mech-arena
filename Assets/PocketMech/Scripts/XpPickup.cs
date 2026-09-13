using UnityEngine;

namespace PocketMech
{
    public sealed class XpPickup : MonoBehaviour
    {
        int amount;
        public static void Drop(Vector2 position, int xp)
        {
            var g = Game.Instance;
            // Merge distant drops when the cap is reached; XP is never silently discarded.
            if (g.Pickups.Count >= 160) { g.Pickups[0].amount += xp; return; }
            var p = Instantiate(Resources.Load<GameObject>("Prefabs/XpPickup"), position, Quaternion.Euler(0, 0, 45), g.World).GetComponent<XpPickup>();
            p.amount = xp; g.Pickups.Add(p);
        }
        void Update()
        {
            var g = Game.Instance; if (g.State != RunState.Playing) return;
            Vector2 d = g.Player.transform.position - transform.position;
            if (d.sqrMagnitude < 12.25f) transform.position = Vector3.MoveTowards(transform.position, g.Player.transform.position, 10 * Time.deltaTime);
            if (d.sqrMagnitude < .64f) { g.Pickups.Remove(this); g.GainXp(amount); Destroy(gameObject); }
        }
    }
    public sealed class Hazard : MonoBehaviour
    {
        float countdown, duration, radius, damage;
        SpriteRenderer ring;
        public static void Warn(Vector2 p, float r, float delay, float power)
        {
            var go = Visuals.Shape("Attack Warning", Game.Instance.World, p, Vector2.one * r * 2, new Color(1, .15f, .2f, .16f), 2, true);
            var h = go.AddComponent<Hazard>(); h.radius = r; h.countdown = h.duration = delay; h.damage = power; h.ring = go.GetComponent<SpriteRenderer>();
            var outline = Visuals.Shape("Warning edge", go.transform, Vector2.zero, Vector2.one, new Color(1, .2f, .15f, .8f), 2); outline.GetComponent<SpriteRenderer>().sprite = Visuals.Ring;
        }
        void Update()
        {
            if (Game.Instance.State != RunState.Playing) return;
            countdown -= Time.deltaTime; ring.color = new Color(1, .15f, .2f, Mathf.Lerp(.28f, .06f, countdown / duration));
            if (countdown > 0) return;
            if (damage > 0 && Vector2.Distance(transform.position, Game.Instance.Player.transform.position) < radius + .4f) Game.Instance.Player.Hurt(damage);
            if (damage > 0) Visuals.Burst(transform.position, Visuals.Red, 12);
            Destroy(gameObject);
        }
    }
}
