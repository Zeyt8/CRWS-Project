using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSpawner>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out UnitSpawner unitSpawner))
        {
            if (unitSpawner.UnitToSpawn.HasValue)
            {
                Entity spawnerEntity = SystemAPI.GetSingletonEntity<UnitSpawner>();

                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(spawnerEntity);
                Entity unit = state.EntityManager.Instantiate(unitPrefabsBuffer[(int)unitSpawner.UnitToSpawn.Value].UnitPrefabEntity);
                float3 pos = float3.zero;
                pos.x = unitSpawner.Random.NextFloat(-unitSpawner.SpawnWidth, unitSpawner.SpawnWidth);
                pos.z = unitSpawner.Random.NextFloat(-unitSpawner.SpawnLength, unitSpawner.SpawnLength);
                pos += SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(pos));

                unitSpawner.UnitToSpawn = null;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }
}
