using HarmonyLib;

namespace SilenceStackedShotSounds;

[HarmonyPatch]
public static class CobraPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ShootSub))]
    private static void MuteAfterShoot(CobraCharacter __instance, bool __result)
    {
        if (__result
            || __instance.shotType == Damage.DamageType.ChargedPsychogun
            || __instance.shotType == Damage.DamageType.SuperPsychogun)
        {
            ShotSilencer.QuietenSounds();
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    private static void OnCobraInit()
    {
        ShotSilencer.Initialize();
    }
}