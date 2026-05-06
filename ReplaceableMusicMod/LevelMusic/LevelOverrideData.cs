using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic;

[Serializable]
public class LevelOverrideData
{
    public int defaultMusic;
    public int arenaMusic;
    public Dictionary<string, int> triggerReplacements;
}