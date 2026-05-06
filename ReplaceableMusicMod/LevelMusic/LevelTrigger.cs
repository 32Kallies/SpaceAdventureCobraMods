using System;
using MusicReplacer.Data;
using UnityEngine;

namespace MusicReplacer.LevelMusic;

[Serializable]
public class LevelTrigger
{
    public int Hash { get; set; }
    public int MusicClip { get; set; }
    public PrimitiveType Type { get; set; }
    public float Radius { get; set; }
    public SimpleVector3 Center { get; set; }
    public SimpleVector3 Size { get; set; }
}