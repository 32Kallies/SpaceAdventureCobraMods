using UnityEngine;

namespace BetterDeathAnimations;

// Set by patches in the TakeDamagePatches class and read by the DeathAnimationPatches class 
public class DisableDisintegrationTag : MonoBehaviour
{
    public bool disableDisintegration = true;
}