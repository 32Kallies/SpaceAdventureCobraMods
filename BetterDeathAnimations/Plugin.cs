using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BetterDeathAnimations;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    
    internal static Assembly Assembly { get; private set; }
    internal static AssetBundle Bundle { get; private set; }
    
    internal static ConfigEntry<bool> AlwaysPlayNewDeathAnimation { get; private set; }
    internal static ConfigEntry<int> ChanceForDisintegrationFail { get; private set; }
    
    private void Awake()
    {
        Logger = base.Logger;

        AlwaysPlayNewDeathAnimation = Config.Bind("General", "Enemies never disintegrate", false,
            "If toggled on, all attacks will trigger the new death animation instead of only revolver and melee attacks.");
        ChanceForDisintegrationFail = Config.Bind("General", "Chance to not disintegrate", 0,
            new ConfigDescription("The chance of enemies dying but not disintegrating, even from psychogun shots and explosions.",
                new AcceptableValueRange<int>(0, 100)));

        Assembly = Assembly.GetExecutingAssembly();
        Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.Location), "Assets", "death_animation"));
        
        Harmony.CreateAndPatchAll(Assembly);
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}