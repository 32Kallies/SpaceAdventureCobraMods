using HarmonyLib;
using JetBrains.Annotations;

namespace AnimeAccurateShots.Replacement;

[HarmonyPatch(typeof(ParticleController), nameof(ParticleController.InitPools))]
public static class PatchProjectilePoolInitialization
{
    [UsedImplicitly]
    private static void Prefix(ParticleController __instance)
    {
        if (__instance.pools == null)
        {
            Plugin.Logger.LogError("Pools not initialized yet");
            return;
        }
        
        foreach (var pool in __instance.pools)
        {
            if (pool.prefab == null)
            {
                continue;
            }
            ProjectileEditUtils.TryPatchProjectile(pool.prefab);
        }
    }
}