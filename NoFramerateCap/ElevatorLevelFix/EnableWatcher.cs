using UnityEngine;

namespace NoFramerateCap.ElevatorLevelFix;

public class EnableWatcher : MonoBehaviour
{
    public ElevatorTriggerFixer fixer;
    
    private void OnEnable()
    {
        fixer.toForceEnabled.Add(gameObject);
    }
}