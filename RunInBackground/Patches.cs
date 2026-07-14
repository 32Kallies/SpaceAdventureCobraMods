using HarmonyLib;
using UnityEngine;

namespace RunInBackground;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioController), nameof(AudioController.Update))]
    private static void Patch()
    {
        Application.runInBackground = true;
    }
}