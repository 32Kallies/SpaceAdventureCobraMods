using System.Collections;
using CobraSoundReplacer.Utils;
using UnityEngine;

namespace MusicReplacer.ReplacementSystem;

public static class MusicPreviewUtils
{
    public static IEnumerator PreviewEClip(audioSelectionData.eCLIP clip, bool playOriginal, ITaskResult<MusicPreview> result)
    {
        if (playOriginal)
        {
            CobraSoundReplacer.API.PlayOriginalUtils.PlayOriginalForNextSoundEvent();
        }
        var playingAudio = AudioController.Instance.PlaySound(clip);
        var preview = new MusicPreview(playingAudio, false);
        result.SetResult(preview);
        yield return null;
    }
    
    public static IEnumerator PreviewCurrentMusic(MusicSound music, bool playOriginal, ITaskResult<MusicPreview> result)
    {
        CAudio.CPlayingAudioData playingAudio;
        bool destroyEmitter = false;
        if (playOriginal)
        {
            CobraSoundReplacer.API.PlayOriginalUtils.PlayOriginalForNextSoundEvent();
            playingAudio = AudioController.Instance.PlaySound(music.EClip);
        }
        else if (MusicReplacementManager.ReplacementData.CanPreviewCustomSound(music))
        {
            if (!MusicReplacementManager.ReplacementData.SoundHasReplacement(music))
                CobraSoundReplacer.API.PlayOriginalUtils.PlayOriginalForNextSoundEvent();
            
            playingAudio = AudioController.Instance.PlaySound(music.EClip);
        }
        else
        {
            var audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            var clipResult = new TaskResult<AudioClip>();
            yield return AudioLoadingUtils.LoadAudioClipFromPath(FileManagement.GetFullPathOfCustomSound(music), clipResult);
            audioSource.clip = clipResult.GetResult();
            audioSource.volume = MusicReplacementManager.ReplacementData.GetSoundVolume(music);
            playingAudio = new CAudio.CPlayingAudioData
            {
                asrc = audioSource
            };
            audioSource.Play();
            destroyEmitter = true;
        }
        var preview = new MusicPreview(playingAudio, destroyEmitter);
        result.SetResult(preview);
        yield return null;
    }
    
    public class MusicPreview
    {
        public MusicPreview(CAudio.CPlayingAudioData playingAudio, bool destroy)
        {
            _playingAudio = playingAudio;
            _destroyEmitter = destroy;
        }

        private CAudio.CPlayingAudioData _playingAudio;
        private readonly bool _destroyEmitter;
        
        public bool StopPreview()
        {
            if (_playingAudio != null && _playingAudio.asrc != null)
            {
                bool wasPlaying = _playingAudio.asrc.isPlaying;
                _playingAudio.asrc.Stop();
                if (_destroyEmitter)
                    Object.Destroy(_playingAudio.asrc.gameObject);
                _playingAudio = null;
                return wasPlaying;
            }

            return false;
        }

        public void SetPreviewVolume(float volume)
        {
            if (!_destroyEmitter)
            {
                _playingAudio.vol = volume;
                _playingAudio.volTarget = volume;
                return;
            }
            if (_playingAudio.asrc != null)
                _playingAudio.asrc.volume = volume;
        }
    }
}