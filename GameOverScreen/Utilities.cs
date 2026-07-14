namespace GameOverScreen;

public static class Utilities
{
    public static bool IsDifficultySpaceCobra()
    {
        return HardcoreStateMemorizer.GetIsHardcore() && LoadSaveController.Instance.PreferencesData.difficulty >= 2;
    }
    
    public static bool IsMultiplayer()
    {
        if (CobraCharacter.players == null) return false;
        int players = 0;
        foreach (var player in CobraCharacter.players)
        {
            if (player != null) players++;
        }
        return players > 1;
    }

    public const int RankSPlusConstant = 256;
    
    // From my MusicRestartFix mod
    public static void KillStaleAudio()
    {
        var sources = AudioController.Audio.audioSrc;
        foreach (var source in sources)
        {
            var audioSource = source.asrc;
            bool shouldStop = audioSource.clip != null && !audioSource.clip.name.StartsWith("mus_loop_main_theme");
            if (shouldStop)
            {
                Plugin.Logger.LogDebug("Stopping unused song");
                source.asrc.Stop();
            }
        }
    }
}