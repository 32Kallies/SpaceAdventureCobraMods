using System.Collections.Generic;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using UnityEngine;

namespace MusicReplacer.MusicReplacementMenu.HomeScreen;

public class HomeMenu : MonoBehaviour
{
    private int _previousChoice;
    private int _mainChoice;

    public RectTransform buttonParent;
    
    private List<ButtonElement> _buttons;
    
    private static readonly List<(string label, MusicMenuEnabler.Tab tab)> Buttons = [
        ("Replace Music Files", MusicMenuEnabler.Tab.MusicReplacer),
        ("Swap Level Music", MusicMenuEnabler.Tab.LevelMusic),
        ("Return to Main Menu", MusicMenuEnabler.Tab.MainMenu)
    ];

    private void Start()
    {
        _buttons = new List<ButtonElement>();
        foreach (var button in Buttons)
        {
            var element = ButtonElement.Create(button.label, () => MusicMenuEnabler.Main.SetTab(button.tab), 110);
            element.RectTransform.SetParent(buttonParent);
            element.RectTransform.localScale = Vector3.one;
            
            _buttons.Add(element);
        }
        _buttons[0].Select();
    }

    private void Update()
    {
        UIController.HandleCursor(ref _mainChoice, _buttons.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate 
            {
                var button = _buttons[_mainChoice];
                button.Interact();
            },
        delegate
        {
            MusicMenuEnabler.Main.SetTab(MusicMenuEnabler.Tab.MainMenu);
        }, OnChoiceChange, OnChoiceChange);
    }

    private void OnChoiceChange()
    {
        if (_buttons.Count == 0)
            return;
        
        if (_buttons.Count > _previousChoice)
        {
            _buttons[_previousChoice].Deselect();
        }
        
        _buttons[_mainChoice].Select();
        _previousChoice = _mainChoice;
    }
}