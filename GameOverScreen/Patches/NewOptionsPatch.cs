using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GameOverScreen.Patches;

[HarmonyPatch]
public static class NewOptionsPatch
{
    private static int _gameplayButtonIndex = -1;
    
    private static int HardcoreConfigValue => Plugin.HardcoreConfig.Value ? 1 : 0;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NUISettingsPanel), nameof(NUISettingsPanel.Awake))]
    public static void AddButton(NUISettingsPanel __instance)
    {
        Plugin.Logger.LogMessage("Patching settings with new hardcore mode option");
        
        // add new gameplay option object
        var gameplaySettingsParent = __instance.transform.Find("settings/gameplay");
        var hardcoreSetting = Object.Instantiate(gameplaySettingsParent.Find("inserts").gameObject, gameplaySettingsParent);
        
        // set up text
        hardcoreSetting.name = "hardcore";
        var textTransform = hardcoreSetting.transform.Find("text");
        Object.DestroyImmediate(textTransform.GetComponentInChildren<TextLocalize>());
        textTransform.GetComponent<Text>().text = "HARDCORE MODE";

        var button = hardcoreSetting.GetComponent<NUIButton>();
        _gameplayButtonIndex = __instance.gameplayButtons.Length;
        __instance.gameplayButtons = __instance.gameplayButtons.Add(button);
        
        hardcoreSetting.SetActive(true);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NUISettingsPanel), nameof(NUISettingsPanel.Update))]
    public static void UpdateCobraDifficultySettingName(NUISettingsPanel __instance)
    {
        if (__instance.containersParents[2].gameObject.activeSelf)
            UpdateSpaceCobraDifficultySettingName(__instance.m_GameplayVariables);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUISettingsPanel.GameplayVariables), nameof(NUISettingsPanel.GameplayVariables.InitialiseVariables))]
    public static void InitializeVariables(NUISettingsPanel.GameplayVariables __instance)
    {
        if (_gameplayButtonIndex == -1)
        {
            Plugin.Logger.LogError("Failed to find valid gameplay button index");
            return;
        }
        __instance.m_Buttons[_gameplayButtonIndex].Initialise((NUIButton.TYPE)(-1));
        __instance.m_Buttons[_gameplayButtonIndex].InitialiseData(0, 1, ["TXT_GENERIC_NO", "TXT_GENERIC_YES"]);
        __instance.m_Buttons[_gameplayButtonIndex].SetDataValue(HardcoreConfigValue);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUISettingsPanel.GameplayVariables), nameof(NUISettingsPanel.GameplayVariables.ValidateVariables))]
    public static void ValidateVariables(NUISettingsPanel.GameplayVariables __instance, bool _validate)
    {
        if (_gameplayButtonIndex == -1)
        {
            Plugin.Logger.LogError("Failed to find valid gameplay button index");
            return;
        }

        if (_validate)
        {
            Plugin.HardcoreConfig.Value = __instance.m_Buttons[_gameplayButtonIndex].GetDataValue() == 1;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUISettingsPanel.GameplayVariables),
        nameof(NUISettingsPanel.GameplayVariables.HasVariablesChanged))]
    public static void PatchHasVariablesChanged(NUISettingsPanel.GameplayVariables __instance, ref bool __result)
    {
        if (__result == true) return;
        __result = HardcoreConfigValue != __instance.m_Buttons[_gameplayButtonIndex].GetDataValue();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUISettingsPanel.GameplayVariables),
        nameof(NUISettingsPanel.GameplayVariables.FrameUpdate))]
    public static void PatchFrameUpdate(NUISettingsPanel.GameplayVariables __instance)
    {
        __instance.m_Buttons[_gameplayButtonIndex].SetInactive(IsInLevel());
        UpdateSpaceCobraDifficultySettingName(__instance);
    }

    private static void UpdateSpaceCobraDifficultySettingName(NUISettingsPanel.GameplayVariables instance)
    {
        bool spaceCobraDifficulty = instance.m_Buttons[_gameplayButtonIndex].GetDataValue() == 1
                                    && instance.m_Buttons[0].GetDataValue() == 2;
        foreach (var localize in instance.m_Buttons[0].dataTextsLocalizes)
        {
            if (spaceCobraDifficulty)
            {
                var text = localize.m_text;
                if (text == null) text = localize.GetComponent<Text>();
                text.text = "<color=#fca730>SPACE COBRA</color>";
            }
            else if (localize.m_text.text.StartsWith("<"))
            {
                // REFRESH TEXT
                instance.m_Buttons[0].SetDataValue(instance.m_Buttons[0].GetDataValue());
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUISettingsPanel.GameplayVariables),
        nameof(NUISettingsPanel.GameplayVariables.SetButtonsDataValue))]
    public static void PatchSetButtonsDataValue(NUISettingsPanel.GameplayVariables __instance)
    {
        __instance.m_Buttons[_gameplayButtonIndex].SetDataValue(HardcoreConfigValue);
    }

    private static bool IsInLevel()
    {
        return LevelController.Instance != null;
    }
}