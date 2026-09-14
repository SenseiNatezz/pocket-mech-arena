using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PocketMech
{
    // Opt-in player harness, never active during normal play. No external test packages required.
    public sealed class PrototypeVerification : MonoBehaviour
    {
        readonly List<string> checks = new List<string>();
        string folder;
        bool failed;
        void Check(bool ok, string name) { checks.Add((ok ? "PASS " : "FAIL ") + name); if (!ok) failed = true; }
        void Error(string message, string stack, LogType kind) { if (kind == LogType.Exception || kind == LogType.Error) { failed = true; checks.Add("RUNTIME ERROR " + message); } }
        IEnumerator Start()
        {
            folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Verification-v08")); Directory.CreateDirectory(folder);
            Application.logMessageReceived += Error;
            yield return null;
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-pmaMotion") >= 0) { yield return MotionPreview(); yield break; }
            if (!Game.Instance.TestMode) { yield return Capture(); yield break; }
            Time.captureDeltaTime = .05f; Application.targetFrameRate = -1; Random.InitState(42);
            foreach (int fps in new[] { 30, 60, 120 })
            {
                Vector2 v = Vector2.zero; float distance = 0;
                for (int frame = 0; frame < fps; frame++) { v = MotionMath.Respond(v, Vector2.right * 5.2f, .045f, 1f / fps); distance += 3.3f * (MotionMath.DashProgress((frame + 1f) / fps) - MotionMath.DashProgress((float)frame / fps)); }
                Check(Mathf.Abs(v.x - 5.2f) < .001f && Mathf.Abs(distance - 3.3f) < .001f, $"Frame-rate-independent steering and dash displacement at {fps} FPS");
            }
            var g = Game.Instance; var p = g.Player;
            g.UI.StartButton.onClick.Invoke(); yield return null;
            Check(g.State == RunState.Briefing && g.UI.MissionButton != null, "Play opens mission selection");
            g.UI.MissionButton.onClick.Invoke(); yield return null;
            Check(g.UI.ConfirmButton != null, "Mission selection opens loadout confirmation");
            g.UI.ConfirmButton.onClick.Invoke(); yield return null;
            Check(g.State == RunState.Playing, "Deploy button starts mission");
            Check(g.Battlefield.Surface != null && g.Battlefield.Surface.vertexCount == 96, "Battlefield has an actual triangulated ground plane");
            Check(!GameObject.Find("Painted environment").GetComponent<SpriteRenderer>().enabled, "Old flat arena picture is hidden during gameplay");
            Check(Vector3.Dot(Camera.main.transform.forward, Vector3.forward) < .85f && Camera.main.orthographic, "Camera uses an oblique orthographic view");
            Check(g.Player.GetComponent<ActorPresentation>() != null, "Existing mech artwork uses angled-view presentation");
            Check(Camera.main.GetComponent<SpaceBackdrop>() != null, "Space backdrop is attached to the gameplay camera");
            Check(Camera.main.orthographicSize < 15, "Camera is closer to the battlefield in portrait view");
            Check(g.Clamp(new Vector2(7, 0)).x > 6.9f && g.Clamp(new Vector2(0, 11)).y > 10.9f, "Expanded arena bounds admit the newly added play area");
            var stick = g.UI.Joystick;
            Canvas.ForceUpdateCanvases();
            var point = RectTransformUtility.WorldToScreenPoint(null, stick.transform.TransformPoint(new Vector3(40, 0, 0)));
            var ev = new PointerEventData(EventSystem.current) { pointerId = 0, position = point };
            stick.OnPointerDown(ev); Check(stick.Value.x > .5f, "Touch joystick produces movement vector");
            Vector2 before = p.transform.position;
            for (int i = 0; i < 10; i++) yield return null;
            Check(p.transform.position.x > before.x, "Joystick moves mech");
            stick.OnPointerUp(ev); Check(stick.Value == Vector2.zero, "Touch release resets movement");
            before = p.transform.position; for (int frame = 0; frame < 6; frame++) yield return null;
            Check(p.MoveVelocity.magnitude < .01f && Vector2.Distance(before, p.transform.position) < .2f, "Releasing movement brakes promptly without sliding");
            p.transform.position = new Vector2(0, -3); p.Automated = true; p.DebugMovement = Vector2.right; before = p.transform.position;
            float hp = p.Health; g.UI.DashButton.onClick.Invoke(); p.Hurt(100);
            Check(p.IsDashing && p.Health == hp, "Dash button grants invulnerability"); Check(!p.TryDash(), "Dash cooldown prevents repeat activation");
            while (p.IsDashing) yield return null;
            Check(Mathf.Abs(Vector2.Distance(before, p.transform.position) - 3.3f) < .06f, "Actual dash travels the specified 3.3 units and samples current input");
            p.Automated = false; p.DebugMovement = Vector2.zero;
            Check(Mathf.Abs(p.Damage / p.FireInterval - 200) < .01f, "Faster firing preserves the 200 base damage-per-second budget");
            g.TogglePause(); float t = g.Elapsed; before = p.transform.position; yield return null; yield return null;
            Check(g.Elapsed == t && (Vector2)p.transform.position == before, "Pause freezes mission and movement"); g.TogglePause();
            g.GainXp(g.NeededXp);
            Check(g.State == RunState.Upgrade && g.UI.UpgradeButtons.Length == 3 && Time.timeScale == 0, "XP opens three upgrade cards and pauses");
            Check(g.Offered[0] == UpgradeKind.TripleShot, "First level offers Triple Shot");
            g.UI.UpgradeButtons[0].onClick.Invoke(); Check(g.Ranks[0] == 1 && g.State == RunState.Playing, "Upgrade selection resumes gameplay");
            float damage = p.Damage, speed = p.Speed, interval = p.FireInterval, max = p.MaxHealth, cooldown = p.DashCooldown;
            for (int i = 1; i < Upgrades.Names.Length; i++) g.ApplyUpgrade((UpgradeKind)i);
            Check(p.Pierce == 1 && p.Damage > damage && p.Speed > speed && p.FireInterval < interval && p.MaxHealth > max && p.DashCooldown < cooldown && p.Shield > 0 && p.CritChance > 0, "All stat upgrades apply");
            Check(p.transform.Find("Turret/Upgrade Cannon L") != null && p.transform.Find("Turret/Upgrade Missile Pod") != null, "Weapon upgrades attach visible equipment");
            Check(Projectile.DistanceToSegment(new Vector2(0, .2f), new Vector2(-10, 0), new Vector2(10, 0)) < .3f, "Fast projectile swept collision");
            var shield = g.Spawn(EnemyKind.Shield, new Vector2(8, 8)); float sh = shield.Health;
            shield.Hurt(100, Vector2.down, false); float front = sh - shield.Health; sh = shield.Health; shield.Hurt(100, Vector2.up, false);
            Check(front < sh - shield.Health, "Shield reduces frontal damage only");
            g.GainXp(100); int queued = 0; while (g.State == RunState.Upgrade && queued++ < 20) g.Choose(0);
            Check(queued > 1 && g.State == RunState.Playing, "Overflow XP produces sequential choices");
            // Exercise the complete real-time systems with an explicitly assisted pilot.
            // Hull restoration isolates progression correctness from human skill/balance.
            p.Automated = true; bool bossSeen = false;
            int frames = 0;
            while (g.State != RunState.Won && g.State != RunState.Lost && frames++ < 9000)
            {
                if (g.State == RunState.Upgrade) { g.UI.UpgradeButtons[0].onClick.Invoke(); yield return null; continue; }
                p.Heal(p.MaxHealth);
                Vector2 destination = new Vector2(Mathf.Sin(g.Elapsed * .14f) * 7, Mathf.Cos(g.Elapsed * .14f) * 11);
                if (g.Pickups.Count > 0 && g.Elapsed % 12 < 7) destination = g.Pickups[0].transform.position;
                p.DebugMovement = (destination - (Vector2)p.transform.position).normalized;
                if (g.Elapsed % 5 < .1f) p.TryDash();
                if (g.BossSpawned) bossSeen = true;
                if (g.Elapsed >= 298 && g.Boss != null && g.Boss.Alive) g.Boss.Hurt(999999, Vector2.up, false);
                yield return null;
            }
            Check(g.EliteSpawned && g.Phase == 8, "Assault Striker and all mission phases trigger");
            Check(bossSeen && g.BossDefeated, "Boss spawns near end and can be defeated");
            Check(g.State == RunState.Won && g.Elapsed == 300 && g.Credits == 1250 && g.Parts == 4 && g.WeaponXp == 12, "Five-minute assisted run reaches Mission Complete with rewards");
            Check(g.ShotsFired > 100 && g.EnemyShotsFired > 0 && g.DamageEvents > 100 && g.Kills > 20, "Sustained combat generates player/enemy shots, hits and kills");
            checks.Add($"METRICS shots={g.ShotsFired}, enemyShots={g.EnemyShotsFired}, hits={g.DamageEvents}, kills={g.Kills}, level={g.Level}, frames={frames}");
            int claimed = g.Profile.credits; int wins = g.Profile.wins;
            g.Finish(true); Check(g.Profile.credits == claimed && g.Profile.wins == wins, "Rewards cannot be claimed twice");
            g.UI.ContinueButton.onClick.Invoke(); yield return null;
            Check(g.State == RunState.Briefing && g.UI.StartButton != null, "Reward Continue opens Back to Base replay flow");
            g.StartRun(); yield return null;
            Check(g.State == RunState.Playing && g.Level == 1 && g.Kills == 0 && g.Ranks[0] == 0 && p.Health == 1000, "Restart clears progression and restores hull");
            p.Hurt(999999); Check(g.State == RunState.Lost, "Hull depletion opens failure screen");
            g.StartRun(); yield return null; g.Finish(false); Check(g.State == RunState.Lost && g.Parts == 0, "Failed extraction grants no victory parts");
            g.GoHome(); yield return null; Check(g.State == RunState.Briefing && Time.timeScale == 1, "Continue returns to home without a frozen time scale");
            g.UI.ShowGarage(); yield return null; g.UI.ShowState(); Check(g.UI.StartButton != null, "Garage inspection returns to deploy screen");
            var profile = g.Profile;
            Check(!profile.Equip(8), "Locked equipment cannot be equipped");
            profile.credits = 0; profile.parts = 0;
            Check(!profile.Buy(8) && !profile.Tune(6), "Insufficient funds block purchases and tuning");
            profile.credits = 5000; profile.parts = 20;
            Check(profile.Buy(8) && profile.Equip(8) && profile.Tune(8) && profile.credits == 3200 && profile.parts == 15, "Buy, equip and tune deduct exact costs");
            g.Player.Init(g.balance);
            Check(g.Player.Pierce == 2 && Mathf.Abs(g.Player.FireInterval - .6f) < .001f && g.Player.Damage > 126, "Railgun and tuning modify actual starting weapon stats");
            profile.owned[10] = false; string bonus = profile.GrantBonus(.1f); int partsBefore = profile.parts;
            Check(profile.owned[10] && bonus.Contains("unlocked"), "Bonus booster drop unlocks equipment");
            profile.GrantBonus(.1f); Check(profile.parts == partsBefore + 2, "Duplicate loot converts to two parts");
            var restored = JsonUtility.FromJson<PilotProfile>(JsonUtility.ToJson(profile)); restored.Validate();
            Check(restored.equipped[3] == 8 && restored.tuning[8] == 1 && restored.credits == profile.credits, "Equipment, tuning and currency survive save serialization");
            profile.mission = 1; g.StartRun(); var veteran = g.Spawn(EnemyKind.Scout, new Vector2(0,8));
            Check(Mathf.Abs(veteran.MaxHealth - 234) < .01f, "Veteran selection increases enemy hull by 30 percent");
            g.Finish(true); Check(g.Credits == 1750, "Veteran mission awards increased credits");
            g.GoHome(); g.UI.SelectArea(1); yield return null;
            Check(profile.area == 1 && profile.mission == 1, "Area selection preserves Veteran difficulty");
            profile.mission = 0; g.UI.MissionButton.onClick.Invoke(); yield return null; g.UI.ConfirmButton.onClick.Invoke(); yield return null;
            Check(g.Enemies.TrueForAll(e => e.Kind == EnemyKind.IceSkimmer), "Frostline deploy uses its own opening roster");
            Check(GameObject.Find("Painted environment").GetComponent<SpriteRenderer>().sprite.name == "FrostArena", "Frostline deploy switches the actual arena texture");
            // Isolate attack execution from the equipped railgun killing the test machines first.
            p.enabled = false;
            var rail = g.Spawn(EnemyKind.RailSentinel, new Vector2(-3, 6));
            var mortar = g.Spawn(EnemyKind.CryoMortar, new Vector2(3, 6));
            p.Automated = true; p.DebugMovement = Vector2.zero;
            for (int i = 0; i < 120; i++) { p.Heal(p.MaxHealth); if (g.State == RunState.Upgrade) g.Choose(0); yield return null; }
            Check(rail != null && rail.SpecialAttacks > 0 && mortar != null && mortar.SpecialAttacks > 0, "Rail lock-on fires twin bolts and Cryo Mortar executes paired blasts");
            p.enabled = true;
            // A second complete assisted run checks the distinct roster, boss and extraction.
            bool frostBossSeen = false; bool frostBossAttacked = false; frames = 0;
            while (g.State != RunState.Won && g.State != RunState.Lost && frames++ < 9000) {
                if (g.State == RunState.Upgrade) { g.Choose(0); yield return null; continue; }
                p.Heal(p.MaxHealth); p.DebugMovement = new Vector2(Mathf.Sin(g.Elapsed), Mathf.Cos(g.Elapsed));
                if (g.Boss != null) { frostBossSeen |= g.Boss.Kind == EnemyKind.GlacierColossus; frostBossAttacked |= g.Boss.SpecialAttacks >= 3; }
                if (g.Elapsed >= 298 && g.Boss != null && g.Boss.Alive) g.Boss.Hurt(999999, Vector2.up, false);
                yield return null;
            }
            Check(frostBossSeen && frostBossAttacked && g.BossDefeated, "Glacier Colossus spawns, cycles its three attacks and registers boss defeat");
            Check(g.State == RunState.Won && g.Elapsed == 300 && g.Credits == 1500, "Frostline five-minute extraction awards its mission reward");
            restored = JsonUtility.FromJson<PilotProfile>(JsonUtility.ToJson(profile)); restored.Validate();
            Check(restored.area == 1 && restored.equipped[3] == 8, "New area and existing equipment survive serialization together");
            var legacy = JsonUtility.FromJson<PilotProfile>("{\"credits\":321,\"mission\":1}"); legacy.Validate();
            Check(legacy.area == 0 && legacy.mission == 1 && legacy.credits == 321, "Legacy profiles keep credits and difficulty and default to Arena A-1");
            profile.mission = 1; g.StartRun(); g.Finish(true); Check(g.Credits == 2000, "Frostline Veteran awards 2000 credits");
            profile.area = 0; g.StartRun(); yield return null;
            Check(GameObject.Find("Painted environment").GetComponent<SpriteRenderer>().sprite == Resources.Load<Sprite>("Illustrated/Arena") && g.Enemies.TrueForAll(e => e.Kind == EnemyKind.Scout), "Switching back restores original environment and roster");
            Check(profile.volatileOnly, "Verification profile cannot overwrite real player saves");
            g.Profile.mission = 0; g.StartRun(); p.enabled = false;
            foreach (var e in g.Enemies.ToArray()) Destroy(e.gameObject); g.Enemies.Clear();
            p.transform.position = new Vector3(0, -3, 0);
            var blastNear = g.Spawn(EnemyKind.Shield, new Vector2(0, 0)); blastNear.enabled = false; blastNear.transform.up = Vector2.up;
            var blastFar = g.Spawn(EnemyKind.Shield, new Vector2(0, 3)); blastFar.enabled = false; blastFar.transform.up = Vector2.up;
            var blastMiss = g.Spawn(EnemyKind.Shield, new Vector2(4, 0)); blastMiss.enabled = false;
            float nearHull = blastNear.Health, farHull = blastFar.Health, missHull = blastMiss.Health;
            g.UI.BlastButton.onClick.Invoke();
            Check(p.Blast.Charging && !p.Blast.TryActivate(), "Blast button starts charge and rejects repeated activation");
            g.TogglePause(); float blastCooldown = p.Blast.Remaining;
            for (int i = 0; i < 12; i++) yield return null;
            Check(p.Blast.Shots == 0 && p.Blast.Remaining == blastCooldown, "Blast charge and cooldown freeze during pause");
            g.TogglePause();
            for (int i = 0; i < 10; i++) yield return null;
            Check(p.Blast.Shots == 1 && (blastNear == null || blastNear.Health < nearHull) && (blastFar == null || blastFar.Health < farHull), "Charged blast pierces two aligned enemies exactly once");
            Check(blastMiss.Health == missHull, "Blast leaves enemies outside its beam unharmed");
            Check(!MagnumBlast.Intersects(new Vector2(0, 14), Vector2.up, .5f) && !MagnumBlast.Intersects(new Vector2(0, -3), Vector2.up, .5f), "Blast respects maximum range and does not fire backwards");
            g.StartRun(); p.enabled = true;
            Check(p.Blast.Remaining == 0 && !p.Blast.Charging && p.Blast.Shots == 0, "Restart resets blast charge and cooldown");
            checks.Add("NOTE Full-run test restores hull and forces any surviving boss defeat at 04:58; this is a progression test, not difficulty validation.");
            File.WriteAllLines(Path.Combine(folder, "smoke-results.txt"), checks); Debug.Log("PMA SMOKE " + (failed ? "FAILED" : "PASSED")); Application.Quit(failed ? 1 : 0);
        }
        IEnumerator Capture()
        {
            var g = Game.Instance;
            yield return new WaitForSecondsRealtime(1);
            CaptureFrame("01-briefing.png"); yield return new WaitForSecondsRealtime(1);
            g.UI.Missions(); yield return null; CaptureFrame("06-mission-select.png");
            g.UI.ShowLoadout(); yield return null; CaptureFrame("07-loadout.png");
            g.UI.ShowGarage(); yield return null; CaptureFrame("04-garage.png"); yield return new WaitForSecondsRealtime(.2f);
            g.StageReviewEncounter(); g.Player.Automated = true;
            for (int i = 0; i < 7; i++) g.Spawn((EnemyKind)(i % 5), new Vector2(Mathf.Sin(i * 2.4f) * 3.4f, Mathf.Cos(i * 2.4f) * 3));
            yield return new WaitForSecondsRealtime(.8f);
            for (int i = -1; i <= 1; i++) Projectile.Launch((Vector2)g.Player.transform.position + new Vector2(i * .5f, 1.4f), Quaternion.Euler(0, 0, i * 13) * Vector2.up, 19, 100, true);
            CaptureFrame("02-combat.png"); yield return new WaitForSecondsRealtime(1);
            g.GainXp(g.NeededXp); yield return null;
            CaptureFrame("03-upgrades.png"); yield return new WaitForSecondsRealtime(1);
            g.Finish(true); yield return null; CaptureFrame("05-rewards.png"); yield return new WaitForSecondsRealtime(.3f);
            g.UI.ShowBase(); yield return null; CaptureFrame("08-back-to-base.png");
            g.UI.SelectArea(1); yield return null; CaptureFrame("09-frostline-select.png");
            g.StageReviewEncounter(); g.Player.Automated = true; g.ApplyUpgrade(UpgradeKind.TripleShot);
            for (int i = 0; i < 6; i++) g.Spawn(i % 3 == 0 ? EnemyKind.IceSkimmer : i % 3 == 1 ? EnemyKind.RailSentinel : EnemyKind.CryoMortar, new Vector2((i % 3 - 1) * 3, i / 3 * 3));
            yield return new WaitForSecondsRealtime(1.1f); CaptureFrame("10-frostline-combat.png");
            g.StageReviewEncounter(); g.Player.Automated = true; g.Player.enabled = false;
            g.Player.Blast.TryActivate(); yield return new WaitForSeconds(.43f); CaptureFrame("11-magnum-blast.png");
            Application.Quit();
        }
        void CaptureFrame(string name)
        {
            // Render the real camera and UI offscreen so review works with a hidden window.
            var camera = Camera.main; var canvas = FindAnyObjectByType<Canvas>();
            var rt = new RenderTexture(540, 960, 24); rt.Create();
            camera.targetTexture = rt; camera.aspect = 540f / 960;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingOrder = 100;
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = rt;
            var image = new Texture2D(540, 960, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(folder, name), image.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = null; canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Destroy(image); rt.Release(); Destroy(rt);
        }
        IEnumerator MotionPreview()
        {
            folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../work/fluid-frames")); Directory.CreateDirectory(folder);
            Time.captureDeltaTime = 1f / 24; Application.targetFrameRate = -1;
            var g = Game.Instance; g.StageReviewEncounter(); g.Player.Automated = true; g.ApplyUpgrade(UpgradeKind.TripleShot);
            for (int i = 0; i < 6; i++) g.Spawn(i % 2 == 0 ? EnemyKind.Shooter : EnemyKind.Scout, new Vector2((i % 3 - 1) * 3, i / 3 * 3));
            for (int frame = 0; frame < 144; frame++)
            {
                if (g.State == RunState.Upgrade) g.Choose(0);
                g.Player.Heal(g.Player.MaxHealth);
                g.Player.DebugMovement = frame < 35 ? Vector2.right : frame < 75 ? Vector2.left : frame < 105 ? Vector2.up : Vector2.down;
                if (frame == 36) g.Player.TryDash();
                yield return null; CaptureFrame($"frame-{frame:0000}.png");
            }
            Application.Quit();
        }
        void OnDestroy() { Application.logMessageReceived -= Error; Time.captureDeltaTime = 0; }
    }
}
