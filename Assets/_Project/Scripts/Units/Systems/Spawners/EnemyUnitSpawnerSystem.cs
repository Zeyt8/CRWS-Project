using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct EnemyUnitSpawnerSystem : ISystem
{
    private const float DISTANCE_IN_FORMATION = 2;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRW<EnemyUnitSpawner> unitSpawner, Entity entity) in SystemAPI.Query<RefRW<EnemyUnitSpawner>>().WithEntityAccess())
        {
            if (unitSpawner.ValueRO.Count > 0)
            {
                // Spawn Unit
                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(entity);
                int unitToSpawn = unitSpawner.ValueRO.Random.NextInt(0, 12);
                UnitPrefabBufferElement unit = unitPrefabsBuffer[unitToSpawn];

                // Set spawn position
                LocalTransform spawnerTransform = SystemAPI.GetComponent<LocalTransform>(entity);
                float3 basePos = spawnerTransform.Position;
                quaternion rotation = spawnerTransform.Rotation;
                float3 localOffset = new float3(
                    unitSpawner.ValueRW.Random.NextFloat(-unitSpawner.ValueRO.SpawnBounds.x, unitSpawner.ValueRO.SpawnBounds.x),
                    0,
                    unitSpawner.ValueRW.Random.NextFloat(-unitSpawner.ValueRO.SpawnBounds.y, unitSpawner.ValueRO.SpawnBounds.y)
                );
                basePos += math.mul(rotation, localOffset);
                SpawnFormation(ecb, ref state, unit, basePos, 1, (UnitTypes)unitToSpawn);

                // Update spawner
                unitSpawner.ValueRW.Count -= unit.Count;
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
