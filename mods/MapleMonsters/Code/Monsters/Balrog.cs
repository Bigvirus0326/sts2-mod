using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 발록 — 어둠의 마왕. 포효로 힘을 끌어올린 뒤 발톱과 화염으로 몰아친다.
/// 패턴: 포효 → 어둠의 발톱 → 화염 숨결(3연타) → 날개 바람 → 반복
/// </summary>
public sealed class Balrog : MapleMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 280, 266);
    public override int MaxInitialHp => MinInitialHp;

    private const int RoarStrength = 3;
    private const int RoarWeak = 2;
    private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 21);
    private int FlameDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private const int FlameHits = 3;
    private int GustDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private const int GustBlock = 18;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() => Cycle(
        new MoveState("ROAR", Roar, new BuffIntent(), new DebuffIntent()),
        new MoveState("DARK_CLAW", DarkClaw, new SingleAttackIntent(ClawDamage)),
        new MoveState("FLAME_BREATH", FlameBreath, new MultiAttackIntent(FlameDamage, FlameHits)),
        new MoveState("WING_GUST", WingGust, new SingleAttackIntent(GustDamage), new DefendIntent())
    );

    private async Task Roar(IReadOnlyList<Creature> targets)
    {
        await BuffSelf<StrengthPower>(RoarStrength);
        await DebuffTargets<WeakPower>(targets, RoarWeak);
    }

    private Task DarkClaw(IReadOnlyList<Creature> targets) => Hit(ClawDamage, vfx: "vfx/vfx_bite");

    private Task FlameBreath(IReadOnlyList<Creature> targets) => Hit(FlameDamage, FlameHits);

    private async Task WingGust(IReadOnlyList<Creature> targets)
    {
        await Block(GustBlock);
        await Hit(GustDamage);
    }
}
