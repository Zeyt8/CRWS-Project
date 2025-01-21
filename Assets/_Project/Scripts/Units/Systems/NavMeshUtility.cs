using Unity.Mathematics;
using UnityEngine.AI;

public static class NavMeshUtility
{
    public static float SampleHeight(float3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1, NavMesh.AllAreas))
        {
            return hit.position.y;
        }
        return position.y;
    }
}

