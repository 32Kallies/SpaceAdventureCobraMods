using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic;

[Serializable]
public class GameLevelData
{
    public Dictionary<int, LevelMusicData> Levels { get; set; }
}