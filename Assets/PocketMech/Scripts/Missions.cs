using UnityEngine;

namespace PocketMech
{
    // Area selection is independent of the legacy profile.mission difficulty field.
    public sealed class MissionDefinition
    {
        public readonly string Name, Background, Subtitle, Briefing;
        public readonly EnemyKind Starter, Boss;
        public readonly string BossName;
        public readonly int Credits;
        public MissionDefinition(string name, string background, string subtitle, string briefing, EnemyKind starter, EnemyKind boss, string bossName, int credits)
        { Name = name; Background = background; Subtitle = subtitle; Briefing = briefing; Starter = starter; Boss = boss; BossName = bossName; Credits = credits; }
        public int Reward(int difficulty) => Credits + (difficulty == 1 ? 500 : 0);
    }
    public static class Missions
    {
        public static readonly MissionDefinition[] All = {
            new MissionDefinition("ARENA A-1", "Arena", "INDUSTRIAL DISTRICT", "Scout / Shooter / Bomber / Shield / Rush\n\n02:40  Elite Assault Striker\n03:45  Heavy enemy warning\n04:20  Heavy Siege Walker\n05:00  Extract after defeating the boss", EnemyKind.Scout, EnemyKind.Boss, "HEAVY SIEGE WALKER", 1250),
            new MissionDefinition("FROSTLINE REACTOR", "FrostArena", "FROZEN REACTOR YARD", "Ice Skimmer / Rail Sentinel / Cryo Mortar\n\n01:30  Reactor vents activate\n02:40  Sentinel reinforcements\n04:20  Glacier Colossus\n05:00  Extract after defeating the boss", EnemyKind.IceSkimmer, EnemyKind.GlacierColossus, "GLACIER COLOSSUS", 1500)
        };
        public static MissionDefinition Get(int area) => All[Mathf.Clamp(area, 0, All.Length - 1)];
    }
}
