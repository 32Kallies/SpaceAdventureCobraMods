using MusicReplacer.MusicReplacementMenu.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu;

public static partial class MusicMenuBuilder
{
    private static LevelMusicMenu CreateLevelMusicMenu(NUIMainMenu menu)
    {
        // ROOT
        var container = menu.transform.Find("container");
        var rootPanel = new GameObject("level music menu");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(container.transform);
        rootRect.localScale = Vector3.one;
        rootRect.anchoredPosition = new Vector2(-1700, 950);
        rootRect.sizeDelta = new Vector2(4000, 2800);
        
        // 1. LEVEL SELECTOR
        var levelSelector = new GameObject("level selector");
        var levelSelectorRect = levelSelector.AddComponent<RectTransform>();
        levelSelectorRect.SetParent(rootRect);
        levelSelectorRect.localScale = Vector3.one;
        levelSelectorRect.anchorMin = Vector2.zero;
        levelSelectorRect.anchorMax  = Vector2.one;
        levelSelectorRect.anchoredPosition += new Vector2(-800, 300);
        levelSelectorRect.sizeDelta += new Vector2(-400, -1000);
        
        // - HEADER
        var header = new GameObject("header");
        var headerRect = header.AddComponent<RectTransform>();
        headerRect.SetParent(levelSelector.transform);
        headerRect.localScale = Vector3.one;
        headerRect.sizeDelta = new Vector2(2600, 200);
        headerRect.localPosition = new Vector3(0, 900, 0);
        var headerText = header.AddComponent<Text>();
        headerText.fontSize = 100;
        headerText.font = ButtonFont;
        headerText.text = "SELECT LEVEL TO SWAP MUSIC";
        headerText.alignment = TextAnchor.MiddleCenter;
        
        // - EPISODES GRID
        var levelGridParent = new GameObject("episodes grid");
        var levelGridRect = levelGridParent.AddComponent<RectTransform>();
        levelGridRect.SetParent(levelSelectorRect);
        levelGridRect.localScale = Vector3.one;
        levelGridRect.anchorMin = Vector2.zero;
        levelGridRect.anchorMax  = Vector2.one;
        levelGridRect.offsetMin = Vector2.zero;
        levelGridRect.offsetMax = Vector2.zero;
        var levelGridLayout = levelGridParent.AddComponent<GridLayoutGroup>();
        levelGridLayout.cellSize = new Vector2(700, 400);
        levelGridLayout.spacing = new Vector2(120, 120);
        levelGridLayout.childAlignment = TextAnchor.MiddleCenter;
        
        // - LEVELS LIST
        var levelsListParent = new GameObject("levels list");
        var levelsListRect = levelsListParent.AddComponent<RectTransform>();
        levelsListRect.SetParent(levelSelectorRect);
        levelsListRect.localScale = Vector3.one;
        levelsListRect.anchorMin = Vector2.zero;
        levelsListRect.anchorMax  = Vector2.one;
        levelsListRect.offsetMin = new Vector2(400, 200);
        levelsListRect.offsetMax = new Vector2(-400, -200);
        var levelsLayout = levelsListParent.AddComponent<VerticalLayoutGroup>();
        levelsLayout.childAlignment = TextAnchor.MiddleCenter;
        levelsLayout.childForceExpandHeight = false;
        levelsLayout.childForceExpandWidth = true;
        levelsLayout.childScaleHeight = false;
        levelsLayout.childScaleWidth = false;
        levelsLayout.childControlHeight = false;
        levelsLayout.padding = new RectOffset(40, 40, 80, 80);
        levelsLayout.spacing = 30;
        
        var levelSelectorComponent = levelSelector.AddComponent<LevelSelectorMenu>();
        levelSelectorComponent.episodesParent = levelGridRect;
        levelSelectorComponent.levelsParent = levelsListRect;
        levelsListParent.SetActive(false);
        
        // 2. LEVEL EDITOR
        var levelEditorObj = new GameObject("level editor");
        var levelEditorRect = levelEditorObj.AddComponent<RectTransform>();
        levelEditorRect.SetParent(rootRect);
        levelEditorRect.localScale = Vector3.one;
        levelEditorRect.anchorMin = Vector2.zero;
        levelEditorRect.anchorMax  = Vector2.one;
        levelEditorRect.anchoredPosition += new Vector2(-800, 300);
        levelEditorRect.sizeDelta += new Vector2(-400, -200f);
        levelEditorObj.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.03f);
        var editLayout = levelEditorObj.AddComponent<VerticalLayoutGroup>();
        editLayout.childScaleHeight = false;
        editLayout.childControlHeight = false;
        editLayout.childForceExpandWidth = true;
        editLayout.childForceExpandHeight = false;
        editLayout.padding = new RectOffset(50, 50, 50, 50);
        editLayout.spacing = 40;

        var levelEditor = levelEditorObj.AddComponent<LevelEditorMenu>();
        levelEditor.rect = levelEditorRect;
        
        // 3. MUSIC SWAP POPUP
        
