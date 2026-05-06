using HarmonyLib;

namespace MusicReplacer.MusicReplacementMenu;

[HarmonyPatch(typeof(NUIMainMenu))]
public static class NewMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NUIMainMenu.RefreshActivePage))]
    private static void CreateMenuPatch(NUIMainMenu __instance, bool _initialisebuttons)
    {
        if (!_initialisebuttons)
        {
            return;
        }
        
        MusicMenuBuilder.BuildMusicReplacementMenu(__instance);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NUIMainMenu.Update))]
    private static void DetectButtonPressPatch(NUIMainMenu __instance)
    {
        if (__instance.mainButtons[__instance.m_MainChoice].Type == MusicMenuBuilder.NewButtonType && Utils.GetClickingOnUI())
        {
            var menuEnabler = MusicMenuEnabler.Main;
            if (menuEnabler)
                menuEnabler.SetTab(MusicMenuEnabler.Tab.MusicEditorHome);
            else
                Plugin.Logger.LogError("Failed to find MusicMenuEnabler");
        }
    }
}