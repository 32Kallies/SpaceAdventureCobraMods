using UnityEngine;

namespace AnimeAccurateShots;

public static class HexUtils
{
    public static Color GetColorFromHexCode(string hex)
    {
        if (!ColorUtility.DoTryParseHtmlColor(hex, out Color32 color))
        {
            color = Color.magenta; 
            Plugin.Logger.LogError("Failed to parse hex color: " + hex);
        }

        return color;
    }
}