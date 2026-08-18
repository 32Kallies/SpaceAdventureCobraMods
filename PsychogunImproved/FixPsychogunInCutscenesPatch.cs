using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace PsychogunImproved;

[HarmonyPatch]
public static class FixPsychogunInCutscenesPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CutscenePlayer), nameof(CutscenePlayer.Start))]
    private static void PatchCutscenePlayerStart(CutscenePlayer __instance)
    {
        var director = __instance.GetComponent<PlayableDirector>();
        if (director == null) return;
        var asset = director.playableAsset;
        if (asset == null) return;

        try
        {
            PatchCutscenePlayer(director, asset.name);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while patching Psychogun for cutscene: " + e);
        }
    }

    private static void PatchCutscenePlayer(PlayableDirector director, string assetName)
    {
        // Patch 2-3 zigoba cutscene
        if (string.Equals(assetName, "timeline_C2_3_2_Zig_Intro_sequence_V2", StringComparison.OrdinalIgnoreCase))
        {
            Transform projectileParent = director.transform.Find("GRP_VFX/PsychogunProjectile");
            
            ReplacePsychogunVfxWithChargedShot(projectileParent);
            
            Plugin.Logger.LogInfo("Patched psychogun for 2-3 Zigoba cutscene");
        }
    }

    private static void ReplacePsychogunVfxWithChargedShot(Transform projectileParent)
    {
        CobraCharacter cobra = CobraCharacter.Instance;

        HideAllInChildren(projectileParent.gameObject);

        var chargedShot = Object.Instantiate(cobra.dependencies.chargedShot, projectileParent);
        chargedShot.GetComponent<Projectile>().enabled = false;
        chargedShot.GetComponent<Rigidbody>().isKinematic = true;
        chargedShot.transform.localPosition = Vector3.zero;
        chargedShot.transform.localEulerAngles = Vector3.zero;
        chargedShot.transform.localScale = Vector3.zero;

        var mainVfx = chargedShot.transform.Find("vfx_charge_trail_00");
        if (mainVfx)
        {
            Object.DestroyImmediate(mainVfx.GetComponent<AutoMoveToParticle>());
            mainVfx.transform.Find("vfx_ps_flipbook_00").transform.localScale = Vector3.one * 0.70f;
        }
        else
        {
            Plugin.Logger.LogError("Could not locate 'vfx_charge_trail_00' child of charged Psychogun shot");
        }

        foreach (ParticleSystem ps in chargedShot.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.None;
        }
            
        // chargedShot.GetComponent<Renderer>().enabled = false;
        // chargedShot.transform.Find("trail").gameObject.SetActive(false);
    }

    private static void HideAllInChildren(GameObject parent)
    {
        foreach (Renderer renderer in parent.GetComponentsInChildren<Renderer>())
            renderer.enabled = false;
        foreach (Light light in parent.GetComponentsInChildren<Light>())
            light.enabled = false;
    }
}