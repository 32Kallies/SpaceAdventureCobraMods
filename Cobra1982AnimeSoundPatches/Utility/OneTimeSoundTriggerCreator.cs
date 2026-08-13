using Cobra1982AnimeSoundPatches.Behaviours;
using CobraSoundReplacer.API;
using UnityEngine;

namespace Cobra1982AnimeSoundPatches.Utility;

public static class OneTimeSoundTriggerCreator
{
    public static void CreateCustomTrigger(string name, Vector3 center, Vector3 size, string clipName)
    {
        var gameObject = new GameObject(name);
        gameObject.SetActive(false);
        
        gameObject.layer = 12;
        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        var behavior = gameObject.AddComponent<OneTimeSoundTrigger>();
        if (!CustomSoundUtils.TryGetEClip(clipName, out var clip))
        {
            Plugin.Logger.LogWarning("Failed to find clip by name " + clipName);
            return;
        }
        behavior.clip = clip;
        
        gameObject.transform.position = center;
        collider.size = size;
        
        gameObject.SetActive(true);
    }
}