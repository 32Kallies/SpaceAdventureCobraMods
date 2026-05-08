namespace MusicReplacer.MusicReplacementMenu.Levels;

public class SwappableMusic
{
    public LevelController.Level level;
    public string levelName;
    public string displayText;
    public audioSelectionData.eCLIP defaultClip;
    public audioSelectionData.eCLIP overrideClip;
    public MusicPointer pointer;

    public audioSelectionData.eCLIP GetCurrentClip()
    {
        var current = defaultClip;
        if (overrideClip != audioSelectionData.eCLIP.NONE)
            current = overrideClip;
        return current;
    }

    public class MusicPointer
    {
        private MusicPointer()
        {
            
        }

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