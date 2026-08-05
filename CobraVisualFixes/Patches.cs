using System;
using HarmonyLib;
using UnityEngine;

namespace CobraVisualFixes;

[HarmonyPatch]
public static class Patches
{
    private static readonly int FloatMetalness = Shader.PropertyToID("_Float_metalness");
    private static readonly int AlbedoTextureProperty = Shader.PropertyToID("_Texture2D_alb");
    private static readonly int SubsurfaceScatteringTextureProperty = Shader.PropertyToID("_Texture2D_sss");
    private const string CobraMaterialName = "mat_chr_Cobra_00";
    private const string CobraBlackSlidingRevolverName = "mat_chr_Cobra_vc";
    private const string CobraBlackSlidingRevolverNameForLvl1 = "mat_CobraVc";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    public static void ReplaceTexturePatch(CobraCharacter __instance)
    {
        try
        {
            FixPsychogunOnOtherSkins(__instance);
            ReplaceMainTexture(__instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while fixing Cobra visuals: " + e);
        }
    }

    private static void FixPsychogunOnOtherSkins(CobraCharacter cobra)
    {
        if (cobra.skinId is not (GameController.COBRASKIN.COBRA_DLC_JACKET
            or GameController.COBRASKIN.COBRA_DLC_SMOKING)) return;

        // GetChild(0) is going to be "msh_chr_Cobra_02" or "msh_chr_Cobra_03"
        Renderer psychogun = cobra.transform.GetChild(0).Find("RIG/MESHES/msh_chr_CobraPsycho")
            .GetComponent<Renderer>();
        var material = new Material(psychogun.sharedMaterial);
        material.SetInt("_AlphaClip", 0);
        material.SetFloat("_Vector1_fresnelPower", 1f);
        material.SetInt("_Boolean_UseFresnel", 1);
        material.SetFloat("_Float_addLight", 1f);
        material.SetFloat(FloatMetalness, 1f);
        material.SetFloat("_SpawnSpace", 1f);
        material.SetVector("_Color_fresnelColor", new Vector4(0.31f, 0.41f, 0.69f, 0f));
        psychogun.sharedMaterial = material;
        var indexOfPsychogunRenderer = cobra.dependencies.allRenderers.IndexOf(psychogun);
        cobra.defaultMats[indexOfPsychogunRenderer] = material;

        Plugin.Logger.LogInfo($"Replaced '{material}' psychogun material on '{psychogun}'");
    }

    private static void ReplaceMainTexture(CobraCharacter cobra)
    {
        bool found = false;

        Shader mainShader = null;
        
        // Fix hair, belt, and revolver
        foreach (var material in cobra.defaultMats)
        {
            if (material == null)
            {
                continue;
            }

            if (material.name.Equals("mat_chr_CobraDelon_00", StringComparison.OrdinalIgnoreCase))
            {
                mainShader = material.shader;
            }
            
            if (material.name.Equals(CobraMaterialName, StringComparison.OrdinalIgnoreCase))
            {
                found = true;
                FixCobraMaterial(material);
            }

            if (material.name.Equals(CobraBlackSlidingRevolverName, StringComparison.OrdinalIgnoreCase))
            {
                material.SetTexture(AlbedoTextureProperty, Plugin.SlidingRevolverTexture);
                material.SetFloat("_Vector1_fresnelPower", 0);
                material.SetFloat("_Vector1_fresnelSpread", 1.39f);
                material.SetFloat("_Float_addLight", 0.286f);
                material.SetFloat("_Float_shadow", 0.8f);
                material.SetFloat("_Vector1_directLight", 0);
                material.SetFloat("_use_vertex_color", 0);
            }

            if (material.name.Equals(CobraBlackSlidingRevolverNameForLvl1, StringComparison.OrdinalIgnoreCase))
            {
                if (mainShader != null)
                {
                    material.shader = mainShader;
                }
                else
                {
                    Plugin.Logger.LogWarning("Main shader not found for 1-1 cobra");
                }
                
                Plugin.Logger.LogInfo("Patching 1-1 cobra visuals");
                
                material.SetTexture(AlbedoTextureProperty, Plugin.SlidingRevolverTexture);
                material.SetTexture("_BaseMap", Plugin.SlidingRevolverTexture);
                material.SetTexture("_Texture2D_sss", Plugin.SlidingRevolverSSS);
                material.SetFloat("_Vector1_fresnelPower", 0);
                material.SetFloat("_Vector1_fresnelSpread", 1.39f);
                material.SetFloat("_Float_addLight", 0.286f);
                material.SetFloat("_Float_shadow", 0.8f);
                material.SetFloat("_Vector1_directLight", 0);
                material.SetFloat("_use_vertex_color", 0);
                material.SetFloat("_QueueControl", 0);
                // darken revolver and subsurface scattering
                material.SetColor("_Color_alb", new Color(0.7f, 0.6f, 0.65f));
                material.SetColor("_Color_sss", new Color(0.6f, 0.7f, 0.6f));
                
                found = true;
            }
        }

        if (!found)
            Plugin.Logger.LogWarning("Material not found!");

        Plugin.Logger.LogInfo("Improved cobra materials");
    }

    private static void FixCobraMaterial(Material material)
    {
        material.SetFloat(FloatMetalness, 0);

        if (Plugin.NewTexture != null)
        {
            material.SetTexture(AlbedoTextureProperty, Plugin.NewTexture);
            material.mainTexture = Plugin.NewTexture;
        }
        else
        {
            Plugin.Logger.LogError("Failed to find new cobra color texture!");
        }

        if (Plugin.NewSubsurfaceScatteringTexture != null)
        {
            material.SetTexture(SubsurfaceScatteringTextureProperty, Plugin.NewSubsurfaceScatteringTexture);
        }
        else
        {
            Plugin.Logger.LogError("Failed to find new cobra SSS texture!");
        }
    }
}