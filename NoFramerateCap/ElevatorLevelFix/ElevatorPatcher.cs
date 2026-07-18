using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NoFramerateCap.ElevatorLevelFix;

[HarmonyPatch(typeof(MakeLasersFreefall), nameof(MakeLasersFreefall.Awake))]
public static class ElevatorPatcher
{
    private static HashSet<string> _triggerNames = ["ElevatorEnd_F0-F1", "ElevatorEnd_F1-F2", "ElevatorEnd_F2-F3"];
    
    private static void Postfix(MakeLasersFreefall __instance)
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
            if (_triggerNames.Contains(trigger.name))
            {
                var watcher = trigger.gameObject.AddComponent<EnableWatcher>();
                watcher.fixer = fixer;
            }
        }
    }
}