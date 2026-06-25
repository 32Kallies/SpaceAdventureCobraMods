using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic.Data;

[Serializable]
public class GlobalLevelOverrideData
{
    public Dictionary<LevelController.Level, LevelOverrideData> levels;

    public Dictionary<LevelController.Level, LevelOverrideData> GetLevels()
    {
        levels ??= new Dictionary<LevelController.Level, LevelOverrideData>();

        return levels;
    }

    public LevelOverrideData GetLevelData(LevelController.Level level)
    {
        var levels = GetLevels();
        if (!levels.TryGetValue(level, out var data))
        {
            data = new LevelOverrideData();
            levels[level] = data;
        }
        return data;
    }
}