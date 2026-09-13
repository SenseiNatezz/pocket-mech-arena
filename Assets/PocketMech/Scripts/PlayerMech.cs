using UnityEngine;

namespace PocketMech
{
    public sealed class PlayerMech : MonoBehaviour
    {
        public float Health, MaxHealth, Damage, Speed, FireInterval, DashCooldown, Shield;
        public float DashRemaining { get; private set; }
        public bool Invulnerable => dashTime > 0 || hitGrace > 0;
        public bool IsDashing => dashTime > 0;
        public Vector2 DebugMovement;
        public bool Automated;
        public int Pierce;
        public float CritChance;
        Transform turret, legs;
        Transform thrusterLeft, thrusterRight;
        Vector2 lastDirection = Vector2.up, dashDirection;
        Vector2 moveVelocity;
        public Vector2 MoveVelocity => moveVelocity;
        float dashElapsed, dashLength, recoil, stepPhase;
        float fire, dashTime, hitGrace, side, missile, nano, barrier, trail;
        public void Init(Balance b)
        {
            transform.position = new Vector3(0, -3, 0); transform.rotation = Quaternion.identity;
            turret = transform.Find("Turret"); legs = transform.Find("Legs");
            if (thrusterLeft == null)
            {
                thrusterLeft = Visuals.Shape("Thruster glow L", legs, new Vector2(-.24f, -.86f), new Vector2(.25f, .6f), Visuals.Blue, 2).transform;
                thrusterRight = Visuals.Shape("Thruster glow R", legs, new Vector2(.24f, -.86f), new Vector2(.25f, .6f), Visuals.Blue, 2).transform;
                thrusterLeft.GetComponent<SpriteRenderer>().sprite = Visuals.Glow; thrusterRight.GetComponent<SpriteRenderer>().sprite = Visuals.Glow;
            }
            Health = MaxHealth = b.playerHealth; Damage = b.damage; Speed = b.playerSpeed; FireInterval = b.shotInterval; DashCooldown = b.dashCooldown;
            Shield = DashRemaining = dashTime = hitGrace = fire = side = missile = 0; nano = 10; barrier = 20; Pierce = 0; CritChance = 0; lastDirection = Vector2.up;
            moveVelocity = Vector2.zero; dashElapsed = recoil = stepPhase = 0; turret.localPosition = legs.localPosition = Vector3.zero; turret.localRotation = legs.localRotation = Quaternion.identity;
            foreach (Transform t in turret) if (t.name.StartsWith("Upgrade")) Destroy(t.gameObject);
            Game.Instance.Profile.Apply(this);
            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
                if (!sprite.name.Contains("glow")) sprite.color = Equipment.Tints[Game.Instance.Profile.color];
        }
        void Update()
        {
            var g = Game.Instance; if (g.State != RunState.Playing) return;
            float dt = Time.deltaTime;
            Vector2 input = ReadMovement();
            if (input.sqrMagnitude > .01f) lastDirection = input.normalized;
            if (Input.GetKeyDown(KeyCode.Space)) TryDash();
            DashRemaining = Mathf.Max(0, DashRemaining - dt); hitGrace -= dt;
            moveVelocity = MotionMath.Respond(moveVelocity, input * Speed, input.sqrMagnitude > .001f ? .045f : .028f, dt);
            Vector2 displacement = moveVelocity * dt;
            if (dashTime > 0)
            {
                float duration = g.balance.dashDuration, oldProgress = MotionMath.DashProgress(dashElapsed / duration);
                dashElapsed = Mathf.Min(duration, dashElapsed + dt);
                displacement = dashDirection * dashLength * (MotionMath.DashProgress(dashElapsed / duration) - oldProgress);
            }
            Vector2 previous = transform.position;
            transform.position = g.Clamp(previous + displacement);
            // Cancel blocked-axis momentum so corners never accumulate drift.
            if (Mathf.Abs(transform.position.x - previous.x) < .00001f) moveVelocity.x = 0;
            if (Mathf.Abs(transform.position.y - previous.y) < .00001f) moveVelocity.y = 0;
            MotionMath.Turn(legs, lastDirection, 540, dt);
            stepPhase += moveVelocity.magnitude * dt * 3.5f;
            legs.localPosition = new Vector3(0, Mathf.Sin(stepPhase) * .035f * Mathf.Clamp01(moveVelocity.magnitude / Speed), 0);
            recoil = Mathf.MoveTowards(recoil, 0, dt * 1.2f);
            turret.localPosition = -(Vector3)turret.up * recoil;
            float flare = (dashTime > 0 ? 1.3f : input.sqrMagnitude > .01f ? .65f : .22f) * (1 + .15f * Mathf.Sin(Time.time * 31));
            thrusterLeft.localScale = thrusterRight.localScale = new Vector3(.35f, flare, 1);
            if (dashTime > 0)
            {
                dashTime -= dt; trail -= dt;
                if (trail <= 0) { Visuals.Afterimage(turret); Visuals.Afterimage(legs); trail = .035f; }
            }
            var target = g.Nearest(transform.position);
            fire -= dt; side -= dt; missile -= dt;
            if (target != null)
            {
                Vector2 dir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                MotionMath.Turn(turret, dir, 1080, dt);
                Vector2 aim = turret.up;
                if (fire <= 0 && Vector2.Angle(aim, dir) < 16)
                {
                    fire = Mathf.Max(fire + FireInterval, FireInterval * .5f);
                    int spread = g.Ranks[(int)UpgradeKind.TripleShot] > 0 ? 1 : 0;
                    for (int i = -spread; i <= spread; i++) Fire(aim, i * (g.Ranks[(int)UpgradeKind.SpreadAmplifier] > 0 ? 18 : 11), Damage);
                    recoil = .075f;
                    SoundBank.Play(0);
                }
                if (g.Ranks[(int)UpgradeKind.SideCannons] > 0 && side <= 0) { side = 2; Fire(dir, -30, Damage * .8f); Fire(dir, 30, Damage * .8f); }
                if (g.Ranks[(int)UpgradeKind.MissilePod] > 0 && missile <= 0) { missile = 3; Projectile.Launch(transform.position, dir, 8, Damage * 2.2f, true, 0, true, target); }
            }
            if (g.Ranks[(int)UpgradeKind.NanoBots] > 0 && (nano -= dt) <= 0) { nano = 10; Heal(MaxHealth * .03f * g.Ranks[(int)UpgradeKind.NanoBots]); }
            if (g.Ranks[(int)UpgradeKind.Barrier] > 0 && (barrier -= dt) <= 0) { barrier = 20; Shield = 150 * g.Ranks[(int)UpgradeKind.Barrier]; }
        }
        Vector2 ReadMovement()
        {
            Vector2 v = Automated ? DebugMovement : Game.Instance.UI.Joystick.Value;
            if (!Automated) v += new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0), (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
            return Vector2.ClampMagnitude(v, 1);
        }
        void Fire(Vector2 dir, float angle, float damage)
        {
            Vector2 d = Quaternion.Euler(0, 0, angle) * dir;
            bool crit = Random.value < CritChance;
            Projectile.Launch((Vector2)transform.position + dir * .75f, d, Game.Instance.Profile.equipped[3] == 8 ? 28 : 19, damage * (crit ? 2 : 1), true, Pierce, false, null, crit);
        }
        public bool TryDash()
        {
            if (Game.Instance.State != RunState.Playing || DashRemaining > 0) return false;
            var input = ReadMovement(); if (input.sqrMagnitude > .01f) lastDirection = input.normalized;
            DashRemaining = DashCooldown; dashTime = Game.Instance.balance.dashDuration; dashElapsed = 0; dashLength = 3.3f * Speed / Game.Instance.balance.playerSpeed; dashDirection = lastDirection; trail = 0; Game.Instance.Dashes++; return true;
        }
        public void Hurt(float amount)
        {
            if (Invulnerable || Game.Instance.State != RunState.Playing) return;
            float absorbed = Mathf.Min(Shield, amount); Shield -= absorbed; amount -= absorbed;
            Health = Mathf.Max(0, Health - amount); hitGrace = .4f;
            Visuals.Number(transform.position, "-" + Mathf.RoundToInt(amount), Visuals.Red); SoundBank.Play(1);
            if (Health <= 0) Game.Instance.Finish(false);
        }
        public void Heal(float n) { Health = Mathf.Min(MaxHealth, Health + n); }
        public void Apply(UpgradeKind k)
        {
            switch (k)
            {
                case UpgradeKind.Piercing: Pierce++; break;
                case UpgradeKind.WeaponOverclock: FireInterval /= 1.2f; break;
                case UpgradeKind.HeavyArmor: float armor = MaxHealth * .2f; MaxHealth += armor; Heal(armor); break;
                case UpgradeKind.AttackSpeed: FireInterval /= 1.25f; break;
                case UpgradeKind.Damage: Damage *= 1.15f; break;
                case UpgradeKind.Thrusters: Speed *= 1.2f; break;
                case UpgradeKind.DashCooldown: DashCooldown *= .8f; break;
                case UpgradeKind.Crit: CritChance += .1f; break;
                case UpgradeKind.Plating: float extra = MaxHealth * .15f; MaxHealth += extra; Heal(extra); break;
                case UpgradeKind.Barrier: Shield = 150 * Game.Instance.Ranks[(int)k]; barrier = 20; break;
                case UpgradeKind.SideCannons:
                    Visuals.Shape("Upgrade Cannon L", turret, new Vector2(-.62f, .3f), new Vector2(.22f, .7f), Visuals.Blue, 4);
                    Visuals.Shape("Upgrade Cannon R", turret, new Vector2(.62f, .3f), new Vector2(.22f, .7f), Visuals.Blue, 4); break;
                case UpgradeKind.MissilePod: Visuals.Shape("Upgrade Missile Pod", turret, new Vector2(-.3f, -.25f), new Vector2(.3f, .45f), new Color(1, .75f, .3f), 5); break;
            }
        }
    }
}
