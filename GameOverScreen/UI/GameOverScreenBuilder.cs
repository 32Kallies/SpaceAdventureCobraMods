using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameOverScreen.UI;

public static class GameOverScreenBuilder
{
    public static void ShowScreen()
    {
        if (GameOverScreen.Instance != null)
            return;

        var canvas = Object.Instantiate(Plugin.Bundle.LoadAsset<GameObject>("GameOverCanvas"));
        canvas.AddComponent<FadeInCanvasGroup>().group = canvas.GetComponent<CanvasGroup>();

        var gameOverScreen = canvas.AddComponent<GameOverScreen>();
        
        var font = GetFont();
        if (font == null)
            return;
        foreach (var text in canvas.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
        }

        var buttonsParent = canvas.transform.Find("Background/Options").GetComponent<RectTransform>();
        gameOverScreen.restartCheckpointButton = AddButton(buttonsParent, "RESTART CHECKPOINT", GameOverScreenActions.RestartFromCheckpoint);
        gameOverScreen.restartStageButton = AddButton(buttonsParent, "RESTART STAGE", GameOverScreenActions.RestartStage);
        gameOverScreen.exitToMenuButton = AddButton(buttonsParent, "EXIT TO MENU", GameOverScreenActions.QuitToMainMenu);
    }

    private static NUIButton AddButton(RectTransform parent, string text, Action action)
    {
        var buttonPrefab = UIController.Instance.uiPausePanelPrefab.GetComponent<NUIPausePanel>()
            .mainButtons[0]
            .gameObject;
        
        var newButtonRect = Object.Instantiate(buttonPrefab, parent).GetComponent<RectTransform>();
        newButtonRect.localPosition = Vector3.zero;
        newButtonRect.localScale = Vector3.one;
        newButtonRect.localEulerAngles = Vector3.zero;
        newButtonRect.sizeDelta = new Vector2(512, 60);
        var textComponent = newButtonRect.GetComponentInChildren<Text>();
        Object.DestroyImmediate(textComponent.GetComponent<TextLocalize>());
        textComponent.text = text;
        
        textComponent.resizeTextMaxSize = textComponent.fontSize;
        textComponent.resizeTextForBestFit = true;


        var custom = newButtonRect.gameObject.AddComponent<CustomButton>();
        custom.OnButtonPressed = action;
        
        return newButtonRect.GetComponent<NUIButton>();
    }

    private static Font GetFont()
    {
        var controller = UIController.Instance;
        if (controller == null)
        {
            Plugin.Logger.LogError("UIController not found");
            return null;
        }

        var presentationPanel = controller.uiLevelPresentationPanelPrefab;
        if (presentationPanel == null)
        {
            Plugin.Logger.LogError("Presentation panel prefab not found");
            return null;
        }

        var planetNameObject = presentationPanel.transform.Find("resume container/planet name");
        if (planetNameObject == null)
        {
            Plugin.Logger.LogError("Planet name text not found");
            return null;
        }

        var text = planetNameObject.GetComponent<Text>();
        if (text == null)
        {
            Plugin.Logger.LogError("Text not found");
            return null;
        }

        return text.font;
    }
}