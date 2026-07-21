using System;
using HarmonyLib;
using UnityEngine;

namespace CobraVisualFixes;

[HarmonyPatch]
public static class Patches
{
    private static readonly int FloatMetalness = Shader.PropertyToID("_Float_metalness");
    private static readonly int AlbedoTextureProperty = Shader.PropertyToID("_Texture2D_alb");
    private const string CobraMaterialName = "mat_chr_Cobra_00";

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
        // Fix hair, belt, and revolver
        foreach (var material in cobra.defaultMats)
        {
            if (material != null && material.name.Equals(CobraMaterialName, StringComparison.OrdinalIgnoreCase))
            {
                material.SetFloat(FloatMetalness, 0);
                if (Plugin.NewTexture != null)
                {
                    material.SetTexture(AlbedoTextureProperty, Plugin.NewTexture);
                }
                else
                {
                    Plugin.Logger.LogError("Failed to find new cobra texture!");
                }

                return;
            }
        }

        Plugin.Logger.LogWarning("Material not found!");
    }
}