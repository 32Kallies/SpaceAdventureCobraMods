using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DodgingTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static ConfigEntry<float> StandingStillDodgeSoundVolumeMultiplier;
    public static ConfigEntry<float> InAirDodgeSoundVolumeMultiplier;
    public static ConfigEntry<bool> DisableDodgeVfx;
    public static ConfigEntry<bool> DodgeMaterialSwapFix;
    public static ConfigEntry<bool> DisableDodgeTrailVfx;
    
    private void Awake()
    {
        Logger = base.Logger;

        StandingStillDodgeSoundVolumeMultiplier = Config.Bind("SFX",
            "StandingStillDodgeSoundVolumeMultiplier",
            0f,
            "The volume multiplier for the standing still dodge sound."
        );
        
        InAirDodgeSoundVolumeMultiplier = Config.Bind("SFX",
            "InAirDodgeSoundVolumeMultiplier",
            0f,
            "The volume multiplier for the in-air dodge sound."
        );

        DisableDodgeVfx = Config.Bind("VFX",
            "DisableDodgeVfx",
            true,
            "If true, the player's blue-green dodging visual effect will be disabled."
        );
        
        DodgeMaterialSwapFix = Config.Bind("VFX",
            "DodgeMaterialSwapFix",
            true,
            "If enabled, this will prevent Cobra from using the incorrect material when dodging or sliding."
        );
        
        DisableDodgeTrailVfx = Config.Bind("VFX",
            "DisableDodgeTrailVfx",
            true,
            "If this option is true, the player's blue-green dodging trail or 'afterglow' effect will be disabled."
        );
        
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
