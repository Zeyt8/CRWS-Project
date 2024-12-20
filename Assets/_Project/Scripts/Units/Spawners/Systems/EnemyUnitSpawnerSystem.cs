using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyUnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyUnitSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out EnemyUnitSpawner unitSpawner))
        {
            if (unitSpawner.Count > 0)
            {
                Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemyUnitSpawner>();

                // Spawn Unit
                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(spawnerEntity);
                Entity unit = state.EntityManager.Instantiate(unitPrefabsBuffer[(int)unitSpawner.Random.NextFloat(0f, 12f)].UnitPrefabEntity);
                if (!SystemAPI.HasBuffer<PathBufferElement>(unit))
                    state.EntityManager.AddBuffer<PathBufferElement>(unit);

                // Set team
                var teamValue = SystemAPI.GetComponent<TeamData>(unit);
                teamValue.Value = 1;
                SystemAPI.SetComponent<TeamData>(unit, teamValue);

                // Set spawn position
                float3 pos = float3.zero;
                pos.x = unitSpawner.Random.NextFloat(-unitSpawner.SpawnWidth, unitSpawner.SpawnWidth);
                pos.z = unitSpawner.Random.NextFloat(-unitSpawner.SpawnLength, unitSpawner.SpawnLength);
                pos += SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(pos));

                // Update spawner
                unitSpawner.Count--;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }
}
