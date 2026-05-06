using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.MusicReplacementMenu.HomeScreen;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu;

public static partial class MusicMenuBuilder
{
    public const NUIButton.TYPE NewButtonType = (NUIButton.TYPE)1353; // arbitrary value, do not copy
    public static Font ButtonFont { get; private set; }

    private static GameObject RestartRequiredWarning { get; set; }

    private const string MainMenuButton = "MUSIC";
    private const string MainMenuButtonJapanese = "音楽";

    public static void ShowRestartRequiredWarning()
    {
        if (RestartRequiredWarning)
            RestartRequiredWarning.SetActive(true);
    }
    
    public static void BuildMusicReplacementMenu(NUIMainMenu menu)
    {
        var button = AddNewButton(menu);
        var enabler = button.gameObject.AddComponent<MusicMenuEnabler>();
        enabler.mainMenuTab = menu.transform.Find("container/title").gameObject;
        enabler.mainMenu = menu;

        RestartRequiredWarning = CreateRestartRequiredWarning(menu);
        RestartRequiredWarning.SetActive(false);

        var window = CreateNewWindow(menu);
        window.gameObject.SetActive(false);
        enabler.musicMenu = window.gameObject;

        var homeMenu = CreateHomeMenu(menu);
        homeMenu.gameObject.SetActive(false);
        enabler.homeTab = homeMenu.gameObject;

        var levelMusic = CreateLevelMusicMenu(menu);
        levelMusic.gameObject.SetActive(false);
        enabler.levelMusicTab = levelMusic.gameObject;
    }

    private static GameObject CreateRestartRequiredWarning(NUIMainMenu menu)
    {
        var parent = menu.transform.Find("container/title");
        
        var textObject = new GameObject("RestartRequired").AddComponent<RectTransform>();
        textObject.SetParent(parent);
        textObject.localScale = Vector3.one;
        textObject.localPosition = new Vector3(0, -240, 0);
        textObject.sizeDelta = new Vector2(1000, 300);
        textObject.pivot = new Vector2(0, 0.5f);
        var textComponent = textObject.gameObject.AddComponent<Text>();
        textComponent.text = "<color=#FFB500><size=60>⚠️</size></color><color=#FFFFFF> Restart required for music changes!</color>";
        textComponent.color = Color.white;
        textComponent.fontSize = 40;
        textComponent.font = ButtonFont;
        textComponent.raycastTarget = false;

        return textObject.gameObject;
    }

    private static HomeMenu CreateHomeMenu(NUIMainMenu menu)
    {
        var container = menu.transform.Find("container");
        var musicPanel = new GameObject("music home menu");
        var homeRect = musicPanel.AddComponent<RectTransform>();
        homeRect.SetParent(container.transform);
        homeRect.localScale = Vector3.one;
        homeRect.anchoredPosition = new Vector2(-1000, 0);
        homeRect.sizeDelta = new Vector2(3000, 2000);
        // musicPanel.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.15f, 0.09f);

        var title = LabelElement.Create("COBRA MUSIC EDITOR", 140, 200);
        title.RectTransform.SetParent(homeRect);
        title.RectTransform.localScale = Vector3.one;
        title.RectTransform.anchoredPosition = new Vector2(0, 800);
        title.text.alignment = TextAnchor.MiddleCenter;
        title.RectTransform.sizeDelta = new Vector2(2000, 200);
        
        var buttonsParent = new GameObject("home buttons");
        var buttonsRect = buttonsParent.AddComponent<RectTransform>();
        buttonsRect.SetParent(homeRect.transform);
        buttonsRect.localScale = Vector3.one;
        buttonsRect.anchorMin = Vector2.zero;
        buttonsRect.anchorMax = Vector2.one;
        buttonsRect.offsetMin = new Vector2(300, 0);
        buttonsRect.offsetMax = new Vector2(-300, -300);
        buttonsRect.anchoredPosition = new Vector2(0, -400);
        var layout = buttonsParent.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 80;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        var homeMenu = musicPanel.AddComponent<HomeMenu>();
        homeMenu.buttonParent = buttonsRect;
        return homeMenu;
    }

    private static NUIButton AddNewButton(NUIMainMenu menu)
    {
        var continueButton = menu.transform.Find("container/title/buttons container/continue").gameObject;
        var newButton = Object.Instantiate(continueButton, continueButton.transform.parent);
        newButton.name = "music replacer";
        var newButtonBehaviour = newButton.GetComponent<NUIButton>();
        var text = newButton.transform.Find("text").gameObject;
        Object.DestroyImmediate(text.GetComponent<TextLocalize>());
        var textComponent = text.GetComponent<Text>();
        textComponent.text =
            LoadSaveController.Instance.PreferencesData.language == TextsController.LANGUAGE.JAPANESE
                ? MainMenuButtonJapanese
                : MainMenuButton;
        ButtonFont = textComponent.font;
        var newButtons = new NUIButton[menu.mainButtons.Length + 1];
        menu.mainButtons.CopyTo(newButtons, 0);
        newButtons[^1] = newButtons[^2];
        newButtons[^2] = newButtonBehaviour;
        menu.mainButtons = newButtons;
        newButton.transform.SetSiblingIndex(newButton.transform.GetSiblingIndex() - 1);
        newButtonBehaviour.Initialise(NewButtonType);
        menu.m_TotalMainButtons++;

        return newButtonBehaviour;
    }
}