using System;
using AnimeAccurateShots.Data;
using UnityEngine;

namespace AnimeAccurateShots.Replacement;

public static class ProjectileEditUtils
{
    private static readonly int ColorD9B06Ddf = Shader.PropertyToID("Color_D9B06DDF");

    public static void TryPatchProjectile(GameObject projectilePrefab)
    {
        if (projectilePrefab.name.Equals("NmiProjectile_Drone", StringComparison.OrdinalIgnoreCase))
        {
            RecolorShotVFX(projectilePrefab, ShotReplacementColors.GetOrangeShotColors(), true);
        }
    }
    
    public static void RecolorShotVFX(GameObject projectile, ShotReplacementColors colors, bool shared)
    {
        if (shared && projectile.GetComponent<EditedProjectile>() != null)
        {
            return;
        }
        
        Renderer mainRenderer = projectile.GetComponent<Renderer>();
        Light pointLight = projectile.GetComponentInChildren<Light>();
        TrailRenderer trailRenderer = projectile.GetComponentInChildren<TrailRenderer>();

        if (mainRenderer != null)
        {
            Material toEdit = shared ? mainRenderer.sharedMaterial : mainRenderer.material;
            toEdit.SetColor(ColorD9B06Ddf, colors.MainColor);
        }

        if (pointLight != null)
        {
            pointLight.color = colors.PointLightColor;
        }
        
        if (trailRenderer != null)
        {
            Material toEdit = shared ? trailRenderer.sharedMaterial : trailRenderer.material;
            toEdit.SetColor(ColorD9B06Ddf, colors.TrailColorBase);
            trailRenderer.startColor = colors.TrailColorStart;
            trailRenderer.endColor = colors.TrailColorEnd;
        }

        if (shared == true)
        {
            projectile.AddComponent<EditedProjectile>();
        }

        MuzzleFlashColors muzzle = colors.MuzzleFlashColors;
        
        if (muzzle != null)
        {
            RecolorParticleSystem(projectile, "vfx_smoke_gun_spark_00", muzzle.SparkColor);
            RecolorParticleSystem(projectile, "vfx_smoke_gun_spark_00/Sphere", muzzle.SphereColor);
            RecolorParticleSystem(projectile, "vfx_smoke_gun_spark_00/Sphere/Main Spikes", muzzle.MainSpikesColor);
            RecolorParticleSystem(projectile, "vfx_smoke_gun_spark_00/Sphere/Sub Spikes", muzzle.SubSpikesColor);
            RecolorParticleSystem(projectile, "vfx_smoke_gun_spark_00/Sphere_01", muzzle.Sphere01Color);
            RecolorParticleSystem(projectile, "vfx_smoke_gun_spark_00/vfx_RevolverHitImpact", muzzle.RevolverImpactColor);
        }
    }

    private static void RecolorParticleSystem(GameObject projectile, string path, Color color)
    {
        var main = projectile.transform.Find(path).GetComponent<ParticleSystem>().main;
        main.startColor = color;
    }
}