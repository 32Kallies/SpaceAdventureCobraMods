namespace MusicReplacer.NewMusicSystem;

public sealed class NewMusicClip
{
    public string ClipName { get; }
    public audioSelectionData.eCLIP CustomClip { get; }

    public NewMusicClip(string clipName, audioSelectionData.eCLIP customClip)
    {
        ClipName = clipName;
        CustomClip = customClip;
    }
}