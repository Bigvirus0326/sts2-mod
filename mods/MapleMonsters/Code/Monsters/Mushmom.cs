using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 머쉬맘 — 거대한 버섯 보스.
/// 1페이즈: 점프 찍기 → 포자 뿌리기 → 몸통 박치기(3연타) → 버섯 성장 → 반복
/// 2페이즈 "분노한 머쉬맘" (체력 50%↓): 힘+2·방어 20을 얻고
///          대점프 → 포자 폭풍 → 몸통 박치기(4연타) → 반복
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

    // 2페이즈
    private const int RageStrength = 2;
    private const int RageBlock = 20;
    private int BigJumpDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 32, 28);
    private int RageSlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private const int RageSlamHits = 4;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() => Phases(
        [
            new MoveState("JUMP_STOMP", JumpStomp, new SingleAttackIntent(StompDamage)),
            new MoveState("SPORE", Spore, new DebuffIntent()),
            new MoveState("BODY_SLAM", BodySlam, new MultiAttackIntent(SlamDamage, SlamHits)),
            new MoveState("GROW", Grow, new BuffIntent(), new DefendIntent()),
        ],
        [
            new MoveState("BIG_JUMP", BigJump, new SingleAttackIntent(BigJumpDamage)),
            new MoveState("SPORE_STORM", SporeStorm, new DebuffIntent(strong: true)),
            new MoveState("RAGE_SLAM", RageSlam, new MultiAttackIntent(RageSlamDamage, RageSlamHits)),
        ]);

    protected override async Task OnPhase2Start()
    {
        await base.OnPhase2Start();
        await BuffSelf<StrengthPower>(RageStrength);
        await Block(RageBlock);
    }

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

    private Task BigJump(IReadOnlyList<Creature> targets) => Hit(BigJumpDamage);

    private async Task SporeStorm(IReadOnlyList<Creature> targets)
    {
        await DebuffTargets<WeakPower>(targets, 2);
        await DebuffTargets<FrailPower>(targets, 2);
        await DebuffTargets<VulnerablePower>(targets, 1);
    }

    private Task RageSlam(IReadOnlyList<Creature> targets) => Hit(RageSlamDamage, RageSlamHits);
}
