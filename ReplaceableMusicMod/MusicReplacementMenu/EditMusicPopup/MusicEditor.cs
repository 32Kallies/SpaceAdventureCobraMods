using System.Collections;
using System.Collections.Generic;
using CobraSoundReplacer.Utils;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.MusicReplacementMenu.MusicEditing;
using MusicReplacer.ReplacementSystem;
using MusicReplacer.Utilities;
using UnityEngine;

namespace MusicReplacer.MusicReplacementMenu.EditMusicPopup;

public class MusicEditor : MonoBehaviour
{
    public RectTransform rect;
    public FileChooserMenu fileChooser;
    public CustomMusicMenu musicMenu;
    
    private readonly List<MusicEditorElementBase> _elements = [];
    private readonly List<ISelectableElement> _selectables = [];

    private int _mainChoice;
    private int _previousChoice;
    private bool _startingUpPreview;

    private MusicPreviewUtils.MusicPreview _preview;

    private MusicSound _activeSound;

    private bool _lastPlayedSoundWasOriginal;
    
    private void Update()
    {
        if (_selectables.Count == 0)
            return;

        if (fileChooser.IsOpen)
            return;
        
        UIController.HandleCursor(ref _mainChoice, _selectables.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate { _selectables[_mainChoice].Interact(); },
            HideWindow, OnChoiceChange, OnChoiceChange);
    }

    public void ShowWindow(MusicSound sound)
    {
        _activeSound = sound;

        if (_elements.Count > 0)
            ClearWindow();

        gameObject.SetActive(true);

        // Header
        AddElement(LabelElement.Create(sound.DisplayName, 130));
        
        // Info
        AddElement(LabelElement.Create("Original file name: " + sound.FileName, 50, 60));
        AddElement(LabelElement.Create("Current sound: " + GetCurrentSoundName()));
        
        // Replace
        AddElement(ButtonElement.Create("Replace this sound", () => fileChooser.Show(sound)));

        // Optional warning(s)
        if (AudioController.Audio.volume[2] <= 0.1f)
            AddElement(LabelElement.Create("<color=#FF0000>WARNING: Music volume is low or disabled!</color>"));
        
        // Preview
        AddElement(ButtonElement.Create("Preview current sound", () => PreviewMusic(false)));
        AddElement(ButtonElement.Create("Preview original sound", () => PreviewMusic(true)));
        
        // Volume slider
        if (MusicReplacementManager.ReplacementData.SoundHasReplacement(sound))
        {
            AddElement(SliderElement.Create("Volume", SetVolume, GetVolume(), 
                0.05f, n => n.ToString("0%"), 130));
            AddElement(LabelElement.Create("Music volume may vary in-game", 50, 60));
        }
        
        // Exit
        AddElement(ButtonElement.Create("Return & Save", HideWindow));

        _mainChoice = 0;
        _previousChoice = 0;
        if (_selectables.Count > 0)
        {
            _selectables[_mainChoice].Select();
        }
    }

    private float GetVolume()
    {
        return MusicReplacementManager.ReplacementData.GetSoundVolume(_activeSound);
    }

    private void SetVolume(float volume)
    {
        MusicReplacementManager.ReplacementData.SetSoundVolume(_activeSound, volume);
        if (_preview != null)
        {
            _preview.SetPreviewVolume(volume);
        }
        MusicMenuBuilder.ShowRestartRequiredWarning();
    }

    private void PreviewMusic(bool originalSound)
    {
        if (StopMusicPreview() && originalSound == _lastPlayedSoundWasOriginal)
            return;

        _lastPlayedSoundWasOriginal = originalSound;
        
        if (_startingUpPreview)
        {
            Plugin.Logger.LogWarning("Already busy attempting to preview music");
            return;
        }
        StartCoroutine(StartSoundPreview(originalSound));
    }

    private IEnumerator StartSoundPreview(bool originalSound)
    {
        _startingUpPreview = true;
        var taskResult = new TaskResult<MusicPreviewUtils.MusicPreview>();
        yield return MusicPreviewUtils.PreviewCurrentMusic(_activeSound, originalSound, taskResult);
        _preview = taskResult.GetResult();
        _startingUpPreview = false;
    }

    private string GetCurrentSoundName()
    {
        if (!MusicReplacementManager.ReplacementData.TryGetCustomSound(_activeSound, out var replacement))
            return "DEFAULT";
        return FileManagement.GetDisplayNameForSoundPath(replacement);
    }

    private bool StopMusicPreview()
    {
        if (_preview == null)
            return false;
        bool stopped = _preview.StopPreview();
        _preview = null;
        return stopped;
    }

    public void HideWindow()
    {
        musicMenu.CloseMusicEditor();
        StopMusicPreview();
        MusicReplacementManager.SaveChanges();
    }

    public bool GetIsShown()
    {
        return isActiveAndEnabled;
    }

    private void OnChoiceChange()
    {
        if (_selectables.Count == 0)
            return;

        if (_previousChoice != _mainChoice && _mainChoice < _selectables.Count)
        {
            _selectables[_previousChoice].Deselect();
        }

        _selectables[_mainChoice].Select();
        _previousChoice = _mainChoice;
    }

    private void ClearWindow()
    {
        foreach (var element in _elements)
        {
            if (element == null)
            {
                Plugin.Logger.LogWarning("Element is null. This should not happen!");
                continue;
            }

            Destroy(element.gameObject);
        }

        _elements.Clear();
        _selectables.Clear();
    }

    private void AddElement(MusicEditorElementBase element)
    {
        element.RectTransform.SetParent(rect);
        element.RectTransform.localScale = Vector3.one;
        if (element is ISelectableElement selectable)
        {
            _selectables.Add(selectable);
        }

        _elements.Add(element);
    }
}