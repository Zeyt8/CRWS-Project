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
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(new float3(0, 0, 0)));
                Movement ms = SystemAPI.GetComponent<Movement>(unit);
                ms.Target = new float3(100, 0, 0);
                SystemAPI.SetComponent(unit, ms);
                unitSpawner.UnitToSpawn = null;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }
}
