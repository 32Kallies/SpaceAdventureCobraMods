using UnityEngine;
using UnityEngine.UI;

namespace GameOverScreen.UI;

public class SPlusRankColorizer : MonoBehaviour
{
    public Text text;
    public float animationDuration = 1.6f;
    
    private static readonly Gradient Gradient = new()
    {
        colorKeys =
        [
            new(ParseHex("#FF3220FF"), 0.05f), // red
            new(ParseHex("#1DC510FF"), 0.24f), // green
            new(ParseHex("#1DC510FF"), 0.38f), // green
            new(ParseHex("#443ABDFF"), 0.6f), // blue
            new(ParseHex("#443ABDFF"), 0.73f), // blue
            new(ParseHex("#FF3220FF"), 0.89f) // red
        ]
    };

    private void Update()
    {
        var t = Time.realtimeSinceStartup / animationDuration % 1f;
        text.color = Gradient.Evaluate(t);
    }

    private static Color ParseHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
        {
            return color;
        }
        Plugin.Logger.LogWarning("Invalid hex: " );
        return Color.black;
    }
}