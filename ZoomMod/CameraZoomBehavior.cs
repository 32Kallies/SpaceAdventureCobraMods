using UnityEngine;

namespace ZoomMod;

public class CameraZoomBehavior : MonoBehaviour
{
    public static CameraZoomBehavior main;
    
    public CameraController controller;
    public float minZoomPercent = 0.2f;
    public float maxZoomPercent = 1f;
    public float absoluteMaxZoomPercent = 1.18f;
    public float zoomSpeed = 0.8f;
    public float absoluteMinFov = 8;
    public float scrollWheelSpeedMultiplier = 8f;
    public float zoomFarOutDuration = 1.2f;
    public float delayWhenZoomInBackToDefault = 0.35f;

    public float slowdownZoomMin = 0.2f;
    public float slowdownZoomMax = 0.4f;
    public float slowdownMultMin = 0f; // occurs at slowdownZoomMin
    public float slowdownMultMax = 1; // occurs at slowdownZoomMax
    
    private float _fovPercent = 1f;

    private float _defaultFov;

    private bool _tryingToZoomFarOut;
    private float _timeStartFarOutZoomAttempt;
    
    private float _timeZoomedToDefaultFromFarOut;
    
    private void Start()
    {
        main = this;
        _defaultFov = controller.defaultFov;
    }

    public bool IsZoomedOut() => _fovPercent >= maxZoomPercent - Mathf.Epsilon;

    private void Update()
    {
        if (PadsController.Instance.isCont(PadsController.BUTTON_RIGHT))
        {
            AdjustZoom(true); // zoom in
        }
        else if (PadsController.Instance.isCont(PadsController.BUTTON_LEFT))
        {
            AdjustZoom(false); // zoom out
        }
        else
        {
            _tryingToZoomFarOut = false;
        }
    }

    public float GetFovMultiplier() => _fovPercent;

    private void AdjustZoom(bool zoomIn)
    {
        if (zoomIn && Mathf.Approximately(_fovPercent, maxZoomPercent) && Time.time < _timeZoomedToDefaultFromFarOut + delayWhenZoomInBackToDefault)
            return;
        
        bool scrollWheel = IsUsingScrollWheel();
        if (scrollWheel)
        {
            zoomIn = !zoomIn;
        }
        
        int direction = zoomIn ? -1 : 1;
        
        var change = direction * zoomSpeed * GetZoomSpeedMultiplier(zoomIn);

        if (scrollWheel)
        {
            change *= scrollWheelSpeedMultiplier;
        }

        float newZoom = _fovPercent + change * Time.deltaTime;

        if (!_tryingToZoomFarOut && Mathf.Approximately(_fovPercent, maxZoomPercent) && !zoomIn)
        {
            _tryingToZoomFarOut = true;
            _timeStartFarOutZoomAttempt = Time.realtimeSinceStartup;
        }

        if (zoomIn && _fovPercent > maxZoomPercent && newZoom <= maxZoomPercent)
        {
            _timeZoomedToDefaultFromFarOut = Time.time;
            newZoom = maxZoomPercent;
        }
        
        _fovPercent = Mathf.Clamp(newZoom, minZoomPercent, GetMaxZoomPercent());
        controller.defaultFov = Mathf.Max(absoluteMinFov, newZoom * _defaultFov);
    }

    private float GetMaxZoomPercent()
    {
        if (_fovPercent > maxZoomPercent)
        {
            return absoluteMaxZoomPercent;
        }
        
        if (!_tryingToZoomFarOut || Time.realtimeSinceStartup < _timeStartFarOutZoomAttempt + zoomFarOutDuration)
        {
            return maxZoomPercent;
        }

        return absoluteMaxZoomPercent;
    }

    private bool IsUsingScrollWheel()
    {
        return CobraCharacter.Instance != null && CobraCharacter.Instance.useMouse;
    }

    private float GetZoomSpeedMultiplier(bool zoomIn)
    {
        if (zoomIn == false)
            return 1;

        return RemapValue(_fovPercent, slowdownZoomMin, slowdownZoomMax, slowdownMultMin, slowdownMultMax);
    }
    
    private static float RemapValue(float value, float originalFrom, float originalTo, float newFrom, float newTo)
    {
        return Mathf.Lerp(newFrom, newTo, Mathf.InverseLerp(originalFrom, originalTo, value));
    }
}