using HarmonyLib;
using UnityEngine;

namespace TeleportationDoorsCheat;

[HarmonyPatch(typeof(CobraCharacter))]
public static class EnableCheatPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CobraCharacter.Start))]
    private static void StartPostfix()
    {
        if (Plugin.AlwaysEnableCheat.Value)
        {
            TeleportationCheatEnabler.ActivateTeleporters();
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CobraCharacter.Update))]
    private static void UpdatePostfix()
    {
        if (Plugin.AllowTeleportationCheatWithHotkey.Value)
        {
            if (Input.GetKey(KeyCode.F2) && Input.GetKeyDown(KeyCode.F6))
            {
                TeleportationCheatEnabler.ActivateTeleporters();
            }
        }
    }
}