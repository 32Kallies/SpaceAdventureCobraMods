using HarmonyLib;

namespace MusicReplacer;

[HarmonyPatch]
public static class FixVideoToArenaMusicPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraVideoPlayer), nameof(CobraVideoPlayer.Stop))]
    public static void CobraVideoPlayerStopPatch()
    {
        var levelController = LevelController.Instance;
        if (levelController == null)
        {
            Plugin.Logger.LogWarning("Level controller is missing!");
            return;
        }

        if (!levelController.IsInArena())
        {
            return;
        }

        var arena = levelController.GetCurrentArena();
        
        if (arena == null || arena.arenaMusic == audioSelectionData.eCLIP.NONE)
        {
            return;
        }

        AudioController.Instance.m_BackUpMusic = arena.arenaMusic;
        
        AudioController.Instance.ProhibitForceMusicUntilNextPlayMusic(false);
        AudioController.Instance.PlayMusic(arena.arenaMusic, true);
        AudioController.Instance.ProhibitForceMusicUntilNextPlayMusic(true);
    }
}