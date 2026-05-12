using UnityEngine;

namespace MusicReplacer.Data;

public struct SimpleVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public SimpleVector3(Vector3 vector) : this(vector.x, vector.y, vector.z)
    {
    }

    public SimpleVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public Vector3 ToVector3() => new(X, Y, Z);
}