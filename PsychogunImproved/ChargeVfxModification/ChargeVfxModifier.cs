using System;
using System.Collections.Generic;
using UnityEngine;

namespace PsychogunImproved.ChargeVfxModification;

public class ChargeVfxModifier : MonoBehaviour
{
    private static readonly int InnerColor = Shader.PropertyToID("_InnerColor");
    private static readonly int OuterColor = Shader.PropertyToID("_OuterColor");
    
    private static readonly Color White = new(1f, 1f, 1f);

    private static Color _blue = new(0.8f, 0.9f, 1f);
    private static Color _yellow = new(1f, 0.82f, 0.57f);
    private static Color _red = new(1, 0.45f, 0.3f);
    private static Color _glint = new(1, 0.95f, 0.7f);
    
    /*
    private static readonly Color Blue = new(0.8f, 1f, 1f);
    private static readonly Color Yellow = new(1f, 0.8f, 0.6f);
    private static readonly Color Red = new(1, 0.6f, 0.6f);
    */
    
    private ParticleSystem[] _systems;
    private List<Material> _laserHitFloorMaterials;

    private Gradient _gradient;
    private float _animationDuration = 0.25f;
    private float _startTime;

    private int _ringIndex;
    private int _glintIndex;

    private void Start()
    {
        _startTime = Time.time;
        _systems = GetComponentsInChildren<ParticleSystem>(true);
        CollectLaserHitFloorMaterials();
        WhitenParticleSystems();
        InitializeGradient();
        UpdateColorsFromAnimation();
    }
    
    private void Update()
    {
        UpdateColorsFromAnimation();
    }

    private void UpdateColorsFromAnimation()
    {
        var t = (Time.time - _startTime) / _animationDuration % 1f;
        var color = _gradient.Evaluate(t);
        for (var i = 0; i < _systems.Length; i++)
        {
            if (i == _ringIndex) continue;
            var system = _systems[i];
            if (system == null) continue;
            var main = system.main;
            if (i == _glintIndex)
                main.startColor = _glint;
            else
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
                
                new GradientColorKey(_yellow, 0 / 3f),
                new GradientColorKey(White, 1.5f / 6f),
                new GradientColorKey(_blue, 2 / 6f),
                new GradientColorKey(_yellow, 0.38f),
                new GradientColorKey(_red, 0.55f),
                new GradientColorKey(White, 4 / 6f),
                new GradientColorKey(_yellow, 5 / 6f),
                new GradientColorKey(_yellow, 1)
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
        for (int i = 0; i < _systems.Length; i++)
        {
            var system = _systems[i];
            var colorOverLifetime = system.colorOverLifetime;
            if (colorOverLifetime.enabled)
            {
                var color = colorOverLifetime.color;
                if (color.mode == ParticleSystemGradientMode.Gradient)
                {
                    var gradient = color.gradientMax;

                    var mode = gradient.mode;
                    GradientColorKey[] colorKeys;
                    if (system.gameObject.name.Equals("vfx_charge_max_ring_00_02", StringComparison.OrdinalIgnoreCase))
                    {
                        colorKeys =
                        [
                            new GradientColorKey(_red, 0.0f),
                            new GradientColorKey(_yellow, 0.5f),
                            new GradientColorKey(White, 1f)
                        ];
                        _ringIndex = i;
                    }
                    else
                    {
                        colorKeys = new GradientColorKey[gradient.colorKeys.Length];
                        for (int j = 0; j < colorKeys.Length; j++)
                        {
                            colorKeys[j] = new GradientColorKey(Color.white, gradient.colorKeys[j].time);
                        }
                    }

                    if (system.gameObject.name.Equals("vfx_chr_attackGlint", StringComparison.OrdinalIgnoreCase))
                    {
                        _glintIndex = i;
                    }

                    var newGradient = new Gradient()
                    {
                        colorKeys = colorKeys,
                        alphaKeys = gradient.alphaKeys,
                        colorSpace = gradient.colorSpace,
                        mode = mode
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