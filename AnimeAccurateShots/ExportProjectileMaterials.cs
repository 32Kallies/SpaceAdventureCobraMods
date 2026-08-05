/*
using System.IO;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace AuthenticShootVfx;

[HarmonyPatch]
public static class ExportProjectileMaterials
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Update))]
    private static void Sample()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SaveDetails();
        }
    }

    private static void SaveDetails()
    {
        var projectiles = Object.FindObjectsOfType<Projectile>();
        foreach (var projectile in projectiles)
        {
            SaveProjectileDetails(projectile);
        }
    }

    private static void SaveProjectileDetails(Projectile projectile)
    {
        string name = projectile.gameObject.name;

        StringBuilder text = new StringBuilder(name);
        text.AppendLine();
        text.AppendLine();

        text.AppendLine("RENDERERS");
        text.AppendLine("=-------------------------------------------=");

        var renderers = projectile.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            if (renderer is TrailRenderer)
            {
                continue;
            }
            
            text.AppendLine(renderer.name + " - " + renderer.GetType());
            text.AppendLine();
            text.AppendLine("\tMATERIALS");
            foreach (var material in renderer.sharedMaterials)
            {
                PrintMaterialDetails(text, material);
            }
        }
        
        if (renderers.Length == 0)
        {
            text.AppendLine("[None]");
        }
        
        text.AppendLine("TRAIL RENDERERS");
        text.AppendLine("=-------------------------------------------=");

        var trails = projectile.GetComponentsInChildren<TrailRenderer>();
        foreach (var trailRenderer in trails)
        {
            text.AppendLine(trailRenderer.name);
            text.AppendLine();
            text.AppendLine("\tStart color: " + trailRenderer.startColor);
            text.AppendLine("\tEnd color: " + trailRenderer.endColor);
            text.AppendLine();
            if (trailRenderer.material == null) continue;
            text.AppendLine("\tMATERIAL");
            PrintMaterialDetails(text, trailRenderer.sharedMaterial);
        }
        
        if (trails.Length == 0)
        {
            text.AppendLine("[None]");
        }
        
        text.AppendLine("LIGHTS");
        text.AppendLine("=-------------------------------------------=");

        var lights = projectile.GetComponentsInChildren<Light>();
        foreach (var light in lights)
        {
            text.AppendLine(light.name);
            text.AppendLine();
            text.AppendLine("\tColor: " + light.color);
        }

        if (lights.Length == 0)
        {
            text.AppendLine("[None]");
        }

        var path = GetSavePath(name);
        File.WriteAllText(path, text.ToString());
    }

    private static void PrintMaterialDetails(StringBuilder sb, Material material)
    {
        if (material == null)
        {
            sb.AppendLine("NULL");
            return;
        }
        sb.AppendLine("\t" + material.name);
        
        sb.AppendLine("\tTEXTURES");
        foreach (string textureProperty in material.GetTexturePropertyNames())
        {
            var texture = material.GetTexture(textureProperty);
            string textureName = texture == null ? "NULL" : texture.name;
            sb.AppendLine($"\t\"{textureProperty}\": \"{textureName}\"");
        }
        
        sb.AppendLine();
        sb.AppendLine("\tFLOATS");
        foreach (string floatProperty in material.GetPropertyNames(MaterialPropertyType.Float))
        {
            var value = material.GetFloat(floatProperty);
            sb.AppendLine($"\t\"{floatProperty}\": \"{value}\"");
        }
        
        sb.AppendLine();
        sb.AppendLine("\tINT");
        foreach (string intProperty in material.GetPropertyNames(MaterialPropertyType.Int))
        {
            var value = material.GetInt(intProperty);
            sb.AppendLine($"\t\"{intProperty}\": \"{value}\"");
        }
        
        sb.AppendLine();
        sb.AppendLine("\tVECTOR");
        foreach (string vectorProperty in material.GetPropertyNames(MaterialPropertyType.Vector))
        {
            var value = material.GetVector(vectorProperty);
            sb.AppendLine($"\t\"{vectorProperty}\": \"{value}\"");
        }

        sb.AppendLine("\tEND MATERIAL");
    }

    private static string GetSavePath(string fileNameWithoutExtension)
    {
        var folder = "Projectile Exports";
        
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return Path.Combine(folder, fileNameWithoutExtension + ".txt");
    }
}
*/