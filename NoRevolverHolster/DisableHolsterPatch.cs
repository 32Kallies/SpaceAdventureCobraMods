using System;
using HarmonyLib;
using UnityEngine;

namespace NoRevolverHolster;

[HarmonyPatch]
public static class DisableHolsterPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    public static void SetHolsterActivePatch(CobraCharacter __instance)
    {
        if (Plugin.DisableRevolverHolster.Value)
        {
            TrySetHolsterActive(__instance.transform, false);
        }
    }
    
    // Copied from my other mod, called JohnsonJacket
    private static void TrySetHolsterActive(Transform cobra, bool active)
    {
        GameObject holster;
        Renderer revolverRenderer;
        try
        {
            Transform holsterRoot = cobra.GetChild(0).Find("RIG/MESHES/CobraHolster");
            holster = holsterRoot.Find("msh_chr_CobraHolster").gameObject;
            revolverRenderer = holsterRoot.Find("msh_CobraRevolverHip_00").GetComponent<Renderer>();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while finding holster on Cobra: " + e);
            return;
        }

        holster.SetActive(active);

        if (revolverRenderer != null)
        {
            revolverRenderer.enabled = active;
        }
        else
        {
            Plugin.Logger.LogError("Holstered revolver renderer not found");
        }
        
        // This is what I change:
        // [cobra]/msh_chr_Cobra_0X/RIG/MESHES/CobraHolster/msh_chr_CobraHolster : GameObject
        // [cobra]/msh_chr_Cobra_0X/RIG/MESHES/CobraHolster/msh_CobraRevolverHip_00 : RendererComponent
    }
}