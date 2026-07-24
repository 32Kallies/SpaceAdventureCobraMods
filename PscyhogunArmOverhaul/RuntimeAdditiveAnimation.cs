// PORTIONS OF THIS CLASS WERE CREATED BY CHATGPT

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using PscyhogunArmOverhaul;

[Serializable]
public class RuntimeAdditiveAnimationData
{
    public string BoneName;
    public SerializableVector3[] Positions;
    public SerializableQuaternion[] Rotations;

    public RuntimeAdditiveAnimationData() {}
}

[Serializable]
public class RuntimeAdditiveAnimationJSON
{
    public float ClipLength;
    public float FrameRate;
    public RuntimeAdditiveAnimationData[] Bones;

    public RuntimeAdditiveAnimationJSON()
    {
        
    }
}

[Serializable]
public struct SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public Vector3 ToVector3() => new Vector3(x, y, z);
    
    public SerializableVector3() {}
}

[Serializable]
public struct SerializableQuaternion
{
    public float x, y, z, w;
    public SerializableQuaternion(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
    public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
    
    public SerializableQuaternion() { }
}

public class RuntimeAdditiveAnimation : MonoBehaviour
{
    [Serializable]
    public class BoneFrame
    {
        public Transform Bone;
        public Quaternion[] Rotations; // one per frame
        public Vector3[] Positions;    // one per frame
    }
    
    public BoneFrame[] AnimatedBones;

    [Header("Clip Settings")]
    public AnimationClip clip;
    public float ClipLength;
    public float FrameRate = 60f;
    
    [TextArea(5, 15)]
    [Tooltip("Paste bone names here, one per line.")]
    public string BoneNamesText;
    
    private bool _isPlaying;
    private float _playbackTime;
    private float _speed = 1f;
    private float _weight = 1f;

    public Action OnLateUpdate;
    
    public void PopulateBonesFromText(Transform root)
    {
        if (string.IsNullOrEmpty(BoneNamesText))
        {
            Debug.LogWarning("BoneNamesText is empty");
            return;
        }

        string[] names = BoneNamesText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        var bones = new List<BoneFrame>();

        foreach (string name in names)
        {
            string trimmed = name.Trim();
            Transform bone = FindChildByName(root, trimmed);
            if (bone == null)
            {
                Debug.LogWarning("Bone not found: " + trimmed);
                continue;
            }

            bones.Add(new BoneFrame { Bone = bone });
        }

        AnimatedBones = bones.ToArray();
        Debug.Log("Populated AnimatedBones with " + AnimatedBones.Length + " bones.");
    }
    
    public void LoadFromJSON(string path, Transform root)
    {
        if (!File.Exists(path))
        {
            Plugin.Logger.LogError("Additive JSON not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        RuntimeAdditiveAnimationJSON data = JsonConvert.DeserializeObject<RuntimeAdditiveAnimationJSON>(json);

        ClipLength = data.ClipLength;
        FrameRate = data.FrameRate;

        var boneList = new List<BoneFrame>();

        foreach (var boneData in data.Bones)
        {
            Transform boneTransform = FindChildByName(root, boneData.BoneName);
            if (boneTransform == null)
            {
                Debug.LogWarning("Bone not found in hierarchy: " + boneData.BoneName);
                continue;
            }

            BoneFrame frame = new BoneFrame
            {
                Bone = boneTransform,
                Positions = boneData.Positions != null ? Array.ConvertAll(boneData.Positions, v => v.ToVector3()) : null,
                Rotations = Array.ConvertAll(boneData.Rotations, q => q.ToQuaternion())
            };

            boneList.Add(frame);
        }

        AnimatedBones = boneList.ToArray();
        Plugin.Logger.LogInfo("Loaded additive animation from JSON");
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name) return t;
        }
        return null;
    }

    public void Play(float speed = 1f, float weight = 1f)
    {
        _playbackTime = 0f;
        _speed = speed;
        _weight = weight;
        _isPlaying = true;
    }
    
    public void PlayInReverse(float speed = 1f, float weight = 1f)
    {
        _playbackTime = ClipLength;
        _speed = -speed;
        _weight = weight;
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    void LateUpdate()
    {
        if (!_isPlaying || AnimatedBones == null) return;

        _playbackTime += Time.deltaTime * _speed;
        float totalTime = ClipLength;
        int totalFrames = Mathf.CeilToInt(totalTime * FrameRate);

        // Compute exact frame index (can be negative for reverse)
        float exactFrame = (_playbackTime / totalTime) * totalFrames;
    
        int frameIndex0, frameIndex1;
        float t;

        if (_speed >= 0)
        {
            frameIndex0 = Mathf.Clamp(Mathf.FloorToInt(exactFrame), 0, totalFrames - 1);
            frameIndex1 = Mathf.Clamp(frameIndex0 + 1, 0, totalFrames - 1);
            t = exactFrame - frameIndex0;
        }
        else
        {
            frameIndex1 = Mathf.Clamp(Mathf.FloorToInt(exactFrame), 0, totalFrames - 1);
            frameIndex0 = Mathf.Clamp(frameIndex1 + 1, 0, totalFrames - 1);
            t = 1f - (exactFrame - Mathf.Floor(exactFrame));
        }

        foreach (var bone in AnimatedBones)
        {
            if (bone.Bone == null || bone.Rotations == null) continue;

            Quaternion rot0 = bone.Rotations[frameIndex0];
            Quaternion rot1 = bone.Rotations[frameIndex1];
            Quaternion rot = Quaternion.Slerp(rot0, rot1, t);

            Vector3 pos = Vector3.zero;
            if (bone.Positions != null && bone.Positions.Length > 0)
            {
                Vector3 pos0 = bone.Positions[frameIndex0];
                Vector3 pos1 = bone.Positions[frameIndex1];
                pos = Vector3.Lerp(pos0, pos1, t);
            }

            bone.Bone.localRotation = rot;
            bone.Bone.localPosition = pos;
        }

        // Stop animation at either end
        if (_playbackTime >= totalTime || _playbackTime <= 0f)
            _isPlaying = false;
        
        OnLateUpdate?.Invoke();
    }
}