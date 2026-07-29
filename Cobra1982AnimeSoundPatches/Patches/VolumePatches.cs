using HarmonyLib;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch]
public static class VolumePatches
{
    private const int PsychogunChargeSlsIndex = 0;
    private const float AudioPlayDuration = 0.98f;
    private const float MinClipPercentForChargeSoundToStick = 0.5f;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioController), nameof(AudioController.PlaySound), typeof(audioSelectionData.eCLIP),
        typeof(float), typeof(byte), typeof(byte), typeof(float))]
    private static void ChangeVolumesPatch(ref audioSelectionData.eCLIP _clip, CAudio.CPlayingAudioData __result)
    {
        if (__result == null)
            return;
        
        if (_clip == audioSelectionData.eCLIP.PLY_SHOOT_PG_CHARGEDLAUNCHED)
        {
            if (__result.asrc == null)
                return;
        
            Plugin.PsychogunMixerGroup.audioMixer.SetFloat("_VolumeGain", Plugin.PyschogunShotDecibelGain.Value);
            __result.asrc.outputAudioMixerGroup = Plugin.PsychogunMixerGroup;
        }
        else if (_clip == audioSelectionData.eCLIP.PLR_PSYCHOGUN_SUPERGUIDED_SHOT)
        {
            if (__result.asrc == null)
                return;
        
            Plugin.SuperShotMixerGroup.audioMixer.SetFloat("_VolumeGain", Plugin.SuperShotDecibelGain.Value);
            __result.asrc.outputAudioMixerGroup = Plugin.SuperShotMixerGroup;
        }
        else if (_clip == audioSelectionData.eCLIP.PLY_SHOOT_PG_CHARGING)
        {
            if (__result.asrc == null)
                return;
        
            Plugin.ChargeSoundMixerGroup.audioMixer.SetFloat("_VolumeGain", Plugin.ChargeSoundDecibelGain.Value);
            __result.asrc.outputAudioMixerGroup = Plugin.ChargeSoundMixerGroup;

        }
    }
    

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioController), nameof(AudioController.UpdateSingleLoopSounds))]
    public static void ForcePsychogunChargeSoundToPlayFullyPatch(AudioController __instance)
    {
        if (__instance.m_SLS_UsedCount[PsychogunChargeSlsIndex] <= 0)
            return;

        if (__instance.m_SLS_PAD[PsychogunChargeSlsIndex] == null)
            return;

        var chargeSound = __instance.m_SLS_PAD[PsychogunChargeSlsIndex];

        if (chargeSound.asrc != null && chargeSound.asrc.clip != null
            && chargeSound.asrc.time > chargeSound.asrc.clip.length * MinClipPercentForChargeSoundToStick
            && chargeSound.asrc.time < chargeSound.asrc.clip.length * AudioPlayDuration)
        {
            __instance.m_SLS_UsedCount[PsychogunChargeSlsIndex] = 2;
        }
    }
}