using System.Collections.Generic;
using MusicReplacer.LevelMusic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using UnityEngine;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class LevelEditorMenu : MonoBehaviour
{
    public LevelMusicMenu menu;
    public RectTransform rect;
    
    private readonly List<MusicEditorElementBase> _elements = [];
    private readonly List<ISelectableElement> _selectables = [];

    private int _mainChoice;
    private int _previousChoice;
    
    private void Update()
    {
        if (_selectables.Count == 0)
            return;
        
        UIController.HandleCursor(ref _mainChoice, _selectables.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate { _selectables[_mainChoice].Interact(); },
            HideWindow, OnChoiceChange, OnChoiceChange);
    }
    
    public void ShowWindow(LevelDefinition level)
    {
        if (_elements.Count > 0)
            ClearWindow();

        gameObject.SetActive(true);

        // Header
        AddElement(LabelElement.Create(GameController.Instance.GetMissionNameText(level.level), 130));
        var description =
            LabelElement.Create(
                TextsController.Instance.GetText(GameController.Instance.GetLevelDescriptionTextId(level.level)), 40,
                60);
        description.text.color = new Color(1, 0.75f, 0.53f);
        AddElement(description);

        var defaultMusic = (audioSelectionData.eCLIP)LevelRipper.GetLevelMusicData(level.level).DefaultMusic;
        var arenaMusic = (audioSelectionData.eCLIP)LevelRipper.GetLevelMusicData(level.level).ArenaMusic;
        
        // Info
        AddElement(LabelElement.Create("AMBIENT MUSIC", 80, 100));
        AddElement(LabelElement.Create("Default: " + defaultMusic, 50, 60));
        AddElement(LabelElement.Create("Current: " + defaultMusic, 50, 60));
        AddElement(ButtonElement.Create("Change ambient music", () => Plugin.Logger.LogMessage("A"), 80));
        AddElement(LabelElement.Create("BATTLE MUSIC", 80, 100));
        AddElement(LabelElement.Create("Default: " + arenaMusic, 50, 60));
        AddElement(LabelElement.Create("Current: " + arenaMusic, 50, 60));
        AddElement(ButtonElement.Create("Change battle music", () => Plugin.Logger.LogMessage("B"), 80));

        AddElement(LabelElement.Create("LEVEL TRIGGER MAP", 90));
        AddElement(LabelElement.Create("[VISUAL LEVEL TRIGGER MAP GOES HERE]", 90));
        AddElement(LabelElement.Create("Selected trigger ID: N/A", 50));
    }
    
    public void HideWindow()
    {
        menu.levelSelector.gameObject.SetActive(true);
        gameObject.SetActive(false);
        // SAVE CHANGES HERE!!!
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