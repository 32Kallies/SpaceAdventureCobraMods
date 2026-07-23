namespace MusicReplacer.Utilities;

using System.Text;
using UnityEngine;

public sealed class BasicHashWriter : IHashWriter
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    private ulong _hash = OffsetBasis;

    // 1000 = nearest 0.001.
    public float FloatPrecision { get; set; } = 1000f;

    public void Write(int value)
    {
        unchecked
        {
            Mix((byte)value);
            Mix((byte)(value >> 8));
            Mix((byte)(value >> 16));
            Mix((byte)(value >> 24));
        }
    }

    public void Write(float value)
    {
        Write(Mathf.RoundToInt(value * FloatPrecision));
    }

    public void Write(bool value)
    {
        Mix(value ? (byte)1 : (byte)0);
    }

    public void Write(string value)
    {
        if (value == null)
        {
            Write(-1);
            return;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(value);

        Write(bytes.Length);

        foreach (byte b in bytes)
            Mix(b);
    }

    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    public ulong ToUInt64()
    {
        return _hash;
    }

    private void Mix(byte value)
    {
        unchecked
        {
            _hash ^= value;
            _hash *= Prime;
        }
    }

    public void Reset()
    {
        _hash = OffsetBasis;
    }
}