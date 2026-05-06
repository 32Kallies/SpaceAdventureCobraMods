using HarmonyLib;

namespace MusicReplacer.LevelMusic;

[HarmonyPatch]
public static class LevelMusicPatcher
{
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    [HarmonyPostfix]
    public static void PatchLevelMusic(LevelController __instance)
    {
        
    }
}