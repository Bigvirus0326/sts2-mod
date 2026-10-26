using System.Text.RegularExpressions;
using BaseLib.Abstracts;
using Godot;

namespace MapleMonsters.Code.Powers;

/// <summary>
/// 메이플 몬스터용 파워(버프/디버프) 공통 기반.
/// 아이콘: art/powers/&lt;id&gt;.png  (id = 클래스 이름 snake_case, 예: DamageCapPower → damage_cap_power)
/// </summary>
public abstract partial class MaplePower : CustomPowerModel
{
    private string IconId => SnakeRegex().Replace(GetType().Name, "$1_$2").ToLowerInvariant();

    public override string CustomPackedIconPath => Icon("powers");
    public override string CustomBigIconPath => Icon("powers/big");

    // 몬스터 기믹은 멀티플레이 인원수에 따라 커지지 않게
    public override bool ShouldScaleInMultiplayer => false;

    private string Icon(string folder)
    {
        var path = $"res://{MainFile.ModId}/images/{folder}/{IconId}.png";
        return ResourceLoader.Exists(path) ? path : $"res://{MainFile.ModId}/images/{folder}/maple_power.png";
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex SnakeRegex();
}
