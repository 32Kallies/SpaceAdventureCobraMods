using HarmonyLib;

namespace DisableBootupScreenMusic;

[HarmonyPatch]
public static class DisableTitleControllerMusicPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.Start))]
    private static void UITitleControllerStartPostfix(UITitleController __instance)
    {
        __instance.gameObject.AddComponent<TitleScreenMusicDisabler>().StopMusic();
    }
}