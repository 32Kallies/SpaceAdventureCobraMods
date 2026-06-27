using HarmonyLib;
using MusicReplacer.Utilities;

namespace MusicReplacer;

[HarmonyPatch]
public static class ScreenshotTakingPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(audioForceMusicTrigger), nameof(audioForceMusicTrigger.Start))]
    public static void PatchMusicTriggerStart(audioForceMusicTrigger __instance)
    {
        var dimensions = TriggerUtils.GetColliderDimensions(__instance.gameObject);
        var hash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
        if (hash == 0)
        {
            Plugin.Logger.LogWarning("Invalid hash on trigger: " + __instance);
            return;
        }
        
        var takeScreenshot = __instance.gameObject.AddComponent<TakeScreenshotOnEnterTrigger>();

        takeScreenshot.hash = hash;
        takeScreenshot.trigger = __instance;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.StartArena))]
    public static void ArenaStartPostfix()
    {
        var arena = LevelController.Instance.GetCurrentArena();
        if (arena == null)
            return;
        ScreenshotGenerator.GenerateScreenshotWithDelay(arena.arenaID, 3.3f);
    }
}