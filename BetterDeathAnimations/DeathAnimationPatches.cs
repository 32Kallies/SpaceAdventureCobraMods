using HarmonyLib;
using UnityEngine;

namespace BetterDeathAnimations;

[HarmonyPatch]
public static class DeathAnimationPatches
{
    private static bool DEBUG = false;
    
    [HarmonyPatch(typeof(NmiPatrouille), nameof(NmiPatrouille.ManageDead))]
    [HarmonyPrefix]
    private static void ReplacePatrouilleDeathAnim(NmiPatrouille __instance, ref bool __result)
    {
        if (!__instance.isDead())
        {
            // Just run original like normal because why not
            return;
        }
        
        // If we ARE dead...
        
        if (!ShouldPlayNewDeathAnimation(__instance))
            return;

        // Make sure this patch will only execute once per npc, the moment death starts
        // technically deathState will still be 0 at this moment, so isDeathStarting will still be false
        // so we only execute if that is so 0
        if (__instance.deathState != 0) return;
        
        if (IsAirborne(__instance)) return;
        
        // If we are dying for the first time
        
        // we can't call base.ManageDead so this basic logic from NmiBasic will suffice
        __instance.hideCharging();
        __instance.hideStun();

        // NEW logic
        
        // Based on NmiPatrouille.ManageDead but there is only one path here (as it only happens on first death)
        __instance.destroyShadowWithDelay(LevelController.Instance.nmiSettings.disappearDelay);
        if (__instance.common.vfxDestroy != null)
        {
            Vector3 vfxDestroyPos = __instance.mainGun == NmiPatrouille.eMainGun.GRENADE
                ? __instance.myBoneHips.position
                : __instance.GetCenter();
            ParticleController.Generate(null, __instance.common.vfxDestroy, vfxDestroyPos);
        }

        __instance.killHandHeldGrenade();
        __instance.lanceFlamme?.gameObject.SetActive(value: false);
        // __instance.common.appearPrefab.ForceEventDeath();
        float disappearDelay = LevelController.Instance.nmiSettings.disappearDelay;
        __instance.bouclier.disappearDelay = disappearDelay;
        __instance.bouclier.isDisplayed = false;
        // __instance.myAnimator.SetFloat(NmiBasic.hashDEATHSPEED, 0.25f);
        __instance.myAnimator.SetFloat(NmiBasic.hashDEATHSPEED, 1f); // new line
        __instance.characterController.excludeLayers = -1;
        if (!__instance.common.isHitFront)
        {
            __instance.transform.forward = -__instance.transform.forward;
            __instance.common.isHitFront = true;
        }

        __instance.myAnimator.SetFloat(NmiBasic.hashFRONTBACK, __instance.common.isHitFront ? 0 : 1);
        __instance.theGoodDeathAnimation = !__instance.myAnimator.GetBool(NmiBasic.hashNEUTRALIZED)
            ? NmiAdvance.eState.SWORD_FINISHING_COBRA
            : NmiAdvance.eState.SHOOTER_DEAD_FINISH;
        
        // __instance.ctrl.setAnim(__instance.theGoodDeathAnimation);
        PlayNewDeathAnimation(__instance);
        
        // back to original logic...
        if (__instance.mainGun == NmiPatrouille.eMainGun.MINIGUN)
        {
            __instance.minigunMaterialController.value = 0f;
            __instance.environmentOscillation.transformCoef = 0f;
            __instance.myAnimator.SetLayerWeight(__instance.animLayerShoot, 0f);
        }

        __result = true;
        
        // we died now, and do this to make sure the original death code doesn't execute again
        __instance.deathState = 2;
    }

