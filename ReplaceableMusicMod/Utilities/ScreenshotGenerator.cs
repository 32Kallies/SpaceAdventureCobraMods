using System.Collections;
using System.IO;
using UnityEngine;

namespace MusicReplacer.Utilities;

public static class ScreenshotGenerator
{
    public static void GenerateScreenshotWithDelay(int triggerId, float delay)
    {
        Plugin.StartCoroutineOnPlugin(GenerateScreenshotWithDelayCoroutine(triggerId, delay));
    }
    
    private static IEnumerator GenerateScreenshotWithDelayCoroutine(int triggerId, float delay)
    {
        yield return new WaitForSeconds(delay);
        GenerateScreenshotForTrigger(triggerId);
    }
    
    public static void GenerateScreenshotForTrigger(int triggerId)
    {
        var level = GameController.Instance.GetCurrentLevelDefinition().levelName;
        
        var path = GetScreenshotFilePath(level, triggerId);
        
        if (File.Exists(path))
            return;
        
        ScreenCapture.CaptureScreenshot(path);
    }

    public static string GetScreenshotFilePath(string levelName, int triggerId)
    {
        return Path.Combine(FileManagement.GetScreenshotsFolder(), GetScreenshotFileName(levelName, triggerId));
    }
    
    private static string GetScreenshotFileName(string levelName, int triggerId)
    {
        return $"{levelName}_{triggerId}.png";
    }
}