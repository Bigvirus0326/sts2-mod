using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 머쉬맘 — 거대한 버섯 보스. 묵직한 점프 찍기와 포자로 압박하고, 점점 커진다.
/// 패턴: 점프 찍기 → 포자 뿌리기 → 몸통 박치기(3연타) → 버섯 성장 → 반복
/// </summary>
public sealed class Mushmom : MapleMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 240, 228);
    public override int MaxInitialHp => MinInitialHp;

    private int StompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private const int SlamHits = 3;
    private const int SporeWeak = 2;
    private const int SporeFrail = 1;
    private int GrowStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private const int GrowBlock = 12;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() => Cycle(
        new MoveState("JUMP_STOMP", JumpStomp, new SingleAttackIntent(StompDamage)),
        new MoveState("SPORE", Spore, new DebuffIntent()),
        new MoveState("BODY_SLAM", BodySlam, new MultiAttackIntent(SlamDamage, SlamHits)),
        new MoveState("GROW", Grow, new BuffIntent(), new DefendIntent())
    );

    private Task JumpStomp(IReadOnlyList<Creature> targets) => Hit(StompDamage);

    private async Task Spore(IReadOnlyList<Creature> targets)
    {
        await DebuffTargets<WeakPower>(targets, SporeWeak);
        await DebuffTargets<FrailPower>(targets, SporeFrail);
    }

    private Task BodySlam(IReadOnlyList<Creature> targets) => Hit(SlamDamage, SlamHits);

    private async Task Grow(IReadOnlyList<Creature> targets)
    {
        await BuffSelf<StrengthPower>(GrowStrength);
        await Block(GrowBlock);
    }
}
