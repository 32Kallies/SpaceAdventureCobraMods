using System;
using HarmonyLib;
using UnityEngine;

namespace VisibleHolsteredRevolverOn1_1Cobra;

[HarmonyPatch]
public static class FixRevolverWhenHolsteredPatch
{
    [HarmonyPrefix] // prefix so this executes before other mods (specifically NoRevolverHolster)
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    private static void CobraShowRevolverInHolsterPatch(CobraCharacter __instance)
    {
        if (__instance.skinId != GameController.COBRASKIN.DELON) return;
        
        try
        {
            Plugin.Logger.LogInfo("Patching 1-1 cobra");
            __instance.transform.Find("msh_chr_Cobra_PreOp/RIG/MESHES/CobraHolster/msh_CobraRevolverHip_00")
                .GetComponent<Renderer>().enabled = true;
            Plugin.Logger.LogInfo("Patched 1-1 cobra successfully");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while attempting to show revolver: " + e);
        }
    }
}