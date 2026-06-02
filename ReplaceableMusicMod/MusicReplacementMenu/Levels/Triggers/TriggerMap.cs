using System;
using System.Collections.Generic;
using System.Linq;
using MusicReplacer.CustomTriggers;
using MusicReplacer.Data;
using MusicReplacer.LevelMusic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.ReplacementSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels.Triggers;

public class TriggerMap : MusicEditorElementBase, ISelectableElement
{
    private bool _selected;

    private int _selectionIndex;
    private EditableTrigger[] _triggers;
    private EditableTrigger[] _newTriggers;
    private MusicSwapPopup _musicSwap;
    private LevelEditorMenu _levelEditor;

    private Text _selectionInfoText;

    private Color _normalIconColor = new(0.4f, 0.1f, 0.1f, 0.7f);
    private Color _selectedIconColor = new(0.6f, 0.2f, 0.2f);
    
    private Color _customNormalIconColor = new(0.75f, 0.45f, 0.1f, 0.8f);
    private Color _customSelectedIconColor = new(0.8f, 0.6f, 0.2f);
    
    private Color _newTriggerColor = new(0.4f, 0f, 0.8f, 0.5f);
    
    private float _defaultIconWidth = 10f;
    private const float MapHeight = 600;

    // The X axis is used to anchor the aspect ratio
    private Bounds _bounds;

    public void SetUp(Text selectionInfoText, MusicSwapPopup swap, LevelEditorMenu levelEditor)
    {
        _selectionInfoText = selectionInfoText;
        _musicSwap = swap;
        _levelEditor = levelEditor;
        UpdateDisplay();
    }

    public static TriggerMap Create(LevelDefinition levelDefinition)
    {
        var element = CreateBase(MapHeight);
        element.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
        var levelTriggers = LevelRipper.GetLevelMusicData(levelDefinition.level).LevelTriggers;

        var editableTriggers = new EditableTrigger[levelTriggers.Count];
        int i = 0;

        foreach (var levelTrigger in levelTriggers.Values)
        {
            SwappableMusic music = SwappableMusic.CreateSwappableTriggerMusic(levelDefinition, levelTrigger);
            bool isCustom = music.DefaultClip != music.GetCurrentClip();
            var trigger = new EditableTrigger
            {
                RawData = levelTrigger,
                Music = music,
                Custom = isCustom
            };
            editableTriggers[i] = trigger;
            i++;
        }

        EditableTrigger[] newTriggers = CustomTriggerParser.ParseAllFiles(FileManagement.GetCustomTriggersFolder())
            .Where(trigger => string.Equals(trigger.Level, levelDefinition.levelName, StringComparison.OrdinalIgnoreCase))
            .Select(trigger => new EditableTrigger
            {
                RawData = new LevelTrigger
                {
                    Size = new SimpleVector3(CustomTriggerUtils.GetCustomTriggerBounds(trigger).Size),
                    Center = new SimpleVector3(CustomTriggerUtils.GetCustomTriggerBounds(trigger).Center),
                    Type = PrimitiveType.Cube
                },
                IsNewTrigger = true,
                Custom = true
            }).ToArray();
        
        Array.Sort(editableTriggers, (a, b) =>
            a.RawData.Center.X.CompareTo(b.RawData.Center.X));

        var map = element.gameObject.AddComponent<TriggerMap>();
        map._triggers = editableTriggers;
        map._newTriggers = newTriggers;
        map.CalculateBounds();
        map.BuildMap();
        return map;
    }
    
    private void BuildMap()
    {
        foreach (var trigger in _triggers)
        {
            trigger.Image = BuildTriggerIcon(trigger);
        }
        foreach (var newTrigger in _newTriggers)
        {
            newTrigger.Image = BuildTriggerIcon(newTrigger);
        }
    }

    private void UpdateDisplay()
    {
        _selectionInfoText.text = GetText();
        for (int i = 0; i < _triggers.Length; i++)
        {
            var trigger = _triggers[i];
            var color = _selectionIndex == i
                ? trigger.Custom ? _customSelectedIconColor : _selectedIconColor // selected
                : trigger.Custom ? _customNormalIconColor : _normalIconColor; // deselected
            _triggers[i].Image.color = color;
        }
    }

