using System.Collections.Generic;
using System.IO;
using MusicReplacer.Utilities;

namespace MusicReplacer.MusicReplacementMenu.Levels.Triggers;

public class SimilarTriggersGroup
{
    private readonly Dictionary<int, int> _positionalHashByHash = new();
    private readonly Dictionary<int, HashSet<EditableTrigger>> _triggerGroupByPositionalHash = new();

    public void AddTrigger(EditableTrigger trigger)
    {
        var positionalHash =
            TriggerUtils.GenerateTriggerHash(trigger.RawData.Center.ToVector3(), trigger.RawData.Size.ToVector3());
        if (_triggerGroupByPositionalHash.TryGetValue(positionalHash, out var set))
        {
            set.Add(trigger);
        }
        else
        {
            var newSet = new HashSet<EditableTrigger>();
            newSet.Add(trigger);
            _triggerGroupByPositionalHash.Add(positionalHash, newSet);
        }
        _positionalHashByHash.Add(trigger.RawData.Hash, positionalHash);
    }

    public string GetFilePathToPreviewScreenshotForTriggerOrNull(string levelName, EditableTrigger trigger)
    {
        if (!_positionalHashByHash.TryGetValue(trigger.RawData.Hash, out var positionalHash))
        {
            Plugin.Logger.LogWarning("Failed to get positional hash for trigger: " + trigger);
            return null;
        }
        if (!_triggerGroupByPositionalHash.TryGetValue(positionalHash, out var set))
        {
            Plugin.Logger.LogWarning("Failed to get set for positional hash: " + positionalHash);
            return null;
        }

        foreach (var candidate in set)
        {
            var path = ScreenshotGenerator.GetScreenshotFilePath(levelName, candidate.RawData.Hash);
            var exists = File.Exists(path);
            if (exists)
            {
                return path;
            }
        }

        return null;
    }
}