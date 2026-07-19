using UnityEngine;

namespace NoFramerateCap.ElevatorLevelFix;

// Keeps the elevator triggers enabled permanently once they are enabled to prevent one-frame windows of collisions
public class EnableWatcher : MonoBehaviour
{
    public ElevatorTriggerFixer fixer;
    
    private void OnEnable()
    {
        fixer.toForceEnabled.Add(gameObject);
    }
}