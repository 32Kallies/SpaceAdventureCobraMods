using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;

public class LabelElement : MusicEditorElementBase
{
    public Text text;
    
    public static LabelElement Create(string labelText, int fontSize = 100, float height = 180)
    {
        var element = CreateBase(height);
        var label = element.gameObject.AddComponent<LabelElement>();
        var textTransform = AddChild("Text", element);
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = new Vector2(50, 0);
        textTransform.offsetMax = new Vector2(-50, 0);
        var text = textTransform.gameObject.AddComponent<Text>();
        if (LoadSaveController.Instance.PreferencesData.language == TextsController.LANGUAGE.JAPANESE)
            fontSize = Mathf.RoundToInt(fontSize * 0.7f);
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleLeft;
        text.font = MusicMenuBuilder.ButtonFont;
        text.color = Color.white;
        text.text = labelText;
        
        label.text = text;
        return label;
    }
}