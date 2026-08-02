using UnityEngine;

namespace DisableBootupScreenMusic;

public class TitleScreenMusicDisabler : MonoBehaviour
{
    private void Update()
    {
        StopMusic();
    }

    public void StopMusic()
    {
        foreach (var src in AudioController.Audio.audioSrc)
        {
            src.asrc.Stop();
        }
    }
}