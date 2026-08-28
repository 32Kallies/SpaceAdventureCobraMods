using HarmonyLib;

namespace PsychogunImproved.ChargeVfxModification;

[HarmonyPatch]
public static class PatchChargeVfx
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    private static void EditChargedShot(CobraCharacter __instance)
    {
        var prefab = __instance.dependencies.chargedShotChargedVFX;
        
        if (prefab.GetComponent<ChargeVfxModifier>() == null)
            prefab.AddComponent<ChargeVfxModifier>();
    }
}