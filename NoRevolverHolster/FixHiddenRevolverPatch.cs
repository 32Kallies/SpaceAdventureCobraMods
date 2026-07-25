using HarmonyLib;

namespace NoRevolverHolster;

[HarmonyPatch]
public static class FixHiddenRevolverPatch
{
    private static float _putRevolverAwayTimer = 1.4f;
    private static bool _shotRevolver;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    public static void StartPostfix(CobraCharacter __instance)
    {
        _shotRevolver = false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ShootRevolver))]
    public static void ShootPostfix(CobraCharacter __instance)
    {
        if (__instance.shootSub && __instance.shotType == Damage.DamageType.Revolver)
            _shotRevolver = true;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.LateUpdate))]
    public static void FixRevolverVisibility(CobraCharacter __instance)
    {
        if (!_shotRevolver)
            return;

        bool showShootingRevolver = __instance.timeSinceLastRevolverShot < _putRevolverAwayTimer;

        if (!showShootingRevolver)
        {
            _shotRevolver = false;
        }
        
        __instance.SetActive(__instance.dependencies.pistolHip, !showShootingRevolver);
        __instance.SetActive(__instance.dependencies.pistolHand,
            showShootingRevolver && (__instance.GetActualSubstate() != CobraCharacter.Substate.Sliding || !__instance.dependencies.amICobra));
        __instance.SetActive(__instance.dependencies.pistolHandL,
            showShootingRevolver && __instance.GetActualSubstate() == CobraCharacter.Substate.Sliding && __instance.dependencies.amICobra);
    }
}