using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GameOverScreen.Patches;

[HarmonyPatch]
public class SpaceCobraDifficultyPatch
{
    private const float DamageMultiplierForInstantDeath = 99999;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.OverrideDamage))]
    public static void DoOneHitInSpaceCobraMode(ref float dmg)
    {
        if (dmg <= Mathf.Epsilon) return;
        if (LevelController.Instance == null)
        {
            return;
        }

        if (Utilities.IsDifficultySpaceCobra())
        {
            dmg *= DamageMultiplierForInstantDeath;
        }
    }
    
    // Updates the text in the details section at the end of a level
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIScorePanel), nameof(NUIScorePanel.RefreshRankDisplay))]
    public static void UpdateEndOfLevelRankText(NUIScorePanel __instance, LevelController.Level currentLevel)
    {
        if (Utilities.IsDifficultySpaceCobra())
        {
            var text = __instance.detailsDifficultyText;
            if (text == null)
            {
                Plugin.Logger.LogError("Failed to find difficulty text localization");
                return;
            }

            text.gameObject.AddComponent<TextUpdater>();
        }
    }

    private class TextUpdater : MonoBehaviour
    {
        private Text _text;
        private void Awake() => _text = GetComponent<Text>();

        private void Update()
        {
            if (_text == null)
            {
                _text = GetComponent<Text>();
            }

            _text.text = "Space Cobra";
        }
    }
}