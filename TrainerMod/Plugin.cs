using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TrainerMod.Enums;

namespace TrainerMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static ConfigEntry<float> PsychogunDamage { get; private set; }
    public static ConfigEntry<float> PsychogunPenetratingShotDamage { get; private set; }
    public static ConfigEntry<float> CurvedShotDamage { get; private set; }
    public static ConfigEntry<GuidedShotMode> CurvedShotMode { get; private set; }
    public static ConfigEntry<float> SuperShotDamageNormal { get; private set; }
    public static ConfigEntry<float> SuperShotDamageElites { get; private set; }
    public static ConfigEntry<bool> UnlimitedSuperShots { get; private set; }
    public static ConfigEntry<float> RevolverDamage { get; private set; }
    public static ConfigEntry<bool> LimitRevolverDamageAgainstCrystalBoy { get; private set; }
    public static ConfigEntry<float> CigarDamage { get; private set; }
    public static ConfigEntry<bool> UnlimitedCigars { get; private set; }
    public static ConfigEntry<float> CobraHealth { get; private set; }

    private const float DefaultPsychogunDamage = 2f;
    private const float DefaultPenetratingPsychogunShotDamage = 2f;
    private const float DefaultCurvedShotDamage = 6f;
    public const float DefaultSuperShotDamageElites = 10f;
    public const float DefaultSuperShotDamageNormal = 1000f;
    private const float DefaultRevolverDamage = 0.5f; // Check if this is correct
    private const float DefaultCigarDamage = 10f;
    private const float DefaultCobraHealth = 8f;

    private const string SettingFormat = "{0}";
    
    private const string SettingFormatNumbers =
        "{0} A value of -1 means this is NOT used and that the game will use the default value.";

    private void Awake()
    {
        Logger = base.Logger;

        RegisterConfig();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void RegisterConfig()
    {
        // Psychogun
        PsychogunDamage = Config.Bind("Psychogun", "Psychogun shot damage", DefaultPsychogunDamage,
            new ConfigDescription(FormatNumberDesc("The damage of the charged Psychogun shot.")));
        PsychogunPenetratingShotDamage = Config.Bind("Psychogun", "Fully charged psychogun shot damage",
            DefaultPenetratingPsychogunShotDamage,
            new ConfigDescription(FormatNumberDesc("The damage of the charged Psychogun shot.")));
        
        // Guided shot
        CurvedShotDamage = Config.Bind("Guided Shot", "Guided shot damage",
            DefaultCurvedShotDamage,
            new ConfigDescription(FormatNumberDesc("The damage of the guided shot.")));
        CurvedShotMode = Config.Bind("Guided Shot", "Guided shot mode",
            GuidedShotMode.Normal,
            new ConfigDescription(FormatDesc("How often the guided shot can be used.")));
        
        // Super shot
        SuperShotDamageNormal = Config.Bind("Super Shot", "Super shot damage - Normal enemies",
            DefaultSuperShotDamageNormal,
            new ConfigDescription(FormatNumberDesc("The super shot damage against most enemy types.")));
        SuperShotDamageElites = Config.Bind("Super Shot", "Super shot damage - Elite enemies",
            DefaultSuperShotDamageElites,
            new ConfigDescription(FormatNumberDesc("The super shot damage against elite enemies and mini bosses.")));
        UnlimitedSuperShots = Config.Bind("Super Shot", "Unlimited super shots",
            false,
            new ConfigDescription(FormatDesc("If true, the requirements for the super shot are disabled.")));
        
        // Revolver
        RevolverDamage = Config.Bind("Revolver", "Revolver damage",
            DefaultRevolverDamage,
            new ConfigDescription(FormatNumberDesc("The damage of the Python 77 Revolver.")));
        LimitRevolverDamageAgainstCrystalBoy = Config.Bind("Revolver", "Limit revolver damage against Crystal Boy",
            true,
            new ConfigDescription(FormatDesc("If true, Crystal Boy will be immune to extra revolver damage (recommended).")));

        // Cigars
        CigarDamage = Config.Bind("Cigars", "Explosive cigar damage",
            DefaultCigarDamage,
            new ConfigDescription(FormatNumberDesc("The damage of the explosive cigars.")));
        UnlimitedCigars = Config.Bind("Cigars", "Unlimited cigars",
            false,
            new ConfigDescription(FormatDesc("If true, the cooldown on cigars is disabled.")));
        
        // Cobra
        CobraHealth = Config.Bind("Cobra", "Cobra health",
            DefaultCobraHealth,
            new ConfigDescription(FormatNumberDesc("The maximum health of Cobra.")));
    }

    private static string FormatNumberDesc(string description)
    {
        return string.Format(SettingFormatNumbers, description);
    }
    
    private static string FormatDesc(string description)
    {
        return string.Format(SettingFormat, description);
    }
}