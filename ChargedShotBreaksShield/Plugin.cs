using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Audio;

namespace ChargedShotsBreakShields;

[BepInDependency("com.lee23.cobrasoundreplacer")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    
    internal static ConfigEntry<float> PenetratingShotDecibelGain { get; private set; }
    internal static AudioMixerGroup LoudSoundMixerGroup { get; private set; }
    
    private void Awake()
    {
        Logger = base.Logger;

        var assembly = Assembly.GetExecutingAssembly();
        Harmony.CreateAndPatchAll(assembly);
        
        PenetratingShotDecibelGain = Config.Bind("General", "Penetrating Shot Decibel Gain", 4f);
        
        try
        {
            var bundleName = "chargedshotsbreakshield";
            var assetName = "LouderGroup";
            var bundlePath = Path.Combine(Path.GetDirectoryName(assembly.Location), bundleName); 

            if (!File.Exists(bundlePath))
            {
                Logger.LogError($"AssetBundle not found at: {bundlePath}");
                return;
            }

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Logger.LogError("Failed to load AssetBundle!");
                return;
            }

            var mixer = bundle.LoadAsset<AudioMixer>(assetName);
            if (mixer == null)
            {
                Logger.LogError($"AudioMixer '{assetName}' not found in bundle!");
                return;
            }

            var groups = mixer.FindMatchingGroups("Master");
            if (groups.Length > 0)
            {
                LoudSoundMixerGroup = groups[0];
                Logger.LogInfo($"Loaded mixer group: {LoudSoundMixerGroup.name}");
            }
            else
            {
                Logger.LogWarning("No 'Master' group found in the mixer!");
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError("Error loading AudioMixer: " + e);
        }

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}