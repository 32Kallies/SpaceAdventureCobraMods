using System;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;

public class ButtonElement : MusicEditorElementBase, ISelectableElement
{
    private static readonly Color NormalColor = new(0.3f, 0.3f, 0.3f, 0.2f);
    private static readonly Color HighlightedColor = new(0.9f, 0.2f, 0.2f, 0.5f);
    
    private static readonly Color NormalTextColor = new(1, 1, 1);
    private static readonly Color HighlightedTextColor = new(0, 0, 0);

    public Image Image { get; private set; }
    public Text Text { get; private set; }

    private Action _onUse;

    public static ButtonElement Create(string labelText, Action onUse, int fontSize = 100)
    {
        var element = CreateBase();
        
        // button
        var graphic = element.gameObject.AddComponent<Image>();
        graphic.color = NormalColor;
        
        // text
        var textTransform = AddChild("Text", element);
        var text = textTransform.gameObject.AddComponent<Text>();
        if (LoadSaveController.Instance.PreferencesData.language == TextsController.LANGUAGE.JAPANESE)
            fontSize = Mathf.RoundToInt(fontSize * 0.7f);
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = MusicMenuBuilder.ButtonFont;
        text.color = NormalTextColor;
        text.text = labelText;
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = new Vector2(50, 0);
        textTransform.offsetMax = new Vector2(-50, 0);
        
        // behaviour
        var buttonElement = element.gameObject.AddComponent<ButtonElement>();
        buttonElement.Text = text;
        buttonElement.Image = graphic;
        buttonElement._onUse = onUse;
        
        return buttonElement;
    }

    public void Select()
    {
        Image.color = HighlightedColor;
        Text.color = HighlightedTextColor;
    }

    public void Deselect()
    {
        Image.color = NormalColor;
        Text.color = NormalTextColor;
    }

    public void Interact()
    {
        _onUse.Invoke();
    }
}