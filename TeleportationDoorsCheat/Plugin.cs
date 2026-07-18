using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace TeleportationDoorsCheat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static ConfigEntry<bool> AllowTeleportationCheatWithHotkey { get; private set; }
    internal static ConfigEntry<bool> AlwaysEnableCheat { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        AllowTeleportationCheatWithHotkey = Config.Bind("General", "AllowTeleportationCheatWithHotkey", true,
            "Allows using the key combination of F2+F6 to activate teleportation doors.");
        AlwaysEnableCheat = Config.Bind("General", "AlwaysEnableCheat", false,
            "Always shows teleportation doors when loading into a stage.");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}