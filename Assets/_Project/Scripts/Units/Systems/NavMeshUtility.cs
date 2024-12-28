using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.AI;

[InternalBufferCapacity(8)]
public struct PathBufferElement : IBufferElementData
{
    public float3 Position;
}

public static class NavMeshUtility
{
    public static void CalculatePath(float3 start, float3 end, DynamicBuffer<PathBufferElement> pathBuffer)
    {
        NavMeshPath navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, navPath))
        {
            pathBuffer.Clear();

            foreach (var corner in navPath.corners)
            {
                pathBuffer.Add(new PathBufferElement { Position = (float3)corner });
            }
        }
    }
}

