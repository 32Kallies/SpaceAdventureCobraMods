using HarmonyLib;
using MusicReplacer.CustomTriggers;
using MusicReplacer.MusicReplacementMenu;
using MusicReplacer.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.LevelMusic;

[HarmonyPatch]
public static class TriggerQueryPatcher
{
    private static GameObject _currentText;

    private const float Duration = 5;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Update))]
    public static void UpdatePostfix(CobraCharacter __instance)
    {
        if (!Input.GetKeyDown(KeyCode.F5)) return;
        
        Display(__instance.transform);
    }

    public static void Display(Transform cobraTransform)
    {
        if (_currentText != null)
        {
            Object.Destroy(_currentText);
        }
        
        var text = GetCoordinatesText();
        
        // Trigger
        var triggerId = GetTriggerIdOrZero(cobraTransform);
        if (triggerId != 0)
        {
            text += "\n<color=#fab514>Closest trigger</color>: ";
            text += triggerId;
            Plugin.Logger.LogMessage("Closest trigger ID: " + triggerId);
            TryTakeScreenshot(triggerId);
        }

        // Arena
        var arenaId = GetArenaIdOrZero();
        if (arenaId != 0)
        {
            text += "\n<color=#eb367b>Current arena ID</color>: ";
            text += arenaId;
            Plugin.Logger.LogMessage("Current arena ID: " + arenaId);
            TryTakeScreenshot(arenaId);
        }
        
        DisplayText(text);
    }

    private static void TryTakeScreenshot(int triggerId)
    {
        if (CobraVideoPlayer.isPlayingVideo)
            return;
        if (Time.timeScale == 0)
            return;
        ScreenshotGenerator.GenerateScreenshotForTrigger(triggerId);
    }

    private static int GetTriggerIdOrZero(Transform playerTransform)
    {
        var colliders = Physics.OverlapSphere(playerTransform.position, 1f, -1, QueryTriggerInteraction.Collide);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<audioForceMusicTrigger>(out _) && collider.GetComponent<CustomTriggerTag>() == null)
            {
                var dimensions = TriggerUtils.GetColliderDimensions(collider.gameObject);
                var hash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
                return hash;
            }
        }
        
        return 0;
    }
    
    private static int GetArenaIdOrZero()
    {
        if (!LevelController.Instance.IsInArena())
            return 0;
        
        var arena = LevelController.Instance.GetCurrentArena();
        
        if (arena == null)
            return 0;
        
        return arena.arenaID;
    }

    private static string GetCoordinatesText()
    {
        var coordinates = CobraCharacter.Instance.transform.position;
        return string.Format(
            "<color=#fab514>Coordinates: </color> ({0}, {1}, {2})",
            Mathf.RoundToInt(coordinates.x).ToString(),
            Mathf.RoundToInt(coordinates.y).ToString(),
            Mathf.RoundToInt(coordinates.z).ToString());
    }

    private static void DisplayText(string message)
    {
        GameObject canvasObj = new GameObject("TriggerPopupCanvas");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        GameObject textObj = new GameObject("TriggerPopupText");
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(canvasObj.transform, false);

        var text = textObj.AddComponent<Text>();

        text.text = message;
        text.fontSize = 48;
        text.font = MusicMenuBuilder.ButtonFont;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, -1f);
        
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);

        textRect.anchoredPosition = new Vector2(20f, -20f);

        textRect.sizeDelta = new Vector2(1000f, 300f);
        
        Object.Destroy(canvasObj, Duration);

        _currentText = canvasObj;
    }
}