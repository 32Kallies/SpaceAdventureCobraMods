using UnityEngine;

namespace PsychogunImproved.Behaviour;

public class ImprovedPsychogunBehaviour : MonoBehaviour
{
    private const float ShootCooldown = 0.8f;

    public RuntimeAdditiveAnimation animation;
    
    private float _timeShootAgain;

    public void StartShotCooldown()
    {
        _timeShootAgain = Time.time + ShootCooldown;
    }

    public void PlayShootAnimation()
    {
        animation.Play();
    }

    public bool GetCanShoot() => Time.time > _timeShootAgain;
}