using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ZoomMod;

[HarmonyPatch(typeof(CameraController), nameof(CameraController.UpdateWithSplineProperties),
    typeof(float), typeof(GameObject), typeof(Vector3), typeof(bool), typeof(float), typeof(bool), typeof(Room))]
public class AllowZoomingOnSplineTranspiler
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Plugin.Logger.LogInfo("Transpiling UpdateWithSplineProperties");
        bool foundFovString = false;
        bool completed = false;
        
        foreach (var code in instructions)
        {
            if (completed)
            {
                yield return code;
                continue;
            }
            
            if (foundFovString)
            {
                if (code.opcode == OpCodes.Stloc_S)
                {
                    Plugin.Logger.LogInfo("Found stloc for fieldOfView");
                    // load float onto stack
                    yield return CodeInstruction.Call(typeof(AllowZoomingOnSplineTranspiler),
                        nameof(GetFovMultiplierForSpline));
                    // multiply the original FOV by the multiplier before storing the local variable
                    yield return new CodeInstruction(OpCodes.Mul);
                    completed = true;
                }
                yield return code;
                continue;
            }
            
            if (code.opcode == OpCodes.Ldstr && code.operand is string operand && string.Equals(operand, "Fov"))
            {
                Plugin.Logger.LogInfo("Found loading of Fov data");
                foundFovString = true;
                yield return code;
            }
            else
            {
                yield return code;
            }
        }
    }

    private static float GetFovMultiplierForSpline()
    {
        var zoom = CameraZoomBehavior.main;
        if (zoom == null) return 1f;
        return zoom.GetFovMultiplier();
    }
}