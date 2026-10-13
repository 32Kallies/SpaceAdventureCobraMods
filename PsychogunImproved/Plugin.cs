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
    internal static ConfigEntry<bool> UnchargedShotsImitateChargedShots { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        ModAssembly = Assembly.GetExecutingAssembly();
        Harmony.CreateAndPatchAll(ModAssembly);

        UnchargedShotsPassThroughEnemies = Config.Bind("General", "Uncharged Psychogun shots pass through enemies",
            true, "If enabled, all shots (instead of only fully charged shots) will pass through enemies on contact.");
        UnchargedShotsImitateChargedShots = Config.Bind("General", "Uncharged Psychogun shots imitate charged shots",
            true, "If enabled, all shots (instead of only fully charged shots) can destroy breakable walls/floors and damage tough enemies.");

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}