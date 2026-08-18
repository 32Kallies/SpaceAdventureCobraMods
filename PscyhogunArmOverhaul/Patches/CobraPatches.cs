using System.IO;
using HarmonyLib;
using UnityEngine;

namespace PscyhogunArmOverhaul.Patches;

[HarmonyPatch(typeof(CobraCharacter))]
public static class CobraPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CobraCharacter.Start))]
    private static void StartPostfix(CobraCharacter __instance)
    {
        var behaviour = __instance.gameObject.AddComponent<NewArmBehaviour>();
        behaviour.character = __instance;
        var armOffSource = __instance.gameObject.AddComponent<AudioSource>();
        armOffSource.playOnAwake = false;
        armOffSource.clip = Plugin.Bundle.LoadAsset<AudioClip>("ArmOff");
        armOffSource.volume = Plugin.ArmOffSound.Value;
        behaviour.armOffSound = armOffSource;

        var armAnimation = __instance.gameObject.AddComponent<RuntimeAdditiveAnimation>();
        armAnimation.LoadFromJSON(
            Path.Combine(Path.GetDirectoryName(Plugin.ModAssembly.Location), "ProtheseOffAnim_Additive.json"),
            __instance.transform);
        behaviour.additiveAnimation = armAnimation;

        var objectRoot = __instance.transform.FindRecursive("object_R");
        var fakeArmTarget = new GameObject("FakeArmTarget");
        fakeArmTarget.transform.SetParent(objectRoot);
        fakeArmTarget.transform.localPosition = new Vector3(-0.001f, -0.014f, -0.021f);
        fakeArmTarget.transform.localEulerAngles = new Vector3(6.405f, 108.98f, 349.083f);
        fakeArmTarget.transform.localScale = Vector3.one;
        behaviour.prostheticArmTarget = fakeArmTarget.transform;
        
        // remove flying off arm
        __instance.protheseOffParams.protheseLifeTime = 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CobraCharacter.PsychogunTrig))]
    private static void ForceArmBackOn(CobraCharacter __instance, CobraCharacter.Buttons button)
    {
        if (NewArmBehaviour.Instance == null || NewArmBehaviour.Instance.GetCanShoot())
            return;

        if (__instance.autoControl.enabled)
        {
            return;
        }

        if (ParametricCinematic.IsPlaying)
        {
            return;
        }

        if (__instance.isTrig(button))
        {
            NewArmBehaviour.Instance.OnFailToShootPsychogun();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CobraCharacter.ShootPsychogun))]
    private static bool DisablePsychogunPrefix(CobraCharacter __instance)
    {
        if (NewArmBehaviour.Instance && !NewArmBehaviour.Instance.GetCanShoot())
        {
            return false;
        }

        return true;
    }
}