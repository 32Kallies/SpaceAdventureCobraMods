using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ProjectileSpeedCustomizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    
    internal static Assembly Assembly { get; private set; }

    internal static ConfigEntry<float> RevolverProjectile { get; private set; }
    internal static ConfigEntry<float> PsychogunProjectile { get; private set; }
    internal static ConfigEntry<float> ChargedProjectile { get; private set; }
    internal static ConfigEntry<float> OtherProjectiles { get; private set; }
    internal static ConfigEntry<float> GuidedShot { get; private set; }
    internal static ConfigEntry<float> EnemyProjectiles { get; private set; }
    internal static ConfigEntry<float> JaneBackgroundShot { get; private set; }
    
    private void Awake()
    {
        Logger = base.Logger;
        
        Assembly = Assembly.GetExecutingAssembly();

        var acceptableValues = new AcceptableValueRange<float>(0.1f, 5f);
        
        // Cobra projectiles
        RevolverProjectile = Config.Bind("Cobra", "Revolver Projectile Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for the Revolver shots.", acceptableValues));
        PsychogunProjectile = Config.Bind("Cobra", "Psychogun Projectile Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for the normal Psychogun shots.", acceptableValues));
        ChargedProjectile = Config.Bind("Cobra", "Charged Psychogun Projectile Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for the charged Psychogun shots. This speed should typically be kept around 1 or higher as the projectile itself is invisible.", acceptableValues));
        GuidedShot = Config.Bind("Cobra", "Guided Shot Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for Cobra's guided shot.", acceptableValues));
        
        // Enemies
        EnemyProjectiles = Config.Bind("Enemies", "Enemy Projectile Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for enemy projectiles.", acceptableValues));
        JaneBackgroundShot = Config.Bind("Enemies", "Jane Sniper Projectiles Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for Jane's sniper shots.", acceptableValues));
        
        // Misc
        OtherProjectiles = Config.Bind("Other", "Other Projectiles Speed Multiplier", 1f,
            new ConfigDescription("The speed multiplier for all of other projectiles.", acceptableValues));
        
        Harmony.CreateAndPatchAll(Assembly);
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}