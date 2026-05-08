using System.Collections;
using System.Collections.Generic;
using CobraSoundReplacer.Utils;
using MusicReplacer.LevelMusic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.ReplacementSystem;
using UnityEngine;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class MusicSwapPopup : MonoBehaviour
{
    public LevelMusicMenu menu;
    public RectTransform rect;
    
    private readonly List<MusicEditorElementBase> _elements = [];
    private readonly List<ISelectableElement> _selectables = [];

    private int _mainChoice;
    private int _previousChoice;
    
    private bool _startingUpPreview;
    private MusicPreviewUtils.MusicPreview _preview;

    private SwappableMusic _activeMusic;
    
    private void Update()
    {
        if (_selectables.Count == 0)
            return;
        
        UIController.HandleCursor(ref _mainChoice, _selectables.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate { _selectables[_mainChoice].Interact(); },
            HideWindow, OnChoiceChange, OnChoiceChange);
    }
    
    public void ShowWindow(SwappableMusic music)
    {
        _activeMusic = music;
        
        if (_elements.Count > 0)
            ClearWindow();

        gameObject.SetActive(true);

        // Header
        AddElement(LabelElement.Create($"Music Data for {music.levelName} - {music.displayText}", 130));
        AddElement(LabelElement.Create("Level: " + music.levelName, 50, 60));
        
        // Info
        var currentMusicName = "Current: " + MusicProcessor.GetLoadNameForEClip(music.GetCurrentClip());
        if (music.defaultClip == music.GetCurrentClip())
        {
            currentMusicName += " (<color=#FFA000>DEFAULT</color>)";
        }
        AddElement(LabelElement.Create(currentMusicName, 60, 100));
        AddElement(LabelElement.Create("Default: " + MusicProcessor.GetLoadNameForEClip(music.defaultClip), 60, 100));
        
        // Buttons
        AddElement(ButtonElement.Create("Set Music Override", () => SetClip(audioSelectionData.eCLIP.MUS_MAINTHEME), 80));
        AddElement(ButtonElement.Create("Preview Current", () => PreviewMusic(false), 80));
        AddElement(ButtonElement.Create("Preview Default", () => PreviewMusic(true), 80));
        AddElement(ButtonElement.Create("Reset to Default", ResetToDefault, 80));
        AddElement(ButtonElement.Create("Return & Save", HideWindow, 80));

        _mainChoice = 0;
        _previousChoice = 0;
        if (_selectables.Count > 0)
        {
            _selectables[_mainChoice].Select();
        }
    }
    
    private void Refresh()
    {
        if (_activeMusic == null)
        {
            Plugin.Logger.LogWarning("Failed to refresh -- active music not set!");
            return;
        }

        StopMusicPreview();
        ShowWindow(_activeMusic);
    }

    private void ResetToDefault()
    {
        SetClip(audioSelectionData.eCLIP.NONE);
        HideWindow();
    }

    private void SetClip(audioSelectionData.eCLIP newClip)
    {
        var data = LevelOverrideManager.Data.GetLevelData(_activeMusic.level);
        var pointer = _activeMusic.pointer;
        switch (pointer.GetCategory())
        {
            case SwappableMusic.MusicCategory.Ambient:
                data.defaultMusic = (int)newClip;
                break;
            case SwappableMusic.MusicCategory.Battle:
                data.arenaMusic = (int)newClip;
                break;
            case SwappableMusic.MusicCategory.Trigger:
                data.GetTriggerReplacements()[pointer.GetTriggerId()] = (int)newClip;
                break;
            default:
                Plugin.Logger.LogWarning("Undefined music category: " + pointer.GetCategory());
                break;
        }

        _activeMusic.overrideClip = newClip;
        Refresh();
        MusicMenuBuilder.ShowRestartRequiredWarning();
    }

    private void PreviewMusic(bool originalSound)
    {
        if (_startingUpPreview)
        {
            Plugin.Logger.LogWarning("Already busy attempting to preview music");
            return;
        }
        
        if (StopMusicPreview())
            return;

        StartCoroutine(StartSoundPreview(originalSound));
    }
    
    private IEnumerator StartSoundPreview(bool originalSound)
    {
        _startingUpPreview = true;
        var taskResult = new TaskResult<MusicPreviewUtils.MusicPreview>();
        var music = _activeMusic.defaultClip;
        if (!originalSound && _activeMusic.overrideClip != audioSelectionData.eCLIP.NONE)
            music = _activeMusic.overrideClip;
        yield return MusicPreviewUtils.PreviewEClip(music, false, taskResult);
        _preview = taskResult.GetResult();
        _startingUpPreview = false;
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
        gameObject.SetActive(false);
        StopMusicPreview();
        menu.levelEditor.Refresh();
        LevelOverrideManager.SaveChanges();
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