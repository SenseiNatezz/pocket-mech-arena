using System;
using UnityEngine;

namespace PocketMech
{
    // Persistent equipment is separate from temporary, per-run upgrade ranks.
    [Serializable]
    public sealed class PilotProfile
    {
        public int credits, parts, weaponXp, wins, mission, color, area;
        public int[] equipped = { 0, 2, 4, 6, 9 };
        public bool[] owned = { true, false, true, false, true, false, true, false, false, true, false };
        public int[] tuning = new int[11];
        [NonSerialized] public bool volatileOnly;
        const string SaveKey = "PocketMech.Profile.v1";
        public static PilotProfile Load(bool isolated)
        {
            if (isolated) return new PilotProfile { volatileOnly = true };
            PilotProfile p = null;
            try { if (PlayerPrefs.HasKey(SaveKey)) p = JsonUtility.FromJson<PilotProfile>(PlayerPrefs.GetString(SaveKey)); } catch (Exception) { }
            if (p == null) p = new PilotProfile { credits = PlayerPrefs.GetInt("Credits"), parts = PlayerPrefs.GetInt("Parts"), weaponXp = PlayerPrefs.GetInt("WeaponXp") };
            p.Validate(); return p;
        }
        public void Validate()
        {
            if (owned == null || owned.Length != Equipment.Items.Length) Array.Resize(ref owned, Equipment.Items.Length);
            if (tuning == null || tuning.Length != Equipment.Items.Length) Array.Resize(ref tuning, Equipment.Items.Length);
            int[] starter = { 0, 2, 4, 6, 9 };
            if (equipped == null || equipped.Length != 5) equipped = (int[])starter.Clone();
            for (int s = 0; s < 5; s++) { owned[starter[s]] = true; int id = equipped[s]; if (id < 0 || id >= owned.Length || !owned[id] || Equipment.Items[id].slot != s) equipped[s] = starter[s]; }
            for (int i = 0; i < tuning.Length; i++) tuning[i] = Mathf.Clamp(tuning[i], 0, 3);
            credits = Mathf.Max(0, credits); parts = Mathf.Max(0, parts); weaponXp = Mathf.Max(0, weaponXp); mission = Mathf.Clamp(mission, 0, 1); area = Mathf.Clamp(area, 0, Missions.All.Length - 1); color = Mathf.Clamp(color, 0, 3);
        }
        public void Save() { if (!volatileOnly) { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(this)); PlayerPrefs.Save(); } }
        public bool Equip(int id)
        {
            if (id < 0 || id >= owned.Length || !owned[id]) return false;
            equipped[Equipment.Items[id].slot] = id; Save(); return true;
        }
        public bool Buy(int id)
        {
            if (id < 0 || id >= owned.Length || owned[id]) return false;
            var item = Equipment.Items[id]; if (credits < item.cost || parts < item.parts) return false;
            credits -= item.cost; parts -= item.parts; owned[id] = true; Save(); return true;
        }
        public bool Tune(int id)
        {
            if (id < 0 || id >= owned.Length || !owned[id] || tuning[id] >= 3) return false;
            int cost = 300 * (tuning[id] + 1); if (credits < cost || parts < 1) return false;
            credits -= cost; parts--; tuning[id]++; Save(); return true;
        }
        public string GrantBonus(float roll)
        {
            // One roll per victorious run. Duplicates turn into useful crafting material.
            int id = roll < .20f ? 10 : roll < .35f ? 7 : -1;
            if (id >= 0) { if (owned[id]) { parts += 2; return Equipment.Items[id].name + " duplicate: +2 parts"; } owned[id] = true; return Equipment.Items[id].name + " unlocked"; }
            if (roll < .60f) { parts += 2; return "Armor Scrap: +2 mech parts"; }
            return "No bonus drop this run";
        }
        public void Apply(PlayerMech p)
        {
            foreach (int id in equipped)
            {
                var item = Equipment.Items[id]; p.MaxHealth += item.hull; p.Damage *= item.damage; p.Speed *= item.speed; p.CritChance += item.crit; p.DashCooldown *= item.dash;
                int rank = tuning[id];
                switch (item.slot) { case 0: p.CritChance += .02f * rank; break; case 1: p.Damage *= 1 + .05f * rank; break; case 2: p.MaxHealth += 50 * rank; break; case 3: p.Damage *= 1 + .05f * rank; break; case 4: p.DashCooldown *= Mathf.Pow(.95f, rank); break; }
            }
            if (equipped[3] == 8) { p.Pierce += 2; p.FireInterval *= 2; p.Damage *= 2.1f; }
            p.Health = p.MaxHealth;
        }
    }
    public sealed class EquipmentItem
    {
        public string name, detail; public int slot, cost, parts; public float hull, damage = 1, speed = 1, crit, dash = 1;
        public EquipmentItem(string n, string d, int s, int c = 0, int partCost = 0) { name = n; detail = d; slot = s; cost = c; parts = partCost; }
    }
    public static class Equipment
    {
        public static readonly string[] Slots = { "HEAD", "ARMS", "ARMOR", "WEAPON", "BOOSTER", "COLOR" };
        public static readonly string[] Colors = { "Ranger Blue", "Arctic Cyan", "Solar Gold", "Rose Alloy" };
        public static readonly Color[] Tints = { Color.white, new Color(.62f, 1, 1), new Color(1, .83f, .48f), new Color(1, .62f, .78f) };
        public static readonly EquipmentItem[] Items = {
            new EquipmentItem("V-01 Optics", "Nearest-target tracking. Tune: +2% crit/rank.", 0),
            new EquipmentItem("V-02 Hunter", "+5% critical chance. Tune: +2% crit/rank.", 0, 700, 2) { crit = .05f },
            new EquipmentItem("A-1 Mounts", "Independent aiming. Tune: +5% damage/rank.", 1),
            new EquipmentItem("A-3 Stabilizers", "+10% weapon damage. Tune: +5% damage/rank.", 1, 900, 2) { damage = 1.1f },
            new EquipmentItem("L-1 Light Alloy", "Agile starter frame. Tune: +50 hull/rank.", 2),
            new EquipmentItem("L-2 Armor", "+200 hull; -5% speed. Tune: +50 hull/rank.", 2, 800, 2) { hull = 200, speed = .95f },
            new EquipmentItem("BR-1 Beam Rifle", "Rapid blue bolts. Tune: +5% damage/rank.", 3),
            new EquipmentItem("Beam Rifle Mod", "+15% damage. Uncommon. Tune: +5% damage/rank.", 3, 1000, 3) { damage = 1.15f },
            new EquipmentItem("RG-1 Railgun", "2.1x damage, half fire rate; pierces 2 targets.", 3, 1500, 4),
            new EquipmentItem("B-1 Thruster", "4s dash recharge. Tune: -5% cooldown/rank.", 4),
            new EquipmentItem("Booster B-2", "+10% speed; -10% dash cooldown. Common.", 4, 600, 2) { speed = 1.1f, dash = .9f }
        };
    }
}
