using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MapleMonsters.Code.Powers;

/// <summary>
/// 넉백 — 이 적이 한 번에 막히지 않은 피해를 Amount 이상 주면, 맞은 플레이어의 손패 카드가 날아간다.
/// (메이플에서 큰 공격에 맞으면 밀려나는 넉백)
/// - 연타는 한 대씩 따로 판정하므로 묵직한 한 방에만 걸린다.
/// - 적의 턴에는 손패가 비어 있으므로, "넉백됨"을 걸어 다음 턴에 뽑는 카드가 날아가게 한다.
/// </summary>
public sealed class KnockbackPower : MaplePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        await base.AfterDamageGiven(choiceContext, dealer, result, props, target, cardSource);
        if (dealer != Owner || Amount <= 0 || result.UnblockedDamage < Amount || target.CurrentHp <= 0)
            return;

        var hand = target.Player?.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return;

        Flash();
        var ctx = new ThrowingPlayerChoiceContext();

        // 플레이어 턴 중에 맞았고 손패가 있으면 바로 날림
        if (CombatState.CurrentSide == target.Side && hand.Count > 0)
        {
            var rng = Owner.Monster?.Rng;
            var flying = hand.OrderBy(_ => rng?.NextInt(1_000_000) ?? Random.Shared.Next())
                .Take(MapleDifficulty.KnockbackCards).ToList();
            await CardCmd.Discard(ctx, flying);
            return;
        }

        // 적의 턴이면 다음 턴에 뽑는 카드가 날아가도록
        await PowerCmd.Apply<KnockedBackPower>(ctx, target, MapleDifficulty.KnockbackCards, Owner, null);
    }
}

/// <summary>
/// 넉백됨 (플레이어 디버프) — 다음 턴 시작에 뽑는 카드 중 Amount장이 바로 날아가 버려진다.
/// </summary>
public sealed class KnockedBackPower : MaplePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await base.AfterCardDrawn(choiceContext, card, fromHandDraw);
        if (!fromHandDraw || Amount <= 0 || card.Owner != Owner.Player)
            return;

        Flash();
        await CardCmd.Discard(choiceContext, card);
        if (Amount <= 1)
            await PowerCmd.Remove(this);
        else
            await PowerCmd.Decrement(this);
    }

    // 뽑을 카드가 없어 남았으면 그 턴이 끝날 때 사라짐
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Owner.Side)
            await PowerCmd.Remove(this);
    }
}
