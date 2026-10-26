using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 발록 — 어둠의 마왕.
/// 1페이즈: 포효 → 어둠의 발톱 → 화염 숨결(3연타) → 날개 바람 → 반복
/// 2페이즈 "봉인 해제" (체력 50%↓): 인공물 2·힘 3을 얻고
///          지옥불(3연타) → 어둠의 발톱 → 공포의 포효 → 날개 폭풍 → 반복
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

    // 2페이즈
    private const int UnsealArtifact = 2;
    private const int UnsealStrength = 3;
    private int HellfireDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private const int HellfireHits = 3;
    private int BigClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 30, 27);
    private int StormDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private const int StormBlock = 24;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine() => Phases(
        [
            new MoveState("ROAR", Roar, new BuffIntent(), new DebuffIntent()),
            new MoveState("DARK_CLAW", DarkClaw, new SingleAttackIntent(ClawDamage)),
            new MoveState("FLAME_BREATH", FlameBreath, new MultiAttackIntent(FlameDamage, FlameHits)),
            new MoveState("WING_GUST", WingGust, new SingleAttackIntent(GustDamage), new DefendIntent()),
        ],
        [
            new MoveState("HELLFIRE", Hellfire, new MultiAttackIntent(HellfireDamage, HellfireHits)),
            new MoveState("DARK_CLAW_2", BigClaw, new SingleAttackIntent(BigClawDamage)),
            new MoveState("TERROR_ROAR", TerrorRoar, new BuffIntent(), new DebuffIntent(strong: true)),
            new MoveState("WING_STORM", WingStorm, new SingleAttackIntent(StormDamage), new DefendIntent()),
        ]);

    protected override async Task OnPhase2Start()
    {
        await base.OnPhase2Start();
        await BuffSelf<ArtifactPower>(UnsealArtifact);
        await BuffSelf<StrengthPower>(UnsealStrength);
    }

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

    private Task Hellfire(IReadOnlyList<Creature> targets) => Hit(HellfireDamage, HellfireHits);

    private Task BigClaw(IReadOnlyList<Creature> targets) => Hit(BigClawDamage, vfx: "vfx/vfx_bite");

    private async Task TerrorRoar(IReadOnlyList<Creature> targets)
    {
        await BuffSelf<StrengthPower>(RoarStrength);
        await DebuffTargets<WeakPower>(targets, 2);
        await DebuffTargets<VulnerablePower>(targets, 2);
    }

    private async Task WingStorm(IReadOnlyList<Creature> targets)
    {
        await Block(StormBlock);
        await Hit(StormDamage);
    }
}
