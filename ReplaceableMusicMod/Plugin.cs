using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MusicReplacer.LevelMusic;
using MusicReplacer.NewMusicSystem;
using MusicReplacer.ReplacementSystem;
using MusicReplacer.Utilities;
using UnityEngine;

namespace MusicReplacer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.lee23.cobrasoundreplacer", "1.3.3")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static Assembly Assembly { get; private set; }

    private static Plugin _main;
    
    internal static AssetBundle Bundle { get; private set; }
    
    private void Awake()
    {
        _main = this;
        
        Assembly = Assembly.GetExecutingAssembly();
        Harmony.CreateAndPatchAll(Assembly);

        // Plugin startup logic
        Logger = base.Logger;
        
        // Register main logic
        LevelRipperUtils.LoadLevelData();
        MusicReplacementManager.Register();
        LevelOverrideManager.Initialize();
        NewMusicLoader.LoadNewMusic();

        Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.Location), "Assets", "episodeassets"));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public static void StartCoroutineOnPlugin(IEnumerator coroutine)
    {
        _main.StartCoroutine(coroutine);
    }
}
