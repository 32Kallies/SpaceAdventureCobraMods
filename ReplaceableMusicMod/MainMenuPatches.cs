using System.Collections;
using CobraSoundReplacer.Utils;
using HarmonyLib;
using MusicReplacer.ReplacementSystem;
using MusicReplacer.Utilities;
using UnityEngine;

namespace MusicReplacer;

[HarmonyPatch]
public static class MainMenuPatches
{
    private const string MainMenuMusic = "mus_loop_main_theme.wav";
    
    // Doesn't seem to do anything
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.Start))]
    public static void TitleStartPostfix()
    {
        ReplaceMainTheme();
    }
    
    // Doesn't seem to do anything
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIMainMenuController), nameof(UIMainMenuController.Start))]
    public static void OtherMenuStartPostfix()
    {
        ReplaceMainTheme();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIMainMenu), nameof(NUIMainMenu.Start))]
    public static void MainMenuStartPostfix()
    {
        ReplaceMainTheme();
    }

    private static void ReplaceMainTheme()
    {
        if (!MusicReplacementManager.ReplacementData.SoundHasReplacement(MainMenuMusic))
        {
            return;
        }
        
        var audioController = AudioController.Instance;
        if (audioController == null)
        {
            Plugin.Logger.LogWarning("Could not find audio controller");
            return;
        }

        foreach (var source in audioController.GetComponentsInChildren<AudioSource>())
        {
            if (source.gameObject.name.Contains(MainMenuMusic))
            {
                Plugin.StartCoroutineOnPlugin(ReplaceMainMenuMusic(source));
                break;
            }
        }
    }

    private static IEnumerator ReplaceMainMenuMusic(AudioSource source)
    {
        if (!MusicReplacementManager.ReplacementData.TryGetCustomSound(MainMenuMusic, out var customSoundPath))
        {
            Plugin.Logger.LogWarning("Failed to find custom main menu music sound");
            yield break;
        }
        var clipResult = new TaskResult<AudioClip>();
        yield return AudioLoadingUtils.LoadAudioClipFromPath(FileManagement.GetFullPathOfCustomSound(customSoundPath), clipResult);
        var clip = clipResult.GetResult();
        if (clip == null)
        {
            Plugin.Logger.LogWarning("Could not load audio clip: " + customSoundPath);
            yield break;
        }
        source.clip = clip;
        source.volume = MusicReplacementManager.ReplacementData.GetSoundVolume(MainMenuMusic);
        source.Play();
    }
}