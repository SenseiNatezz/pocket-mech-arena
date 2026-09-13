using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PocketMech
{
    public sealed class ArenaUI : MonoBehaviour
    {
        public VirtualJoystick Joystick { get; private set; }
        public Button DashButton { get; private set; }
        public Button StartButton { get; private set; }
        public Button MissionButton { get; private set; }
        public Button ConfirmButton { get; private set; }
        public Button ContinueButton { get; private set; }
        public Button[] UpgradeButtons { get; private set; }
        RectTransform safe, overlay, hud;
        Text timer, health, level, dash, wave, banner, bannerSub, bossName;
        Image hpFill, xpFill, bossFill;
        GameObject bossPanel;
        float announceUntil;
        Font font;
        static Sprite plate;
        readonly Color steel = new Color(.23f, .33f, .43f), light = new Color(.76f, .86f, .95f), gold = new Color(1f, .72f, .19f);
        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = new GameObject("Mobile Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; canvas.GetComponent<Canvas>().sortingOrder = 100;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(540, 960); scaler.matchWidthOrHeight = .5f;
            new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            safe = Panel("Safe Area", canvas.transform, Color.clear, false); Stretch(safe); safe.gameObject.AddComponent<SafeArea>();
            hud = Panel("Battle HUD", safe, Color.clear, false); Stretch(hud);
            var hull = Panel("Hull plate", hud, new Color(.1f, .15f, .2f)); Place(hull, new Vector2(.5f, 1), new Vector2(-115, -43), new Vector2(270, 64));
            Label(hull, "HP", 20, new Vector2(-108, 13), new Vector2(40, 25), Color.white, true);
            hpFill = Bar(hull, new Vector2(14, 13), new Vector2(204, 19), new Color(.4f, .95f, .23f));
            health = Label(hull, "1000 / 1000", 16, new Vector2(0, -15), new Vector2(250, 25), Color.white, true);
            var wavePlate = Panel("Wave plate", hud, new Color(.1f, .15f, .2f)); Place(wavePlate, new Vector2(.5f, 1), new Vector2(102, -43), new Vector2(145, 64));
            wave = Label(wavePlate, "WAVE 1 / 5\nEnemies: 3", 16, Vector2.zero, new Vector2(135, 58), Color.white, true);
            ButtonAt(hud, "II", new Vector2(.5f, 1), new Vector2(219, -43), new Vector2(58, 64), () => Game.Instance.TogglePause());
            xpFill = Bar(hud, Vector2.zero, new Vector2(476, 6), Visuals.Blue); Place((RectTransform)xpFill.transform.parent, new Vector2(.5f, 1), new Vector2(0, -90), new Vector2(476, 6));
            level = Label(hud, "LV 01", 13, Vector2.zero, new Vector2(80, 25), Color.white, true); Place(level.rectTransform, new Vector2(.5f, 1), new Vector2(-207, -109), new Vector2(80, 25));
            timer = Label(hud, "00:00 / 05:00", 14, Vector2.zero, new Vector2(155, 25), Color.white, true); Place(timer.rectTransform, new Vector2(.5f, 1), new Vector2(170, -109), new Vector2(155, 25));
            bossPanel = Panel("Boss", hud, new Color(.13f, .1f, .13f)).gameObject; Place((RectTransform)bossPanel.transform, new Vector2(.5f, 1), new Vector2(0, -152), new Vector2(455, 49));
            bossName = Label(bossPanel.transform, "HEAVY SIEGE WALKER", 16, new Vector2(0, 8), new Vector2(440, 23), Color.white, true);
            bossFill = Bar(bossPanel.transform, new Vector2(0, -13), new Vector2(414, 7), Visuals.Red);
            var joy = Panel("Movement Stick", hud, new Color(.29f, .39f, .5f, .6f), false); joy.GetComponent<Image>().sprite = Visuals.Disc;
            Place(joy, new Vector2(0, 0), new Vector2(109, 112), new Vector2(162, 162)); Border(joy, light, 2);
            Label(joy, "<                 >", 26, Vector2.zero, new Vector2(155, 40), light);
            Label(joy, "^", 26, new Vector2(0, 54), new Vector2(40, 30), light);
            Label(joy, "v", 22, new Vector2(0, -55), new Vector2(40, 30), light);
            var knob = Panel("Thumb", joy, new Color(.5f, .71f, .91f), false); knob.GetComponent<Image>().sprite = Visuals.Disc; Place(knob, new Vector2(.5f, .5f), Vector2.zero, new Vector2(66, 66)); Border(knob, light, 2);
            Joystick = joy.gameObject.AddComponent<VirtualJoystick>(); Joystick.Knob = knob;
            DashButton = ButtonAt(hud, ">>", new Vector2(1, 0), new Vector2(-93, 105), new Vector2(122, 122), () => Game.Instance.Player.TryDash());
            DashButton.GetComponent<Image>().sprite = Visuals.Disc; DashButton.GetComponent<Image>().type = Image.Type.Simple; DashButton.GetComponent<Image>().color = new Color(.03f, .27f, .65f); Border(DashButton.transform, Visuals.Blue, 3);
            dash = DashButton.GetComponentInChildren<Text>(); dash.fontSize = 45; dash.color = Visuals.Blue;
            var hint = Label(hud, "DASH", 12, Vector2.zero, new Vector2(100, 20), Color.white, true); Place(hint.rectTransform, new Vector2(1, 0), new Vector2(-93, 30), new Vector2(100, 20));
            banner = Label(hud, "", 26, Vector2.zero, new Vector2(490, 40), gold, true); Place(banner.rectTransform, new Vector2(.5f, 1), new Vector2(0, -212), new Vector2(500, 40));
            bannerSub = Label(hud, "", 14, Vector2.zero, new Vector2(490, 48), Color.white); Place(bannerSub.rectTransform, new Vector2(.5f, 1), new Vector2(0, -248), new Vector2(460, 48));
            overlay = Panel("Menu Layer", safe, Color.clear, false); Stretch(overlay); ShowState();
        }
        public void Announce(string title, string subtitle, float seconds) { banner.text = title; bannerSub.text = subtitle; announceUntil = Time.time + seconds; }
        void Update()
        {
            var g = Game.Instance; if (g.Player == null) return;
            timer.text = $"{(int)g.Elapsed / 60:00}:{(int)g.Elapsed % 60:00} / 05:00";
            health.text = $"{Mathf.CeilToInt(g.Player.Health)} / {Mathf.CeilToInt(g.Player.MaxHealth)}" + (g.Player.Shield > 0 ? $"  +{(int)g.Player.Shield}" : "");
            hpFill.fillAmount = g.Player.Health / g.Player.MaxHealth; xpFill.fillAmount = (float)g.Xp / g.NeededXp; level.text = $"LV {g.Level:00}";
            wave.text = $"WAVE {Mathf.Min(5, (int)(g.Elapsed / 60) + 1)} / 5\nEnemies: {g.Enemies.Count}";
            dash.text = g.Player.DashRemaining > 0 ? g.Player.DashRemaining.ToString("0.0") : ">>";
            dash.fontSize = g.Player.DashRemaining > 0 ? 30 : 45; DashButton.interactable = g.Player.DashRemaining <= 0 && g.State == RunState.Playing;
            bossName.text = g.Mission.BossName;
            bossPanel.SetActive(g.Boss != null && g.Boss.Alive); if (g.Boss != null) bossFill.fillAmount = g.Boss.Health / g.Boss.MaxHealth;
            if (Time.time > announceUntil) { banner.text = ""; bannerSub.text = ""; }
        }
        void ClearMenu()
        {
            foreach (Transform t in overlay) { t.gameObject.SetActive(false); Destroy(t.gameObject); }
            UpgradeButtons = null; overlay.GetComponent<Image>().color = Color.clear; overlay.GetComponent<Image>().raycastTarget = true;
        }
        void Backdrop(string name, float shade = 0)
        {
            var r = Panel("Illustrated backdrop", overlay, Color.white, false); Stretch(r); r.GetComponent<Image>().sprite = Resources.Load<Sprite>("Illustrated/" + name);
            if (shade > 0) { var dim = Panel("Tint", overlay, new Color(.02f, .04f, .1f, shade), false); Stretch(dim); }
        }
        public void ShowState()
        {
            if (overlay == null) return; ClearMenu(); var g = Game.Instance;
            overlay.gameObject.SetActive(g.State != RunState.Playing); hud.gameObject.SetActive(g.State != RunState.Briefing);
            if (g.State == RunState.Playing) return;
            if (g.State == RunState.Briefing) { Home(); return; }
            if (g.State == RunState.Upgrade)
            {
                overlay.GetComponent<Image>().color = new Color(0, .015f, .03f, .18f);
                var panel = Panel("Upgrade Frame", overlay, new Color(.12f, .2f, .27f)); Place(panel, new Vector2(.5f, .5f), new Vector2(0, -28), new Vector2(512, 260)); Border(panel, light, 2);
                Label(panel, "CHOOSE UPGRADE", 23, new Vector2(0, 102), new Vector2(480, 32), Color.white, true);
                UpgradeButtons = new Button[g.Offered.Length];
                for (int i = 0; i < g.Offered.Length; i++)
                {
                    int index = i; var kind = g.Offered[i]; Color edge = kind == UpgradeKind.AttackSpeed || kind == UpgradeKind.Damage ? gold : Visuals.Blue;
                    var b = ButtonAt(panel, "", new Vector2(.5f, .5f), new Vector2((i - 1) * 161, -17), new Vector2(148, 188), () => g.Choose(index));
                    b.GetComponent<Image>().color = new Color(.14f, .24f, .33f); Border(b.transform, edge, 2); UpgradeButtons[i] = b;
                    if (kind == UpgradeKind.TripleShot)
                    {
                        for (int bolt = -1; bolt <= 1; bolt++) { var v = Panel("Blue bolt icon", b.transform, Visuals.Blue, false); v.GetComponent<Image>().sprite = Visuals.Disc; Place(v, new Vector2(.5f, .5f), new Vector2(bolt * 27, 47 + bolt * 5), new Vector2(10, 51)); v.localRotation = Quaternion.Euler(0, 0, -27); Border(v, new Color(.1f, .5f, 1, .6f), 3); }
                    }
                    else if (kind == UpgradeKind.Thrusters)
                    {
                        var v = Panel("Thruster icon", b.transform, Color.white, false); v.GetComponent<Image>().sprite = Resources.Load<Sprite>("Illustrated/Legs"); v.GetComponent<Image>().preserveAspect = true; Place(v, new Vector2(.5f, .5f), new Vector2(0, 45), new Vector2(72, 65));
                    }
                    else Label(b.transform, Icon(kind), 45, new Vector2(0, 46), new Vector2(130, 63), edge, true);
                    Label(b.transform, Upgrades.Names[(int)kind], 17, new Vector2(0, -12), new Vector2(132, 48), Color.white, true);
                    Label(b.transform, ShortDescription(kind), 13, new Vector2(0, -61), new Vector2(132, 50), light);
                }
                Label(panel, $"LEVEL {g.Level}  /  SELECT ONE TO CONTINUE", 11, new Vector2(0, -118), new Vector2(480, 20), light); return;
            }
            if (g.State == RunState.Paused)
            {
                overlay.GetComponent<Image>().color = new Color(.03f, .06f, .1f, .8f);
                Label(overlay, "MISSION PAUSED", 36, new Vector2(0, 100), new Vector2(500, 60), Color.white, true);
                ButtonAt(overlay, "RESUME", new Vector2(.5f, .5f), new Vector2(0, 0), new Vector2(420, 63), g.TogglePause, true);
                ButtonAt(overlay, "RESTART MISSION", new Vector2(.5f, .5f), new Vector2(0, -83), new Vector2(420, 57), g.StartRun);
                ButtonAt(overlay, "HOME", new Vector2(.5f, .5f), new Vector2(0, -160), new Vector2(420, 57), g.GoHome); return;
            }
            bool won = g.State == RunState.Won; Backdrop("Hero", won ? 0 : .6f);
            var title = Label(overlay, won ? "MISSION\nCOMPLETE!" : "MISSION\nFAILED", 56, new Vector2(0, 336), new Vector2(510, 142), won ? gold : Visuals.Red, true); title.fontStyle = FontStyle.BoldAndItalic;
            var rewards = Panel("Rewards", overlay, new Color(.12f, .2f, .28f)); Place(rewards, new Vector2(.5f, .5f), new Vector2(0, -259), new Vector2(494, 222));
            Label(rewards, "REWARDS", 23, new Vector2(0, 83), new Vector2(460, 30), Color.white, true);
            string[] values = { $"+{g.Credits:N0}\nCredits", $"+{g.Parts}\nMech Parts", $"+{g.WeaponXp}\nWeapon XP" }; string[] symbols = { "$", "PARTS", "XP" };
            for (int i = 0; i < 3; i++)
            {
                var card = Panel("Reward", rewards, steel); Place(card, new Vector2(.5f, .5f), new Vector2((i - 1) * 154, -17), new Vector2(141, 153)); Border(card, i == 0 ? gold : light, 1);
                Label(card, symbols[i], i == 1 ? 21 : 39, new Vector2(0, 36), new Vector2(125, 47), i == 0 ? gold : Visuals.Blue, true);
                Label(card, values[i], 18, new Vector2(0, -31), new Vector2(132, 65), Color.white, true);
            }
            Label(overlay, g.BonusDrop, 15, new Vector2(0, -380), new Vector2(500, 26), gold, true);
            ContinueButton = ButtonAt(overlay, "CONTINUE   >>", new Vector2(.5f, .5f), new Vector2(0, -422), new Vector2(476, 62), ShowBase, true);
        }
        void Home()
        {
            Game.Instance.Player.Init(Game.Instance.balance);
            Backdrop("Hero");
            var title = Label(overlay, "POCKET", 61, new Vector2(-12, 387), new Vector2(490, 80), Color.white, true); title.fontStyle = FontStyle.BoldAndItalic;
            var logo = Label(overlay, "MECH ARENA", 52, new Vector2(0, 327), new Vector2(526, 80), gold, true); logo.fontStyle = FontStyle.BoldAndItalic;
            var stats = Panel("Mech stats", overlay, new Color(.12f, .2f, .27f)); Place(stats, new Vector2(.5f, .5f), new Vector2(0, -160), new Vector2(478, 51));
            Label(stats, $"POWER  {Mathf.RoundToInt(Game.Instance.Player.Damage / Game.Instance.Player.FireInterval + Game.Instance.Player.MaxHealth * .05f)}     HULL  {Game.Instance.Player.MaxHealth:0}     DASH  {Game.Instance.Player.DashCooldown:0.0}s", 17, Vector2.zero, new Vector2(460, 38), Color.white, true);
            StartButton = ButtonAt(overlay, ">   PLAY", new Vector2(.5f, .5f), new Vector2(0, -228), new Vector2(434, 60), Missions, true);
            ButtonAt(overlay, "GARAGE", new Vector2(.5f, .5f), new Vector2(0, -297), new Vector2(434, 54), ShowGarage);
            ButtonAt(overlay, "MISSIONS", new Vector2(.5f, .5f), new Vector2(0, -361), new Vector2(434, 54), Missions);
            ButtonAt(overlay, "MECHS", new Vector2(.5f, .5f), new Vector2(0, -424), new Vector2(434, 54), Mechs);
        }
        int garageSlot;
        public void ShowGarage() { garageSlot = 3; GaragePage(); }
        void GaragePage()
        {
            ClearMenu(); Backdrop("Hero", .7f); var p = Game.Instance.Profile;
            Label(overlay, "GARAGE / VX-01 RANGER", 29, new Vector2(0, 414), new Vector2(510, 48), Color.white, true);
            Label(overlay, $"{p.credits:N0} CREDITS     {p.parts} PARTS     {p.weaponXp} WEAPON XP", 16, new Vector2(0, 366), new Vector2(515, 35), gold, true);
            for (int i = 0; i < 6; i++) { int tab = i; var b = ButtonAt(overlay, Equipment.Slots[i], new Vector2(.5f,.5f), new Vector2(-170 + (i % 3) * 170, 309 - (i / 3) * 52), new Vector2(160,44), () => { garageSlot = tab; GaragePage(); }); b.GetComponentInChildren<Text>().fontSize = 17; if (i == garageSlot) Border(b.transform, Visuals.Blue, 2); }
            if (garageSlot == 5)
            {
                for (int i = 0; i < Equipment.Colors.Length; i++) { int color = i; var b = ButtonAt(overlay, Equipment.Colors[i] + (p.color == i ? " / EQUIPPED" : ""), new Vector2(.5f,.5f), new Vector2(0, 130 - i * 100), new Vector2(450,70), () => { p.color = color; p.Save(); Game.Instance.Player.Init(Game.Instance.balance); GaragePage(); }); b.GetComponent<Image>().color = Equipment.Tints[i] * .7f; }
                Label(overlay, "Paint is applied to your battle mech.", 17, new Vector2(0,-285), new Vector2(485,40), light);
            }
            else
            {
                int row = 0;
                for (int i = 0; i < Equipment.Items.Length; i++)
                {
                    int id = i; var item = Equipment.Items[id]; if (item.slot != garageSlot) continue;
                    var panel = Panel(item.name, overlay, new Color(.1f,.18f,.26f)); Place(panel, new Vector2(.5f,.5f), new Vector2(0, 145 - row++ * 166), new Vector2(510,152));
                    bool equipped = p.equipped[garageSlot] == id;
                    Label(panel, item.name + (equipped ? " / EQUIPPED" : p.owned[id] ? " / OWNED" : " / LOCKED"), 20, new Vector2(0,49), new Vector2(495,35), equipped ? Visuals.Blue : Color.white, true);
                    Label(panel, item.detail, 15, new Vector2(0,15), new Vector2(480,40), light);
                    var equip = ButtonAt(panel, p.owned[id] ? equipped ? "EQUIPPED" : "EQUIP" : $"{item.cost} C + {item.parts} P", new Vector2(.5f,.5f), new Vector2(-126,-43), new Vector2(237,42), () => { if (!p.owned[id]) p.Buy(id); p.Equip(id); GaragePage(); }); equip.GetComponentInChildren<Text>().fontSize = 17;
                    equip.interactable = !equipped && (p.owned[id] || p.credits >= item.cost && p.parts >= item.parts);
                    var tune = ButtonAt(panel, p.tuning[id] >= 3 ? "TUNED 3/3" : $"TUNE {p.tuning[id]}/3: {300 * (p.tuning[id]+1)} C + 1 P", new Vector2(.5f,.5f), new Vector2(126,-43), new Vector2(237,42), () => { p.Tune(id); GaragePage(); }); tune.GetComponentInChildren<Text>().fontSize = 15;
                    tune.interactable = p.owned[id] && p.tuning[id] < 3 && p.credits >= 300 * (p.tuning[id]+1) && p.parts >= 1;
                }
            }
            ButtonAt(overlay, "< BASE", new Vector2(.5f,.5f), new Vector2(-128,-421), new Vector2(238,58), ShowBase);
            ButtonAt(overlay, "LOADOUT >", new Vector2(.5f,.5f), new Vector2(128,-421), new Vector2(238,58), ShowLoadout, true);
        }
        public void Missions()
        {
            var g = Game.Instance; var p = g.Profile; var mission = g.Mission;
            ClearMenu(); Backdrop(mission.Background, .66f);
            Label(overlay, "SELECT MISSION", 34, new Vector2(0,414), new Vector2(510,50), Color.white, true);
            for (int i = 0; i < PocketMech.Missions.All.Length; i++) {
                int area = i;
                var b = ButtonAt(overlay, i == 0 ? "ARENA A-1" : "FROSTLINE", new Vector2(.5f,.5f), new Vector2(i == 0 ? -126 : 126, 342), new Vector2(239,58), () => SelectArea(area), p.area == i);
                b.GetComponentInChildren<Text>().fontSize = 23;
            }
            Label(overlay, mission.Name, 32, new Vector2(0,265), new Vector2(510,60), gold, true);
            Label(overlay, "5-MINUTE SURVIVAL / " + mission.Subtitle, 16, new Vector2(0,217), new Vector2(510,40), Visuals.Blue, true);
            ButtonAt(overlay, "STANDARD", new Vector2(.5f,.5f), new Vector2(-126,145), new Vector2(239,53), () => { p.mission = 0; p.Save(); Missions(); }, p.mission == 0);
            ButtonAt(overlay, "VETERAN", new Vector2(.5f,.5f), new Vector2(126,145), new Vector2(239,53), () => { p.mission = 1; p.Save(); Missions(); }, p.mission == 1);
            Label(overlay, p.mission == 0 ? "STANDARD / Recommended power: 250" : "VETERAN / Enemy hull +30% / More credits", 18, new Vector2(0,86), new Vector2(500,40), gold, true);
            Label(overlay, mission.Briefing, 19, new Vector2(0,-64), new Vector2(500,235), Color.white);
            Label(overlay, $"VICTORY: {mission.Reward(p.mission):N0} credits / 4 parts / 12 weapon XP\n60% bonus: booster, rifle mod or armor scrap", 18, new Vector2(0,-242), new Vector2(495,75), light);
            MissionButton = ButtonAt(overlay, "START MISSION >", new Vector2(.5f,.5f), new Vector2(0,-350), new Vector2(465,62), ShowLoadout, true);
            ButtonAt(overlay, "< BASE", new Vector2(.5f,.5f), new Vector2(0,-424), new Vector2(465,50), ShowBase);
        }
        public void SelectArea(int area) { Game.Instance.Profile.area = Mathf.Clamp(area, 0, PocketMech.Missions.All.Length - 1); Game.Instance.Profile.Save(); Missions(); }
        public void ShowLoadout()
        {
            ClearMenu(); Backdrop("Hero", .55f); var g = Game.Instance; var p = g.Profile; g.Player.Init(g.balance);
            Label(overlay, "LOADOUT", 38, new Vector2(0,402), new Vector2(500,65), Color.white, true);
            Label(overlay, "VX-01 RANGER", 30, new Vector2(0,332), new Vector2(500,50), gold, true);
            var stats = Panel("Starting stats", overlay, new Color(.06f,.14f,.22f,.94f)); Place(stats,new Vector2(.5f,.5f),new Vector2(0,209),new Vector2(482,163));
            Label(stats, $"HULL {g.Player.MaxHealth:0}      SPEED {g.Player.Speed:0.0}\nDAMAGE {g.Player.Damage:0}      FIRE {g.Player.FireInterval:0.00}s\nDASH {g.Player.DashCooldown:0.0}s      CRIT {g.Player.CritChance:P0}", 23, Vector2.zero, new Vector2(460,145), Color.white, true);
            for (int slot=0;slot<5;slot++) { int s=slot; int id=p.equipped[slot]; var b=ButtonAt(overlay, Equipment.Slots[slot]+" / "+Equipment.Items[id].name, new Vector2(.5f,.5f),new Vector2(0,67-slot*57),new Vector2(480,48),()=> {garageSlot=s; GaragePage();}); b.GetComponentInChildren<Text>().fontSize=19; }
            Label(overlay, $"{Equipment.Colors[p.color]} / {g.Mission.Name} / {(p.mission==1 ? "Veteran" : "Standard")}\nRun upgrades reset; equipment stays with you.", 17, new Vector2(0,-244),new Vector2(495,60),light);
            ConfirmButton=ButtonAt(overlay,"CONFIRM & DEPLOY",new Vector2(.5f,.5f),new Vector2(0,-347),new Vector2(465,65),g.StartRun,true);
            ButtonAt(overlay,"< MISSION SELECT",new Vector2(.5f,.5f),new Vector2(0,-425),new Vector2(465,50),Missions);
        }
        public void ShowBase()
        {
            Game.Instance.GoHome(); ClearMenu(); Backdrop("Hero", .6f); var p=Game.Instance.Profile;
            Label(overlay,"BACK TO BASE",38,new Vector2(0,326),new Vector2(510,70),Color.white,true);
            Label(overlay,$"{p.credits:N0} CREDITS / {p.parts} PARTS\n{p.wins} VICTORIES / {p.weaponXp} WEAPON XP",21,new Vector2(0,222),new Vector2(500,90),gold,true);
            StartButton=ButtonAt(overlay,"PLAY AGAIN",new Vector2(.5f,.5f),new Vector2(0,65),new Vector2(460,64),Game.Instance.StartRun,true);
            ButtonAt(overlay,"GO TO GARAGE",new Vector2(.5f,.5f),new Vector2(0,-24),new Vector2(460,62),ShowGarage);
            ButtonAt(overlay,"CHANGE LOADOUT",new Vector2(.5f,.5f),new Vector2(0,-112),new Vector2(460,62),ShowLoadout);
            ButtonAt(overlay,"SELECT ANOTHER MISSION",new Vector2(.5f,.5f),new Vector2(0,-200),new Vector2(460,62),Missions);
            ButtonAt(overlay,"HOME",new Vector2(.5f,.5f),new Vector2(0,-370),new Vector2(460,56),ShowState);
        }
        void Mechs() => Info("VX-01 RANGER", "SMALL MECHS. BIG MOMENTS.\n\nYour agile starter mech is ready to deploy.\n\nCustomize five equipment slots and paint in the garage.\nOne playable mech and two arenas in this prototype.", ShowState);
        void Info(string heading, string copy, UnityEngine.Events.UnityAction back, bool deploy = false)
        {
            ClearMenu(); Backdrop("Hero", .76f); Label(overlay, heading, 35, new Vector2(0, 270), new Vector2(505, 85), gold, true);
            Label(overlay, copy, 19, new Vector2(0, 20), new Vector2(490, 350), Color.white);
            if (deploy) StartButton = ButtonAt(overlay, "DEPLOY RANGER", new Vector2(.5f, .5f), new Vector2(0, -245), new Vector2(450, 62), Game.Instance.StartRun, true);
            ButtonAt(overlay, "<   BACK", new Vector2(.5f, .5f), new Vector2(0, -337), new Vector2(450, 60), back);
        }
        static string Icon(UpgradeKind k) => new[] { "///", "->>", "^^", "+", ">>", "4s", "| |", "/^", "!", "[+]", "(O)", "+ +", "< >", "^^^", "[++]" }[(int)k];
        static string ShortDescription(UpgradeKind k) => new[] { "+2 extra\nblue bolts", "+1 target\npenetration", "Fire 25%\nfaster", "+15% weapon\ndamage", "+20% move\nspeed", "20% shorter\nrecharge", "Twin support\nguns", "Homing\nmissiles", "+10% critical\nchance", "+15% max\nhull", "Rechargeable\nshield", "Repair hull\nevery 10s", "Wider Triple\nShot spread", "Fire 20%\nfaster", "+20% max\nhull" }[(int)k];
        RectTransform Panel(string name, Transform parent, Color color, bool framed = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var im = go.GetComponent<Image>(); im.color = color; im.raycastTarget = color.a > 0;
            if (framed && color.a > 0) { im.sprite = Plate(); im.type = Image.Type.Sliced; } return (RectTransform)go.transform;
        }
        static Sprite Plate()
        {
            if (plate != null) return plate;
            const int w = 96, h = 64; var tex = new Texture2D(w, h, TextureFormat.RGBA32, false); var p = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                float cx = Mathf.Clamp(x, 9, w - 10), cy = Mathf.Clamp(y, 9, h - 10); float r = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float edge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y)); float value = edge < 2 || r > 8 ? .22f : edge < 4 || r > 6 ? .95f : Mathf.Lerp(.65f, 1, (float)y / h);
                p[y * w + x] = r > 9 ? Color.clear : new Color(value, value, value, 1);
            }
            tex.SetPixels(p); tex.Apply(); plate = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12)); return plate;
        }
        static void Border(Transform t, Color color, float size) { var line = t.gameObject.AddComponent<Outline>(); line.effectColor = color; line.effectDistance = new Vector2(size, -size); }
        Text Label(Transform parent, string text, int size, Vector2 pos, Vector2 dimensions, Color color, bool bold = false)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var t = go.GetComponent<Text>(); t.font = font; t.fontSize = size; t.text = text; t.color = color; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            var shadow = go.AddComponent<Shadow>(); shadow.effectColor = new Color(.015f, .03f, .065f, .9f); shadow.effectDistance = new Vector2(1.5f, -2);
            if (size >= 45) Border(go.transform, new Color(.02f, .065f, .14f), 2);
            Place(t.rectTransform, new Vector2(.5f, .5f), pos, dimensions); return t;
        }
        Button ButtonAt(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, bool primary = false)
        {
            var r = Panel(text + " Button", parent, primary ? gold : steel); Place(r, anchor, pos, size); var b = r.gameObject.AddComponent<Button>(); b.onClick.AddListener(action);
            Label(r, text, size.x < 120 && size.y < 100 ? 16 : primary ? 26 : 23, Vector2.zero, size - new Vector2(8, 4), primary ? new Color(.05f, .1f, .14f) : Color.white, true); return b;
        }
        Image Bar(Transform parent, Vector2 p, Vector2 size, Color color)
        {
            var r = Panel("Track", parent, new Color(.025f, .04f, .06f), false); Place(r, new Vector2(.5f, .5f), p, size);
            var f = Panel("Fill", r, color, false); Stretch(f); var im = f.GetComponent<Image>(); im.sprite = Visuals.Square; im.type = Image.Type.Filled; im.fillMethod = Image.FillMethod.Horizontal; return im;
        }
        public static void Place(RectTransform r, Vector2 anchor, Vector2 p, Vector2 size) { r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = p; r.sizeDelta = size; }
        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    }
}
