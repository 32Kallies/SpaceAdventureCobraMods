using System.Collections.Generic;
using UnityEngine;

namespace NoFramerateCap.ElevatorLevelFix;

public class ElevatorTriggerFixer : MonoBehaviour
{
    public static ElevatorTriggerFixer Instance { get; private set; }

    // public Dictionary<int, GameObject> endTriggerByArenaId = new();
    public Dictionary<int, GameObject> endTriggerByArenaPersistentHash = new();
    public List<GameObject> toForceEnabled = new();

    private void Awake()
    {
        Instance = this;
    }

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

    /*
    public void OnArenaCompleted(int arenaId)
    {
        Plugin.Logger.LogInfo("On arena completed: " + arenaId);

        // Valid arena IDs are in the range [1,3] (yes, that is inclusive)
        if (arenaId is < 1 or > 3)
        {
            return;
        }

        if (!endTriggerByArenaId.TryGetValue(arenaId, out var toEnable))
        {
            Plugin.Logger.LogError("Failed to find trigger by arena ID " + arenaId);
            return;
        }

        if (!toEnable.activeSelf)
        {
            Plugin.Logger.LogWarning($"Elevator end trigger of ID {arenaId} is deactivated. Activating forcefully.");
        }

        Plugin.Logger.LogInfo("Enabling elevator end trigger by ID: " + arenaId);
        toEnable.SetActive(true);
    }
    */

    public void OnArenaCompletedUsingPersistentHash(int persistentArenaId)
    {
        Plugin.Logger.LogInfo("On arena completed: " + persistentArenaId);
        
        if (!endTriggerByArenaPersistentHash.TryGetValue(persistentArenaId, out var toEnable))
        {
            Plugin.Logger.LogError("Failed to find trigger by arena ID " + persistentArenaId);
            return;
        }

        if (!toEnable.activeSelf)
        {
            Plugin.Logger.LogWarning($"Elevator end trigger of ID {persistentArenaId} is deactivated. Activating forcefully.");
        }

        Plugin.Logger.LogInfo("Enabling elevator end trigger by ID: " + persistentArenaId);
        toEnable.SetActive(true);
    }
}