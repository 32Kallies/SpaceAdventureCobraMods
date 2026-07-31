using HarmonyLib;
using UnityEngine;

namespace GameOverScreen.Patches;

[HarmonyPatch]
public static class CobraDeathToCrystalBoyPatches
{
    private static bool _killedCobra;
    private static float _timeJustKilledCobra;
    private static float _secondsAfterDeathToWatch = 5.5f; // arbitrary magic number based on that long animation

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiCrystalBowie), nameof(NmiCrystalBowie.KillCobraIfNear))]
    private static void PatchNormalCrystalBoy(ref bool __result)
    {
        if (__result) OnCrystalBoyKilledCobra();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiCrystalBowieClone), nameof(NmiCrystalBowieClone.KillCobraIfNear))]
    private static void PatchCloneCrystalBoy(ref bool __result)
    {
        if (__result) OnCrystalBoyKilledCobra();
    }

    private static void OnCrystalBoyKilledCobra()
    {
        _killedCobra = true;
        _timeJustKilledCobra = Time.time;
    }

    public static bool WasCobraJustKilledByCrystalBoy()
    {
        if (!_killedCobra) return false;
        return Time.time < _timeJustKilledCobra + _secondsAfterDeathToWatch;
    }
}