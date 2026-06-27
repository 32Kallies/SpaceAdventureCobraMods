using System.IO;
using MusicReplacer.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels.Triggers;

public class ScreenshotPreview : MonoBehaviour
{
    public RawImage image;

    private Texture2D _lastLoaded;

    public void UpdateScreenshot(string filePath)
    {
        bool exists = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        
        if (exists)
        {
            _lastLoaded = LoadPNG(filePath);
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
            tex.LoadImage(fileData);
        }
        return tex;
    }
}