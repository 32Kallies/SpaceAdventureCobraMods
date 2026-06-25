using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic.Data;

[Serializable]
public class RippedLevelMusicData
{
    public string LevelName { get; set; }
    public int LevelId { get; set; }
    public int DefaultMusic { get; set; }
    public int ArenaMusic { get; set; }
    public Dictionary<int, LevelTrigger> LevelTriggers { get; set; }
    public Dictionary<int, LevelArenaTrigger> ArenaTriggers { get; set; }
}