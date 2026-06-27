using MusicReplacer.LevelMusic.Data;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels.Triggers;

public class EditableTrigger
{
    public LevelTrigger RawData { get; set; }
    public SwappableMusic Music { get; set; }
    public Image Image { get; set; }
    public bool Custom { get; set; }
    public bool IsNewTrigger { get; set; }
    public bool IsArena { get; set; }

    public override string ToString()
    {
        if (RawData != null)
        {
            return "Trigger-" + RawData.Hash;
        }

        return ToString();
    }
}