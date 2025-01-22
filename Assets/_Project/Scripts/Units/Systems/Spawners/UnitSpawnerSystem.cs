using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(EnemyUnitSpawnerSystem))]
partial struct UnitSpawnerSystem : ISystem
{
    private const float DISTANCE_IN_FORMATION = 2;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRW<UnitSpawner> unitSpawner, Entity entity) in SystemAPI.Query<RefRW<UnitSpawner>>().WithEntityAccess())
        {
            if (unitSpawner.ValueRO.SpawnPosition.HasValue)
            {
                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(entity);
                UnitPrefabBufferElement unit = unitPrefabsBuffer[(int)unitSpawner.ValueRO.UnitToSpawn];

                SpawnFormation(ecb, ref state, unit, unitSpawner.ValueRO.SpawnPosition.Value, 0, unitSpawner.ValueRO.UnitToSpawn);

                unitSpawner.ValueRW.SpawnPosition = null;
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private void SpawnFormation(EntityCommandBuffer ecb, ref SystemState state, UnitPrefabBufferElement unit, float3 basePos, int team, UnitTypes type)
    {
        Entity prefabElement = unit.UnitPrefabEntity;
        int count = unit.Count;
        int length = (int)math.ceil(math.sqrt(count / 2.0f));
        int width = length * 2;

        // Spawn leader
        Entity leader = ecb.Instantiate(prefabElement);
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
        ecb.SetComponent(leader, LocalTransform.FromPosition(basePos));

        for (int i = 0; i < count; i++)
        {
            Entity follower = ecb.Instantiate(prefabElement);
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
            ecb.SetComponent(follower, LocalTransform.FromPosition(pos));
        }
    }
}
