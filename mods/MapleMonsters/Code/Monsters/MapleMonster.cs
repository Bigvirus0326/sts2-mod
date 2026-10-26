using System.Text.RegularExpressions;
using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace MapleMonsters.Code.Monsters;

/// <summary>
/// 메이플 몬스터 공통 기반.
/// - 그림: art/monsters/&lt;id&gt;.png 한 장(왼쪽을 바라보는 투명 PNG)을 그대로 몬스터 모습으로 사용합니다.
///   id는 클래스 이름의 snake_case (예: ZombieMushmom → zombie_mushmom).
/// - 공격/버프/디버프/방어를 한 줄로 쓰는 도우미와, 패턴을 순서대로 반복하는 Cycle()을 제공합니다.
/// </summary>
public abstract partial class MapleMonster : CustomMonsterModel
{
    public string ImageId => ToSnake(GetType().Name);

    public string ImagePath => $"res://{MainFile.ModId}/images/monsters/{ImageId}.png";

    // 스파인 애니메이션 대신 이미지 한 장을 쓰므로, 미리 불러올 자원은 이미지뿐입니다.
    public override IEnumerable<string> AssetPaths => [ImagePath];

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromResource(PreloadManager.Cache.GetTexture2D(ImagePath));

    // ───── 행동 도우미 ─────

    /// <summary>플레이어에게 공격. hits &gt; 1 이면 연속 공격.</summary>
    protected async Task Hit(int damage, int hits = 1, string vfx = "vfx/vfx_attack_blunt")
    {
        var cmd = DamageCmd.Attack(damage).FromMonster(this).WithHitFx(vfx);
        if (hits > 1) cmd = cmd.WithHitCount(hits);
        await cmd.Execute(null);
    }

    /// <summary>자기 자신에게 파워(버프) 부여.</summary>
    protected async Task BuffSelf<T>(int amount) where T : PowerModel =>
        await PowerCmd.Apply<T>(new ThrowingPlayerChoiceContext(), Creature, amount, Creature, null);

    /// <summary>대상들(보통 플레이어)에게 파워(디버프) 부여.</summary>
    protected async Task DebuffTargets<T>(IReadOnlyList<Creature> targets, int amount) where T : PowerModel =>
        await PowerCmd.Apply<T>(new ThrowingPlayerChoiceContext(), targets, amount, Creature, null);

    /// <summary>방어도 획득.</summary>
    protected async Task Block(int amount) =>
        await CreatureCmd.GainBlock(Creature, amount, ValueProp.Move, null);

    /// <summary>체력 회복.</summary>
    protected async Task HealSelf(int amount) =>
        await CreatureCmd.Heal(Creature, amount);

    /// <summary>주어진 행동들을 순서대로 반복하는 패턴 (A → B → C → A …).</summary>
    protected static MonsterMoveStateMachine Cycle(params MoveState[] moves)
    {
        Link(moves);
        return new MonsterMoveStateMachine(moves, moves[0]);
    }

    private static void Link(MoveState[] moves)
    {
        for (var i = 0; i < moves.Length; i++)
            moves[i].FollowUpState = moves[(i + 1) % moves.Length];
    }

    // ───── 2페이즈 ─────

    private MoveState? _phase2Start;
    public bool InPhase2 { get; private set; }

    /// <summary>
    /// 1페이즈 패턴을 반복하다가, 2페이즈가 되면 2페이즈 패턴을 반복.
    /// 기본은 체력이 MapleDifficulty.Phase2HpPercent 이하가 될 때 전환 (TriggersPhase2AtHp로 끌 수 있음).
    /// </summary>
    protected MonsterMoveStateMachine Phases(MoveState[] phase1, MoveState[] phase2)
    {
        Link(phase1);
        Link(phase2);
        _phase2Start = phase2[0];
        return new MonsterMoveStateMachine([.. phase1, .. phase2], phase1[0]);
    }

    /// <summary>false면 체력으로는 2페이즈가 되지 않음 (예: 부활할 때 전환하는 좀비).</summary>
    protected virtual bool TriggersPhase2AtHp => true;

    /// <summary>2페이즈 진입 연출/효과. 기본: 자신의 디버프를 모두 지움.</summary>
    protected virtual async Task OnPhase2Start()
    {
        foreach (var p in Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
            await PowerCmd.Remove(p);
    }

    public async Task EnterPhase2()
    {
        if (InPhase2 || _phase2Start == null || Creature.CurrentHp <= 0)
            return;
        InPhase2 = true;
        await OnPhase2Start();
        SetMoveImmediate(_phase2Start, forceTransition: true);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
        if (target != Creature || InPhase2 || !TriggersPhase2AtHp || Creature.CurrentHp <= 0)
            return;
        if (Creature.CurrentHp * 100 <= Creature.MaxHp * MapleDifficulty.Phase2HpPercent)
            await EnterPhase2();
    }

    private static string ToSnake(string name) =>
        SnakeRegex().Replace(name, "$1_$2").ToLowerInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex SnakeRegex();
}
