using System.Collections.Generic;
using MusicReplacer.LevelMusic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.ReplacementSystem;
using UnityEngine;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class LevelEditorMenu : MonoBehaviour
{
    public LevelMusicMenu menu;
    public RectTransform rect;

    public MusicSwapPopup swap;
    
    private readonly List<MusicEditorElementBase> _elements = [];
    private readonly List<ISelectableElement> _selectables = [];

    private LevelDefinition _currentLevel;
    
    private int _mainChoice;
    private int _previousChoice;
    
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

        var ambientMusic = GetSwappableAmbientMusic(level);
        var battleMusic = GetSwappableBattleMusic(level);
        
        // Info
        AddElement(LabelElement.Create("AMBIENT MUSIC", 80, 100));
        var currentAmbientText = "Current: " + MusicProcessor.GetLoadNameForEClip(ambientMusic.GetCurrentClip());
        if (ambientMusic.defaultClip == ambientMusic.GetCurrentClip())
        {
            currentAmbientText += " (<color=#FFA000>DEFAULT</color>)";
        }
        AddElement(LabelElement.Create(currentAmbientText, 50, 60));
        AddElement(LabelElement.Create("Default: " + MusicProcessor.GetLoadNameForEClip(ambientMusic.defaultClip), 50, 60));
        AddElement(ButtonElement.Create("Change ambient music", () => SwapMusic(ambientMusic), 80));
        
        AddElement(LabelElement.Create("BATTLE MUSIC", 80, 100));
        var currentBattleText = "Current: " + MusicProcessor.GetLoadNameForEClip(battleMusic.GetCurrentClip());
        if (battleMusic.defaultClip == battleMusic.GetCurrentClip())
        {
            currentBattleText += " (<color=#FFA000>DEFAULT</color>)";
        }
        AddElement(LabelElement.Create(currentBattleText, 50, 60));
        AddElement(LabelElement.Create("Default: " + MusicProcessor.GetLoadNameForEClip(battleMusic.defaultClip), 50, 60));
        AddElement(ButtonElement.Create("Change battle music", () => SwapMusic(battleMusic), 80));

        AddElement(LabelElement.Create("LEVEL TRIGGER MAP", 90));
        AddElement(LabelElement.Create("[VISUAL LEVEL TRIGGER MAP GOES HERE]", 90));
        AddElement(LabelElement.Create("Selected trigger ID: N/A", 50, 60));
        AddElement(ButtonElement.Create("RESET TRIGGERS", () => Plugin.Logger.LogMessage("C"), 80));

        _mainChoice = 0;
        _previousChoice = 0;
        if (_selectables.Count > 0)
        {
            _selectables[_mainChoice].Select();
        }
    }

    private SwappableMusic GetSwappableAmbientMusic(LevelDefinition definition)
    {
        var music = GetSwappableMusicBase(definition);
        music.displayText = "Ambient Music";
        music.defaultClip = (audioSelectionData.eCLIP)LevelRipper.GetLevelMusicData(definition.level).DefaultMusic;
        music.overrideClip = (audioSelectionData.eCLIP)LevelOverrideManager.Data.GetLevelData(definition.level).defaultMusic;
        music.pointer = SwappableMusic.MusicPointer.GetAmbient();
        return music;
    }
    
    private SwappableMusic GetSwappableBattleMusic(LevelDefinition definition)
    {
        var music = GetSwappableMusicBase(definition);
        music.displayText = "Battle Music";
        music.defaultClip = (audioSelectionData.eCLIP)LevelRipper.GetLevelMusicData(definition.level).ArenaMusic;
        music.overrideClip =
            (audioSelectionData.eCLIP)LevelOverrideManager.Data.GetLevelData(definition.level).arenaMusic;
        music.pointer = SwappableMusic.MusicPointer.GetBattle();
        return music;
    }

    private SwappableMusic GetSwappableMusicBase(LevelDefinition definition)
    {
        var music = new SwappableMusic
        {
            level = definition.level,
            levelName = GameController.Instance.GetMissionNameText(definition.level)
        };
        return music;
    }

    private void SwapMusic(SwappableMusic music)
    {
        swap.ShowWindow(music);
    }
    
    public void HideWindow()
    {
        menu.levelSelector.gameObject.SetActive(true);
        gameObject.SetActive(false);
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