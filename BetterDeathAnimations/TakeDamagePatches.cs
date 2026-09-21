using HarmonyLib;

namespace BetterDeathAnimations;

[HarmonyPatch]
public static class TakeDamagePatches
{
    [HarmonyPatch(typeof(NmiPatrouille), nameof(NmiPatrouille.TakeDamage))]
    [HarmonyPostfix]
    private static void PatchPatrouilleDisintegration(NmiPatrouille __instance, float dmg, Damage.DamageType dmgType)
    {
        UpdateDamageTag(__instance, dmg, dmgType);
    }
    
    [HarmonyPatch(typeof(NmiAdvance), nameof(NmiAdvance.TakeDamage))]
    [HarmonyPostfix]
    private static void PatchNmiSwordDisintegration(NmiAdvance __instance, float dmg, Damage.DamageType dmgType)
    {
        if (__instance is NmiSword)
            UpdateDamageTag(__instance, dmg, dmgType);
    }

    private static void UpdateDamageTag(NmiAdvance enemy, float dmg, Damage.DamageType dmgType)
    {
        if (dmg <= 0f) return;
        DisableDisintegrationTag tag;
        if (dmgType is Damage.DamageType.Revolver or Damage.DamageType.Melee)
        {
            // Disable disintegration for revolver and melee attacks
            if (!enemy.gameObject.TryGetComponent<DisableDisintegrationTag>(out tag))
                tag = enemy.gameObject.AddComponent<DisableDisintegrationTag>();
            tag.disableDisintegration = true;
        }
        else
        {
            // Re-enable disintegration for other damage types
            if (enemy.gameObject.TryGetComponent<DisableDisintegrationTag>(out tag))
                tag.disableDisintegration = false;
        }
    }
}