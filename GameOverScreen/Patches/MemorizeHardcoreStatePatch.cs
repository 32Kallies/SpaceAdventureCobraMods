using HarmonyLib;

namespace GameOverScreen.Patches;

[HarmonyPatch]
public static class MemorizeHardcoreStatePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    public static void StartPostfix(LevelController __instance)
    {
        if (HardcoreStateMemorizer.Instance != null)
        {
            Plugin.Logger.LogWarning("Warning: multiple hardcore state memorizers in scene!");
        }
        __instance.gameObject.AddComponent<HardcoreStateMemorizer>().isHardcore = Plugin.HardcoreConfig.Value;
    }
}