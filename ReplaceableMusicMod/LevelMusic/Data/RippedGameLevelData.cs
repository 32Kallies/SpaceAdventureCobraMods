using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic.Data;

[Serializable]
public class RippedGameLevelData
{
    public Dictionary<int, RippedLevelMusicData> Levels { get; set; }
}