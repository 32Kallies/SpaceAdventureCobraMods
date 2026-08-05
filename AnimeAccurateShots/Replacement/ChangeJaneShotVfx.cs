using System;
using AnimeAccurateShots.Data;
using HarmonyLib;
using UnityEngine;

namespace AnimeAccurateShots.Replacement;

// EARLY JANE
[HarmonyPatch]
public static class ChangeJaneShotVfxForSniping
{
    private const float MinZPosForSuperheatedShots = 299;
    private const float MaxZPosForSuperheatedShots = 305;

    private static float _timeCanUpdateAgain;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiBossJane), nameof(NmiBossJane.shootManage))]
    private static void UpdateShotPatch(NmiBossJane __instance)
    {
        if (Time.time < _timeCanUpdateAgain) return;
        _timeCanUpdateAgain = Time.time + 0.1f;
        
        bool isSuperHeated = IsSuperHeated();
        Color color = isSuperHeated ? new Color(1, 0, 1) : new Color(0, 0.9f, 1);
        if (isSuperHeated)
        {
            ProjectileEditUtils.RecolorShotVFX(__instance.projectilePrefab, ShotReplacementColors.GetJaneShotColors(),
                false);   
        }
        else
        {
            ProjectileEditUtils.RecolorShotVFX(__instance.projectilePrefab, ShotReplacementColors.GetJaneColdShotColors(),
                false);
        }
        foreach (var ps in __instance.projectilePrefab.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.startColor = color;
            if (ps.gameObject.name.Equals("Fire ShockWave", StringComparison.OrdinalIgnoreCase))
            {
                ps.GetComponent<Renderer>().material.SetColor("_Brillant_core_color", color);
            }
        }
    }

    private static bool IsSuperHeated()
    {
        var level = LevelController.Instance;
        if (level == null) return false;
        if (level.level != LevelController.Level.EP05_LVL02_MinecartMadness) return false;

        var z = CobraCharacter.Instance.transform.position.z;
        return z > MinZPosForSuperheatedShots && z < MaxZPosForSuperheatedShots;
    }
}

// BOSS FIGHT
[HarmonyPatch]
public static class ChangeJaneShotVfxForBossFight
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiBossJaneV2), nameof(NmiBossJaneV2.Start))]
    private static void StartPatch(NmiBossJaneV2 __instance)
    {
        ProjectileEditUtils.RecolorShotVFX(__instance.projectilePrefab, ShotReplacementColors.GetJaneShotColors(),
            false);
        var superShot = __instance.rapidProjectilePrefab;
        ProjectileEditUtils.RecolorShotVFX(superShot, ShotReplacementColors.GetJaneShotColors(), false);
        var superShotParticles = superShot.transform.Find("vfx_JaneBigProjectile");
        foreach (var ps in superShotParticles.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.startColor = new Color(3, 0.4f, 3);
        }

        superShotParticles.Find("missileShockwaveContinuous_03").gameObject.SetActive(false);
        superShotParticles.Find("vfx_Electricity_RedWarning").gameObject.SetActive(false);
        var dogWheelMain = superShot.transform.Find("vfx_Jane_MuzzleFlash/vfx_DogWheel_TurnSmoke")
            .GetComponent<ParticleSystem>().main;
        dogWheelMain.startColor = new Color(1, 0, 1);
    }
}