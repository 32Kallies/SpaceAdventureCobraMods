using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GameOverScreen;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    
    internal static Assembly Assembly { get; private set; }
    internal static AssetBundle Bundle { get; private set; }
    internal static ConfigEntry<bool> HardcoreConfig { get; private set; }

    private static Plugin _instance;
    
    private void Awake()
    {
        _instance = this;
        Logger = base.Logger;
        
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