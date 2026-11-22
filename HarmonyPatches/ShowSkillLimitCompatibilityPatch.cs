using System.Linq;
using System.Reflection;
using HarmonyLib;
using HeroPathAutoBuildAssignment.Views;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.CharacterDeveloper;
using TaleWorlds.ScreenSystem;

namespace HeroPathAutoBuildAssignment.HarmonyPatches;

[HarmonyPatch]
internal static class ShowSkillLimitCompatibilityPatch
{
    private const string ShowSkillLimitOwner = "ShowSkillLimit";

    private static MethodBase? _targetMethod;
    private static MethodInfo? _showSkillLimitPostfix;

    private static bool _isRemoved;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PerkSelectionBarWidget), "OnLateUpdate")]
    private static void Postfix()
    {
        var topScreen = ScreenManager.TopScreen;

        bool isOurScreen = topScreen is TemplateManagerScreen;

        if (isOurScreen && !_isRemoved)
        {
            RemoveShowSkillLimitPatch();
        }
        else if (!isOurScreen && _isRemoved)
        {
            RestoreShowSkillLimitPatch();
        }
    }

    private static void RemoveShowSkillLimitPatch()
    {
        _targetMethod = AccessTools.Method(
            typeof(PerkSelectionBarWidget),
            "OnLateUpdate"
        );

        var patches = Harmony.GetPatchInfo(_targetMethod);

        var patch = patches?.Postfixes.FirstOrDefault(p => p.owner == ShowSkillLimitOwner
        );

        if (patch == null)
            return;

        _showSkillLimitPostfix = patch.PatchMethod;

        var harmony = new Harmony(ShowSkillLimitOwner);
        harmony.Unpatch(_targetMethod, _showSkillLimitPostfix);

        _isRemoved = true;
    }

    private static void RestoreShowSkillLimitPatch()
    {
        if (_targetMethod == null || _showSkillLimitPostfix == null)
            return;

        var harmony = new Harmony(ShowSkillLimitOwner);

        harmony.Patch(
            _targetMethod,
            postfix: new HarmonyMethod(_showSkillLimitPostfix)
        );

        _isRemoved = false;
    }
    
    internal static void OnTemplateManagerOpened()
    {
        RemoveShowSkillLimitPatch();
    }

    internal static void OnTemplateManagerClosed()
    {
        RestoreShowSkillLimitPatch();
    }
}