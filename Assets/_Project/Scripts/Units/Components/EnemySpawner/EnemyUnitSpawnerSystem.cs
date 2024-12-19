using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyUnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSpawner>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out EnemyUnitSpawner unitSpawner))
        {
            if(unitSpawner.Count <= 1000)
            {
                Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemyUnitSpawner>();

                DynamicBuffer<UnitPrefabBufferElement> unitPrefabsBuffer = state.EntityManager.GetBuffer<UnitPrefabBufferElement>(spawnerEntity);
                Entity unit = state.EntityManager.Instantiate(unitPrefabsBuffer[(int)unitSpawner.Random.NextFloat(0f, 12f)].UnitPrefabEntity);
                var teamValue = SystemAPI.GetComponent<Team>(unit);
                teamValue.Value = 1;
                SystemAPI.SetComponent<Team>(unit, teamValue);
                float3 pos = float3.zero;
                pos.x = unitSpawner.Random.NextFloat(-unitSpawner.SpawnWidth, unitSpawner.SpawnWidth);
                pos.z = unitSpawner.Random.NextFloat(-unitSpawner.SpawnLength, unitSpawner.SpawnLength);
                pos += SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(pos));
                unitSpawner.Count++;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }
}
