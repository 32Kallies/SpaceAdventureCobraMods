namespace TeleportationDoorsCheat;

public static class TeleportationCheatEnabler
{
    // This is slow
    public static void ActivateTeleporters()
    {
        var levelActivities = UnityEngine.Object.FindObjectsOfType<LevelActivity>();
        foreach (var obj in levelActivities)
        {
            obj.isDebug = false;
        }
    } 
}