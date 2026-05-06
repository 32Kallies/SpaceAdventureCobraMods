using MusicReplacer.ReplacementSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.MusicEditing;

public class MusicButton : MonoBehaviour
{
    private Color _normalColor;
    public MusicSound data;
    public Color selectedColor;
    public Image image;
    public GameObject usingCustomSoundVisual;

    private bool _normalColorSet;

    private void Start()
    {
        RefreshVisuals();
    }
    
    public void SetStateSelected(bool selected)
    {
        if (!_normalColorSet)
        {
            _normalColor = image.color;
            _normalColorSet = true;
        }
        image.color = selected ? selectedColor : _normalColor;
    }

    public void RefreshVisuals()
    {
        usingCustomSoundVisual.SetActive(MusicReplacementManager.ReplacementData.SoundHasReplacement(data));
    }
}