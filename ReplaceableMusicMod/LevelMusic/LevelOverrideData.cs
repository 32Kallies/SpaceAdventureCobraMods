using System;
using System.Collections.Generic;

namespace MusicReplacer.LevelMusic;

[Serializable]
public class LevelOverrideData
{
    public int defaultMusic;
    public int arenaMusic;
    public Dictionary<string, int> triggerReplacements;

    public Dictionary<string, int> GetTriggerReplacements()
    {
        triggerReplacements ??= new Dictionary<string, int>();
        return triggerReplacements;
    }
}