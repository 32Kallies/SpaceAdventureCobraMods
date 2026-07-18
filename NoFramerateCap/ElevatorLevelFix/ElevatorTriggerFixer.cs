using System.Collections.Generic;
using UnityEngine;

namespace NoFramerateCap.ElevatorLevelFix;

public class ElevatorTriggerFixer : MonoBehaviour
{
    public List<GameObject> toForceEnabled = new();

    private void Update()
    {
        foreach (var toEnable in toForceEnabled)
        {
            if (toEnable != null)
            {
                if (!toEnable.activeSelf)
                {
                    Plugin.Logger.LogWarning("Fixing activation state of deactivated elevator end trigger!");
                }
                toEnable.SetActive(true);
            }
        }
    }
}