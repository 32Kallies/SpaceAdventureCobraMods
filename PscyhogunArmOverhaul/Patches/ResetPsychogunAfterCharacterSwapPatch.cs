using HarmonyLib;

namespace PscyhogunArmOverhaul.Patches;

[HarmonyPatch]
public static class ResetPsychogunAfterCharacterSwapPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NUICharacterPanel), nameof(NUICharacterPanel.OnDestroy))]
    private static void OnBeforeCloseSkinSelectionMenu(NUICharacterPanel __instance)
    {
        // if the skin was changed, based on logic from original function
        if (LoadSaveController.Instance.PreferencesData.currentCobraSkin != __instance.m_AvailableSkins[__instance.m_CurrentSkinIndex])
        {
            // reset psychogun state for new skin
            var state = PsychogunStateRememberer.GetInstance(true);
            if (state != null)
            {
                state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, true);
                state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, false);
            }
        }
    }
}