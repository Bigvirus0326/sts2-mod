using MapleMonsters.Code.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MapleMonsters.Code.Powers;

/// <summary>
/// 언데드 — 처음 쓰러질 때 한 번, 최대 체력의 일부로 되살아난다. (좀비 계열 몬스터)
/// 되살아나면 이 파워는 사라지고, 메이플 몬스터라면 2페이즈로 넘어간다.
/// </summary>
public sealed class UndeadPower : MaplePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private bool _used;

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner || _used)
            return base.ShouldDie(creature);
        _used = true;
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        await base.AfterPreventingDeath(creature);
        if (creature != Owner)
            return;

        Flash();
        var hp = Math.Max(1, (int)Math.Ceiling(Owner.MaxHp * MapleDifficulty.UndeadReviveHpPercent / 100m));
        await CreatureCmd.SetCurrentHp(Owner, hp);
        if (Owner.Monster is MapleMonster maple)
            await maple.EnterPhase2();
        await PowerCmd.Remove(this);
    }
}
