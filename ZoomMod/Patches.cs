using HarmonyLib;
using UnityEngine;

namespace ZoomMod;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))]
    public static void CameraStartPostfix(CameraController __instance)
    {
        __instance.gameObject.AddComponent<CameraZoomBehavior>().controller = __instance;
    }
    
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Spline), nameof(CameraController.Start))]
    public static void CameraStartPostfix(CameraController __instance)
    {
        __instance.gameObject.AddComponent<CameraZoomBehavior>().controller = __instance;
    }
    */

    private static float _prevOffsetX; 
    private static float _prevOffsetY;
    private static float _smoothing = 2f;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Update))]
    private static void CobraUpdatePrefix(CobraCharacter __instance)
    {
        _prevOffsetX = __instance.cameraOffsetX;
        _prevOffsetY = __instance.cameraOffsetY;
        
        float t = 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
        __instance.cameraOffsetInertiaIn = t;
        __instance.cameraOffsetInertiaOut = t;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Update))]
    public static void CobraUpdatePostfix(CobraCharacter __instance)
    {
        var zoom = CameraZoomBehavior.main;
        if (zoom == null || zoom.IsZoomedOut())
            return;
        
        bool flag3 = (!__instance.isMoving || __instance.GetActualSubstate() == CobraCharacter.Substate.Hanging || __instance.GetActualSubstate() == CobraCharacter.Substate.ClimbingBoots) && !CobraCharacter.isMulti();
        Vector2 rightStick = __instance.GetRightStick();
        if (Mathf.Abs(rightStick.x) > 0.15f && flag3)
        {
        }
        else
        {
            __instance.cameraOffsetX = _prevOffsetX;
        }
        if (Mathf.Abs(rightStick.y) > 0.15f && flag3)
        {
        }
        else
        {
            __instance.cameraOffsetY = _prevOffsetY;
        }
    }
}