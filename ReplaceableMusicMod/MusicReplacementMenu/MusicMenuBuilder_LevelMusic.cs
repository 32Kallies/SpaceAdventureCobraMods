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
        levelEditorRect.sizeDelta += new Vector2(-400, -600);
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
        
        // Finalize
        var levelMusicMenu = rootPanel.AddComponent<LevelMusicMenu>();
        levelMusicMenu.levelSelector = levelSelectorComponent;
        levelMusicMenu.levelEditor = levelEditor;

        levelSelectorComponent.menu = levelMusicMenu;
        levelEditor.menu = levelMusicMenu;
        
        levelEditor.gameObject.SetActive(false);
        
        return levelMusicMenu;
    }
}