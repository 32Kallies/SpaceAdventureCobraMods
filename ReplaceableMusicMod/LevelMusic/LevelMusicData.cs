using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic;

[Serializable]
public class LevelMusicData
{
    public string LevelName { get; set; }
    public int LevelId { get; set; }
    public int DefaultMusic { get; set; }
    public int ArenaMusic { get; set; }
    public Dictionary<int, LevelTrigger> LevelTriggers { get; set; }
}