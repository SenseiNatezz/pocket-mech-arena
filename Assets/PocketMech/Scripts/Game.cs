using System.Collections.Generic;
using UnityEngine;

namespace PocketMech
{
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }
        public Balance balance;
        public RunState State { get; private set; } = RunState.Briefing;
        public PlayerMech Player { get; private set; }
        public ArenaUI UI { get; private set; }
        public Transform World { get; private set; }
        public RaisedBattlefield Battlefield { get; private set; }
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<XpPickup> Pickups = new List<XpPickup>();
        public float Elapsed { get; private set; }
        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int NeededXp => 12 + (Level - 1) * 8;
        public int Kills { get; private set; }
        public int Credits { get; private set; }
        public int Parts { get; private set; }
        public int WeaponXp { get; private set; }
        public bool BossSpawned { get; private set; }
        public bool BossDefeated { get; private set; }
        public Enemy Boss { get; private set; }
        public bool Headless { get; private set; }
        public bool TestMode { get; private set; }
        public bool VerificationMode { get; private set; }
        public readonly int[] Ranks = new int[Upgrades.Names.Length];
        public PilotProfile Profile { get; private set; }
        public MissionDefinition Mission => Missions.Get(Profile.area);
        public bool Frostline => Profile.area == 1;
        public string BonusDrop { get; private set; } = "";
        public bool EliteSpawned { get; private set; }
        public int Phase { get; private set; }
        float nextHazard;
        public UpgradeKind[] Offered { get; private set; }
        float spawnTimer;
        bool warning;
        RunState resumeState;
        public int ShotsFired, EnemyShotsFired, DamageEvents, Dashes, LevelChoices;
        void Awake()
        {
            Instance = this;
#if UNITY_WEBGL && !UNITY_EDITOR
            var args = new string[0];
#else
            var args = System.Environment.GetCommandLineArgs();
#endif
            TestMode = System.Array.IndexOf(args, "-pmaSmoke") >= 0;
            VerificationMode = TestMode || System.Array.IndexOf(args, "-pmaCapture") >= 0 || System.Array.IndexOf(args, "-pmaMotion") >= 0;
            Headless = System.Array.IndexOf(args, "-nographics") >= 0;
            Application.targetFrameRate = 60;
#if !UNITY_WEBGL || UNITY_EDITOR
            Screen.orientation = ScreenOrientation.Portrait;
#endif
            if (balance == null) balance = Resources.Load<Balance>("RunBalance");
            Profile = PilotProfile.Load(VerificationMode);
            Visuals.Initialize();
            World = new GameObject("Runtime Entities").transform;
            Battlefield = new GameObject("Raised battlefield").AddComponent<RaisedBattlefield>();
            var prefab = Resources.Load<GameObject>("Prefabs/PlayerMech");
            Player = Instantiate(prefab, World).GetComponent<PlayerMech>(); Player.Init(balance);
            Player.gameObject.AddComponent<ActorPresentation>();
            UI = gameObject.AddComponent<ArenaUI>(); UI.Build();
            gameObject.AddComponent<SoundBank>();
            if (VerificationMode) gameObject.AddComponent<PrototypeVerification>();
        }
        void ApplyEnvironment()
        {
            var environment = GameObject.Find("Painted environment");
            if (environment == null) return;
            var renderer = environment.GetComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>("Illustrated/" + Mission.Background);
            renderer.transform.localScale = new Vector3(13f / renderer.sprite.bounds.size.x, 23.12f / renderer.sprite.bounds.size.y, 1);
            renderer.enabled = false;
            Battlefield.Rebuild(Frostline);
        }
        public void StartRun()
        {
            Time.timeScale = 1;
            ApplyEnvironment();
            foreach (Transform t in World) if (t != Player.transform) Destroy(t.gameObject);
            Enemies.Clear(); Pickups.Clear(); Projectile.ResetPool();
            System.Array.Clear(Ranks, 0, Ranks.Length);
            Elapsed = 0; Level = 1; Xp = Kills = Credits = Parts = WeaponXp = 0;
            EliteSpawned = false; Phase = 0; nextHazard = Frostline ? 90 : 105; BonusDrop = "";
            BossSpawned = BossDefeated = warning = false; Boss = null; spawnTimer = 2;
            ShotsFired = EnemyShotsFired = DamageEvents = Dashes = LevelChoices = 0;
            Player.Init(balance); State = RunState.Playing; UI.ShowState();
            UI.Announce(Mission.Name, "Move with the stick. Weapons fire automatically.", 5);
            for (int i = 0; i < 3; i++) Spawn(Mission.Starter);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
            if (State != RunState.Playing) return;
            Elapsed = Mathf.Min(balance.runSeconds, Elapsed + Time.deltaTime);
            if (!warning && Elapsed >= 225) { warning = true; UI.Announce("WARNING", "HEAVY ENEMY APPROACHING  -  prepare for 04:20.", 7); }
            if (!BossSpawned && Elapsed >= balance.bossArrival) { BossSpawned = true; Boss = Spawn(Mission.Boss, new Vector2(0, 6.5f)); UI.Announce(Mission.BossName, "Break its core. Keep moving.", 4); }
            int phase = Elapsed < 5 ? 0 : Elapsed < 11 ? 1 : Elapsed < 20 ? 2 : Elapsed < 45 ? 3 : Elapsed < 90 ? 4 : Elapsed < 135 ? 5 : Elapsed < 180 ? 6 : Elapsed < 225 ? 7 : 8;
            if (phase != Phase)
            {
                Phase = phase;
                string[] titles = { "", "AUTO-FIRE ONLINE", "DASH TO EVADE", "FIRST WAVE", "CROSS FIRE", "SHIELD BOTS", "ESCALATION", "POWER PHASE", "" };
                string[] hints = { "", "Keep moving while your rifle tracks the nearest enemy.", "Tap the right button. Brief invulnerability, then recharge.", "Scouts and Rush drones. Watch the red charge warnings.", "Shooter drones fire red bolts. Move across their aim.", "Flank shields. Leave marked hazard zones before they detonate.", "Bombers and an elite Assault Striker are incoming.", "Larger waves. Combine your weapons and keep collecting XP.", "" };
                if (Frostline) {
                    titles[3] = "ICE SKIMMERS"; hints[3] = "Weaving drones close in. Keep space around your mech.";
                    titles[4] = "RAIL SENTINELS"; hints[4] = "They stop to lock their aim. Sidestep the twin red bolts.";
                    titles[5] = "REACTOR VENTS"; hints[5] = "Paired danger zones erupt. Dash out of the red circles.";
                    titles[6] = "CRYO MORTARS"; hints[6] = "Mortars bracket your position with paired blasts.";
                }
                if (phase < 8) UI.Announce(titles[phase], hints[phase], 5);
            }
            if (!EliteSpawned && Elapsed >= 160) {
                EliteSpawned = true;
                if (Frostline) { Spawn(EnemyKind.RailSentinel); Spawn(EnemyKind.RailSentinel); UI.Announce("SENTINEL REINFORCEMENTS", "Two rail platforms are entering the yard.", 5); }
                else { Spawn(EnemyKind.Elite); UI.Announce("ELITE ASSAULT STRIKER", "Evade its charge and three-shot bursts.", 5); }
            }
            if (Elapsed >= nextHazard && Elapsed < 225) { nextHazard += 16;
                if (Frostline) { Vector2 at = Player.transform.position; Hazard.Warn(Clamp(at + Vector2.left * 1.6f), 1.2f, 1.6f, 90); Hazard.Warn(Clamp(at + Vector2.right * 1.6f), 1.2f, 1.6f, 90); }
                else Hazard.Warn(Player.transform.position, 1.3f, 1.5f, 90); }
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0 && Elapsed < balance.runSeconds)
            {
                int count = Elapsed < 20 ? 1 : Elapsed < 90 ? 2 : Elapsed < 180 ? 3 : Elapsed < 225 ? 4 : 1;
                for (int i = 0; i < count; i++) if (Enemies.Count < balance.maxEnemies) Spawn(PickEnemy());
                spawnTimer = Elapsed < 20 ? 4 : Elapsed < 90 ? 3.4f : Elapsed < 180 ? 2.5f : Elapsed < 225 ? 2.1f : 3.5f;
            }
            if (Elapsed >= balance.runSeconds) Finish(BossDefeated);
        }
        EnemyKind PickEnemy()
        {
            int roll = Random.Range(0, 100);
            if (Frostline) {
                if (Elapsed >= 100 && roll < 24) return EnemyKind.CryoMortar;
                if (Elapsed >= 35 && roll < 55) return EnemyKind.RailSentinel;
                return EnemyKind.IceSkimmer;
            }
            if (Elapsed >= 135 && roll < 16) return EnemyKind.Bomber;
            if (Elapsed >= 90 && roll < 32) return EnemyKind.Shield;
            if (Elapsed >= 45 && roll < 54) return EnemyKind.Shooter;
            if (Elapsed >= 20 && roll < 72) return EnemyKind.Rush;
            return EnemyKind.Scout;
        }
        public Enemy Spawn(EnemyKind kind, Vector2? location = null)
        {
            Vector2 p;
            if (location.HasValue) p = location.Value;
            else
            {
                // Spawn beyond the camera, with a guaranteed safety distance from the pilot.
                p = Vector2.zero;
                for (int i = 0; i < 12; i++)
                {
                    p = Clamp((Vector2)Player.transform.position + Random.insideUnitCircle.normalized * 14, 1);
                    if (Vector2.Distance(p, Player.transform.position) > 7) break;
                }
            }
            var prefab = Resources.Load<GameObject>("Prefabs/" + kind);
            var e = Instantiate(prefab, p, Quaternion.identity, World).GetComponent<Enemy>();
            e.Init(kind, Elapsed); e.gameObject.AddComponent<ActorPresentation>(); Enemies.Add(e); return e;
        }
        public Enemy Nearest(Vector2 p, float range = 15)
        {
            Enemy best = null; float d = range * range;
            foreach (var e in Enemies) if (e != null && e.Alive) { float n = ((Vector2)e.transform.position - p).sqrMagnitude; if (n < d) { best = e; d = n; } }
            return best;
        }
        public Vector2 Clamp(Vector2 p, float margin = .6f)
        {
            p = new Vector2(Mathf.Clamp(p.x, -balance.arenaHalfSize.x + margin, balance.arenaHalfSize.x - margin), Mathf.Clamp(p.y, -balance.arenaHalfSize.y + margin, balance.arenaHalfSize.y - margin));
            float edge = Mathf.Abs(p.x) / Mathf.Max(1, 9.4f-margin) + Mathf.Abs(p.y) / Mathf.Max(1,12.1f-margin);
            return edge > 1 ? p / edge : p;
        }
        public void EnemyKilled(Enemy e)
        {
            Kills++; Enemies.Remove(e);
            if (e.IsBoss) { BossDefeated = true; UI.Announce(Mission.BossName + " DOWN", "Hold until extraction at 05:00.", 5); SoundBank.Play(3); }
            XpPickup.Drop(e.transform.position, e.IsBoss ? 30 : e.Kind == EnemyKind.Elite ? 15 : e.Kind == EnemyKind.Shield ? 3 : 2);
        }
        public void GainXp(int n) { Xp += n; CheckLevel(); }
        void CheckLevel()
        {
            if (State != RunState.Playing || Xp < NeededXp) return;
            Xp -= NeededXp; Level++;
            var choices = new List<UpgradeKind>();
            for (int i = 0; i < Ranks.Length; i++) if (Ranks[i] < Upgrades.MaxRank((UpgradeKind)i) && (i != (int)UpgradeKind.SpreadAmplifier || Ranks[0] > 0)) choices.Add((UpgradeKind)i);
            if (choices.Count == 0) { Player.Heal(100); return; }
            Offered = new UpgradeKind[Mathf.Min(3, choices.Count)];
            for (int i = 0; i < Offered.Length; i++)
            {
                UpgradeKind[][] storyboard = {
                    new[] { UpgradeKind.TripleShot, UpgradeKind.AttackSpeed, UpgradeKind.Plating },
                    new[] { UpgradeKind.Piercing, UpgradeKind.Thrusters, UpgradeKind.Damage },
                    new[] { UpgradeKind.SideCannons, UpgradeKind.DashCooldown, UpgradeKind.NanoBots },
                    new[] { UpgradeKind.SpreadAmplifier, UpgradeKind.Crit, UpgradeKind.Barrier },
                    new[] { UpgradeKind.MissilePod, UpgradeKind.WeaponOverclock, UpgradeKind.HeavyArmor }
                };
                var preferred = Level <= 6 ? storyboard[Level - 2][i] : (UpgradeKind)(-1);
                int pick = choices.Contains(preferred) ? choices.IndexOf(preferred) : Random.Range(0, choices.Count);
                Offered[i] = choices[pick]; choices.RemoveAt(pick);
            }
            State = RunState.Upgrade; Time.timeScale = 0; UI.Joystick.ResetStick(); UI.ShowState(); SoundBank.Play(2);
        }
        public void Choose(int index)
        {
            if (State != RunState.Upgrade || index < 0 || index >= Offered.Length) return;
            ApplyUpgrade(Offered[index]); LevelChoices++; State = RunState.Playing; Time.timeScale = 1; UI.ShowState(); CheckLevel();
        }
        public void ApplyUpgrade(UpgradeKind kind)
        {
            if (Ranks[(int)kind] >= Upgrades.MaxRank(kind)) return;
            Ranks[(int)kind]++; Player.Apply(kind);
        }
        public void TogglePause()
        {
            if (State == RunState.Playing) { resumeState = State; State = RunState.Paused; Time.timeScale = 0; UI.Joystick.ResetStick(); UI.ShowState(); }
            else if (State == RunState.Paused) { State = resumeState; Time.timeScale = 1; UI.ShowState(); }
        }
        void OnApplicationPause(bool paused) { if (paused && State == RunState.Playing) TogglePause(); }
        void OnApplicationFocus(bool focus) { if (!focus && !VerificationMode && State == RunState.Playing) TogglePause(); }
        public void GoHome() { State = RunState.Briefing; Time.timeScale = 1; UI.Joystick.ResetStick(); UI.ShowState(); }
        public void StageReviewEncounter()
        {
            if (!VerificationMode) return;
            StartRun(); Elapsed = 260; warning = true; EliteSpawned = true; Phase = 8; nextHazard = 300; BossSpawned = true; Player.transform.position = new Vector2(0, -5.5f);
            Boss = Spawn(Mission.Boss, new Vector2(0, 5));
            UI.Announce("", "", 0);
        }
        public void Finish(bool won)
        {
            if (State != RunState.Playing && State != RunState.Upgrade && State != RunState.Paused) return;
            State = won ? RunState.Won : RunState.Lost; Time.timeScale = 0; UI.Joystick.ResetStick();
            Credits = won ? Mission.Reward(Profile.mission) : Kills * 5; Parts = won ? 4 : 0; WeaponXp = won ? 12 : Mathf.FloorToInt(Elapsed / 60);
            Profile.credits += Credits; Profile.parts += Parts; Profile.weaponXp += WeaponXp;
            if (won) { Profile.wins++; BonusDrop = Profile.GrantBonus(Random.value); }
            Profile.Save();
            UI.ShowState(); SoundBank.Play(won ? 3 : 1);
        }
        void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }
    }
}
