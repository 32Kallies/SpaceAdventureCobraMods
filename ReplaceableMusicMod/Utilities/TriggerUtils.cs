using System;
using UnityEngine;

namespace MusicReplacer.Utilities;

public static class TriggerUtils
{
    public static (PrimitiveType type, Vector3 center, Vector3 size) GetColliderDimensions(GameObject obj)
    {
        var box = obj.GetComponent<BoxCollider>();
        if (box != null)
        {
            return (PrimitiveType.Cube, obj.transform.position, box.size);
        }
        var sphere = obj.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            return (PrimitiveType.Sphere, obj.transform.position, Vector3.one * sphere.radius * 2f);
        }

        throw new Exception("Trigger does not have a box or sphere collider!");
    }

    public static int GenerateTriggerHash(Vector3 center, Vector3 size)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int cz = Mathf.RoundToInt(center.z);

        int sx = Mathf.RoundToInt(size.x);
        int sy = Mathf.RoundToInt(size.y);
        int sz = Mathf.RoundToInt(size.z);

        return GetStableHash(cx, cy, cz, sx, sy, sz);
    }
    
    private static int GetStableHash(int cX, int cY, int cZ, int sX, int sY, int sZ)
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 31 + cX.GetHashCode();
            hash = hash * 31 + cY.GetHashCode();
            hash = hash * 31 + cZ.GetHashCode();

            hash = hash * 31 + sX.GetHashCode();
            hash = hash * 31 + sY.GetHashCode();
            hash = hash * 31 + sZ.GetHashCode();

            return hash;
        }
    }
}