using HarmonyLib;
using MapleMonsters.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MapleMonsters.Code.Acts;

/// <summary>
/// 메이플 월드의 법칙 — 메이플 월드 막의 모든 전투가 시작될 때, 모든 적(빌려 온 기본 몬스터 포함)에게
/// 몬스터 레벨(체력 증가) · 넉백 · 데미지 상한 · 제한 시간을 붙입니다. 숫자는 MapleDifficulty에서 조절.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class MapleWorldRules
{
    [HarmonyPostfix]
    private static void AfterBeforeCombatStart(ref Task __result, IRunState runState, ICombatState? combatState)
    {
        __result = ApplyAfter(__result, runState, combatState);
    }

    private static async Task ApplyAfter(Task original, IRunState runState, ICombatState? combatState)
    {
        await original;
        if (combatState == null || runState.Act is not MapleWorldAct)
            return;

        var roomType = combatState.Encounter?.RoomType ?? RoomType.Monster;
        var (hpBonus, knockback, cap, timeLimit) = roomType switch
        {
            RoomType.Boss => (MapleDifficulty.BossHpBonusPercent, MapleDifficulty.BossKnockbackDamage, MapleDifficulty.BossDamageCap, MapleDifficulty.BossTimeLimitTurns),
            RoomType.Elite => (MapleDifficulty.EliteHpBonusPercent, MapleDifficulty.EliteKnockbackDamage, MapleDifficulty.EliteDamageCap, MapleDifficulty.EliteTimeLimitTurns),
            _ => (MapleDifficulty.NormalHpBonusPercent, MapleDifficulty.NormalKnockbackDamage, MapleDifficulty.NormalDamageCap, MapleDifficulty.NormalTimeLimitTurns),
        };

        foreach (var enemy in combatState.Enemies.ToList())
        {
            try
            {
                await Apply(enemy, hpBonus, knockback, cap, timeLimit);
            }
            catch (Exception e)
            {
                MainFile.Logger.Error($"메이플 월드 기믹 적용 실패 ({enemy.Monster?.Id}): {e}");
            }
        }
    }

    private static async Task Apply(Creature enemy, int hpBonusPercent, int knockback, int cap, int timeLimit)
    {
        if (enemy.CurrentHp <= 0)
            return;

        if (hpBonusPercent > 0)
        {
            var newMax = (int)Math.Ceiling(enemy.MaxHp * (100 + hpBonusPercent) / 100m);
            await CreatureCmd.SetMaxAndCurrentHp(enemy, newMax);
        }

        var ctx = new ThrowingPlayerChoiceContext();
        if (knockback > 0)
            await PowerCmd.Apply<KnockbackPower>(ctx, enemy, knockback, enemy, null);
        if (cap > 0)
            await PowerCmd.Apply<DamageCapPower>(ctx, enemy, cap, enemy, null);
        if (timeLimit > 0)
            await PowerCmd.Apply<TimeLimitPower>(ctx, enemy, timeLimit, enemy, null);
    }
}
