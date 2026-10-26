using System.Text.RegularExpressions;
using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
        for (var i = 0; i < moves.Length; i++)
            moves[i].FollowUpState = moves[(i + 1) % moves.Length];
        return new MonsterMoveStateMachine(moves, moves[0]);
    }

    private static string ToSnake(string name) =>
        SnakeRegex().Replace(name, "$1_$2").ToLowerInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex SnakeRegex();
}
