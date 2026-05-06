using MusicReplacer.MusicReplacementMenu.EditMusicPopup;
using MusicReplacer.MusicReplacementMenu.EditMusicPopup.Elements;
using MusicReplacer.MusicReplacementMenu.HomeScreen;
using MusicReplacer.MusicReplacementMenu.MusicEditing;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu;

public static partial class MusicMenuBuilder
{
    private static CustomMusicMenu CreateNewWindow(NUIMainMenu menu)
    {
        var container = menu.transform.Find("container");
        var musicPanel = new GameObject("music editing menu");
        var musicPanelRect = musicPanel.AddComponent<RectTransform>();
        musicPanelRect.SetParent(container.transform);
        musicPanelRect.localScale = Vector3.one;
        musicPanelRect.anchoredPosition = new Vector2(-800, 0);
        musicPanelRect.sizeDelta = new Vector2(4000, 2800);
        musicPanel.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.15f, 0.09f);

        var scrollView = new GameObject("ScrollRect").AddComponent<RectTransform>();
        scrollView.SetParent(musicPanelRect);
        scrollView.localScale = Vector3.one;
        scrollView.anchorMin = Vector2.zero;
        scrollView.anchorMax = Vector2.one;
        scrollView.offsetMin = new Vector2(0, 0);
        scrollView.offsetMax = new Vector2(0, -200);
        scrollView.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.1f, 0, 0.02f);

        var header = new GameObject("Header").AddComponent<RectTransform>();
        header.SetParent(musicPanelRect);
        header.localScale = Vector3.one;
        header.anchorMin = new Vector2(0, 1);
        header.anchorMax = Vector2.one;
        header.offsetMin = new Vector2(0, -200);
        header.offsetMax = new Vector2(0, 0);
        var headerText = header.gameObject.AddComponent<Text>();
        headerText.font = ButtonFont;
        headerText.fontSize = 100;
        headerText.alignment = TextAnchor.MiddleCenter;

        var viewport = new GameObject("Viewport").AddComponent<RectTransform>();
        viewport.SetParent(scrollView);
        viewport.localScale = Vector3.one;
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;

        viewport.gameObject.AddComponent<Image>();
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content").AddComponent<RectTransform>();
        content.SetParent(viewport);
        content.localScale = Vector3.one;
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;
        var sizeFitter = content.gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.viewport = viewport;
        scrollRect.scrollSensitivity = 50;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 25;
        layoutGroup.childControlHeight = false;

        // MUSIC EDIT POPUP
        var editMusicWindow = new GameObject("EditMusicWindow");
        var editMusicRect = editMusicWindow.AddComponent<RectTransform>();
        editMusicRect.SetParent(musicPanelRect);
        editMusicRect.localScale = Vector3.one;
        editMusicRect.anchoredPosition = new Vector2(0, 0);
        editMusicRect.sizeDelta = new Vector2(3000, 2400);
        editMusicWindow.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.03f);
        var editLayout = editMusicWindow.AddComponent<VerticalLayoutGroup>();
        editLayout.childScaleHeight = false;
        editLayout.childControlHeight = false;
        editLayout.childForceExpandWidth = true;
        editLayout.childForceExpandHeight = false;
        editLayout.padding = new RectOffset(50, 50, 50, 50);
        editLayout.spacing = 40;

        // FILE SELECTION MENU

        var fileChooserWindow = new GameObject("choose files window");
        var fileChooserRect = fileChooserWindow.AddComponent<RectTransform>();
        fileChooserRect.SetParent(musicPanelRect);
        fileChooserRect.localScale = Vector3.one;
        fileChooserRect.anchoredPosition = new Vector2(0, 0);
        fileChooserRect.sizeDelta = new Vector2(4000, 2800);
        fileChooserWindow.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.03f);

        var fileChooserHeader = new GameObject("Text").AddComponent<RectTransform>();
        fileChooserHeader.SetParent(fileChooserRect);
        fileChooserHeader.localScale = Vector3.one;
        fileChooserHeader.anchorMin = new Vector2(0, 1);
        fileChooserHeader.anchorMax = Vector2.one;
        fileChooserHeader.offsetMin = new Vector2(0, -200);
        fileChooserHeader.offsetMax = Vector2.zero;
        var fileChooserText = fileChooserHeader.gameObject.AddComponent<Text>();
        fileChooserText.font = ButtonFont;
        fileChooserText.fontSize = 100;
        fileChooserText.alignment = TextAnchor.MiddleCenter;

        var fileChooserViewport = new GameObject("Viewport").AddComponent<RectTransform>();
        fileChooserViewport.SetParent(fileChooserRect);
        fileChooserViewport.localScale = Vector3.one;
        fileChooserViewport.anchorMin = Vector2.zero;
        fileChooserViewport.anchorMax = Vector2.one;
        fileChooserViewport.offsetMin = Vector2.zero;
        fileChooserViewport.offsetMax = new Vector2(0, -200);

        fileChooserViewport.gameObject.AddComponent<Image>();
        fileChooserViewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        var fileChooserContent = new GameObject("Content").AddComponent<RectTransform>();
        fileChooserContent.SetParent(fileChooserViewport);
        fileChooserContent.localScale = Vector3.one;
        fileChooserContent.anchorMin = Vector2.zero;
        fileChooserContent.anchorMax = Vector2.one;
        fileChooserContent.pivot = new Vector2(0.5f, 1);
        fileChooserContent.offsetMin = Vector2.zero;
        fileChooserContent.offsetMax = Vector2.zero;
        fileChooserContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        var fileChooserLayout = fileChooserContent.gameObject.AddComponent<GridLayoutGroup>();
        fileChooserLayout.childAlignment = TextAnchor.UpperCenter;
        fileChooserLayout.cellSize = new Vector2(600, 180);
        fileChooserLayout.spacing = new Vector2(50, 50);
        fileChooserLayout.padding = new RectOffset(50, 50, 50, 50);
        fileChooserLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        fileChooserLayout.constraintCount = FileChooserMenu.ElementsPerRow;

        var filesScrollRect = fileChooserWindow.gameObject.AddComponent<ScrollRect>();
        filesScrollRect.content = fileChooserContent;
        filesScrollRect.horizontal = false;
        filesScrollRect.vertical = true;
        filesScrollRect.viewport = fileChooserViewport;
        filesScrollRect.scrollSensitivity = 50;
        filesScrollRect.movementType = ScrollRect.MovementType.Clamped;

        // CREATE MENUS AND ASSIGN VALUES
        // File chooser menu
        var fileChooser = fileChooserWindow.AddComponent<FileChooserMenu>();
        fileChooser.content = fileChooserContent;
        fileChooser.header = fileChooserText;
        fileChooser.scrollRect = filesScrollRect;
        fileChooserWindow.SetActive(false);

        // Editor menu
        var musicEditor = editMusicWindow.AddComponent<MusicEditor>();
        musicEditor.rect = editMusicRect;
        editMusicWindow.SetActive(false);
        musicEditor.fileChooser = fileChooser;

        // Selection menu
        var menuComponent = musicPanel.AddComponent<CustomMusicMenu>();
        menuComponent.content = content;
        menuComponent.rect = scrollRect;
        menuComponent.categoryText = headerText;
        menuComponent.musicEditor = musicEditor;

        // Cross-references
        fileChooser.editor = musicEditor;
        musicEditor.musicMenu = menuComponent;

        return menuComponent;
    }
}