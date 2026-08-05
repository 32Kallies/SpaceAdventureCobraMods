namespace Cobra1982AnimeSoundPatches.Patches.NmiMobilePatches;

public sealed class NmiShootSound(audioSelectionData.eCLIP clip)
{
    public audioSelectionData.eCLIP Clip { get; init; } = clip;
}