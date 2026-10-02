using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PsychogunImproved;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static Assembly ModAssembly { get; private set; }

    internal static ConfigEntry<bool> UnchargedShotsPassThroughEnemies { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        ModAssembly = Assembly.GetExecutingAssembly();
        Harmony.CreateAndPatchAll(ModAssembly);

        UnchargedShotsPassThroughEnemies = Config.Bind("General", "Uncharged Psychogun shots pass through enemies",
            true, "If enabled, all shots (instead of only fully charged shots) will pass through enemies on contact.");

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}