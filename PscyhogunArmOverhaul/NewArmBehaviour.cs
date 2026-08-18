using System.Collections;
using Rewired;
using UnityEngine;
using UnityEngine.Animations;

namespace PscyhogunArmOverhaul;

public class NewArmBehaviour : MonoBehaviour
{
    public static NewArmBehaviour Instance { get; private set; }
    
    public CobraCharacter character;
    public Transform prostheticArmTarget;

    public RuntimeAdditiveAnimation additiveAnimation;
    public AudioSource armOffSound;
    
    private bool _puttingArmBackOn;
    private bool _takingArmOff;
    
    private bool _prostheticOn;
    private bool _forceGrabArmModelEnabled;

    private GameObject _newFist;

    private LevelController.Level _level;
    private bool _johnsonDiscoveredPsychogun; // for level 1-2 only
    
    private void Start()
    {
        Instance = this;
        // ensure that this only works on the first time in a stage (not upon switching characters)
        if (PsychogunStateRememberer.GetInstance(true) == null)
        {
            PsychogunStateRememberer.GetInstance().SetToken(Token.HardCodedTokens.ForcePsychogunOff, true);
            _prostheticOn = true;
        }
        else
        {
            _prostheticOn = TokenController.GetTokenValue(Token.HardCodedTokens.ForcePsychogunOn) <= 0 &&
                            TokenController.GetTokenValue(Token.HardCodedTokens.ForcePsychogunOff) >= 0;
        }
        _newFist = Instantiate(character.dependencies.unskinnedProthese);
        _newFist.SetActive(false);
        Destroy(_newFist.GetComponent<ParentConstraint>());

        additiveAnimation.OnLateUpdate = DoLateUpdate;

        if (LevelController.Instance != null)
        {
            _level = LevelController.Instance.level;
        }

        if (_level == LevelController.Level.EP01_LVL02_Casino_BossVaiken)
        {
            var position = character.transform.position;
            _johnsonDiscoveredPsychogun = !(position.y is > -2 and < 2 && position.x is > 14 and < 60);
        }
        else
        {
            _johnsonDiscoveredPsychogun = true;
        }
    }

    public bool GetCanShoot()
    {
        if (_takingArmOff || _puttingArmBackOn)
            return false;
        
        return !_prostheticOn;
    }
    
    private void Update()
    {
        if (_puttingArmBackOn || _takingArmOff)
            return;

        if (!CanChangeArmState())
        {
            return;
        }

        if (CutscenePlayer.IsPlaying)
        {
            return;
        }

        if (CanTakeOffArm())
        {
            if (Input.GetKeyDown(Plugin.KeyboardBinding.Value) || GetRightStickClick())
            {
                if (ShouldTakeArmOffInstantlyForEp1Lvl2())
                {
                    TakeArmOffInstantly();
                }
                else
                {
                    StartCoroutine(TakeArmOff());
                }
            }
        }
        else if (CanPutOnArm())
        {
            if (Input.GetKeyDown(Plugin.KeyboardBinding.Value) || GetRightStickClick())
            {
                StartCoroutine(PutArmBackOn());
            }
        }
    }

    private bool CanTakeOffArm()
    {
        return _prostheticOn && TokenController.GetTokenValue(Token.HardCodedTokens.ForcePsychogunOff) <= 1;
    }
    
    private bool CanPutOnArm()
    {
        return !_prostheticOn && TokenController.GetTokenValue(Token.HardCodedTokens.ForcePsychogunOn) <= 1;
    }

    private bool CanChangeArmState()
    {
        if (character.isDieThenTeleportBackStarted)
        {
            return false;
        }

        if (character.timeSinceMelee < character.melee.noShootAfterMeleeDelay &&
            character.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.23f)
        {
            return false;
        }

        // Disable psychogun opening at beginning of 1-2
        if (_level == LevelController.Level.EP01_LVL02_Casino_BossVaiken)
        {
            var position = character.transform.position;
            if (position.y is > -2 and < 2 && position.x is > 14 and < 60)
            {
                return false;
            }
        }

        return true;
    }

