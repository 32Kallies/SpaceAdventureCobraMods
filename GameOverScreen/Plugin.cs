using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GameOverScreen;

[BepInDependency(AuthenticSoundPatchesModGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private const string AuthenticSoundPatchesModGuid = "com.lee23.cobra1982animesoundpatches";
    
    internal static new ManualLogSource Logger;
    
    internal static Assembly Assembly { get; private set; }
    internal static AssetBundle Bundle { get; private set; }
    internal static ConfigEntry<bool> HardcoreConfig { get; private set; }

    private static Plugin _instance;
    
    internal static bool AuthenticSoundPatchesModInstalled { get; private set; }
    
    private void Awake()
    {
        _instance = this;
        Logger = base.Logger;
        
        AuthenticSoundPatchesModInstalled = Chainloader.PluginInfos.ContainsKey(AuthenticSoundPatchesModGuid);
        
        HardcoreConfig = Config.Bind("General", "Hardcore", false, "Enable hardcore mode: restart level on death");
        Assembly = Assembly.GetExecutingAssembly();
        Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.Location), "Assets", "gameoverscreen"));
        
        Harmony.CreateAndPatchAll(Assembly);
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public static Coroutine RunCoroutineOnPlugin(IEnumerator coroutine)
    {
        return _instance.StartCoroutine(coroutine);
    }
}