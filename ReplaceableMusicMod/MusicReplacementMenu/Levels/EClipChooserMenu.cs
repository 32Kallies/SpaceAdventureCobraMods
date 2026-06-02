using System.Collections.Generic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.NewMusicSystem;
using MusicReplacer.ReplacementSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class EClipChooserMenu : MonoBehaviour
{
    public bool IsOpen => isActiveAndEnabled;

    private readonly List<List<MusicEditorElementBase>> _elements = [];
    private readonly List<List<ISelectableElement>> _selectables = [];

    public LevelMusicMenu menu;
    public Text header;
    public RectTransform content;
    public ScrollRect scrollRect;

    public const int ElementsPerRow = 7;
    
    private int _row;
    private int _column;
    
    private Vector2Int _previousChoice;
    
    private SwappableMusic _currentMusic;
    
    private void Update()
    {
        UIController.HandleCursor(ref _row, _selectables.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate
            {
                Plugin.Logger.LogMessage("Clicked " + _row);
                _selectables[_row][_column].Interact();
            },
            Hide, OnChoiceChange, OnChoiceChange);
        
        if (Utils.GetButtonPushed(PadsController.LS_LEFT))
            MoveColumn(false);
        else if (Utils.GetButtonPushed(PadsController.LS_RIGHT))
            MoveColumn(true);
    }
    
    private void MoveColumn(bool right)
    {
        _column = Mathf.Clamp(_column + (right ? 1 : -1), 0, _selectables[_row].Count - 1);
        AudioController.Instance.PlaySound(audioSelectionData.eCLIP.UI_SELECTCHANGE, 0.4f);
        OnChoiceChange();
    }
    
    private void ClearWindow()
    {
        foreach (var row in _elements)
        {
            foreach (var element in row)
            {
                if (element == null)
                {
                    Plugin.Logger.LogWarning("Element is null. This should not happen!");
                    continue;
                }
                Destroy(element.gameObject);
            }
        }
        _elements.Clear();
        _selectables.Clear();
    }

    public void Show(SwappableMusic music)
    {
        if (_elements.Count > 0)
            ClearWindow();
        
        gameObject.SetActive(true);
        
        header.text = $"{music.LevelName} - {music.DisplayText}";
        
        DrawWindow(music);
        
        _row = 0;
        _column = 0;
        _previousChoice = new Vector2Int(0, 0);
        if (_selectables.Count > 0)
        {
            _selectables[_row][_column].Select();
        }

        _currentMusic = music;
    }

    private void DrawWindow(SwappableMusic music)
    {
        AddElement(ButtonElement.Create("CANCEL", Hide, 80), 0);
        AddElement(ButtonElement.Create("FOLDER", FileManagement.OpenCustomSoundsFolder, 80), 0);
        AddElement(ButtonElement.Create("RESET", () => SetClip(music, audioSelectionData.eCLIP.NONE), 80), 0);
        
        // Placeholders hack to align all elements
        var rowToFill = _elements.Count - 1;
        while (_elements[rowToFill].Count < ElementsPerRow)
        {
            AddElement(PlaceholderElement.Create(), rowToFill);
        }
        
        int numStartingRows = _elements.Count;

        var allMusic = MusicProcessor.GetAllMusicByCategory(true);
        var newMusic = NewMusicLoader.GetNewMusicClips();

        int i = 0;
        foreach (var musicTuple in allMusic)
        {
            var row = i / ElementsPerRow + numStartingRows;
            
            // Get requirements for display text
            var categoryName = MusicProcessor.GetNameForCategory(musicTuple.category);
            var clipName = MusicProcessor.GetLoadNameForEClip(musicTuple.sound.EClip);
            var clipColor = GetColorHexString(musicTuple.category);
            
            // Build display text
            var text = $"<b><color={clipColor}>{categoryName}</color></b>:{clipName}";
            if (MusicProcessor.TryGetMusicSoundForEClip(musicTuple.sound.EClip, out var musicSound))
            {
                if (MusicReplacementManager.ReplacementData.TryGetCustomSound(musicSound, out var path))
                {
                    text += "\n\n<b><color=#FFA000>CUSTOM</color></b>: " + FileManagement.GetDisplayNameForSoundPath(path);
                }
            }
            
            // Add element
            var element = ButtonElement.Create(text, () => SetClip(music, musicTuple.sound.EClip), 40);
            element.Text.alignment = TextAnchor.UpperLeft;
            AddElement(element, row);
            i++;
        }
        
        foreach (var newSound in newMusic)
        {
            var row = i / ElementsPerRow + numStartingRows;
            
            // Get requirements for display text
            var categoryName = "CUSTOM";
            var clipName = newSound.ClipId;
            var clipColor = "#FFA000";
            
            // Build display text
            var text = $"<b><color={clipColor}>{categoryName}</color></b>:{clipName}";
            
            // Add element
            var element = ButtonElement.Create(text, () => SetClip(music, newSound.CustomClip), 40);
            element.Text.alignment = TextAnchor.UpperLeft;
            AddElement(element, row);
            i++;
        }
    }

    // This method was generated by ChatGPT, NOT by me
    private static string GetColorHexString(MusicCategory category)
    {
        return category switch
        {
            MusicCategory.Main      => "#FFD166", // warm gold
            MusicCategory.Generic   => "#A0AEC0", // neutral gray-blue
            MusicCategory.Reward    => "#06D6A0", // vibrant green
            MusicCategory.Villain   => "#EF476F", // crimson red
            MusicCategory.City      => "#118AB2", // urban blue
            MusicCategory.Cliff     => "#8D6E63", // rocky brown
            MusicCategory.Pyramid   => "#F4A261", // sandy amber
            MusicCategory.Ruins     => "#6C757D", // weathered stone
            MusicCategory.Sewers    => "#5C8001", // murky green
            MusicCategory.SnowCliff => "#90E0EF", // icy cyan
            MusicCategory.Unused    => "#7B2CBF", // purple/magenta
            _                       => "#FFFFFF"
        };
    }

    private void SetClip(SwappableMusic music, audioSelectionData.eCLIP clip)
    {
        music.SetClip(clip);
        Hide();
    }
    
    private void OnChoiceChange()
    {
        if (_selectables.Count == 0)
            return;

        if (_selectables[_row].Count == 0)
        {
            _row = _previousChoice.x;
            _column = _previousChoice.y;
            return;
        }
        
        _column = Mathf.Clamp(_column, 0, _selectables[_row].Count - 1);

        if (!_previousChoice.Equals(new Vector2Int(_row, _column)))
        {
            _selectables[_previousChoice.x][_previousChoice.y].Deselect();
        }
        
        _selectables[_row][_column].Select();
        _previousChoice = new Vector2Int(_row, _column);

        scrollRect.verticalNormalizedPosition = GetVerticalNormalizedScrollPos();
    }

    public void Hide()
    {
        menu.levelEditor.swap.Refresh();
        gameObject.SetActive(false);
    }

    private int GetMaxRows()
    {
        return _selectables.Count;
    }

    private int GetMaxColumns(int row)
    {
        return _selectables[row].Count;
    }
    
    private void AddElement(MusicEditorElementBase element, int row)
    {
        while (_selectables.Count <= row)
        {
            _selectables.Add(new List<ISelectableElement>());
        }
        while (_elements.Count <= row)
        {
            _elements.Add(new List<MusicEditorElementBase>());
        }
        
        element.RectTransform.SetParent(content);
        element.RectTransform.localScale = Vector3.one;
        if (element is ISelectableElement selectable)
        {
            _selectables[row].Add(selectable);
        }
        _elements[row].Add(element);
    }
    
    private float GetVerticalNormalizedScrollPos()
    {
        var percent = 1f - (float)_row / (_selectables.Count - 1);
        return percent;
    }
}