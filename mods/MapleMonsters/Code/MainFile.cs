using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace MapleMonsters.Code;

/// <summary>
/// 모드 진입점. 게임이 모드를 불러올 때 Initialize()를 한 번 호출합니다.
/// 새 기능은 Code/ 아래에 파일을 추가하고, Harmony 패치는 [HarmonyPatch] 클래스로 만들면
/// 아래 PatchAll이 자동으로 적용합니다.
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "MapleMonsters";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Godot 씬에 이 모드의 스크립트를 붙여 쓰려면 아래 줄의 주석을 푸세요.
        // Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);

        new Harmony(ModId).PatchAll(assembly);

        Logger.Info($"{ModId} loaded.");
    }
}
