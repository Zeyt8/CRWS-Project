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
        // spawn here when UI button pressed
        // How? I dunno
        if (SystemAPI.TryGetSingleton<UnitSpawner>(out UnitSpawner unitSpawner))
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space))
            {
                Entity unit = state.EntityManager.Instantiate(unitSpawner.UnitPrefabEntity);
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(new float3(0, 0, 0)));
            }
        }
    }
}
