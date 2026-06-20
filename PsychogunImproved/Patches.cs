using System.IO;
using HarmonyLib;
using UnityEngine;

namespace PsychogunImproved;

[HarmonyPatch]
public static class Patches
{
    private static bool _upgradedPsychogunShotDirty = false;

    private static string _chargedPsychogunPrefabName;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    public static void StartPostfix(CobraCharacter __instance)
    {
        var behaviour = __instance.gameObject.AddComponent<ImprovedPsychogunBehaviour>();

        var armAnimation = __instance.gameObject.AddComponent<RuntimeAdditiveAnimation>();
        armAnimation.LoadFromJSON(
            Path.Combine(Path.GetDirectoryName(Plugin.ModAssembly.Location),
                "RecoilAnimation.json"),
            __instance.transform);
        armAnimation.transitionDuration = 0.1f;
        
        behaviour.animation = armAnimation;

        _chargedPsychogunPrefabName = __instance.dependencies.chargedShot.name;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ShootPsychogun))]
    public static bool AlwaysShootChargedShotPatch(CobraCharacter __instance, Vector2 dir)
    {
        var behaviour = __instance.gameObject.GetComponent<ImprovedPsychogunBehaviour>();
        if (behaviour == null)
        {
            Plugin.Logger.LogWarning("Failed to find ImprovedPsychogunBehaviour!");
            return true;
        }

        if (behaviour.GetCanShoot())
        {
            __instance.ShootChargedShot(dir);
            behaviour.StartShotCooldown();
            _upgradedPsychogunShotDirty = true;
        }

        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ShootChargedShot))]
    public static void PlayShootRecoilAnimation(CobraCharacter __instance)
    {
        var behaviour = __instance.gameObject.GetComponent<ImprovedPsychogunBehaviour>();
        if (behaviour == null)
        {
            Plugin.Logger.LogWarning("Failed to find ImprovedPsychogunBehaviour!");
            return;
        }

        behaviour.PlayShootAnimation();
        _upgradedPsychogunShotDirty = false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ParticleController), nameof(ParticleController.Generate), typeof(GameObject),
        typeof(GameObject), typeof(bool), typeof(bool), typeof(GameObject), typeof(string))]
    public static void FullyChargedShotParticlePatch(GameObject __result, GameObject particle)
    {
        if (_upgradedPsychogunShotDirty || !particle.name.Equals(_chargedPsychogunPrefabName))
        {
            return;
        }

        Plugin.Logger.LogInfo("Patching charged shot particle for fully charged shot");

        try
        {
            __result.GetComponent<MeshRenderer>().enabled = false;
            
            var projectile = __result.GetComponent<Projectile>();
            projectile.defaultReaction = Damage.Reaction.PassThrough;

            const float radiusMultiplier = 6f;
            
            projectile.speed = 60;
            projectile.overlapSphereRadiusAtInit *= radiusMultiplier;
            projectile.capsuleCastRadiusAtInitAndUpdate *= radiusMultiplier;
            
            
            var collider = __result.GetComponent<SphereCollider>();
            collider.radius *= radiusMultiplier;
            
            __result.transform.Find("trail").GetComponent<TrailRenderer>().enabled = false;
            __result.transform.Find("Point Light").GetComponent<Light>().enabled = false;
            
            var flipbook = __result.transform.Find("vfx_charge_trail_00/vfx_ps_flipbook_00");
            var particleSystem = flipbook.GetComponent<ParticleSystem>();
            var shape = particleSystem.shape;
            shape.position = new Vector3(0f, 0f, 10.5f);
            var main = particleSystem.main;
            main.startSizeXMultiplier = 22; // original value is 6
            main.startSizeYMultiplier = 4;
            // this animation will apply to all
            flipbook.gameObject.AddComponent<AnimateChargedShot>().renderer = particleSystem.GetComponent<Renderer>();
            
            var longFlipbook = Object.Instantiate(flipbook, flipbook.transform.parent, true);
            var longParticleSystem = longFlipbook.GetComponent<ParticleSystem>();
            var longShape = longParticleSystem.shape;
            longShape.position = new Vector3(0f, 0f, 21f);
            var longMain = longParticleSystem.main;
            longMain.startSizeXMultiplier = 40;
            longMain.startSizeYMultiplier = 4;
            var longLight = longFlipbook.GetComponentInChildren<Light>();
            if (longLight) longLight.enabled = false;
            
            var wideFlipbook = Object.Instantiate(flipbook, flipbook.transform.parent, true);
            var wideParticleSystem = wideFlipbook.GetComponent<ParticleSystem>();
            var wideShape = wideParticleSystem.shape;
            wideShape.position = new Vector3(0f, 0f, 2f);
            var wideMain = wideParticleSystem.main;
            wideMain.startSizeXMultiplier = 5; // original value is 6
            wideMain.startSizeYMultiplier = 6.3f; // original value is 4
            var wideLight = wideFlipbook.GetComponentInChildren<Light>();
            if (wideLight) wideLight.enabled = false;

            flipbook.GetComponent<ParticleSystemRenderer>().maxParticleSize = 4;
            longFlipbook.GetComponent<ParticleSystemRenderer>().maxParticleSize = 5;
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while patching fully charged shot particle: " + e);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ShootSub))]
    public static void CleanUpDirtyCheck(CobraCharacter __instance)
    {
        _upgradedPsychogunShotDirty = false;
    }
}