using System.Collections.Generic;

namespace Cobra1982AnimeSoundPatches;

public sealed class SoundReplacementData
{
    public static readonly SoundReplacementData Enemies = new()
    {
        Replacements = new Dictionary<audioSelectionData.eCLIP, SoundClipReplacements>
        {
            // RIFLE SOUNDS
            {
                audioSelectionData.eCLIP.NMI_SHOOT_RIFLE, new SoundClipReplacements([
                    new PrefixReplacement("NmiCrystalBoy_Light_ShotBasic", "1982_anime_NPC_shoot"),
                    new PrefixReplacement("NmiZigoba_Light", "1_2_stage_and_2_1_normal_shot"),
                    new PrefixReplacement("NmiMindSlave_Light", "brainwashed_female_shot"),
                    new PrefixReplacement("NmiMindSlave_Elite", "brainwashed_female_shot"),
                    new PrefixReplacement("NmiSnowGuerillas_Elite", "snow_gorilla_shot"),
                    new PrefixReplacement("NmiSnowGuerillas_Light", "snow_gorilla_shot"),
                ])
            },
            // SHOTGUN SOUNDS
            {
                audioSelectionData.eCLIP.NMI_SHOOT_MULTI, new SoundClipReplacements([
                    new PrefixReplacement("NmiCrystalBoy_Elite_ShotSpread", "1982_anime_NPC_multi_shoot"),
                    new PrefixReplacement("NmiZigoba_Elite", "1_2_stage_and_2_1_spread_shot"),
                    new PrefixReplacement("NmiMindSlave_Elite", "brainwashed_female_shot"),
                    new PrefixReplacement("NmiSnowGuerillas_Elite", "snow_gorilla_shot"),
                    new PrefixReplacement("NmiSnowGuerillas_Light", "snow_gorilla_shot"),
                ])
            }
        }
    };

    public Dictionary<audioSelectionData.eCLIP, SoundClipReplacements> Replacements { get; private set; }

    public sealed class SoundClipReplacements(PrefixReplacement[] replacements)
    {
        public PrefixReplacement[] Replacements { get; } = replacements;
    }

    public sealed class PrefixReplacement(string namePrefix, string newCustomEClipName)
    {
        public string NamePrefix { get; } = namePrefix;
        public string NewCustomEClipName { get; } = newCustomEClipName;
    }
}