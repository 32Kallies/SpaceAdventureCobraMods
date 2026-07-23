using HarmonyLib;

namespace MusicReplacer.Utilities;

[HarmonyPatch]
public static class TempPatch
{
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    [HarmonyPostfix]
    public static void StartPostfix()
    {
        LevelRipperUtils.DumpCurrentLevel();
    }
}