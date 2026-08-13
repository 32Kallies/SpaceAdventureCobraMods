using Cobra1982AnimeSoundPatches.Utility;
using HarmonyLib;
using UnityEngine;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch]
public static class LevelSpecificPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    private static void StartPostfix(LevelController __instance)
    {
        LevelController.Level level = __instance.level;
        
        if (level == LevelController.Level.EP03_LVL01_Graveyard)
        {
            OneTimeSoundTriggerCreator.CreateCustomTrigger(
                "TurtleDiscoveryElevatorTrigger", new Vector3(1149, -115, 0),
                new Vector3(3, 2.5f, 3), "turtle_elevator_and_reveal");
        }
    }
}