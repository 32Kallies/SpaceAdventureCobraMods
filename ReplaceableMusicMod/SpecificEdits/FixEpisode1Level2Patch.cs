using HarmonyLib;

namespace MusicReplacer.SpecificEdits;

[HarmonyPatch]
public static class FixEpisode1Level2Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioController), nameof(AudioController.PlayMusic))]
    public static bool DisableVaikenMusic(audioSelectionData.eCLIP _clip)
    {
        var level = LevelController.Instance;
        if (level == null || level.level != LevelController.Level.EP01_LVL02_Casino_BossVaiken) return true;
        if (_clip == audioSelectionData.eCLIP.MUS_VAIKEN_1)
            return false;
        return true;
    }
}