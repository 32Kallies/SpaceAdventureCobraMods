using UnityEngine;

namespace MusicReplacer.Utilities;

public class TakeScreenshotOnEnterTrigger : MonoBehaviour
{
    public audioForceMusicTrigger trigger;
    public long hash;

    private bool _tookScreenshot;
    
    private void Update()
    {
        if (_tookScreenshot)
            return;

        if (trigger == null)
        {
            Plugin.Logger.LogWarning("Trigger is missing!");
            return;
        }
        
        if (trigger.m_Collider != null && LevelController.Instance != null && CobraCharacter.Instance != null && audioReverbTrigger.IsPointInCollider(trigger.m_Collider, CobraCharacter.Instance.transform.position))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        _tookScreenshot = true;
        ScreenshotGenerator.GenerateScreenshotWithDelay(hash, 2);
    }
}