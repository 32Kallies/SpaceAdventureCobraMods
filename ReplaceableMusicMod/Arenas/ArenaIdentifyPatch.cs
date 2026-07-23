using HarmonyLib;

namespace MusicReplacer.Arenas;

[HarmonyPatch]
public static class ArenaIdentifyPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiArena), nameof(NmiArena.Init))]
    public static void InitPostfix(NmiArena __instance)
    {
        if (__instance.GetComponent<ArenaIdentifier>() != null) return;
        __instance.gameObject.AddComponent<ArenaIdentifier>().SetId(ArenaHashGenerator.Compute(__instance.gameObject));
    }
}