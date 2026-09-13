using UnityEngine;

namespace PocketMech
{
    [CreateAssetMenu(menuName = "Pocket Mech/Run Balance")]
    public sealed class Balance : ScriptableObject
    {
        public float runSeconds = 300, bossArrival = 260;
        public Vector2 arenaHalfSize = new Vector2(12, 19);
        public float playerHealth = 1000, playerSpeed = 5.2f, damage = 100;
        public float shotInterval = .5f, dashCooldown = 4, dashDuration = .28f;
        public float bossHealth = 8500;
        public int maxEnemies = 65;
    }
    public enum RunState { Briefing, Playing, Upgrade, Paused, Won, Lost }
    public enum EnemyKind { Scout, Shooter, Bomber, Shield, Rush, Boss, Elite }
    public enum UpgradeKind { TripleShot, Piercing, AttackSpeed, Damage, Thrusters, DashCooldown, SideCannons, MissilePod, Crit, Plating, Barrier, NanoBots, SpreadAmplifier, WeaponOverclock, HeavyArmor }
    public static class Upgrades
    {
        public static readonly string[] Names = { "Triple Shot", "Piercing", "+Attack Speed", "Overcharged Core", "Faster Thrusters", "Dash Cooldown", "Side Cannons", "Missile Pod", "Critical Core", "Reinforced Plating", "Energy Barrier", "Repair Nano-bots", "Spread Amplifier", "Weapon Overclock", "Heavy Armor" };
        public static readonly string[] Details = { "Fire three blue bolts in a spread.", "Bolts pass through one more enemy.", "Beam Rifle fires 25% faster.", "Increase all weapon damage by 15%.", "Move 20% faster around the arena.", "Reduce dash recharge by 20%.", "Twin support guns fire every 2 seconds.", "Launch a homing, explosive missile.", "+10% chance to deal double damage.", "+15% maximum hull. Restore that amount.", "Absorb 150 damage; recharge every 20s.", "Repair 3% of maximum hull every 10s.", "Widen Triple Shot from 11 to 18 degrees.", "Fire all primary weapons 20% faster.", "+20% maximum hull. Restore that amount." };
        public static int MaxRank(UpgradeKind k) => k == UpgradeKind.SpreadAmplifier || k == UpgradeKind.TripleShot || k == UpgradeKind.SideCannons || k == UpgradeKind.MissilePod ? 1 : 4;
    }
}
