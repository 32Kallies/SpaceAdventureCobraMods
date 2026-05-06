using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.ReplacementSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.MusicEditing;

public class CustomMusicMenu : MonoBehaviour
{
    private int _previousChoice;
    private int _mainChoice;

    public RectTransform content;
    public ScrollRect rect;
    public Text categoryText;
    public MusicEditor musicEditor;
    
    private readonly List<MusicButton> _buttons = [];

    private MusicCategory _largestValidCategory;
    private MusicCategory _selectedCategory;
    
    private void Start()
    {
        _largestValidCategory = (MusicCategory)Enum.GetValues(typeof(MusicCategory)).Cast<int>().Last();
        DisplayCategory(MusicCategory.Main);
    }

    private void Update()
    {
        if (musicEditor.GetIsShown())
            return;
        
        UIController.HandleCursor(ref _mainChoice, _buttons.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate 
            {
                var button = _buttons[_mainChoice];
                OpenMusicEditor(button.data);
            },
        delegate
        {
            MusicMenuEnabler.Main.SetTab(MusicMenuEnabler.Tab.MusicEditorHome);
        }, OnChoiceChange, OnChoiceChange);

        if (Utils.GetButtonPushed(PadsController.LS_LEFT))
            MoveCategory(false);
        else if (Utils.GetButtonPushed(PadsController.LS_RIGHT))
            MoveCategory(true);
    }

    private void MoveCategory(bool up)
    {
        var categoryValue = (int)_selectedCategory + (up ? 1 : -1);
        MusicCategory newCategory;
        if (categoryValue < 0)
            newCategory = _largestValidCategory;
        else if (categoryValue > (int)_largestValidCategory)
            newCategory = 0;
        else
            newCategory = (MusicCategory)categoryValue;
        DisplayCategory(newCategory);
        AudioController.Instance.PlaySound(audioSelectionData.eCLIP.UI_SELECTCHANGE, 0.6f);
    }

    private void OnChoiceChange()
    {
        if (_buttons.Count == 0)
            return;
        
        if (_buttons.Count > _previousChoice)
        {
            _buttons[_previousChoice].SetStateSelected(false);
        }
        
        _buttons[_mainChoice].SetStateSelected(true);
        _previousChoice = _mainChoice;
        if (_buttons.Count > 1)
            rect.verticalNormalizedPosition = GetVerticalNormalizedScrollPos();
    }

    private float GetVerticalNormalizedScrollPos()
    {
        var percent = 1f - (float)_mainChoice / (_buttons.Count - 1);
        var adjusted = CurvedIdentity(percent, 2);
        return Mathf.Clamp01(adjusted);
    }
    
    private static float CurvedIdentity(float x, float n)
    {
        float xn = Mathf.Pow(x, n);
        float oneMinusXn = Mathf.Pow(1f - x, n);
        return xn / (xn + oneMinusXn);
    }

    private void DisplayCategory(MusicCategory category)
    {
        _buttons.Clear();
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        var tracks = MusicProcessor.GetMusicForCategory(category);
        foreach (var track in tracks)
        {
            _buttons.Add(AddButton(track));
        }

        rect.verticalNormalizedPosition = 1;
        StartCoroutine(SetScrollToTopWithDelay());
        _mainChoice = 0;
        OnChoiceChange();
        categoryText.text = $"[←] Category: {GetNameForCategory(category)} [→]";
        _selectedCategory = category;
    }

    private static string GetNameForCategory(MusicCategory category)
    {
        if (category == MusicCategory.SnowCliff)
        {
            return "Snow Cliff";
        }

        if (category == MusicCategory.Unused)
        {
            return "Unused / Broken";
        }
        
        return category.ToString();
    }

    private IEnumerator SetScrollToTopWithDelay()
    {
        yield return null;
        rect.verticalNormalizedPosition = 1;
    }

    private void RefreshExistingButtons()
    {
        foreach (var button in _buttons)
        {
            button.RefreshVisuals();
        }
    }

    private MusicButton AddButton(MusicSound sound)
    {
        var button = new GameObject("Button").AddComponent<RectTransform>();
        button.SetParent(content);
        button.localScale = Vector3.one;
        button.anchorMin = Vector2.zero;
        button.anchorMax = new Vector2(1, 0);
        button.offsetMin = new Vector2(0, -100);
        button.offsetMax = new Vector2(0, 100);
        
        // image
        var image = button.gameObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        
        // main text
        var textObject = new GameObject("Text").AddComponent<RectTransform>();
        textObject.SetParent(button);
        textObject.localScale = Vector3.one;
        textObject.anchorMin = Vector2.zero;
        textObject.anchorMax = new Vector2(0.8f, 1);
        const int padding = 50;
        textObject.offsetMin = new Vector2(padding, padding);
        textObject.offsetMax = new Vector2(-padding, -padding);
        var text = textObject.gameObject.AddComponent<Text>();
        text.font = MusicMenuBuilder.ButtonFont;
        text.text = sound.DisplayName;
        text.alignment = TextAnchor.MiddleLeft;

        int fontSize = 80;
        if (LoadSaveController.Instance.PreferencesData.language == TextsController.LANGUAGE.JAPANESE)
            fontSize = Mathf.RoundToInt(fontSize * 0.7f);
        text.fontSize = fontSize;
        
        // has replacement visuals
        var hasReplacementObject = new GameObject("HasReplacement").AddComponent<RectTransform>();
        hasReplacementObject.SetParent(button);
        hasReplacementObject.localScale = Vector3.one;
        hasReplacementObject.anchorMin = new Vector2(0.8f, 0);
        hasReplacementObject.anchorMax = Vector2.one;
        hasReplacementObject.offsetMin = new Vector2(padding, padding);
        hasReplacementObject.offsetMax = new Vector2(-padding, -padding);
        var replacedText = hasReplacementObject.gameObject.AddComponent<Text>();
        replacedText.font = MusicMenuBuilder.ButtonFont;
        replacedText.text = "<color=#FFFF00>CUSTOM</color>";
        replacedText.alignment = TextAnchor.MiddleRight;
        replacedText.fontSize = fontSize;
        
        // button component
        var buttonComponent = button.gameObject.AddComponent<MusicButton>();
        buttonComponent.image = image;
        buttonComponent.selectedColor = new Color(1, 0.2f, 0, 0.4f);
        buttonComponent.data = sound;
        buttonComponent.usingCustomSoundVisual = hasReplacementObject.gameObject;
        return buttonComponent;
    }

    public void OpenMusicEditor(MusicSound music)
    {
        musicEditor.ShowWindow(music);
    }

    public void CloseMusicEditor()
    {
        musicEditor.gameObject.SetActive(false);
        RefreshExistingButtons();
    }
}