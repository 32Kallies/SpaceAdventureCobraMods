using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MusicReplacer.CustomTriggers;

public static class CustomTriggerParser
{
    public static List<CustomTrigger> ParseAllFiles(string inFolder)
    {
        var result = new List<CustomTrigger>();
        var filesInFolder = Directory.GetFiles(inFolder, "*.txt", SearchOption.AllDirectories);
        foreach (var file in filesInFolder)
        {
            var triggers = ParseFile(File.ReadAllText(file));
            foreach (var trigger in triggers)
            {
                ValidateLevelTrigger(file, trigger);
            }
            result.AddRange(triggers);
        }
        return result;
    }
    
    private static List<CustomTrigger> ParseFile(string input)
    {
        var cleaned = StripExamples(input);
        var blocks = SplitBlocks(cleaned);

        var result = new List<CustomTrigger>();

        foreach (var block in blocks)
        {
            var trigger = ParseBlock(block);
            if (trigger != null)
            {
                result.Add(trigger);
            }
        }

        return result;
    }

    // IGNORE EXAMPLE
    private static string StripExamples(string input)
    {
        const string start = "[EXAMPLE]";
        const string end = "[EXAMPLE END]";

        while (true)
        {
            var s = input.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (s < 0) break;

            var e = input.IndexOf(end, s, StringComparison.OrdinalIgnoreCase);
            if (e < 0) break;

            input = input.Remove(s, e + end.Length - s);
        }

        return input;
    }

    // SPLIT INTO BLOCKS
    private static List<string> SplitBlocks(string input)
    {
        return input
            .Split(["---"], StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToList();
    }

    // PARSE AN INDIVIDUAL BLOCK
    private static CustomTrigger ParseBlock(string block)
    {
        var lines = block
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var kvpSeparatorIndex = line.IndexOf(':');
            if (kvpSeparatorIndex <= 0) continue;

            var key = line[..kvpSeparatorIndex].Trim();
            var value = line[(kvpSeparatorIndex + 1)..].Trim();

            dict[key] = value;
        }

        if (!dict.TryGetValue("Name", out var name))
            return null;

        string music = string.Empty;
        if (dict.TryGetValue("Music", out var musicFullName))
        {
            music = Path.GetFileNameWithoutExtension(musicFullName);
        }
        dict.TryGetValue("Level", out var level);

        int priority = ParseInt(dict.GetValueOrDefault("Priority"));

        // DETECT TYPE

        // Simple box
        if (dict.ContainsKey("Center") && dict.ContainsKey("Size"))
        {
            var t = new CenterBoxTrigger
            {
                Name = name,
                Music = music ?? string.Empty,
                Level = level ?? string.Empty,
                Priority = priority,
                Center = ParseVector2(dict["Center"]),
                Size = ParseSize(dict["Size"])
            };

            return t;
        }

        // Between corners
        if (dict.ContainsKey("Corner 1") && dict.ContainsKey("Corner 2"))
        {
            var t = new CornerBoxTrigger
            {
                Name = name,
                Music = music ?? string.Empty,
                Level = level ?? string.Empty,
                Priority = priority,
                Corner1 = ParseVector2(dict["Corner 1"]),
                Corner2 = ParseVector2(dict["Corner 2"])
            };

            return t;
        }

        // Vertical line (left to right)
        if (dict.ContainsKey("Left Coordinate") && dict.ContainsKey("Right Coordinate"))
        {
            var t = new VerticalLineTrigger
            {
                Name = name,
                Music = music ?? string.Empty,
                Level = level ?? string.Empty,
                Priority = priority,
                Left = ParseFloat(dict["Left Coordinate"]),
                Right = ParseFloat(dict["Right Coordinate"])
            };

            return t;
        }

        return null;
    }

    private static (float X, float Y) ParseVector2(string s)
    {
        var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return (
            ParseFloat(parts.ElementAtOrDefault(0)),
            ParseFloat(parts.ElementAtOrDefault(1))
        );
    }

    private static (float W, float H) ParseSize(string s)
    {
        var parts = s.Split('x', StringSplitOptions.RemoveEmptyEntries);
        return (
            ParseFloat(parts.ElementAtOrDefault(0)),
            ParseFloat(parts.ElementAtOrDefault(1))
        );
    }

    private static float ParseFloat(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return 0;

        return float.TryParse(
            s.Trim(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var value
        )
            ? value
            : 0;
    }

    private static int ParseInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return 0;

        return int.TryParse(
            s.Trim(),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var value
        )
            ? value
            : 0;
    }
    
    private static void ValidateLevelTrigger(string textFilePath, CustomTrigger trigger)
    {
        var fileName = Path.GetFileName(textFilePath);

        if (!ValidateLevelString(trigger.Level))
        {
            Warn("Invalid level string: " + trigger.Level);
        }

        if (trigger.Music.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Warn("Invalid music filename: " + trigger.Music);
        }

        if (trigger is CenterBoxTrigger centerBoxTrigger &&
            (centerBoxTrigger.Size.W == 0 || centerBoxTrigger.Size.H == 0))
        {
            Warn($"Invalid box size ({centerBoxTrigger.Size.W}, {centerBoxTrigger.Size.H})");
        }
        
        if (trigger is CornerBoxTrigger cornerTrigger &&
            Mathf.Approximately(cornerTrigger.Corner1.X, cornerTrigger.Corner2.X) |
            Mathf.Approximately(cornerTrigger.Corner1.Y, cornerTrigger.Corner2.Y))
        {
            Warn("Invalid box size (corners overlap)");
        }
        
        if (trigger is VerticalLineTrigger verticalLine && Mathf.Approximately(verticalLine.Left, verticalLine.Right))
        {
            Warn("Invalid horizontal size (zero width; left and right points are identical)");
        }

        return;
        
        void Warn(string message)
        {
            Plugin.Logger.LogWarning($"Issue detected in trigger '{trigger.Name}' ({fileName}) - {message}");
        }
    }
    
    private static bool ValidateLevelString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        if (!parts[0].StartsWith("EP", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!parts[1].StartsWith("LV", StringComparison.OrdinalIgnoreCase))
            return false;

        return int.TryParse(parts[0][2..], out _)
               && int.TryParse(parts[1][2..], out _);
    }
}