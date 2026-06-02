using UnityEngine;

namespace MusicReplacer.CustomTriggers;

public class CustomTriggerBounds(Vector3 center, Vector3 size)
{
    public Vector3 Center { get; } = center;
    public Vector3 Size { get; } = size;
}