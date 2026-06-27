using System.IO;
using MusicReplacer.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels.Triggers;

public class ScreenshotPreview : MonoBehaviour
{
    public RawImage image;

    private Texture2D _lastLoaded;

    public void UpdateScreenshot(string levelName, int triggerHash)
    {
        var path = ScreenshotGenerator.GetScreenshotFilePath(levelName, triggerHash);
        var exists = File.Exists(path);
        
        if (exists)
        {
            _lastLoaded = LoadPNG(path);
            image.texture = _lastLoaded;
        }
        else
        {
            Destroy(_lastLoaded);
        }

        image.enabled = exists;
    }
    
    private static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath)) 	{
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}