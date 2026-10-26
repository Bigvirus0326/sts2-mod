using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Unlocks;

namespace MapleMonsters.Code.Acts;

/// <summary>
/// 메이플 월드 — 1막 자리에 기존 1막 대신 등장할 수 있는 메이플 전용 막.
///
/// 지금 단계:
///  - 보스: 메이플 보스만 등장 (Encounters/MapleBossEncounter.cs, IsValidForAct로 자동 추가됨)
///  - 일반/엘리트 전투, 이벤트, 지도·배경·음악: 메이플 몹이 준비될 때까지 기본 1막(Overgrowth) 것을 임시로 빌려 씀
/// 메이플 일반/엘리트 몹을 추가하면 아래 GenerateAllEncounters에서 빌려 쓰는 부분을 줄여 나가면 됩니다.
/// </summary>
public sealed class MapleWorldAct : CustomActModel
{
    public MapleWorldAct() : base(actNumber: 1) { }

    /// <summary>임시로 빌려 쓰는 기본 1막.</summary>
    private static ActModel Borrowed => ModelDb.Act<Overgrowth>();

    // 기본 1막의 일반/엘리트 전투만 가져오고 보스는 제외 (보스는 메이플 보스만)
    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
        Borrowed.GenerateAllEncounters().Where(e => e.RoomType != RoomType.Boss);

    public override IEnumerable<EventModel> AllEvents => Borrowed.AllEvents;

    public override int Index => 0;
    public override bool IsDefault => false;
    public override bool IsUnlocked(UnlockState unlockState) => true;

    public override bool Equals(object? obj) => obj is MapleWorldAct;
    public override int GetHashCode() => typeof(MapleWorldAct).GetHashCode();

    // ── 지도·배경·음악·상자: 임시로 기본 1막과 동일 ──
    public override Color MapTraveledColor => Borrowed.MapTraveledColor;
    public override Color MapUntraveledColor => Borrowed.MapUntraveledColor;
    public override Color MapBgColor => Borrowed.MapBgColor;

    public override string[] BgMusicOptions => Borrowed.BgMusicOptions;
    public override string[] MusicBankPaths => Borrowed.MusicBankPaths;
    public override string AmbientSfx => Borrowed.AmbientSfx;

    public override string ChestSpineResourcePath => Borrowed.ChestSpineResourcePath;
    public override string ChestSpineSkinNameNormal => Borrowed.ChestSpineSkinNameNormal;
    public override string ChestSpineSkinNameStroke => Borrowed.ChestSpineSkinNameStroke;
    public override string ChestOpenSfx => Borrowed.ChestOpenSfx;

    protected override string CustomMapTopBgPath => Borrowed.MapTopBgPath;
    protected override string CustomMapMidBgPath => Borrowed.MapMidBgPath;
    protected override string CustomMapBotBgPath => Borrowed.MapBotBgPath;
    protected override string CustomRestSiteBackgroundPath => Borrowed.RestSiteBackgroundPath;
    protected override string CustomBackgroundScenePath => Borrowed.BackgroundScenePath;
    protected override BackgroundAssets CustomGenerateBackgroundAssets(Rng rng) => Borrowed.GenerateBackgroundAssets(rng);
}
