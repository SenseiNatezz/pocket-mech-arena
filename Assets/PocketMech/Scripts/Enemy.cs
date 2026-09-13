using UnityEngine;

namespace PocketMech
{
    public sealed class Enemy : MonoBehaviour
    {
        public EnemyKind Kind { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool Alive => Health > 0;
        public bool IsBoss => Kind == EnemyKind.Boss || Kind == EnemyKind.GlacierColossus;
        public int SpecialAttacks { get; private set; }
        public float Radius => IsBoss ? 1.3f : Kind == EnemyKind.Elite ? .8f : .48f;
        float speed, attack, charge, chargeClock, contact, railWindup;
        Vector2 railAim;
        int bossPattern;
        Vector2 chargeDirection;
        Vector2 velocity, knockback;
        Transform artwork; float phase;
        Transform turret;
        public void Init(EnemyKind kind, float elapsed)
        {
            Kind = kind;
            float scale = 1 + elapsed / 900;
            MaxHealth = IsBoss ? Game.Instance.balance.bossHealth : (kind == EnemyKind.Shield ? 560 : kind == EnemyKind.Rush ? 200 : kind == EnemyKind.Bomber ? 300 : 180) * scale;
            if (kind == EnemyKind.Elite) MaxHealth = 1800;
            if (kind == EnemyKind.IceSkimmer) MaxHealth = 150 * scale;
            if (kind == EnemyKind.RailSentinel) MaxHealth = 340 * scale;
            if (kind == EnemyKind.CryoMortar) MaxHealth = 420 * scale;
            if (Game.Instance.Profile.mission == 1) MaxHealth *= 1.3f;
            Health = MaxHealth; speed = IsBoss ? .8f : kind == EnemyKind.Shield ? 1.3f : kind == EnemyKind.Rush ? 2.3f : 1.7f;
            if (kind == EnemyKind.IceSkimmer) speed = 2.6f;
            if (kind == EnemyKind.CryoMortar) speed = 1.2f;
            attack = Random.Range(1f, 2.5f); chargeClock = 2; turret = transform.Find("Turret");
            // The illustrated enemies face down on the source sheet; gameplay forward is up.
            var shell = transform.Find("Illustrated shell"); if (shell != null) shell.localRotation = Quaternion.Euler(0, 0, 180);
            artwork = shell; phase = Random.value * 6.28f;
            if (kind == EnemyKind.Shooter) attack = .6f;
            if (kind == EnemyKind.Elite) { transform.localScale *= 1.55f; speed = 2.1f; if (shell != null) shell.GetComponent<SpriteRenderer>().color = new Color(1, .6f, .65f); }
        }
        void Update()
        {
            var g = Game.Instance; if (g.State != RunState.Playing || !Alive) return;
            float dt = Time.deltaTime;
            Vector2 delta = g.Player.transform.position - transform.position;
            float distance = delta.magnitude; Vector2 dir = delta.normalized;
            Vector2 movement = dir * speed;
            if (Kind == EnemyKind.Shooter) movement *= distance < 5 ? -1 : distance > 7 ? 1 : 0;
            if (Kind == EnemyKind.Bomber || Kind == EnemyKind.CryoMortar) movement *= distance > 6 ? 1 : .2f;
            if (IsBoss) movement *= distance > 6 ? 1 : 0;
            if (Kind == EnemyKind.Rush || Kind == EnemyKind.Elite)
            {
                chargeClock -= dt;
                if (chargeClock <= 0) { chargeClock = 3.3f; charge = .9f; chargeDirection = dir; Hazard.Warn((Vector2)transform.position + dir * 2, .6f, .55f, 0); }
                if (charge > 0) { charge -= dt; movement = charge > .4f ? Vector2.zero : chargeDirection * 8; }
            }
            if (Kind == EnemyKind.IceSkimmer) movement += new Vector2(-dir.y, dir.x) * Mathf.Sin(Time.time * 3.2f + phase) * 1.5f;
            if (Kind == EnemyKind.RailSentinel) {
                movement *= distance < 5 ? -1 : distance > 7 ? 1 : 0;
                if (railWindup > 0) {
                    movement = Vector2.zero; railWindup -= dt;
                    if (railWindup <= 0) {
                        Vector2 side = new Vector2(-railAim.y, railAim.x) * .26f;
                        Projectile.Launch((Vector2)transform.position + side, railAim, 10, 65, false);
                        Projectile.Launch((Vector2)transform.position - side, railAim, 10, 65, false);
                        SpecialAttacks++;
                    }
                }
            }
            // Local separation prevents a swarm from collapsing into a single unreadable pile.
            Vector2 separation = Vector2.zero;
            foreach (var other in g.Enemies)
            {
                if (other == this || !other.Alive) continue;
                Vector2 d = transform.position - other.transform.position; float sq = d.sqrMagnitude;
                if (sq > .001f && sq < 1.1f) separation += d.normalized * (1.1f - sq);
            }
            velocity = MotionMath.Respond(velocity, movement + separation, Kind == EnemyKind.Rush && charge > 0 ? .035f : .12f, dt);
            transform.position = g.Clamp((Vector2)transform.position + (velocity + knockback) * dt, Radius);
            knockback *= Mathf.Exp(-dt * 12);
            MotionMath.Turn(turret != null ? turret : transform, dir, IsBoss ? 100 : 300, dt);
            if (artwork != null) artwork.localPosition = new Vector3(0, Mathf.Sin(Time.time * 5 + phase) * (IsBoss ? .015f : .045f), 0);
            contact -= dt;
            if (distance < Radius + .45f && contact <= 0) { contact = 1; g.Player.Hurt(IsBoss ? 150 : 65); }
            attack -= dt;
            if (attack > 0) return;
            switch (Kind)
            {
                case EnemyKind.RailSentinel:
                    attack = 3.1f; railWindup = .85f; railAim = dir;
                    // Lock the target now, allowing the pilot to sidestep during the windup.
                    for (int i = 1; i <= 3; i++) Hazard.Warn((Vector2)transform.position + dir * (i * 2), .3f, .85f, 0);
                    break;
                case EnemyKind.CryoMortar:
                    attack = 4.5f; SpecialAttacks++;
                    Vector2 lateral = new Vector2(-dir.y, dir.x) * 1.3f;
                    Hazard.Warn(g.Clamp((Vector2)g.Player.transform.position + lateral), 1.1f, 1.5f, 95);
                    Hazard.Warn(g.Clamp((Vector2)g.Player.transform.position - lateral), 1.1f, 1.5f, 95);
                    break;
                case EnemyKind.GlacierColossus:
                    attack = 3.2f; SpecialAttacks++;
                    switch (bossPattern++ % 3) {
                        case 0:
                            for (int i = 0; i < 12; i++) Shoot(dir, i * 30 + bossPattern * 13, 3.8f, 65);
                            break;
                        case 1:
                            for (int i = -1; i <= 1; i++) Hazard.Warn(g.Clamp((Vector2)g.Player.transform.position + Vector2.right * i * 2.5f), 1, 1.65f, 120);
                            break;
                        case 2:
                            for (int i = 0; i < 3; i++) if (g.Enemies.Count < g.balance.maxEnemies) g.Spawn(EnemyKind.IceSkimmer, g.Clamp((Vector2)transform.position + Random.insideUnitCircle * 3));
                            break;
                    }
                    break;
                case EnemyKind.Elite: attack = 2.2f; for (int i = -1; i <= 1; i++) Shoot(dir, i * 16, 6, 65); break;
                case EnemyKind.Shooter: attack = 1.8f; Shoot(dir, 0, 5, 45); break;
                case EnemyKind.Bomber: attack = 3.8f; Hazard.Warn(g.Player.transform.position, 1.6f, 1.25f, 110); break;
                case EnemyKind.Boss:
                    attack = 2.8f;
                    switch (bossPattern++ % 4)
                    {
                        case 0: for (int i = -2; i <= 2; i++) Shoot(dir, i * 17, 5.4f, 95); break;
                        case 1: Hazard.Warn(transform.position, 4.2f, 1.2f, 180); break;
                        case 2: for (int i = -1; i <= 1; i++) Projectile.Launch((Vector2)transform.position + dir * 1.4f, Quaternion.Euler(0, 0, i * 35) * dir, 4.5f, 90, false, 0, true); break;
                        case 3: for (int i = 0; i < 4; i++) if (g.Enemies.Count < g.balance.maxEnemies) g.Spawn(i % 2 == 0 ? EnemyKind.Scout : EnemyKind.Shooter, g.Clamp((Vector2)transform.position + Random.insideUnitCircle * 3)); break;
                    }
                    break;
                default: attack = 2; break;
            }
        }
        void Shoot(Vector2 dir, float angle, float speedValue, float damage) => Projectile.Launch((Vector2)transform.position + dir * .7f, Quaternion.Euler(0, 0, angle) * dir, speedValue, damage, false);
        public void Hurt(float amount, Vector2 incoming, bool critical)
        {
            if (!Alive) return;
            if (Kind == EnemyKind.Shield && Vector2.Dot(transform.up, -incoming.normalized) > .35f) amount *= .3f;
            Health = Mathf.Max(0, Health - amount); Game.Instance.DamageEvents++;
            if (!IsBoss) knockback += incoming.normalized * .8f;
            Visuals.Number((Vector2)transform.position + Vector2.up * .5f, Mathf.RoundToInt(amount).ToString() + (critical ? "!" : ""), critical ? new Color(1, .8f, .25f) : Color.white);
            if (Health <= 0)
            {
                Visuals.Burst(transform.position, IsBoss ? Visuals.Red : new Color(1, .65f, .22f), IsBoss ? 40 : 9);
                Game.Instance.EnemyKilled(this); Destroy(gameObject);
            }
        }
    }
}
