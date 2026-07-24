using System;
using HarmonyLib;
using UnityEngine;

namespace JohnsonJacket;

[HarmonyPatch]
public static class Patches
{
    private const string VideoNameToReturnSkin = "CS2D_V_1_2_2";

    // Prevent cobra's revolver from shooting during the first part of level 1-2
    // if his skin is VISUALLY the jacket skin
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ShootRevolver))]
    public static bool DisableRevolverShootingAtBeginningOfEp1Lvl2(CobraCharacter __instance)
    {
        if (!IsOnEpisode1Level2())
        {
            return true; // run original, revolver always works
        }
        
        // Revolver always works when not using jacket skin
        if (__instance.skinId != GameController.COBRASKIN.COBRA_DLC_JACKET)
        {
            return true;
        }

        // run original if Cobra has visited his house, otherwise, cobra cannot shoot the revolver
        return CobraHouseCutsceneRememberer.HasCobraVisitedHouse();
    }

    // Forces Cobra to use the white jacket and hides his revolver during the first part of level 1-2
    // only doing anything before entering his apartment/house
    // Only runs if using 1) the default red skin or 2) white jacket skin, as applied in settings
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    public static void ForceJacketCobraSkinOnEp1Lvl2(CobraCharacter __instance)
    {
        // Check we are on the right level
        if (!IsOnEpisode1Level2())
            return;
        
        // Code beyond here only executes on level 1-2


        // Remove the Revolver holster if Cobra's house has NOT been visited while using the jacket
        if (__instance.skinId == GameController.COBRASKIN.COBRA_DLC_JACKET && !CobraHouseCutsceneRememberer.HasCobraVisitedHouse())
        {
            TrySetHolsterActive(__instance.transform, false);
        }
      
        GameController.COBRASKIN currentSkin = LoadSaveController.Instance.PreferencesData.currentCobraSkin;

        // Skip if the normal skin isn't being used
        if (currentSkin != GameController.COBRASKIN.COBRA)
            return;

        // Skip if we already visited Cobra's house/apartment
        if (CobraHouseCutsceneRememberer.HasCobraVisitedHouse())
        {
            return;
        }
        
        // Track Ep1 lvl2 starting (not very useful at the moment)
        CobraHouseCutsceneRememberer.OnEp1Lvl2Started();

        // Use the jacket skin
        Plugin.Logger.LogInfo("Forcing Cobra to use jacket skin");
        TokenController.SetTokenValue(Token.HardCodedTokens.CobraSkin, (int)GameController.COBRASKIN.COBRA_DLC_JACKET);
    }

    // Switches Cobra back to his default skin after going through the house/apartment cutscene
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraVideoPlayer), nameof(CobraVideoPlayer.Stop))]
    public static void TakeOffJacketPatch(CobraVideoPlayer __instance)
    {
        if (!__instance.videoName.Equals(VideoNameToReturnSkin, StringComparison.OrdinalIgnoreCase))
            return;
        
        GameController.COBRASKIN currentSkin = LoadSaveController.Instance.PreferencesData.currentCobraSkin;
        
        // Only works for default Cobra skin and Jacket skin
        if (currentSkin != GameController.COBRASKIN.COBRA &&
            currentSkin != GameController.COBRASKIN.COBRA_DLC_JACKET) return;
        
        Plugin.Logger.LogInfo("Forcing Cobra to use default skin");
        CobraHouseCutsceneRememberer.OnCutsceneCompleted();
        TokenController.SetTokenValue(Token.HardCodedTokens.CobraSkin, (int)GameController.COBRASKIN.COBRA);
    }

    private static bool IsOnEpisode1Level2()
    {
        var levelController = LevelController.Instance;
        if (levelController == null) return false;
        return levelController.level == LevelController.Level.EP01_LVL02_Casino_BossVaiken;
    }

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