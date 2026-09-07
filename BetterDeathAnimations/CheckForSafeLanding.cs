using UnityEngine;

namespace BetterDeathAnimations;

public static class CheckForSafeLanding
{
    public static bool HasGroundAtPoint(Vector3 groundPoint, float radius, float heightFromGround, int concentricChecks,
        int quality, float depthTolerance)
    {
        if (concentricChecks <= 0 || quality <= 0) return false;
        const int groundLayer =
            (1 << 0) // layer 0: default
            | (1 << 20); // layer 20: 1-way platform
        for (int ring = 1; ring <= concentricChecks; ring++)
        {
            float ringRadius = radius * ring / concentricChecks;
            for (int i = 0; i < quality; i++)
            {
                float angle = i * (Mathf.PI * 2f / quality);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;
                Vector3 origin = groundPoint + offset + Vector3.up * heightFromGround;
                if (!Physics.Raycast(origin, Vector3.down, heightFromGround + depthTolerance, groundLayer,
                        QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
            }
        }

        return true;
    }
}