using HarmonyLib;

namespace MusicReplacer.LevelMusic;

[HarmonyPatch]
public static class LevelMusicPatcher
{
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    [HarmonyPrefix]
    public static void PatchLevelMusic(LevelController __instance)
    {
        var levelDef = GameController.Instance.GetCurrentLevelDefinition();
        var overrideMusic = LevelOverrideManager.Data.GetLevelData(levelDef.level);
        
        if (overrideMusic.defaultMusic != 0)
        {
            levelDef.defaultMusic.EnumValue = (audioSelectionData.eCLIP)overrideMusic.defaultMusic;
        }

        if (overrideMusic.arenaMusic != 0)
        {
            levelDef.arenaMusic.EnumValue = (audioSelectionData.eCLIP)overrideMusic.arenaMusic;
        }
    }
}