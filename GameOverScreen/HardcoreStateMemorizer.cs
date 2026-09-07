using UnityEngine;

namespace GameOverScreen;

// Prevents you from cheating and changing to hardcore while the game is running
public class HardcoreStateMemorizer : MonoBehaviour
{
    public static HardcoreStateMemorizer Instance { get; private set; }

    public bool isHardcore;
    
    private void Awake()
    {
        Instance = this;
    }

    public static bool GetIsHardcore()
    {
        if (Instance != null)
        {
            return Instance.isHardcore;
        }
        // Nonissue, no logging needed
        // Plugin.Logger.LogWarning("Failed to find HardcoreStateMemorizer!");
        return Plugin.HardcoreConfig.Value;
    }
}