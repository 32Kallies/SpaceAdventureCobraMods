using System;
using System.Collections.Generic;
using MusicReplacer.LevelMusic;
using UnityEngine;
using UnityEngine.UI;

namespace MusicReplacer.MusicReplacementMenu.Levels;

public class LevelSelectorMenu : MonoBehaviour
{
    public LevelMusicMenu menu;
    public RectTransform episodesParent;
    public RectTransform levelsParent;

    private readonly Color _deselected = new(0.5f, 0.5f, 0.5f, 0.5f);
    private readonly Color _selected = new(1, 1, 1);

    private readonly Color _levelDeselected = new(0.5f, 0.5f, 0.5f, 0.7f);
    private readonly Color _levelSelected = new(0.65f, 0.3f, 0.3f);

    private List<EpisodeButton> _episodeButtons;
    private List<LevelButton> _levelButtons;

    private int _lastChosenEpisode = -1;
    private int _lastChosenLevel = -1;
    private int _chosenEpisode;
    private int _chosenLevel;

    private bool _choosingLevel;

    private void Start()
    {
        GenerateEpisodeButtons();
        
        _chosenEpisode = 0;
        UpdateEpisodeSelection();

        ClearLevelButtons();
    }

    private void OnEnable()
    {
        if (_episodeButtons != null)
        {
            UpdateEpisodeOverrideCheck();
        }
    }

    private void OnDisable()
    {
        StopChoosingLevel();
        ClearLevelButtons();
    }

    private void StartChoosingLevel(int episodeButtonIndex)
    {
        _choosingLevel = true;
        _chosenLevel = 0;
        levelsParent.gameObject.SetActive(true);
        GenerateLevelButtons(_episodeButtons[episodeButtonIndex].episode);
    }

