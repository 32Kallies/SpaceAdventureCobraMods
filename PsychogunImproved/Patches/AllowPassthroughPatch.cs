using HarmonyLib;
using JetBrains.Annotations;

namespace PsychogunImproved.Patches;

[HarmonyPatch(typeof(NmiBasic), nameof(NmiBasic.TakeDamage))]
internal static class AllowPassthroughPatchNmiBasic
{
    [UsedImplicitly]
    private static void Postfix(Damage.DamageType dmgType, ref Damage.Reaction __result)
    {
        if (PassthroughPatchUtils.TryOverrideDamageReactionResult(dmgType, out var reaction))
        {
            __result = reaction;
        }
    }
}

[HarmonyPatch(typeof(NmiMobile), nameof(NmiMobile.TakeDamage))]
internal static class AllowPassthroughPatchNmiMobile
{
    [UsedImplicitly]
    private static void Postfix(Damage.DamageType dmgType, ref Damage.Reaction __result)
    {
        if (PassthroughPatchUtils.TryOverrideDamageReactionResult(dmgType, out var reaction))
        {
            __result = reaction;
        }
    }
}

internal static class PassthroughPatchUtils
{
    public static bool TryOverrideDamageReactionResult(Damage.DamageType damageType, out Damage.Reaction reaction)
    {
        if (damageType != Damage.DamageType.Psychogun || !Plugin.UnchargedShotsPassThroughEnemies.Value)
        {
            reaction = default;
            return false;
        }

        reaction = Damage.Reaction.PassThrough;
        return true;
    }
}