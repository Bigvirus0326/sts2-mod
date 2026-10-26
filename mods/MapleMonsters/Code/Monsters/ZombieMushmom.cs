using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 좀비머쉬맘 — 저주받은 머쉬맘. 저주의 포자로 약하게 만들고, 생명력을 빨아들여 오래 버틴다.
/// 패턴: 저주의 포자 → 썩은 박치기 → 생명력 흡수 → 썩은 박치기 → 반복
/// </summary>
public sealed class ZombieMushmom : MapleMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 260, 248);
    public override int MaxInitialHp => MinInitialHp;

    private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int DrainDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
    private const int DrainHeal = 12;
    private int CurseVulnerable => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private const int CurseWeak = 1;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() => Cycle(
        new MoveState("CURSED_SPORE", CursedSpore, new DebuffIntent(strong: true)),
        new MoveState("ROTTEN_SLAM", RottenSlam, new SingleAttackIntent(SlamDamage)),
        new MoveState("DRAIN", Drain, new SingleAttackIntent(DrainDamage), new HealIntent()),
        new MoveState("ROTTEN_SLAM_2", RottenSlam, new SingleAttackIntent(SlamDamage))
    );

    private async Task CursedSpore(IReadOnlyList<Creature> targets)
    {
        await DebuffTargets<VulnerablePower>(targets, CurseVulnerable);
        await DebuffTargets<WeakPower>(targets, CurseWeak);
    }

    private Task RottenSlam(IReadOnlyList<Creature> targets) => Hit(SlamDamage);

    private async Task Drain(IReadOnlyList<Creature> targets)
    {
        await Hit(DrainDamage, vfx: "vfx/vfx_bite");
        await HealSelf(DrainHeal);
    }
}
