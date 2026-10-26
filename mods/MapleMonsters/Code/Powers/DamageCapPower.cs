using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MapleMonsters.Code.Powers;

/// <summary>
/// 데미지 상한 — 한 번에 Amount를 넘는 피해를 받지 않는다. (메이플 보스의 데미지 상한)
/// 한 방에 몰아치는 강한 캐릭터를 견제하는 핵심 기믹.
/// </summary>
public sealed class DamageCapPower : MaplePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0)
            return base.ModifyDamageCap(target, props, dealer, cardSource, cardPlay);
        return Math.Min(Amount, base.ModifyDamageCap(target, props, dealer, cardSource, cardPlay));
    }
}
