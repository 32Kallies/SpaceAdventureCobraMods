using UnityEngine;

namespace GameOverScreen;

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
        Plugin.Logger.LogWarning("Failed to find HardcoreStateMemorizer!");
        return Plugin.HardcoreConfig.Value;
    }
}