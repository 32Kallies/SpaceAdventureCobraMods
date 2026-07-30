using UnityEngine;

namespace Cobra1982AnimeSoundPatches.Behaviours;

public class CrystalBoyPassThroughSoundPlayer : MonoBehaviour
{
    public float radius = 0.7f;
    public audioSelectionData.eCLIP clip;
    private bool _hit;

    private static ushort _volumeMultiplier = 2;
    
    private static readonly Collider[] SharedBuffer = new Collider[16];
    
    private void FixedUpdate()
    {
        if (_hit) return;
        
        var hits = Physics.OverlapSphereNonAlloc(transform.position, radius, SharedBuffer, -1);
        for (int i = 0; i < hits; i++)
        {
            if (SharedBuffer[i] == null)
                continue;
            var boy = SharedBuffer[i].GetComponentInParent<NmiCrystalBowie>();
            if (boy != null)
            {
                OnHitCrystalBoy();
                return;
            }
        }
    }

    private void OnHitCrystalBoy()
    {
        _hit = true;
        for (ushort i = 0; i < _volumeMultiplier; i++)
        {
            AudioController.Instance.PlaySound(clip);
        }
    }
}