    [HarmonyPatch(typeof(NmiSword), nameof(NmiSword.ManageDead))]
    [HarmonyPrefix]
    private static void ReplaceSwordEnemyDeathAnim(NmiSword __instance, ref bool __result)
    {
        if (!__instance.isDead())
        {
            // Just run original like normal because why not
            return;
        }
        
        // If we ARE dead...

        if (!ShouldPlayNewDeathAnimation(__instance))
            return;

        // Make sure this patch will only execute once per npc, the moment death starts
        // technically deathState will still be 0 at this moment, so isDeathStarting will still be false
        // so we only execute if that is so 0
        if (__instance.deathState != 0) return;

        if (IsAirborne(__instance)) return;
        
        // If we are dying for the first time
        
        // we can't call base.ManageDead so this basic logic from NmiBasic will suffice
        __instance.hideCharging();
        __instance.hideStun();

        // NEW logic

        __instance.destroyShadowWithDelay(LevelController.Instance.nmiSettings.disappearDelay);
        if (__instance.common.vfxDestroy != null)
        {
            ParticleController.Generate(null, __instance.common.vfxDestroy, __instance.transform.position);
        }

        // __instance.common.appearPrefab.ForceEventDeath();
        float disappearDelay = LevelController.Instance.nmiSettings.disappearDelay;
        __instance.bouclier.disappearDelay = disappearDelay;
        __instance.bouclier.isDisplayed = false;
        __instance.myAnimator.SetFloat(NmiBasic.hashDEATHSPEED, 0.25f);
        __instance.CleanAllWaitingProjectiles();
        if (!__instance.common.isHitFront)
        {
            __instance.transform.forward = -__instance.transform.forward;
            __instance.common.isHitFront = true;
        }
        
        __instance.characterController.excludeLayers = -1;
        // replace death animation
        // __instance.ctrl.setAnim(__instance.common.isHitFront ? NmiAdvance.eState.SWORD_DEAD_FRONT : NmiAdvance.eState.SNIPER_DEAD);
        PlayNewDeathAnimation(__instance);
        
        __result = true;
        
        // we died now, and do this to make sure the original death code doesn't execute again
        __instance.deathState = 2;
    }

    private static bool ShouldPlayNewDeathAnimation(NmiAdvance enemy)
    {
        // Only run this patch if disintegration has been disabled by the TakeDamagePatches 
        if (Plugin.AlwaysPlayNewDeathAnimation.Value) return true;

        if (enemy.TryGetComponent<DisableDisintegrationTag>(out var disintegration) &&
            disintegration.disableDisintegration) return true;
        
        // Disable disintegration randomly based on the setting
        int skipDisintegrationChance = Plugin.ChanceForDisintegrationFail.Value;
        if (skipDisintegrationChance == 0 || Random.value * 100 < skipDisintegrationChance)
        {
            return false;
        }

        return true;
    }

    private static void PlayNewDeathAnimation(NmiAdvance enemy)
    {
        if (enemy.characterController != null)
            enemy.characterController.enabled = false;
        else
            Plugin.Logger.LogWarning($"No character controller associated with {enemy}; cannot disable the hitbox");
        enemy.ctrl.myAnimator.runtimeAnimatorController =
            Plugin.Bundle.LoadAsset<RuntimeAnimatorController>("CustomDeathController");
        Vector3 knockbackLandPosition = enemy.transform.position + enemy.transform.forward * -2;
        if (CheckForSafeLanding.HasGroundAtPoint(knockbackLandPosition, 0.4f, 0.3f, 2, 8, 0.2f))
        {
            enemy.ctrl.myAnimator.SetBool("knockback", true);
            foreach (var skinned in enemy.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var bounds = skinned.bounds;
                bounds.Encapsulate(new Bounds(knockbackLandPosition, new Vector3(2, 2, 3)));
                skinned.bounds = bounds;
            }
        }

        if (DEBUG)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cube.GetComponent<MeshRenderer>().material = null;
            cube.transform.localScale = new Vector3(0.8f, 0.04f, 0.8f);
            cube.transform.position = knockbackLandPosition;
            cube.GetComponent<Collider>().enabled = false;
        }
    }

    private static bool IsAirborne(NmiAdvance enemy)
    {
        return !CheckForSafeLanding.HasGroundAtPoint(enemy.transform.position, 0.25f, 0.5f,
            2, 6, 0.6f);
    }
}