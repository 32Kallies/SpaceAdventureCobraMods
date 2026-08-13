using UnityEngine;

namespace Cobra1982AnimeSoundPatches.Utility;

public static class DisableMusicTriggerUtils
{
    public static void DisableMusicTriggersAroundCobra()
    {
        var cobra = CobraCharacter.Instance;
        if (cobra == null)
        {
            Plugin.Logger.LogWarning("Cobra does not exist; can't disable music trigger");
            return;
        }

        int disabled = 0;
        var colliders = Physics.OverlapSphere(cobra.transform.position, 2);
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<audioForceMusicTrigger>() != null)
            {
                collider.gameObject.SetActive(false);
                disabled++;
            }
        }
        Plugin.Logger.LogInfo($"Disabled {disabled} music triggers around Cobra");
    }
}