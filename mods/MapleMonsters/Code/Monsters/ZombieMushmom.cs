using MapleMonsters.Code.Powers;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 좀비머쉬맘 — 저주받은 머쉬맘. [언데드]: 처음 쓰러지면 체력 40%로 되살아나 2페이즈로.
/// 1페이즈: 저주의 포자 → 썩은 박치기 → 생명력 흡수 → 썩은 박치기 → 반복
/// 2페이즈 (부활 후): 저주 폭발 → 생명력 대흡수 → 썩은 일격 → 반복
/// </summary>
public sealed class ZombieMushmom : MapleMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 190);
    public override int MaxInitialHp => MinInitialHp;

    private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int DrainDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
    private const int DrainHeal = 12;
    private int CurseVulnerable => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private const int CurseWeak = 1;

    // 2페이즈
    private int BigDrainDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 13);
    private const int BigDrainHeal = 15;
    private int RotStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 22);

    // 부활할 때 2페이즈로 넘어가므로 체력 50% 전환은 쓰지 않음
    protected override bool TriggersPhase2AtHp => false;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await BuffSelf<UndeadPower>(1);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() => Phases(
        [
            new MoveState("CURSED_SPORE", CursedSpore, new DebuffIntent(strong: true)),
            new MoveState("ROTTEN_SLAM", RottenSlam, new SingleAttackIntent(SlamDamage)),
            new MoveState("DRAIN", Drain, new SingleAttackIntent(DrainDamage), new HealIntent()),
            new MoveState("ROTTEN_SLAM_2", RottenSlam, new SingleAttackIntent(SlamDamage)),
        ],
        [
            new MoveState("CURSE_BURST", CurseBurst, new DebuffIntent(strong: true)),
            new MoveState("BIG_DRAIN", BigDrain, new SingleAttackIntent(BigDrainDamage), new HealIntent()),
            new MoveState("ROT_STRIKE", RotStrike, new SingleAttackIntent(RotStrikeDamage)),
        ]);

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

    private async Task CurseBurst(IReadOnlyList<Creature> targets)
    {
        await DebuffTargets<VulnerablePower>(targets, 3);
        await DebuffTargets<WeakPower>(targets, 2);
    }

    private async Task BigDrain(IReadOnlyList<Creature> targets)
    {
        await Hit(BigDrainDamage, vfx: "vfx/vfx_bite");
        await HealSelf(BigDrainHeal);
    }

    private Task RotStrike(IReadOnlyList<Creature> targets) => Hit(RotStrikeDamage);
}
