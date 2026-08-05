using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches.NmiMobilePatches;

[HarmonyPatch(typeof(NmiShoot))]
public static class PatchInit
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NmiShoot.Init))]
    private static void OnInit(NmiShoot __instance)
    {
        if (NmiShootSoundDatabase.TryGetNmiShootSoundForWeaponType(__instance.weaponType, out NmiShootSound sound))
        {
            __instance.SetShootClip(sound);
        }
        else
        {
            __instance.SetShootClip(NmiShootSoundDatabase.Default);
        }
    }
}

[HarmonyPatch(typeof(NmiShoot), nameof(NmiShoot.Update))]
public static class PatchShootSound
{
    private static int GetShootClip(NmiShoot @this)
    {
        return (int)@this.ShootClip;
    }
    
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 188))
            .ThrowIfInvalid("Could not find place to hook for NmiShoot.Update transpiler")
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0), // load `this`
                CodeInstruction.Call(typeof(PatchShootSound), nameof(GetShootClip))) 
            .InstructionEnumeration();
        /*
        bool found = false;
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Is(OpCodes.Ldc_I4, 188))
            {
                yield return new CodeInstruction()
                found = true;
                continue;
            }
            yield return instruction;
        }

        if (!found)
        {
            Plugin.Logger.LogError("Failed to find place to hook for NmiShoot.Update transpiler");
        }
        */
    }
}