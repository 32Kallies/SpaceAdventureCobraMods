using UnityEngine;

namespace SilenceStackedShotSounds;

public class ShotSoundGradualSilencer : MonoBehaviour
{
    private bool _initialized;
    private CAudio.CPlayingAudioData _data;
    private float _startTargetVolume;
    private float _actualStartVolume;
    private float _fadeDuration;
    private float _startFadeTime;
    private float _targetVolume;
    
    public void Initialize(CAudio.CPlayingAudioData data, float fadeDuration, float targetVolume)
    {
        _data = data;
        _startTargetVolume = data.volTarget;
        _actualStartVolume = data.vol;
        _fadeDuration = fadeDuration;
        _startFadeTime = Time.realtimeSinceStartup;
        _targetVolume = data.volTarget * targetVolume;

        if (fadeDuration > Mathf.Epsilon)
        {
            _initialized = true;
        }
        else
        {
            // instantly stop to avoid divide by zero errors
            data.asrc.Stop();
        }
    }

    private void Update()
    {
        if (!_initialized || _data == null) return;
        
        var t = Mathf.Clamp01((Time.realtimeSinceStartup - _startFadeTime) / _fadeDuration);
        _data.volTarget = Mathf.Lerp(_startTargetVolume, _targetVolume, t);
        _data.vol = Mathf.Lerp(_actualStartVolume, _targetVolume, t);
    }
}