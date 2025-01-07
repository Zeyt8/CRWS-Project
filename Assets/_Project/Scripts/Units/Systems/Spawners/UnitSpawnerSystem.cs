using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.UIElements;
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
            if (unitSpawner.UnitToSpawn.HasValue)
            {
                Entity spawnerEntity = SystemAPI.GetSingletonEntity<UnitSpawner>();

                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(spawnerEntity);
                UnitPrefabBufferElement unit = unitPrefabsBuffer[(int)unitSpawner.UnitToSpawn.Value];

                float3 basePos = SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;
                basePos.x += unitSpawner.Random.NextFloat(-unitSpawner.SpawnWidth, unitSpawner.SpawnWidth);
                basePos.z += unitSpawner.Random.NextFloat(-unitSpawner.SpawnLength, unitSpawner.SpawnLength);

                SpawnFormation(ref state, unit, basePos);

                unitSpawner.UnitToSpawn = null;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }

    [BurstCompile]
    private void SpawnFormation(ref SystemState state, UnitPrefabBufferElement unit, float3 basePos)
    {
        Entity prefabElement = unit.UnitPrefabEntity;
        uint count = unit.Count;
        int length = (int)math.ceil(math.sqrt(count / 2.0f));
        int width = length * 2;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        // Spawn leader
        Entity leader = state.EntityManager.Instantiate(prefabElement);
        if (!SystemAPI.HasBuffer<PathBufferElement>(leader))
            state.EntityManager.AddBuffer<PathBufferElement>(leader);
        ecb.AddComponent(leader, new LeaderPathfinding
        {
            CurrentPathIndex = 0,
            Target = float3.zero,
            CurrentPosition = basePos,
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
                SeparationDistances = new float3(1, 1.25f, 1.5f),
            });
            SystemAPI.SetComponent(follower, LocalTransform.FromPosition(pos));
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