    private string GetText()
    {
        if (_triggers.Length == 0)
        {
            return "[NO TRIGGERS AVAILABLE]";
        }

        var music = _triggers[_selectionIndex].Music;
        var musicName = MusicProcessor.GetFriendlyNameForEClip(music.GetCurrentClip());
        var triggerIndexText = (_selectionIndex + 1).ToString("00");
        var triggerCountText = _triggers.Length.ToString("00");
        var hash = _triggers[_selectionIndex].RawData.Hash;
        var label = $"TRIGGER {triggerIndexText}/{triggerCountText}";
        if (music.GetCurrentClip() != music.DefaultClip)
            label = $"<color=#FFA000>{label}</color>";
        return $"[{label}] Music: {musicName}\t\tID: {hash}";
    }

    private void Update()
    {
        if (!CanNavigate())
        {
            return;
        }

        if (Utils.GetButtonPushed(PadsController.LS_LEFT))
            Navigate(false);
        else if (Utils.GetButtonPushed(PadsController.LS_RIGHT))
            Navigate(true);
    }

    private void CalculateBounds()
    {
        if (_triggers.Length == 0)
        {
            _bounds = new Bounds();
            return;
        }

        _bounds = new Bounds(_triggers[0].RawData.Center.ToVector3(), _triggers[0].RawData.GetEncapsulatingSize());
        foreach (var trigger in _triggers)
        {
            var triggerBounds = new Bounds(trigger.RawData.Center.ToVector3(), trigger.RawData.GetEncapsulatingSize());
            _bounds.Encapsulate(triggerBounds);
        }
    }

    private bool CanNavigate()
    {
        return _selected && _triggers.Length > 1 && !_musicSwap.GetIsShown();
    }

    private void Navigate(bool positive)
    {
        SetSelectionIndex(_selectionIndex += positive ? 1 : -1);
        AudioController.Instance.PlaySound(audioSelectionData.eCLIP.UI_SELECTCHANGE, 0.4f);
    }

    public void SetSelectionIndex(int index)
    {
        _selectionIndex = index % _triggers.Length;
        if (_selectionIndex < 0) _selectionIndex = _triggers.Length - 1;

        UpdateDisplay();
    }

    public int GetSelectionIndex()
    {
        return _selectionIndex;
    }

    public void Interact()
    {
        if (_selected && _triggers.Length > 0)
        {
            _levelEditor.SaveCurrentState();
            _musicSwap.ShowWindow(_triggers[_selectionIndex].Music);
        }
    }

    public void Select()
    {
        _selected = true;
    }

    public void Deselect()
    {
        _selected = false;
    }

    private class EditableTrigger
    {
        public LevelTrigger RawData { get; set; }
        public SwappableMusic Music { get; set; }
        public Image Image { get; set; }
        public bool Custom { get; set; }
        public bool IsNewTrigger { get; set; }
    }

    // Creates an icon for each trigger and sets its position into the bounds of the mesh
    private Image BuildTriggerIcon(EditableTrigger trigger)
    {
        var mapRoot = RectTransform;
        var bounds = _bounds;

        var iconObject = new GameObject($"Trigger_{trigger.RawData.Hash}");
        iconObject.transform.SetParent(mapRoot, false);

        var image = iconObject.AddComponent<Image>();
        
        if (trigger.IsNewTrigger)
            image.color = _newTriggerColor;
        else
            image.color = trigger.Custom ? _customNormalIconColor : _normalIconColor;

        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        var mapWidth = mapRoot.rect.width;
        var mapHeight = mapRoot.rect.height;

        // Map positions into bounds
        var triggerPos = trigger.RawData.Center.ToVector3();

        float x = (triggerPos.x - bounds.min.x) / bounds.size.x;
        float y = (triggerPos.y - bounds.min.y) / bounds.size.y;

        // X width is dominant here
        var worldWidth = bounds.size.x;
        var pixelsPerUnit = mapWidth / worldWidth;

        // Scale icon size
        var triggerSize = trigger.RawData.GetEncapsulatingSize();

        var width = Mathf.Max(_defaultIconWidth, triggerSize.x * pixelsPerUnit);
        var height = Mathf.Max(_defaultIconWidth, triggerSize.y * pixelsPerUnit);
        height = Mathf.Min(height, MapHeight);

        // Finalize
        rect.sizeDelta = new Vector2(width, height);

        rect.anchoredPosition = new Vector2(
            x * mapWidth,
            y * mapHeight
        );

        return image;
    }
}