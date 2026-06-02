using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CobraSoundReplacer.Utils;

public static class AudioLoadingUtils
{
    public static IEnumerator LoadAudioClipFromPath(string filePath, ITaskResult<AudioClip> result)
    {
        var type = GetAudioType(filePath);

        if (type == AudioType.UNKNOWN)
        {
            Plugin.Logger.LogError("Unknown audio file type for file " + filePath);
            yield break;
        }

        var task = LoadClip(filePath, type);
        while (!task.IsCompleted && !task.IsCanceled)
        {
            yield return null;
        }
        
        var clip = task.Result;
        
        if (clip == null)
        {
            Plugin.Logger.LogError("Failed to load audio file " + filePath);
            yield break;
        }

        clip.name = Path.GetFileNameWithoutExtension(filePath);

        result.SetResult(clip);
    }
    
    private static async Task<AudioClip> LoadClip(string path, AudioType type)
    {
        AudioClip clip = null;
        using var uwr = UnityWebRequestMultimedia.GetAudioClip(path, type);
        uwr.SendWebRequest();

        // wrap tasks in try/catch, otherwise it'll fail silently
        try
        {
            while (!uwr.isDone) await Task.Delay(5);

            if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                Plugin.Logger.LogError($"Audio processing error: {uwr.error}");
            
            else
            {
                clip = DownloadHandlerAudioClip.GetContent(uwr);
            }
        }
        catch (Exception err)
        {
            Debug.Log($"{err.Message}, {err.StackTrace}");
        }

        return clip;
    }

    private static AudioType GetAudioType(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".wave" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            ".ogv" => AudioType.OGGVORBIS,
            ".oga" => AudioType.OGGVORBIS,
            ".aiff" => AudioType.AIFF,
            ".aif" => AudioType.AIFF,
            ".aifc" => AudioType.AIFF,
            _ => AudioType.UNKNOWN
        };
    }
}