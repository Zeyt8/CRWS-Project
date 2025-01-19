using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyUnitSpawnerSystem : ISystem
{
    private const float DISTANCE_IN_FORMATION = 2;

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
                int unitToSpawn = unitSpawner.Random.NextInt(0, 12);
                UnitPrefabBufferElement unit = unitPrefabsBuffer[unitToSpawn];

                // Set spawn position
                float3 basePos = SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;
                basePos.x += unitSpawner.Random.NextFloat(-unitSpawner.SpawnBounds.x, unitSpawner.SpawnBounds.x);
                basePos.z += unitSpawner.Random.NextFloat(-unitSpawner.SpawnBounds.y, unitSpawner.SpawnBounds.y);
                SpawnFormation(ref state, unit, basePos, 1, (UnitTypes)unitToSpawn);

                // Update spawner
                unitSpawner.Count -= unit.Count;
                
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }

    [BurstCompile]
    private void SpawnFormation(ref SystemState state, UnitPrefabBufferElement unit, float3 basePos, int team, UnitTypes type)
    {
        Entity prefabElement = unit.UnitPrefabEntity;
        int count = unit.Count;
        int length = (int)math.ceil(math.sqrt(count / 2.0f));
        int width = length * 2;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        // Spawn leader
        Entity leader = state.EntityManager.Instantiate(prefabElement);
        ecb.AddComponent(leader, new LeaderPathfinding
        {
            CurrentPathIndex = 0,
            Target = float3.zero,
            IsMoving = false,
        });
        ecb.AddComponent(leader, new TeamData
        {
            Value = team,
        });
        ecb.AddComponent(leader, new UnitTypeData
        {
            Value = type,
        });
        SystemAPI.SetComponent(leader, LocalTransform.FromPosition(basePos));

        for (int i = 0; i < count; i++)
        {
            Entity follower = state.EntityManager.Instantiate(prefabElement);
            float3 pos = basePos;
            pos.x += (i % width) * DISTANCE_IN_FORMATION - (width - 1) * DISTANCE_IN_FORMATION / 2;
            pos.z -= (i / width) * DISTANCE_IN_FORMATION + DISTANCE_IN_FORMATION;
            ecb.AddComponent(follower, new FollowerPathfinding
            {
                Leader = leader,
                FormationOffset = pos - basePos,
                ViewRadius = 5,
                AvoidanceRadius = 1,
            });
            ecb.AddComponent(follower, new TeamData
            {
                Value = team,
            });
            ecb.AddComponent(follower, new UnitTypeData
            {
                Value = type,
            });
            SystemAPI.SetComponent(follower, LocalTransform.FromPosition(pos));
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
