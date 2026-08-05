using AnimeAccurateShots.Data;
using HarmonyLib;
using UnityEngine;

namespace AnimeAccurateShots.Replacement;

[HarmonyPatch]
public static class ChangeCrystalBoyShotVfx
{
    private const float Scale = 1f / 3f;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiCrystalBowie), nameof(NmiCrystalBowie.Start))]
    private static void StartPatch(NmiCrystalBowie __instance)
    {
        GameObject firstProjectile = __instance.commonParams.projectile;
        PatchProjectile(firstProjectile);
        GameObject secondProjectile = __instance.phase5Params.projectile;
        PatchProjectile(secondProjectile);
    }

    private static void PatchProjectile(GameObject projectile)
    {
        ProjectileEditUtils.RecolorShotVFX(projectile, ShotReplacementColors.GetCrystalBoyShotColors(), false);
        projectile.transform.localScale = new Vector3(0.15f, 0.15f, 0.32f) * Scale;
        projectile.GetComponent<SphereCollider>().radius = 0.1f * 3f;
        TrailRenderer trail = projectile.GetComponentInChildren<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.08f;
    }
}