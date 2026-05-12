using System.Collections.Generic;
using MusicReplacer.LevelMusic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.MusicReplacementMenu.Levels.Triggers;
using MusicReplacer.ReplacementSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class LevelEditorMenu : MonoBehaviour
{
    public LevelMusicMenu menu;
    public RectTransform rect;

    public MusicSwapPopup swap;
    
    private readonly List<MusicEditorElementBase> _elements = [];
    private readonly List<ISelectableElement> _selectables = [];

    private LevelDefinition _currentLevel;
    private TriggerMap _map;
    
    private int _mainChoice;
    private int _previousChoice;

    private static readonly SaveState SavedState = new();

    private bool _confirmReset;
    private Text _resetButtonText;
    
    private void Update()
    {
        if (swap.gameObject.activeSelf)
            return;
        
        if (_selectables.Count == 0)
            return;
        
        UIController.HandleCursor(ref _mainChoice, _selectables.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate { _selectables[_mainChoice].Interact(); },
            HideWindow, OnChoiceChange, OnChoiceChange);
    }

    public void Refresh()
    {
        if (_currentLevel != null)
        {
            ShowWindow(_currentLevel);
        }
        else
        {
            Plugin.Logger.LogWarning("Failed to refresh -- no sound set!");
        }
    }
    
    public void ShowWindow(LevelDefinition level)
    {
        _currentLevel = level;
        _confirmReset = false;
        
        if (_elements.Count > 0)
            ClearWindow();

        gameObject.SetActive(true);

        // Header
        AddElement(LabelElement.Create(GameController.Instance.GetMissionNameText(level.level), 130));
        var descriptionText =
            TextsController.Instance.GetText(GameController.Instance.GetLevelDescriptionTextId(level.level));
        var description = LabelElement.Create(descriptionText, 40, 60);
        description.text.color = new Color(1, 0.75f, 0.53f);
        AddElement(description);

        var ambientMusic = SwappableMusic.GetSwappableAmbientMusic(level);
        var battleMusic = SwappableMusic.GetSwappableBattleMusic(level);
        
        // Info
        AddElement(LabelElement.Create("AMBIENT MUSIC", 80, 100));
        var currentAmbientText = "Current: " + MusicProcessor.GetFriendlyNameForEClip(ambientMusic.GetCurrentClip());
        if (ambientMusic.DefaultClip == ambientMusic.GetCurrentClip())
        {
            currentAmbientText += " (<color=#FFA000>DEFAULT</color>)";
        }
        AddElement(LabelElement.Create(currentAmbientText, 50, 60));
        AddElement(LabelElement.Create("Default: " + MusicProcessor.GetFriendlyNameForEClip(ambientMusic.DefaultClip), 50, 60));
        AddElement(ButtonElement.Create("Change ambient music", () => SwapMusic(ambientMusic), 80));
        
        AddElement(LabelElement.Create("BATTLE MUSIC", 80, 100));
        var currentBattleText = "Current: " + MusicProcessor.GetFriendlyNameForEClip(battleMusic.GetCurrentClip());
        if (battleMusic.DefaultClip == battleMusic.GetCurrentClip())
        {
            currentBattleText += " (<color=#FFA000>DEFAULT</color>)";
        }
        AddElement(LabelElement.Create(currentBattleText, 50, 60));
        AddElement(LabelElement.Create("Default: " + MusicProcessor.GetFriendlyNameForEClip(battleMusic.DefaultClip), 50, 60));
        AddElement(ButtonElement.Create("Change battle music", () => SwapMusic(battleMusic), 80));

        AddElement(LabelElement.Create("LEVEL TRIGGER MAP", 90));
        _map = TriggerMap.Create(level);
        AddElement(_map);
        var selectedTriggerText = LabelElement.Create("PLACEHOLDER", 50, 60);
        AddElement(selectedTriggerText);
        _map.SetUp(selectedTriggerText.text, swap, this);
        var resetTriggers = ButtonElement.Create("RESET ALL TRIGGERS", ResetAllTriggers, 40);
        resetTriggers.RectTransform.sizeDelta = new Vector2(
            resetTriggers.RectTransform.sizeDelta.x,
            70);
        _resetButtonText = resetTriggers.Text;
        AddElement(resetTriggers);

        _mainChoice = 0;
        _previousChoice = 0;
        if (_selectables.Count > 0)
        {
            _selectables[_mainChoice].Select();
        }
    }

    private void ResetAllTriggers()
    {
        if (!_confirmReset)
        {
            _resetButtonText.text = "RESET ALL TRIGGERS - PRESS AGAIN TO CONFIRM";
            _confirmReset = true;
            return;
        }
        var levelData = LevelOverrideManager.Data.GetLevelData(_currentLevel.level);
        levelData.GetTriggerReplacements().Clear();
        Refresh();
    }

    private void SwapMusic(SwappableMusic music)
    {
        SaveCurrentState();
        swap.ShowWindow(music);
    }
    
    public void HideWindow()
    {
        menu.levelSelector.gameObject.SetActive(true);
        gameObject.SetActive(false);
        LevelOverrideManager.SaveChanges();
    }

    public void SaveCurrentState()
    {
        SavedState.MainChoice = _mainChoice;
        if (_map != null)
        {
            SavedState.TriggerIndex = _map.GetSelectionIndex();
        }
    }

    public void RestoreState()
    {
        _mainChoice = SavedState.MainChoice;
        OnChoiceChange();
        if (_selectables[_mainChoice] is TriggerMap map)
        {
            map.SetSelectionIndex(SavedState.TriggerIndex);
        }
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

    private class SaveState
    {
        public int MainChoice { get; set; }
        public int TriggerIndex { get; set; }
    }
}