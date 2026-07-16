using System.Collections.Generic;
using UnityEngine;

namespace GameOverScreen.UI;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    public float buttonMoveDelay = 1f;
    public float buttonPressDelay = 1.01f;
    
    public NUIButton restartStageButton;
    public NUIButton restartCheckpointButton;
    public NUIButton exitToMenuButton;
    
    private List<NUIButton> _buttons;
    private int _buttonChoice;
    private float _startTime;
    private audioSelectionData.eCLIP _silence;

    private bool _pressedAny;
    
    public static bool IsScreenShown { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Plugin.Logger.LogError("Multiple GameOverScreens found! Destroying new instance");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        IsScreenShown = true;
    }

    private void Start()
    {
        PlayMusic();
        InitializeButtons(HardcoreStateMemorizer.GetIsHardcore());
    }

    private void PlayMusic()
    {
        if (!CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("cobra_died", out var diedClip))
        {
            Plugin.Logger.LogError("Failed to find Cobra Died sound");
            return;
        }
        
        AudioController.Instance.PlaySound(diedClip);
        
        if (!CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("game_over_screen_silence", out _silence))
        {
            _silence = audioSelectionData.eCLIP.NONE;
            Plugin.Logger.LogError("Failed to find silence sound");
        }
    }

    private void InitializeButtons(bool hardcore)
    {
        _buttons = new List<NUIButton>();
        if (hardcore)
        {
            restartCheckpointButton.gameObject.SetActive(false);
        }
        else
        {
            _buttons.Add(restartCheckpointButton);
        }
        _buttons.Add(restartStageButton);
        _buttons.Add(exitToMenuButton);
        UpdateSelectedButton();
        _startTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (_silence != audioSelectionData.eCLIP.NONE)
        {
            var audio = AudioController.Instance;
            audio.m_ForcedMusic = audioSelectionData.eCLIP.NONE;
            audio.ForceMusicThisFrame(_silence, false, 1337);
        }

        if (_pressedAny) return;
        if (Time.realtimeSinceStartup < _startTime + buttonMoveDelay) return;
        
        UIController.HandleCursor(ref _buttonChoice, _buttons.Count, 1, 2,
            _allowbuttonscycle: false, UIFooter.PREDEFINEDTYPE.GENERIC_VALIDATE, delegate
        {
            _buttons[_buttonChoice].TrigClick(delegate
            {
                if (Time.realtimeSinceStartup < _startTime + buttonPressDelay) return;
                var customButton = _buttons[_buttonChoice].GetComponent<CustomButton>();
                if (customButton == null)
                {
                    Plugin.Logger.LogError("Failed to find CustomButton with callback on button object");
                }
                else
                {
                    customButton.Click();
                }

                _pressedAny = true;
            });
        });

        UpdateSelectedButton();
    }

    private void UpdateSelectedButton()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].IsSelected = _buttonChoice == i;
        }
    }

    public static void Close()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
    }

    private void OnDestroy()
    {
        IsScreenShown = false;
    }
}