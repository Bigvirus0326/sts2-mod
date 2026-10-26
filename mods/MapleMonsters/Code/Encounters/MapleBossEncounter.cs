using BaseLib.Abstracts;
using MapleMonsters.Code.Acts;
using MapleMonsters.Code.Monsters;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rooms;

namespace MapleMonsters.Code.Encounters;

/// <summary>
/// 메이플 월드의 보스 전투(보스 1마리). 새 보스를 추가하려면:
///   public sealed class XxxBoss() : MapleBossEncounter&lt;Xxx&gt;;
/// 한 줄이면 되고, localization의 encounters.json에 제목/패배 문구를 넣으면 됩니다.
/// </summary>
public abstract class MapleBossEncounter<TBoss> : CustomEncounterModel where TBoss : MapleMonster
{
    protected MapleBossEncounter() : base(RoomType.Boss) { }

    public override bool IsValidForAct(ActModel act) => act is MapleWorldAct;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<TBoss>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<TBoss>().ToMutable(), null)];

    // TODO: 전용 보스 지도 아이콘. 지금은 기본 게임 보스(밴텀)의 아이콘을 임시로 빌려 씀.
    private static EncounterModel IconSource => ModelDb.Encounter<VantomBoss>();
    public override string BossNodePath => IconSource.BossNodePath;
    public override MegaSkeletonDataResource? BossNodeSpineResource => IconSource.BossNodeSpineResource;
}

public sealed class MushmomBoss : MapleBossEncounter<Mushmom>;

public sealed class ZombieMushmomBoss : MapleBossEncounter<ZombieMushmom>;

public sealed class BalrogBoss : MapleBossEncounter<Balrog>;
