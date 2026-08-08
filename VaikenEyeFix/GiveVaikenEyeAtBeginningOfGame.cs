using HarmonyLib;

namespace VaikenEyeFix;

[HarmonyPatch]
public static class GiveVaikenEyeAtBeginningOfGame
{
    // A postfix is fine because the dialogue will only appear in the next Update call
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIDialogPanel), nameof(UIDialogPanel.SetDialog))]
    private static void Patch(string _dialogid, UIDialogPanel __instance)
    {
        if (!string.Equals(_dialogid, "EP01_LVL01_VN06"))
        {
            return;
        }

        Plugin.Logger.LogInfo("Patching Vaiken 1-1 dialogue");
        
        for (int i = __instance.m_FirstLine; i < __instance.m_LastLine; i++)
        {
            var line = __instance.m_Lines[i];
            line.rightCharacterPose = UIDialogPanel.POSE.Confident;
            __instance.m_Lines[i] = line;
        }
    }
}