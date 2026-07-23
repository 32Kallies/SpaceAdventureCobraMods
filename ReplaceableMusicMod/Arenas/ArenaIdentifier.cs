using UnityEngine;

namespace MusicReplacer.Arenas;

public class ArenaIdentifier : MonoBehaviour
{
    public long id;
    public bool isInitialized;

    public void SetId(long id)
    {
        this.id = id;
        isInitialized = true;
    }
    
    public static long GetArenaId(NmiArena arena)
    {
        var identifier = arena.GetComponent<ArenaIdentifier>();
        
        if (identifier == null)
        {
            Plugin.Logger.LogError("Failed to find Arena ID on arena: " + arena);
            return 0;
        }

        if (!identifier.isInitialized)
        {
            Plugin.Logger.LogWarning("Arena ID not yet initialized on arena: " + arena);
        }

        return identifier.id;
    }
}