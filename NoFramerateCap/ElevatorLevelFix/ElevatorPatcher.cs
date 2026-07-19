using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NoFramerateCap.ElevatorLevelFix;

// This file sets up two individual fixes for the elevators
[HarmonyPatch]
public static class ElevatorPatcher
{
    private static readonly HashSet<string> TriggerNames = ["ElevatorEnd_F0-F1", "ElevatorEnd_F1-F2", "ElevatorEnd_F2-F3"];

    private static readonly Dictionary<string, int> IDByTriggerNames = new()
    {
        {
            "ElevatorEnd_F0-F1", -2323
        },
        {
            "ElevatorEnd_F1-F2", -2114
        },
        {
            "ElevatorEnd_F2-F3", -2319
        }
    };

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NmiArena), nameof(NmiArena.Init))]
    private static void OnInitArena(NmiArena __instance)
    {
        var levelController = LevelController.Instance;
        if (levelController == null ||
            levelController.level != LevelController.Level.EP08_LVL02_BossCrystalBowie) return;
        __instance.gameObject.AddComponent<ElevatorArenaIdentifier>().arena = __instance;
    }

    // Immediately before the arena is completed, if on level 8-2, notify the ElevatorTriggerInstance of the completed arena
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.StopArena))]
    private static void BeforeArenaStops(LevelController __instance)
    {
        // Only function on level 8-2
        if (__instance.level != LevelController.Level.EP08_LVL02_BossCrystalBowie) return;
        
        NmiArena nmiArena = __instance.nmiSettings.nmiArena;
        
        if (nmiArena == null) return;
        
        if (ElevatorTriggerFixer.Instance == null)
        {
            Plugin.Logger.LogError("Failed to find ElevatorTriggerFixer");
        }
        else
        {
            // ElevatorTriggerFixer.Instance.OnArenaCompleted(__instance.nmiSettings.nmiArena.arenaID);
            var persistentArenaHash = nmiArena.GetComponent<ElevatorArenaIdentifier>();
            if (persistentArenaHash == null)
            {
                Plugin.Logger.LogError("Failed to find ElevatorArenaIdentifier");
                return;
            }
            ElevatorTriggerFixer.Instance.OnArenaCompletedUsingPersistentHash(persistentArenaHash.GetId());
        }
    }
    
    // Add the ElevatorTriggerInstance to the elevator and set up watchers that force themselves to STAY enabled
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MakeLasersFreefall), nameof(MakeLasersFreefall.Awake))]
    private static void PatchElevator(MakeLasersFreefall __instance)
    {
        if (LevelController.Instance == null ||
            LevelController.Instance.level != LevelController.Level.EP08_LVL02_BossCrystalBowie)
        {
            return;
        }

        if (!__instance.gameObject.name.Contains("B03_Z01_Resettable", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fixer = __instance.gameObject.AddComponent<ElevatorTriggerFixer>();
        
        var triggers = __instance.GetComponentsInChildren<GenericTrigger>(true);
        foreach (var trigger in triggers)
        {
            if (!TriggerNames.Contains(trigger.name)) continue;
            var watcher = trigger.gameObject.AddComponent<EnableWatcher>();
            watcher.fixer = fixer;
            if (IDByTriggerNames.TryGetValue(trigger.name, out var id))
            {
                fixer.endTriggerByArenaPersistentHash.Add(id, trigger.gameObject);
            }
            else
            {
                Plugin.Logger.LogError("Valid elevator trigger could not be matched to an ID");
            }
        }
    }
}