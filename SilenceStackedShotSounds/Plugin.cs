using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SilenceStackedShotSounds;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static ConfigEntry<float> TargetVolume { get; private set; }
    internal static ConfigEntry<float> TimePadding { get; private set; }
    internal static ConfigEntry<float> FadeDuration { get; private set; }
    
    private void Awake()
    {
        Logger = base.Logger;
        
        TargetVolume = Config.Bind("General", "Volume When Quiet", 0.3f,
            new ConfigDescription("The volume of previous shot sounds after a new shot is fired.",
                new AcceptableValueRange<float>(0, 1)));

        TimePadding = Config.Bind("General", "Time Padding", 0.05f,
            new ConfigDescription("The number of seconds a sound must have played for it to be considered for becoming quiet.",
                new AcceptableValueRange<float>(0.01f, 0.5f)));

        FadeDuration = Config.Bind("General", "Fade Duration", 0.7f,
            new ConfigDescription("The number of seconds it takes a sound to reach the target volume.",
                new AcceptableValueRange<float>(0f, 1.5f)));

        var assembly = Assembly.GetExecutingAssembly();
        Harmony.CreateAndPatchAll(assembly);
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}