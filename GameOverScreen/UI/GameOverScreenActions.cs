using System.Collections;
using GameOverScreen.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameOverScreen.UI;

public static class GameOverScreenActions
{
    public static void RestartFromCheckpoint()
    {
        GameOverScreen.Close();
        DeathPatch.RestartFromCheckpoint();
    }
    
    // Inspired by NUIPausePanel.Update
    public static void RestartStage()
    {
        if (CobraCharacter.Instance != null)
        {
            CobraCharacter.Instance.OnLevelQuitFromPause();
            GameController.Instance.SaveGameDataWhenPossible = true;
        }

        Plugin.RunCoroutineOnPlugin(KillStaleAudioCoroutine());
        GameController.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static IEnumerator KillStaleAudioCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Utilities.KillStaleAudio();
    }
    
    // Inspired by NUIPausePanel.Update - RETURN TO HUB
    public static void QuitToMainMenu()
    {
        GameOverScreen.Close();
        if (CobraCharacter.Instance != null)
        {
            CobraCharacter.Instance.OnLevelQuitFromPause();
            GameController.Instance.SaveGameDataWhenPossible = true;
        }
        ActivitiesController.BeginActivity("1");
        GameController.Instance.LoadScene(GameController.Instance.hubScene);
    }
}