using System.Collections.Generic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.ReplacementSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.EditMusicPopup;

public class FileChooserMenu : MonoBehaviour
{
    public bool IsOpen => isActiveAndEnabled;

    private readonly List<List<MusicEditorElementBase>> _elements = [];
    private readonly List<List<ISelectableElement>> _selectables = [];

    public Text header;
    public RectTransform content;
    public ScrollRect scrollRect;
    public MusicEditor editor;

    public const int ElementsPerRow = 5;

    private int _row;
    private int _column;

    private Vector2Int _previousChoice;

    private const float ScanInterval = 0.5f;
    private float _timeScanAgain;

    private MusicSound _currentSound;
    private string[] _knownSoundPaths;

    private void Update()
    {
        UIController.HandleCursor(ref _row, _selectables.Count, 1, 2, _allowbuttonscycle: false,
            UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate
            {
                Plugin.Logger.LogMessage("Clicked " + _row);
                _selectables[_row][_column].Interact();
            },
            Hide, OnChoiceChange, OnChoiceChange);

        if (Utils.GetButtonPushed(PadsController.LS_LEFT))
            MoveColumn(false);
        else if (Utils.GetButtonPushed(PadsController.LS_RIGHT))
            MoveColumn(true);

        if (Time.time > _timeScanAgain)
        {
            Scan();
            _timeScanAgain = Time.time + ScanInterval;
        }
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


    public void Show(MusicSound sound)
    {
        if (_elements.Count > 0)
            ClearWindow();

        gameObject.SetActive(true);

        header.text = sound.DisplayName;

        DrawWindow(sound);

        _row = 0;
        _column = 0;
        _previousChoice = new Vector2Int(0, 0);
        if (_selectables.Count > 0)
        {
            _selectables[_row][_column].Select();
        }

        _currentSound = sound;
        _timeScanAgain = Time.time + ScanInterval;
    }

    private void DrawWindow(MusicSound music)
    {
        AddElement(ButtonElement.Create("CANCEL", Hide, 80), 0);
        AddElement(ButtonElement.Create("FOLDER", FileManagement.OpenCustomSoundsFolder, 80), 0);
        AddElement(ButtonElement.Create("RESET", () => SetCustomSound(music, null), 80), 0);

        // Placeholders hack to align all elements
        var rowToFill = _elements.Count - 1;
        while (_elements[rowToFill].Count < ElementsPerRow)
        {
            AddElement(PlaceholderElement.Create(), rowToFill);
        }

        int numStartingRows = _elements.Count;

        _knownSoundPaths =
            FileManagement.GetAllSoundFilesInFolder(FileManagement.GetCustomSoundsFolder(),
                FileManagement.GetModFolder());

        if (_knownSoundPaths.Length == 0)
        {
            AddElement(LabelElement.Create("FOLDER IS EMPTY", 70), numStartingRows);
        }

        for (int i = 0; i < _knownSoundPaths.Length; i++)
        {
            var row = i / ElementsPerRow + numStartingRows;
            var soundFile = _knownSoundPaths[i];
            var soundName = FileManagement.GetDisplayNameForSoundPath(_knownSoundPaths[i]);
            AddElement(
                ButtonElement.Create(soundName, () => SetCustomSound(music, soundFile), 40),
                row);
        }
    }

    private void SetCustomSound(MusicSound music, string customSoundPath)
    {
        if (string.IsNullOrEmpty(customSoundPath))
        {
            MusicReplacementManager.ReplacementData.SetSoundToDefault(music);
        }
        else
        {
            MusicReplacementManager.ReplacementData.SetReplacement(music, customSoundPath);
        }

        MusicMenuBuilder.ShowRestartRequiredWarning();
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
        editor.ShowWindow(_currentSound);
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

    private void Scan()
    {
        if (!CompareCustomSoundArrays(_knownSoundPaths,
                FileManagement.GetAllSoundFilesInFolder(FileManagement.GetCustomSoundsFolder(),
                    FileManagement.GetModFolder())))
        {
            Show(_currentSound);
        }
    }

    private bool CompareCustomSoundArrays(string[] a, string[] b)
    {
        if (a == null || b == null)
            return false;

        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (!a[i].Equals(b[i]))
                return false;
        }

        return true;
    }
}