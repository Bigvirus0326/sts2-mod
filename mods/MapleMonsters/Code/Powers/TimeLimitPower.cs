using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MapleMonsters.Code.Powers;

/// <summary>
/// 제한 시간 — 적의 턴이 시작될 때마다 1씩 줄어들고, 1이 되면 그때부터 매 턴 힘을 얻는다.
/// 오래 버티며 키우는 플레이를 견제. (메이플 보스전의 제한 시간)
/// </summary>
public sealed class TimeLimitPower : MaplePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);
        if (side != Owner.Side || Owner.CurrentHp <= 0)
            return;

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner,
            MapleDifficulty.TimeLimitStrengthPerTurn, Owner, null);
    }
}
