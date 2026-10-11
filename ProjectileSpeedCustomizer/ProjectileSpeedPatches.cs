using HarmonyLib;
using UnityEngine;

namespace ProjectileSpeedCustomizer;

[HarmonyPatch]
internal static class ProjectileSpeedPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.ApplyExternalMove))]
    private static void SetSpeedPostfix(Projectile __instance, ref Vector3 delta)
    {
        if (__instance is GuidedProjectile)
        {
            delta *= Plugin.GuidedShot.Value;
            return;
        }

        if (__instance.damageType is Damage.DamageType.NmiProjectile)
        {
            if (__instance.gameObject.name.Contains("BossJaneNoZExagerration"))
            {
                delta *= Plugin.JaneBackgroundShot.Value;
            }
            delta *= Plugin.EnemyProjectiles.Value;
            return;
        }

        switch (__instance.damageType)
        {
            case Damage.DamageType.Revolver:
                delta *= Plugin.RevolverProjectile.Value;
                return;
            case Damage.DamageType.Psychogun:
                delta *= Plugin.PsychogunProjectile.Value;
                return;
            case Damage.DamageType.ChargedPsychogun:
                delta *= Plugin.ChargedProjectile.Value;
                return;
            default:
                delta *= Plugin.OtherProjectiles.Value;
                return;
        }
    }
    
    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.SetSpeed))]
    private static void SetSpeedPrefix(Projectile __instance, ref float spd)
    {
        spd *= Plugin.ProjectileSpeedTest.Value;
    }
    */
}