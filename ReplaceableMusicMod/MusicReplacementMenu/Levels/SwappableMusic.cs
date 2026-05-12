using MusicReplacer.LevelMusic;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class SwappableMusic
{
    public string LevelName { get; private set; }
    public string DisplayText { get; private set; }
    public audioSelectionData.eCLIP DefaultClip { get; private set; }
    public audioSelectionData.eCLIP OverrideClip { get; private set; }
    
    private LevelController.Level _level;
    private MusicPointer _pointer;

    public audioSelectionData.eCLIP GetCurrentClip()
    {
        var current = DefaultClip;
        if (OverrideClip != audioSelectionData.eCLIP.NONE)
            current = OverrideClip;
        return current;
    }
    
    public static SwappableMusic CreateSwappableTriggerMusic(LevelDefinition definition, LevelTrigger triggerData)
    {
        var overrideClip = audioSelectionData.eCLIP.NONE;
        if (LevelOverrideManager.Data.GetLevelData(definition.level).GetTriggerReplacements().TryGetValue(triggerData.Hash.ToString(), out var clip))
        {
            overrideClip = (audioSelectionData.eCLIP)clip;
        }
        var music = new SwappableMusic
        {
            _level = definition.level,
            LevelName = GameController.Instance.GetMissionNameText(definition.level),
            DisplayText = $"Trigger {triggerData.Hash} Music",
            DefaultClip = (audioSelectionData.eCLIP)triggerData.MusicClip,
            OverrideClip = overrideClip,
            _pointer = MusicPointer.GetTrigger(triggerData.Hash.ToString())
        };
        
        return music;
    }
    
    public static SwappableMusic GetSwappableAmbientMusic(LevelDefinition definition)
    {
        var music = GetSwappableMusicBase(definition);
        music.DisplayText = "Ambient Music";
        music.DefaultClip = (audioSelectionData.eCLIP)LevelRipper.GetLevelMusicData(definition.level).DefaultMusic;
        music.OverrideClip = (audioSelectionData.eCLIP)LevelOverrideManager.Data.GetLevelData(definition.level).defaultMusic;
        music._pointer = MusicPointer.GetAmbient();
        return music;
    }
    
    public static SwappableMusic GetSwappableBattleMusic(LevelDefinition definition)
    {
        var music = GetSwappableMusicBase(definition);
        music.DisplayText = "Battle Music";
        music.DefaultClip = (audioSelectionData.eCLIP)LevelRipper.GetLevelMusicData(definition.level).ArenaMusic;
        music.OverrideClip =
            (audioSelectionData.eCLIP)LevelOverrideManager.Data.GetLevelData(definition.level).arenaMusic;
        music._pointer = MusicPointer.GetBattle();
        return music;
    }

    private static SwappableMusic GetSwappableMusicBase(LevelDefinition definition)
    {
        var music = new SwappableMusic
        {
            _level = definition.level,
            LevelName = GameController.Instance.GetMissionNameText(definition.level)
        };
        return music;
    }

    public void SetClip(audioSelectionData.eCLIP newClip)
    {
        OverrideClip = newClip;
        var data = LevelOverrideManager.Data.GetLevelData(_level);
        switch (_pointer.GetCategory())
        {
            case MusicCategory.Ambient:
                data.defaultMusic = (int)newClip;
                break;
            case MusicCategory.Battle:
                data.arenaMusic = (int)newClip;
                break;
            case MusicCategory.Trigger:
                if (newClip == audioSelectionData.eCLIP.NONE)
                    data.GetTriggerReplacements().Remove(_pointer.GetTriggerId());
                else
                    data.GetTriggerReplacements()[_pointer.GetTriggerId()] = (int)newClip;
                break;
            default:
                Plugin.Logger.LogWarning("Undefined music category: " + _pointer.GetCategory());
                break;
        }
    }

    public class MusicPointer
    {
        private MusicPointer() { }

        public static MusicPointer GetAmbient()
        {
            return new MusicPointer
            {
                _category = MusicCategory.Ambient
            };
        }
        
        public static MusicPointer GetBattle()
        {
            return new MusicPointer
            {
                _category = MusicCategory.Battle
            };
        }
        
        public static MusicPointer GetTrigger(string id)
        {
            return new MusicPointer
            {
                _category = MusicCategory.Trigger,
                _triggerId = id
            };
        }
        
        private MusicCategory _category;

        private string _triggerId;
        
        public MusicCategory GetCategory()
        {
            return _category;
        }

        public string GetTriggerId()
        {
            if (GetCategory() != MusicCategory.Trigger)
            {
                Plugin.Logger.LogError("Pointer category is not Trigger");
            }

            return _triggerId;
        }
    }

    public enum MusicCategory
    {
        Ambient,
        Battle,
        Trigger
    }
}