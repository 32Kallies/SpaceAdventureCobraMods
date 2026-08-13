using UnityEngine;

namespace Cobra1982AnimeSoundPatches.Behaviours;

public class OneTimeSoundTrigger : MonoBehaviour
{
    public audioSelectionData.eCLIP clip;

    private Collider _collider;
    private bool _played;

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnDestroy()
    {
        _collider = null;
    }

    private void Update()
    {
        if (_played) return;
        
        if (LevelController.Instance != null && CobraCharacter.Instance != null && audioReverbTrigger.IsPointInCollider(_collider, CobraCharacter.Instance.transform.position))
        {
            AudioController.Instance.PlaySound(clip);
            _played = true;
        }
    }
}
