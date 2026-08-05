using HarmonyLib;
using UnityEngine;

namespace AnimeAccurateShots.Replacement;

[HarmonyPatch]
public static class ChangeJaneIntroCutsceneShotVfx
{
    private static readonly int ColorProperty = Shader.PropertyToID("Color_D9B06DDF");
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CutscenePlayer), nameof(CutscenePlayer.StartPlaying))]
    private static void Patch(CutscenePlayer __instance)
    {
        if (__instance.m_PlayableDirector == null) return;
        var playableAsset = __instance.m_PlayableDirector.playableAsset;
        if (playableAsset == null) return;
        if (!playableAsset.name.Equals("timeline_C5_2_1_Jane_Intro_sequence")) return;
        
        // PATCH JANE INTRO SEQUENCE CUTSCENE
        var projectileParent = __instance.transform.Find("GRP_VFX/GRP_VFX_PsychogunProjectile");
        foreach (var ps in projectileParent.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.startColor = new Color(1, 0.1f, 1);
        }

        // trail renderers
        foreach (var trail in projectileParent.GetComponentsInChildren<TrailRenderer>(true))
        {
            trail.startColor = Color.magenta;
            trail.endColor = Color.magenta;
        }

        // lights
        foreach (var light in projectileParent.GetComponentsInChildren<Light>())
        {
            light.color = new Color(1, 0.2f, 1);
        }

        // all renderers
        foreach (var renderer in projectileParent.GetComponentsInChildren<Renderer>(true))
        {
            var material = renderer.material;
            if (material == null) continue;
            if (material.HasProperty(ColorProperty))
            {
                material.SetColor(ColorProperty, Color.magenta);
            }
        }
        
        // particle trails
        foreach (var psRenderer in projectileParent.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var material = psRenderer.trailMaterial;
            if (material == null) continue;
            if (material.HasProperty(ColorProperty))
            {
                material.SetColor(ColorProperty, new Color(30, 2, 29));
            }
        }
    }
}