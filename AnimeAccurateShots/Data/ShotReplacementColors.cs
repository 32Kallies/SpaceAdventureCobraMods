using UnityEngine;

namespace AnimeAccurateShots.Data;

public class ShotReplacementColors(
    Color mainColor,
    Color trailColorStart,
    Color trailColorEnd,
    Color trailColorBase,
    Color pointLightColor)
{
    public Color MainColor { get; } = mainColor;
    public Color TrailColorStart { get; } = trailColorStart;
    public Color TrailColorEnd { get; } = trailColorEnd;
    public Color TrailColorBase { get; } = trailColorBase;
    public Color PointLightColor { get; } = pointLightColor;
    public MuzzleFlashColors MuzzleFlashColors { get; set; }

    public static ShotReplacementColors GetOrangeShotColors()
    {
        return new ShotReplacementColors(new Color(3.03f, 1.07f, 0.18f), new Color(0.988f, 1, 0),
            new Color(0.984f, 1, 0), new Color(5.99f, 0.76f, 0), new Color(0.996f, 0.361f, 0, 1));
    }
    
    public static ShotReplacementColors GetBlueShotColors()
    {
        return new ShotReplacementColors(new Color(0.19f, 0.64f, 3.03f), new Color(0, 0.596f, 1),
            new Color(0, 0.596f, 1), new Color(0, 0.60f, 5.99f), new Color(0, 0.596f, 1));
    }
    
    public static ShotReplacementColors GetCrystalBoyShotColors()
    {
        return new ShotReplacementColors(new Color(0f, 3f, 2f), new Color(0.11f, 0.48f, 0.19f),
            new Color(0.11f, 0.5f, 0.17f, 0f), new Color(0, 5.70f, 5.99f), new Color(0, 0.796f, 0.9f))
        {
            MuzzleFlashColors = new MuzzleFlashColors()
            {
                SparkColor = HexUtils.GetColorFromHexCode("#0053FF"),
                SphereColor = HexUtils.GetColorFromHexCode("#4EE662FF"),
                MainSpikesColor = HexUtils.GetColorFromHexCode("#009D93FF"),
                SubSpikesColor = HexUtils.GetColorFromHexCode("#00FF8AFF"),
                Sphere01Color = HexUtils.GetColorFromHexCode("#9DFFC4FF"),
                RevolverImpactColor = new Color(0, 0.5f, 1000)
            }
        };
    }
    
    public static ShotReplacementColors GetJaneShotColors()
    {
        return new ShotReplacementColors(new Color(3, 1, 3), new Color(1, 0.23f, 0.38f),
            new Color(1, 0.23f, 0.38f), new Color(3, 1, 3), new Color(1f, 0.296f, 1));
    }
    
    public static ShotReplacementColors GetJaneColdShotColors()
    {
        return new ShotReplacementColors(new Color(0.19f, 0.64f, 3.03f), new Color(0, 0.596f, 1),
            new Color(0, 0.596f, 1), new Color(0, 0.60f, 5.99f), new Color(0, 0.596f, 1));
    }
}