using System;

namespace MusicReplacer.NewMusicSystem;

public sealed class NewMusicClip
{
    public string ClipId { get; }
    [Obsolete]
    public string ClipPath { get; }
    public audioSelectionData.eCLIP CustomClip { get; }

    public NewMusicClip(string clipId, string clipPath, audioSelectionData.eCLIP customClip)
    {
        ClipId = clipId;
        ClipPath = clipPath;
        CustomClip = customClip;
    }
}