using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace NoFramerateCap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    public static ConfigEntry<int> FrameRate;
    public static ConfigEntry<bool> DisableVsync;

    private void Awake()
    {
        FrameRate = Config.Bind(
            "General",
            "NewFrameRate",
            90,
            "The new target framerate for the game."
        );
        
        DisableVsync = Config.Bind(
            "General",
            "DisableVsync",
            false,
            "If true, vsync will be disabled."
        );
        
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}
