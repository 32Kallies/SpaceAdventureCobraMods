using System;
using UnityEngine;

namespace NoFramerateCap.ElevatorLevelFix;

public class ElevatorArenaIdentifier : MonoBehaviour
{
    public NmiArena arena;
    private int _id;
    
    private void Start()
    {
        _id = IdentifyElevatorArenaByFirstWaveChild(arena);
        Plugin.Logger.LogInfo($"Identified arena {gameObject} with ID: {_id}");
    }

    public int GetId()
    {
        return _id;
    }

    private static int IdentifyElevatorArenaByFirstWaveChild(NmiArena arena)
    {
        var waves = arena.transform.Find("Waves");
        if (waves == null)
        {
            return -1;
        }

        var firstWave = waves.GetChild(0);
        if (firstWave == null)
        {
            return -1;
        }

        var firstEnemy = firstWave.transform.GetChild(0);

        if (firstEnemy != null && firstEnemy.name.Equals("label", StringComparison.OrdinalIgnoreCase))
        {
            firstEnemy = firstWave.transform.GetChild(1);
        }
        
        if (firstEnemy == null)
        {
            return -1;
        }

        var firstEnemyPosition = firstEnemy.position;
        int hash = unchecked(Mathf.RoundToInt(firstEnemyPosition.x)
                             + 31 * Mathf.RoundToInt(firstEnemyPosition.y)
                             + 31 * 31 * Mathf.RoundToInt(firstEnemyPosition.z));
        return hash;
    }
}