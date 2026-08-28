using System;
using System.Collections.Generic;
using UnityEngine;

namespace PsychogunImproved.ChargeVfxModification;

public class ChargeVfxModifier : MonoBehaviour
{
    private static readonly int InnerColor = Shader.PropertyToID("_InnerColor");
    private static readonly int OuterColor = Shader.PropertyToID("_OuterColor");
    
    /*
    private static readonly Color Blue = new(0.5f, 0.9f, 1f);
    private static readonly Color Yellow = new(1f, 1f, 0.5f);
    private static readonly Color Red = new(1, 0.4f, 0.5f);
    */
    
    private static readonly Color White = new(1f, 1f, 1f);
    private static readonly Color Blue = new(0.8f, 1f, 1f);
    private static readonly Color Yellow = new(1f, 0.8f, 0.6f);
    private static readonly Color Red = new(1, 0.6f, 0.6f);
    
    private ParticleSystem[] _systems;
    private List<Material> _laserHitFloorMaterials;

    private Gradient _gradient;
    private float _animationDuration = 0.25f;
    private float _startTime;

    private void Start()
    {
        _startTime = Time.time;
        _systems = GetComponentsInChildren<ParticleSystem>(true);
        CollectLaserHitFloorMaterials();
        WhitenParticleSystems();
        InitializeGradient();
    }
    
    private void Update()
    {
        UpdateColorsFromAnimation();
    }

    private void UpdateColorsFromAnimation()
    {
        var t = (Time.time - _startTime) / _animationDuration % 1f;
        var color = _gradient.Evaluate(t);
        foreach (var system in _systems)
        {
            if (system == null) continue;
            var main = system.main;
            main.startColor = color;
        }

        foreach (var material in _laserHitFloorMaterials)
        {
            if (material == null) continue;
            material.SetColor(InnerColor, color);
            material.SetColor(OuterColor, color);
        }
    }

    private void InitializeGradient()
    {
        _gradient = new Gradient
        {
            colorKeys =
            [
                new GradientColorKey(Yellow, 0 / 3f),
                new GradientColorKey(White, 1 / 6f),
                new GradientColorKey(Blue, 2 / 6f),
                new GradientColorKey(White, 0.45f),
                new GradientColorKey(Red, 0.55f),
                new GradientColorKey(White, 4 / 6f),
                new GradientColorKey(Yellow, 5 / 6f),
                new GradientColorKey(Yellow, 1)
            ]
        };
    }

    private void CollectLaserHitFloorMaterials()
    {
        _laserHitFloorMaterials = new List<Material>();
        foreach (var system in _systems)
        {
            if (system.name.Contains("LaserHitYellowFloor", StringComparison.OrdinalIgnoreCase))
            {
                _laserHitFloorMaterials.Add(system.GetComponent<Renderer>().material);
            }
        }
    }

    private void WhitenParticleSystems()
    {
        foreach (var system in _systems)
        {
            var colorOverLifetime = system.colorOverLifetime;
            if (colorOverLifetime.enabled)
            {
                var color = colorOverLifetime.color;
                if (color.mode == ParticleSystemGradientMode.Gradient)
                {
                    var gradient = color.gradientMax;
                    var colorKeys = new GradientColorKey[gradient.colorKeys.Length];
                    for (int i = 0; i < colorKeys.Length; i++)
                    {
                        colorKeys[i] = new GradientColorKey(Color.white, gradient.colorKeys[i].time);
                    }

                    var newGradient = new Gradient()
                    {
                        colorKeys = colorKeys,
                        alphaKeys = gradient.alphaKeys,
                        colorSpace = gradient.colorSpace,
                        mode = gradient.mode
                    };
                    color.gradient = newGradient;
                    color.gradientMax = newGradient;
                    colorOverLifetime.color = color;
                }
            }
            var main = system.main;
            main.startColor = Color.white;
        }

        foreach (var material in _laserHitFloorMaterials)
        {
            material.SetColor(InnerColor, Color.white);
            material.SetColor(OuterColor, new Color(1, 1, 0.8f));
        }
    }

    private void OnDestroy()
    {
        if (_laserHitFloorMaterials != null)
        {
            foreach (var material in _laserHitFloorMaterials)
            {
                Destroy(material);
            }
        }
    }
}