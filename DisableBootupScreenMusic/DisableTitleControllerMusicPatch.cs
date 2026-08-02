using HarmonyLib;

namespace DisableBootupScreenMusic;

[HarmonyPatch]
public static class DisableTitleControllerMusicPatch
{
    // skips the PlayMusic call
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.Start))]
    private static bool ReplaceUITitleControllerStart(UITitleController __instance)
    {
        LoadSaveController.Instance.ResetGameData();
        LoadSaveController.Instance.ResetPreferencesData();
        if (PlatformController.Instance.Platform.IsSignInRequired())
        {
            __instance.m_Status = UITitleController.STATUS.HANDLESIGNIN;
        }
        else
        {
            __instance.m_Status = UITitleController.STATUS.WAITFORANYBUTTONPRESSED;
        }
        __instance.actionText.TextID = __instance.statusTextsId[(int)__instance.m_Status];
        return false; // skip original method
    }
}