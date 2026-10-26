namespace MapleMonsters.Code;

/// <summary>
/// 메이플 월드 난이도 조절판 — 숫자만 바꿔서 밸런스를 맞추세요.
/// (메이플 캐릭터 모드가 강한 것을 전제로, 메이플 월드 막의 모든 적에게 기믹이 붙습니다)
/// </summary>
public static class MapleDifficulty
{
    // ── 몬스터 레벨: 최대 체력 증가 (%) ──
    public const int NormalHpBonusPercent = 25;
    public const int EliteHpBonusPercent = 35;
    public const int BossHpBonusPercent = 15;

    // ── 데미지 상한: 한 번에 받을 수 있는 최대 피해 (0 = 없음) ──
    public const int NormalDamageCap = 0;
    public const int EliteDamageCap = 35;
    public const int BossDamageCap = 25;

    // ── 제한 시간: N턴이 지나면 매 턴 힘을 얻음 (0 = 없음) ──
    public const int NormalTimeLimitTurns = 0;
    public const int EliteTimeLimitTurns = 6;
    public const int BossTimeLimitTurns = 8;
    public const int TimeLimitStrengthPerTurn = 3;

    // ── 보스 2페이즈: 체력이 이 비율(%) 이하가 되면 전환 ──
    public const int Phase2HpPercent = 50;

    // ── 좀비 계열 부활: 부활 시 회복하는 최대 체력 비율(%) ──
    public const int UndeadReviveHpPercent = 40;
}
