using UnityEngine;

namespace PsychogunImproved;

public class AnimateChargedShot : MonoBehaviour
{
    private static readonly Color Blue = new(0f, 0.2f, 5.4f);
    private static readonly Color Yellow = new(1.274f, 1.5f, 0.445f);
    private static readonly Color Red = new(3.674f, 0.1f, 0.145f);

    public Renderer renderer;
    
    private float _startTime;
    private Color _defaultColor;
    private Material _material;
    
    private static readonly Frame[] Animation =
    [
        new(0f, Yellow),
        new(0.05f, Blue),
        new(0.11f, Red),
        Frame.GetDefault(0.17f)
    ];

    private static readonly int ColorProperty = Shader.PropertyToID("Color_D9B06DDF");

    private void Start()
    {
        _startTime = Time.time;
        _material = renderer.material;
        _defaultColor = _material.GetColor(ColorProperty);
    }

    private void Update()
    {
        _material.SetColor(ColorProperty, GetColorForFrame(GetCurrentFrame()));
    }

    private Color GetColorForFrame(Frame frame)
    {
        if (frame.UseDefault) return _defaultColor;
        return frame.Color;
    }

    private Frame GetCurrentFrame()
    {
        var time = Time.time - _startTime;
        for (int i = Animation.Length - 1; i >= 0; i--)
        {
            if (time >= Animation[i].Time)
            {
                return Animation[i];
            }
        }

        return default;
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }

    private struct Frame
    {
        public float Time { get; }
        public Color Color { get; }
        public bool UseDefault { get; }

        public Frame(float time, Color color) : this(time, color, false)
        {
        }

        private Frame(float time, Color color, bool useDefault)
        {
            Time = time;
            Color = color;
            UseDefault = useDefault;
        }
        
        public static Frame GetDefault(float time) => new(time, Color.white, true);
    }
}