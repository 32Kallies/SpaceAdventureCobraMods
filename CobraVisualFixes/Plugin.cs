using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CobraVisualFixes;

[BepInIncompatibility("com.lee23.revolverfix")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static Texture2D NewTexture { get; private set; }
    internal static Texture2D NewSubsurfaceScatteringTexture { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        var assembly = Assembly.GetExecutingAssembly();
        Harmony.CreateAndPatchAll(assembly);

        // LOAD TEXTURE
        
        var modFolder = Path.GetDirectoryName(assembly.Location);
        NewTexture = LoadTexture(Path.Combine(modFolder, "NewCobraTexture.png"), "NewCobraColorTexture");
        NewSubsurfaceScatteringTexture = LoadTexture(Path.Combine(modFolder, "NewCobraSSSTexture.png"), "NewCobraSSSTexture");
        
        // LOG COMPLETION!
        
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private static Texture2D LoadTexture(string path, string name)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(path);

            // 2x2 is a placeholder size, will be replaced by LoadImage
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = name
            };

            if (!texture.LoadImage(fileData))
            {
                Logger.LogError("Failed to decode texture!");
                return null;
            }

            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            return texture;
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load texture, exception thrown: " + e);
            return null;
        }
    }
}