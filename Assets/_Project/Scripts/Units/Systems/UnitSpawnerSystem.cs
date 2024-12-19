using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out UnitSpawner unitSpawner))
        {
            if (unitSpawner.UnitToSpawn.HasValue)
            {
                Entity unit = state.EntityManager.Instantiate(unitSpawner.UnitPrefabEntity);

                float3 spawnPosition = new float3(0, 0, 0);
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(spawnPosition));

                float3 targetPosition = new float3(130, 0, -50);

                if (!SystemAPI.HasBuffer<PathBufferElement>(unit))
                    state.EntityManager.AddBuffer<PathBufferElement>(unit);

                var pathBuffer = SystemAPI.GetBuffer<PathBufferElement>(unit);
                NavMeshUtility.CalculatePath(spawnPosition, targetPosition, pathBuffer);

                Movement ms = SystemAPI.GetComponent<Movement>(unit);
                ms.Target = targetPosition;
                ms.CurrentPathIndex = 0; 
                SystemAPI.SetComponent(unit, ms);

                unitSpawner.UnitToSpawn = null;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }
}
