using GameOverScreen.UI;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;

namespace GameOverScreen.Patches;

[HarmonyPatch]
public static class RankSPlusPatches
{
    private const string SPlusRankText = "S+";
    
    // COMPUTE RANK PATCH
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIScorePanel), nameof(NUIScorePanel.ComputeRank))]
    public static void ComputeRankPatch(LevelController.Level currentLevel, ref int __result)
    {
        if (Utilities.IsDifficultySpaceCobra())
        {
            int hitsReceived = TokenController.GetTokenValue(Token.HardCodedTokens.NbHitReceived);
            if (hitsReceived <= 0)
            {
                __result = Utilities.RankSPlusConstant;
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIScorePanel), nameof(NUIScorePanel.GetRankText))]
    public static void HubScreenPatch(int _rank, ref string __result)
    {
        if (_rank == Utilities.RankSPlusConstant)
        {
            __result = SPlusRankText;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIScorePanel), nameof(NUIScorePanel.GetRankText))]
    public static void RankTextPatch(int _rank, ref string __result)
    {
        if (_rank == Utilities.RankSPlusConstant)
        {
            __result = SPlusRankText;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIScorePanel), nameof(NUIScorePanel.GetRankColor))]
    public static void RankColorPatch(int _rank, ref Color __result)
    {
        if (_rank == Utilities.RankSPlusConstant)
        {
            __result = new Color(1, 0, 0.29f);
        }
    }

    // At end of level: adds the colorizer if needed AND updates the text to use the best fit if needed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIScorePanel), nameof(NUIScorePanel.RefreshRankDisplay))]
    public static void UpdateEndOfLevelRankText(NUIScorePanel __instance, LevelController.Level currentLevel)
    {
        int rank = NUIScorePanel.ComputeRank(currentLevel);
        UpdateRankTextAppearance(__instance.rankText, rank);
        UpdateRankTextAppearance(__instance.detailsRankText, rank);
    }

    // In episode selector: adds the colorizer if needed AND updates the text to use the best fit if needed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUILevelSelectionBox), nameof(NUILevelSelectionBox.Update))]
    public static void UpdateEpisodeSelectionRankText(NUILevelSelectionBox __instance)
    {
        int rank = NUIScorePanel.GetRank(__instance.m_Level, -1);
        UpdateRankTextAppearance(__instance.rankText, rank);
    }

    // In SPECIAL episode selector: adds the colorizer if needed AND updates the text to use the best fit if needed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUISpecialLevelSelectionBox), nameof(NUISpecialLevelSelectionBox.Update))]
    public static void UpdateSpecialEpisodeSelectionRankText(NUISpecialLevelSelectionBox __instance)
    {
        int rank = NUIScorePanel.GetRank(__instance.m_Level, -1);
        UpdateRankTextAppearance(__instance.rankText, rank);
    }

    // In pause menu: adds the colorizer if needed AND updates the text to use the best fit if needed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIPausePanel), nameof(NUIPausePanel.Awake))]
    public static void UpdatePauseMenuRankText(NUIPausePanel __instance)
    {
        if (__instance.m_IsMultiLevel) return;
        if (__instance.rankText != null)
        {
            int rank = NUIScorePanel.GetRank();
            UpdateRankTextAppearance(__instance.rankText, rank);
        }
    }

    private static void UpdateRankTextAppearance(Text text, int rank)
    {
        if (rank == Utilities.RankSPlusConstant)
        {
            if (text.GetComponent<SPlusRankColorizer>() != null) return;
            var colorizer = text.gameObject.AddComponent<SPlusRankColorizer>();
            colorizer.text = text;
            text.resizeTextMaxSize = text.fontSize;
            text.resizeTextForBestFit = true;
        }
        else
        {
            Object.Destroy(text.GetComponent<SPlusRankColorizer>());
        }
    }
}