        var musicSwapObj = new GameObject("music swap popup");
        var musicSwapRect = musicSwapObj.AddComponent<RectTransform>();
        musicSwapRect.SetParent(rootRect);
        musicSwapRect.localScale = Vector3.one;
        musicSwapRect.anchorMin = Vector2.zero;
        musicSwapRect.anchorMax  = Vector2.one;
        musicSwapRect.anchoredPosition += new Vector2(-800, 300);
        musicSwapRect.sizeDelta += new Vector2(-300, -100);
        musicSwapObj.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.03f);
        var swapLayout = musicSwapObj.AddComponent<VerticalLayoutGroup>();
        swapLayout.childScaleHeight = false;
        swapLayout.childControlHeight = false;
        swapLayout.childForceExpandWidth = true;
        swapLayout.childForceExpandHeight = false;
        swapLayout.padding = new RectOffset(50, 50, 50, 50);
        swapLayout.spacing = 40;

        var musicSwapper = musicSwapObj.AddComponent<MusicSwapPopup>();
        musicSwapper.rect = musicSwapRect;
        levelEditor.swap = musicSwapper;
        
        // 4. ECLIP CHOOSER MENU
        
        var eclipChooserWindow = new GameObject("choose eclips window");
        var eclipChooserRect = eclipChooserWindow.AddComponent<RectTransform>();
        eclipChooserRect.SetParent(rootRect);
        eclipChooserRect.localScale = Vector3.one;
        eclipChooserRect.anchoredPosition = new Vector2(0, 0);
        eclipChooserRect.sizeDelta = new Vector2(4000, 2800);
        eclipChooserRect.localPosition += new Vector3(900, -800, 0);
        eclipChooserWindow.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.03f);

        var eclipChooserHeader = new GameObject("Text").AddComponent<RectTransform>();
        eclipChooserHeader.SetParent(eclipChooserRect);
        eclipChooserHeader.localScale = Vector3.one;
        eclipChooserHeader.anchorMin = new Vector2(0, 1);
        eclipChooserHeader.anchorMax = Vector2.one;
        eclipChooserHeader.offsetMin = new Vector2(0, -200);
        eclipChooserHeader.offsetMax = Vector2.zero;
        var eclipChooserText = eclipChooserHeader.gameObject.AddComponent<Text>();
        eclipChooserText.font = ButtonFont;
        eclipChooserText.fontSize = 100;
        eclipChooserText.alignment = TextAnchor.MiddleCenter;

        var eclipChooserViewport = new GameObject("Viewport").AddComponent<RectTransform>();
        eclipChooserViewport.SetParent(eclipChooserRect);
        eclipChooserViewport.localScale = Vector3.one;
        eclipChooserViewport.anchorMin = Vector2.zero;
        eclipChooserViewport.anchorMax = Vector2.one;
        eclipChooserViewport.offsetMin = Vector2.zero;
        eclipChooserViewport.offsetMax = new Vector2(0, -200);

        eclipChooserViewport.gameObject.AddComponent<Image>();
        eclipChooserViewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        var eclipChooserContent = new GameObject("Content").AddComponent<RectTransform>();
        eclipChooserContent.SetParent(eclipChooserViewport);
        eclipChooserContent.localScale = Vector3.one;
        eclipChooserContent.anchorMin = Vector2.zero;
        eclipChooserContent.anchorMax = Vector2.one;
        eclipChooserContent.pivot = new Vector2(0.5f, 1);
        eclipChooserContent.offsetMin = Vector2.zero;
        eclipChooserContent.offsetMax = Vector2.zero;
        eclipChooserContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        var eclipChooserLayout = eclipChooserContent.gameObject.AddComponent<GridLayoutGroup>();
        eclipChooserLayout.childAlignment = TextAnchor.UpperCenter;
        eclipChooserLayout.cellSize = new Vector2(500, 400);
        eclipChooserLayout.spacing = new Vector2(25, 25);
        eclipChooserLayout.padding = new RectOffset(50, 50, 50, 50);
        eclipChooserLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        eclipChooserLayout.constraintCount = EClipChooserMenu.ElementsPerRow;

        var eclipScrollRect = eclipChooserWindow.gameObject.AddComponent<ScrollRect>();
        eclipScrollRect.content = eclipChooserContent;
        eclipScrollRect.horizontal = false;
        eclipScrollRect.vertical = true;
        eclipScrollRect.viewport = eclipChooserViewport;
        eclipScrollRect.scrollSensitivity = 50;
        eclipScrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        var eclipChooser = eclipChooserWindow.AddComponent<EClipChooserMenu>();
        eclipChooser.content = eclipChooserContent;
        eclipChooser.header = eclipChooserText;
        eclipChooser.scrollRect = eclipScrollRect;
        
        // Finalize
        var levelMusicMenu = rootPanel.AddComponent<LevelMusicMenu>();
        levelMusicMenu.levelSelector = levelSelectorComponent;
        levelMusicMenu.levelEditor = levelEditor;

        levelSelectorComponent.menu = levelMusicMenu;
        levelEditor.menu = levelMusicMenu;
        musicSwapper.menu = levelMusicMenu;
        eclipChooser.menu = levelMusicMenu;

        musicSwapper.eClipChooser = eclipChooser;
        
        levelEditor.gameObject.SetActive(false);
        musicSwapper.gameObject.SetActive(false);
        eclipChooser.gameObject.SetActive(false);
        
        return levelMusicMenu;
    }
}