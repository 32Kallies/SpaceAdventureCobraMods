using UnityEngine;

namespace GameOverScreen.UI;

public class GameOverScreenMute : MonoBehaviour
{
    private float[] _defaults;
    
    private void Start()
    {
        var cAudio = AudioController.Audio;
        _defaults = new float[cAudio.volume.Length];
        for (int i = 0; i < cAudio.volume.Length; i++)
        {
            _defaults[i] = cAudio.volume[i];
            if (i > 0 && // check if NOT general volume 
                i != (int)CAudio.eVolumeType.ui) // AND check if NOT UI volume
            {
                cAudio.volume[i] = 0;
            } 
        }
    }

    private void OnDestroy()
    {
        if (_defaults != null)
        {
            var cAudio = AudioController.Audio;
            for (int i = 0; i < cAudio.volume.Length; i++)
            {
                cAudio.volume[i] = _defaults[i];
            }
        }
    }
}