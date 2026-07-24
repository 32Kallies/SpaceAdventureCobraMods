using UnityEngine;

namespace JohnsonJacket;

public class CobraHouseCutsceneRememberer : MonoBehaviour
{
    private static CobraHouseCutsceneRememberer Instance { get; set; }

    private bool _cutsceneCompleted;

    public static bool HasCobraVisitedHouse()
    {
        return Instance != null && Instance._cutsceneCompleted;
    }

    public static void OnEp1Lvl2Started()
    {
        if (Instance != null) return;
        var controller = LevelController.Instance;
        if (controller != null)
        {
            Instance = controller.gameObject.AddComponent<CobraHouseCutsceneRememberer>();
        }
    }

    public static void OnCutsceneCompleted()
    {
        if (Instance == null) OnEp1Lvl2Started();
        Instance._cutsceneCompleted = true;
    }
}