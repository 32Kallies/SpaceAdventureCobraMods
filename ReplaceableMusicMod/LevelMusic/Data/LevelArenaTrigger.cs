using System;

namespace MusicReplacer.LevelMusic.Data;

[Serializable]
public class LevelArenaTrigger : LevelTrigger
{
    public bool PlayArenaJingle { get; set; }
    public bool PlayArenaWallIn { get; set; }
    public bool PlayArenaWallOut { get; set; }
    public bool PlayArenaVictory { get; set; }
    public bool DestroyAtEnd { get; set; }
}