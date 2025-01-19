using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

partial struct UnitSpawnerSystem : ISystem
{
    private const float DISTANCE_IN_FORMATION = 2;

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
            if (unitSpawner.SpawnPosition.HasValue)
            {
                Entity spawnerEntity = SystemAPI.GetSingletonEntity<UnitSpawner>();
                float3 basePos = SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;

                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(spawnerEntity);
                UnitPrefabBufferElement unit = unitPrefabsBuffer[(int)unitSpawner.UnitToSpawn];

                SpawnFormation(ref state, unit, unitSpawner.SpawnPosition.Value, 0, unitSpawner.UnitToSpawn);

                unitSpawner.SpawnPosition = null;
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