    private void StopChoosingLevel()
    {
        _choosingLevel = false;
        _chosenLevel = 0;
        levelsParent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_choosingLevel)
        {
            UIController.HandleCursor(ref _chosenLevel, _levelButtons.Count, 1, 2, _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate
            {
                menu.levelEditor.ShowWindow(_levelButtons[_chosenLevel].level);
                gameObject.SetActive(false);
            }, StopChoosingLevel);
            if (_chosenLevel != _lastChosenLevel)
            {
                UpdateLevelSelection();
            }
        }
        else
        {
            UIController.HandleCursorXY(ref _chosenEpisode, _episodeButtons.Count, 4, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, UIFooter.PREDEFINEDTYPE.GENERIC_BACK, _allowbuttonscycle: false, delegate
            {
                OnClickEpisode(_chosenEpisode);
            }, () => MusicMenuEnabler.Main.SetTab(MusicMenuEnabler.Tab.MusicEditorHome));
            if (_chosenEpisode != _lastChosenEpisode)
            {
                UpdateEpisodeSelection();
            }
        }
    }

    private void UpdateEpisodeSelection()
    {
        for (int i = 0; i < _episodeButtons.Count; i++)
        {
            _episodeButtons[i].background.color = i == _chosenEpisode ? _selected : _deselected;
        }
        
        _lastChosenEpisode = _chosenEpisode;
    }
    
    private void UpdateLevelSelection()
    {
        for (int i = 0; i < _levelButtons.Count; i++)
        {
            _levelButtons[i].background.color = i == _chosenLevel ? _levelSelected : _levelDeselected;
        }
        
        _lastChosenLevel = _chosenLevel;
    }

    private void UpdateEpisodeOverrideCheck()
    {
        var episodes = _episodeButtons;
        if (episodes == null)
            return;
        foreach (var episodeButton in episodes)
        {
            episodeButton.overrideIndicator.SetActive(GetEpisodeHasOverride(episodeButton.episode));
        }
    }

    private void OnClickEpisode(int buttonIndex)
    {
        StartChoosingLevel(buttonIndex);
    }

    private static Sprite GetSpriteForEpisode(int episodeUid)
    {
        string name = null;
        
        if (episodeUid >= 0)
        {
            var offset = episodeUid + 1;
            name = "Episode" + offset.ToString("00");
        }

        if (!string.IsNullOrEmpty(name))
        {
            return Plugin.Bundle.LoadAsset<Sprite>(name);
        }
        
        Plugin.Logger.LogWarning("Failed to find asset name for episode ID " + episodeUid);
        return null;
    }

    private static bool GetEpisodeHasOverride(EpisodeDefinition episodeDef)
    {
        var episodes = GameController.Instance.GetLevelsDefinitions(episodeDef.UID);
        foreach (var level in episodes)
        {
            if (GetLevelHasOverride(level))
            {
                return true;
            }
        }

        return false;
    }
    
    private static bool GetLevelHasOverride(LevelDefinition levelDef)
    {
        var defaultData = LevelRipper.GetLevelMusicData(levelDef.level);
        var moddedData = LevelOverrideManager.Data.GetLevelData(levelDef.level);

        var currentDefaultMusic = moddedData.AccessDefaultMusic().GetEClip();
        var currentArenaMusic = moddedData.AccessArenaMusic().GetEClip();
        
        if (currentDefaultMusic != 0 && currentDefaultMusic != defaultData.DefaultMusic)
            return true;
        if (currentArenaMusic != 0 && currentArenaMusic != defaultData.ArenaMusic)
            return true;
        if (moddedData.HasAnyTriggerReplacements())
            return true;
        
        return false;
    }

    private void ClearLevelButtons()
    {
        foreach (Transform child in levelsParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void GenerateLevelButtons(EpisodeDefinition episode)
    {
        _levelButtons = new List<LevelButton>();
        _lastChosenLevel = -1;

        ClearLevelButtons();
        
        var levels = GameController.Instance.GetLevelsDefinitions(episode.UID);
        foreach (var level in levels)
        {
            var button = new GameObject("level " + level.levelName);
            var rect = button.AddComponent<RectTransform>();
            rect.SetParent(levelsParent);
            rect.localScale = Vector3.one;
            rect.sizeDelta = new Vector2(800, 300);
            var background = button.AddComponent<Image>();
            background.color = _levelDeselected;
            
            var textGo = new GameObject("Text");
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.SetParent(rect);
            textRect.localScale = Vector3.one;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(0, -100);
            textRect.offsetMax = new Vector2(0, -40);
            var text = textGo.AddComponent<Text>();
            text.font = MusicMenuBuilder.ButtonFont;
            text.fontSize = 90;
            text.alignment = TextAnchor.UpperCenter;
            text.text = GameController.Instance.GetMissionNameText(level.level);
            text.color = Color.white;
            
            var overrideGo = new GameObject("OverrideText");
            var overrideRect = overrideGo.AddComponent<RectTransform>();
            overrideRect.SetParent(rect);
            overrideRect.localScale = Vector3.one;
            overrideRect.anchorMin = Vector2.zero;
            overrideRect.anchorMax = Vector2.one;
            overrideRect.offsetMin = new Vector2(0, -100);
            overrideRect.offsetMax = new Vector2(0, -40);
            overrideRect.localPosition += Vector3.down * 190;
            var overrideText = overrideGo.AddComponent<Text>();
            overrideText.font = MusicMenuBuilder.ButtonFont;
            overrideText.fontSize = 40;
            overrideText.alignment = TextAnchor.UpperCenter;
            overrideText.text = "HAS CUSTOM OVERRIDE";
            overrideText.color = Color.yellow;
            
            var descGo = new GameObject("Description");
            var descRect = descGo.AddComponent<RectTransform>();
            descRect.SetParent(rect);
            descRect.localScale = Vector3.one;
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = new Vector2(0, -160);
            var desc = descGo.AddComponent<Text>();
            desc.font = MusicMenuBuilder.ButtonFont;
            desc.fontSize = 50;
            desc.alignment = TextAnchor.UpperCenter;
            desc.text = TextsController.Instance.GetText(
                GameController.Instance.GetLevelDescriptionTextId(level.level));
            desc.color = Color.white;
            
            overrideGo.SetActive(GetLevelHasOverride(level));
            
            _levelButtons.Add(new LevelButton
            {
                level = level,
                background = background,
                overrideIndicator = overrideGo
            });
        }
    }
    
    private void GenerateEpisodeButtons()
    {
        _episodeButtons = new List<EpisodeButton>();
        _lastChosenEpisode = -1;
        
        var episodes = GameController.Instance.episodesDefs;
        foreach (var episode in episodes)
        {
            if (episode.UID < 0)
                continue;
            
            if (episode.UID == 11)
                continue;
            
            var button = new GameObject("episode " + episode.UID);
            var rect = button.AddComponent<RectTransform>();
            rect.SetParent(episodesParent);
            rect.localScale = Vector3.one;
            var grid = episodesParent.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                Plugin.Logger.LogWarning("GridLayoutGroup not found on " + episodesParent.gameObject.name);
            }
            rect.sizeDelta = grid.cellSize;
           
            var bg = button.AddComponent<Image>();
            bg.color = _deselected;
            
            var mask = new GameObject("Mask");
            var maskRect = mask.AddComponent<RectTransform>();
            maskRect.SetParent(rect);
            maskRect.localScale = Vector3.one;
            maskRect.localPosition = Vector3.zero;
            maskRect.sizeDelta = new Vector2(165, 87) * 3;
            maskRect.anchoredPosition = new Vector2(0, 30);
            mask.AddComponent<Image>();
            var maskComponent = mask.AddComponent<Mask>();
            maskComponent.showMaskGraphic = false;

            var imageGo = new GameObject("LevelImage");
            var imageRect = imageGo.AddComponent<RectTransform>();
            imageRect.SetParent(maskRect);
            imageRect.localScale = Vector3.one;
            imageRect.localPosition = Vector3.zero;
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.sizeDelta += new Vector2(0, 80);
            imageRect.transform.localPosition += Vector3.down * 50;
            var image = imageGo.AddComponent<Image>();
            image.sprite = GetSpriteForEpisode(episode.UID);

            var levelText = new GameObject("Text");
            var levelTextRect = levelText.AddComponent<RectTransform>();
            levelTextRect.SetParent(button.transform);
            levelTextRect.localScale = Vector3.one;
            levelTextRect.localPosition = Vector3.zero;
            levelTextRect.anchorMin = Vector2.zero;
            levelTextRect.anchorMax = Vector2.one;
            levelTextRect.pivot = new Vector2(0.5f, 0);
            levelTextRect.offsetMin = new Vector2(0, 15);
            var text = levelText.AddComponent<Text>();
            text.font = MusicMenuBuilder.ButtonFont;
            text.fontSize = 70;
            text.alignment = TextAnchor.LowerCenter;
            text.text = TextsController.Instance.GetText(GameController.Instance.GetEpisodeNumberTextId(episode.UID));
            text.color = Color.black;
            
            var overrideTextObj = new GameObject("OverrideText");
            var overrideTextRect = overrideTextObj.AddComponent<RectTransform>();
            overrideTextRect.SetParent(button.transform);
            overrideTextRect.localScale = Vector3.one;
            overrideTextRect.localPosition = Vector3.zero;
            overrideTextRect.anchorMin = Vector2.zero;
            overrideTextRect.anchorMax = Vector2.one;
            overrideTextRect.pivot = new Vector2(0.5f, 0);
            overrideTextRect.offsetMin = new Vector2(0, -60);
            overrideTextRect.offsetMax = new Vector2(0, -15);
            overrideTextRect.localPosition += Vector3.down * 390;
            var overrideText = overrideTextObj.AddComponent<Text>();
            overrideText.font = MusicMenuBuilder.ButtonFont;
            overrideText.fontSize = 35;
            overrideText.alignment = TextAnchor.UpperCenter;
            overrideText.text = "HAS CUSTOM OVERRIDE";
            overrideText.color = Color.yellow;
            
            _episodeButtons.Add(new EpisodeButton
            {
                background = bg,
                uid = episode.UID,
                episode = episode,
                overrideIndicator = overrideTextObj
            });
        }

        UpdateEpisodeOverrideCheck();
    }

    private class EpisodeButton
    {
        public Image background;
        public int uid;
        public EpisodeDefinition episode;
        public GameObject overrideIndicator;
    }
    
    private class LevelButton
    {
        public Image background;
        public LevelDefinition level;
        public GameObject overrideIndicator;
    }
}