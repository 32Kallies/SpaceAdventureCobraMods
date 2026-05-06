using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic;

[Serializable]
public class GlobalLevelOverrideData
{
    public Dictionary<LevelController.Level, LevelOverrideData> data;
}