    private void DoLateUpdate()
    {
        _newFist.SetActive(_forceGrabArmModelEnabled);
        if (_forceGrabArmModelEnabled)
        {
            _newFist.transform.position = prostheticArmTarget.position;
            _newFist.transform.eulerAngles = prostheticArmTarget.eulerAngles;
        }
    }

    private float GetAnimationDuration(float speed)
    {
        return additiveAnimation.ClipLength / speed;
    }

    public void OnFailToShootPsychogun()
    {
        if (!CanChangeArmState())
            return;
        
        if (!_takingArmOff && !_puttingArmBackOn && _prostheticOn)
        {
            if (ShouldTakeArmOffInstantlyForEp1Lvl2())
            {
                TakeArmOffInstantly();
            }
            else
            {
                StartCoroutine(TakeArmOff(3f));
            }
        }
    }

    public void PutOnArmForDialogue()
    {
        if (CanChangeArmState() && CanPutOnArm())
        {
            StartCoroutine(PutArmBackOn(1));
        }
    }
    
    public void TakeOffArm()
    {
        if (CanChangeArmState() && CanTakeOffArm())
        {
            StartCoroutine(TakeArmOff(1));
        }
    }
    
    private IEnumerator PutArmBackOn(float speedMultiplier = 1f)
    {
        _puttingArmBackOn = true;
        
        PlayAdditiveAnimation(speedMultiplier);

        yield return new WaitForSeconds(GetAnimationDuration(speedMultiplier) * 0.2f);
        
        _forceGrabArmModelEnabled = true;
        
        yield return new WaitForSeconds(GetAnimationDuration(speedMultiplier) * 0.3f);

        _forceGrabArmModelEnabled = false;
        // finish putting on
        var state = PsychogunStateRememberer.GetInstance();
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, false);
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, true);
        character.ProtheseOn();
        
        _prostheticOn = true;
        _puttingArmBackOn = false;
    }

    private IEnumerator TakeArmOff(float speed = 2f)
    {
        _takingArmOff = true;
        PlayAdditiveAnimation(speed, true);
        yield return new WaitForSeconds(GetAnimationDuration(speed) * 0.5f);
        
        armOffSound.Play();
        _forceGrabArmModelEnabled = true;
        // finish taking off
        var state = PsychogunStateRememberer.GetInstance();
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, true);
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, false);
        
        yield return new WaitForSeconds(GetAnimationDuration(speed) * 0.3f);
        _forceGrabArmModelEnabled = false;

        _prostheticOn = false;
        _takingArmOff = false;
    }

    public void TakeArmOffInstantly()
    {
        if (_takingArmOff)
            return;
        
        Plugin.Logger.LogInfo("Taking arm off instantly");
        var state = PsychogunStateRememberer.GetInstance();
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, true);
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, false);
        _forceGrabArmModelEnabled = false;
        _prostheticOn = false;
        _takingArmOff = false;
        _johnsonDiscoveredPsychogun = true;
    }
    
    public void PutArmOnInstantly()
    {
        if (_puttingArmBackOn || _takingArmOff) return;
        if (_prostheticOn) return;
        
        Plugin.Logger.LogInfo("Putting arm on instantly");

        var state = PsychogunStateRememberer.GetInstance();
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, false);
        state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, true);
        character.ProtheseOn();
        _forceGrabArmModelEnabled = false;
        _prostheticOn = true;
        _puttingArmBackOn = false;
    }
    
    private void PlayAdditiveAnimation(float speed = 1f, bool reverse = false)
    {
        if (reverse)
            additiveAnimation.PlayInReverse(speed);
        else
            additiveAnimation.Play(speed);
    }

    private static bool GetRightStickClick()
    {
        if (ReInput.controllers.joystickCount == 0)
            return false;
        Joystick j = ReInput.controllers.Joysticks[0];
        return j.GetButton(10);
    }

    private bool ShouldTakeArmOffInstantlyForEp1Lvl2()
    {
        if (_level != LevelController.Level.EP01_LVL02_Casino_BossVaiken)
        {
            return false;
        }
        
        if (_johnsonDiscoveredPsychogun)
        {
            return false;
        }
        
        var position = character.transform.position;
        if (position.x is > 133 and < 146 && position.y is > -2 and < 2)
        {
            return true;
        }

        return false;
    }
}