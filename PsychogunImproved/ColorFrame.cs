using UnityEngine;

namespace PsychogunImproved;

public struct ColorFrame
{
    public float Time { get; }
    public Color Color { get; }
    public bool UseDefault { get; }

    public ColorFrame(float time, Color color) : this(time, color, false)
    {
    }

    private ColorFrame(float time, Color color, bool useDefault)
    {
        Time = time;
        Color = color;
        UseDefault = useDefault;
    }
        
    public static ColorFrame GetDefault(float time) => new(time, Color.white, true);
}