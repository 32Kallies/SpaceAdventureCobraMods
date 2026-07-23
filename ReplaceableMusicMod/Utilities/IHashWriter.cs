using UnityEngine;

namespace MusicReplacer.Utilities;

public interface IHashWriter
{
    void Write(int value);
    void Write(float value);
    void Write(bool value);
    void Write(string value);
    void Write(Vector3 value);
    void Write(Quaternion value);
    void Reset();
    ulong ToUInt64();
}