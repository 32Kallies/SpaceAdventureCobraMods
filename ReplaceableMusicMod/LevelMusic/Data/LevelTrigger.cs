using System;
using MusicReplacer.Data;
using UnityEngine;

namespace MusicReplacer.LevelMusic.Data;

[Serializable]
public class LevelTrigger
{
    public long Hash { get; set; }
    public int MusicClip { get; set; }
    public PrimitiveType Type { get; set; }
    public float Radius { get; set; }
    public SimpleVector3 Center { get; set; }
    public SimpleVector3 Size { get; set; }

    public Vector3 GetEncapsulatingSize()
    {
        switch (Type)
        {
            case PrimitiveType.Sphere:
                return Vector3.one * (Radius * 2);
            case PrimitiveType.Cube:
                return Size.ToVector3();
            default:
                throw new NotImplementedException("Unhandled trigger type: " + Type);
        }
    }
}