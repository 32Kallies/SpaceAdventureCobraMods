using HarmonyLib;

namespace PsychogunImproved.Patches;

// patches various base-game objects to allow them to be damageable by uncharged shots as well
[HarmonyPatch]
internal static class UnchargedShotDamagePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Start))]
    private static void Destructible_StartPostfix(Destructible __instance)
    {
        if (!Plugin.UnchargedShotsImitateChargedShots.Value)
            return;

        if ((__instance.destructibleBy & Damage.DamageTypeFlag.ChargedPsychogun) != 0)
            __instance.destructibleBy |= Damage.DamageTypeFlag.Psychogun;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiMomie), nameof(NmiMomie.Start))]
    private static void NmiMomie_StartPostfix(NmiMomie __instance)
    {
        if (!Plugin.UnchargedShotsImitateChargedShots.Value)
            return;

        if ((__instance.common.damageableBy & Damage.DamageTypeFlag.ChargedPsychogun) != 0)
            __instance.common.damageableBy |= Damage.DamageTypeFlag.Psychogun;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiTique), nameof(NmiTique.Start))]
    private static void NmiTique_StartPostfix(NmiTique __instance)
    {
        if (!Plugin.UnchargedShotsImitateChargedShots.Value)
            return;

        if ((__instance.common.damageableBy & Damage.DamageTypeFlag.ChargedPsychogun) != 0)
            __instance.common.damageableBy |= Damage.DamageTypeFlag.Psychogun;
    }
}