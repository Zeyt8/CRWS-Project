using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

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
                Entity prefabElement = unitPrefabsBuffer[(int)unitSpawner.UnitToSpawn.Value].UnitPrefabEntity;
                uint count = unitPrefabsBuffer[(int)unitSpawner.UnitToSpawn.Value].Count;
                int length = (int)math.ceil(math.sqrt(count / 2.0f));
                int width = length * 2;

                float3 basePos = float3.zero;
                basePos.x = unitSpawner.Random.NextFloat(-unitSpawner.SpawnWidth, unitSpawner.SpawnWidth);
                basePos.z = unitSpawner.Random.NextFloat(-unitSpawner.SpawnLength, unitSpawner.SpawnLength);
                basePos += SystemAPI.GetComponent<LocalTransform>(spawnerEntity).Position;

                for (int i = 0; i < count; i++)
                {
                    Entity unit = state.EntityManager.Instantiate(prefabElement);
                    float3 pos = basePos;
                    pos.x += (i % width) * DISTANCE_IN_FORMATION - (width - 1) * DISTANCE_IN_FORMATION / 2;
                    pos.z += (i / width) * DISTANCE_IN_FORMATION - (length - 1) * DISTANCE_IN_FORMATION / 2;
                    SystemAPI.SetComponent(unit, LocalTransform.FromPosition(pos));
                }

                float3 targetPosition = new float3(130, 0, -50);

                if (!SystemAPI.HasBuffer<PathBufferElement>(unit))
                    state.EntityManager.AddBuffer<PathBufferElement>(unit);

                var pathBuffer = SystemAPI.GetBuffer<PathBufferElement>(unit);
                NavMeshUtility.CalculatePath(spawnPosition, targetPosition, pathBuffer);

                Movement ms = SystemAPI.GetComponent<Movement>(unit);
                ms.Target = targetPosition;
                ms.CurrentPathIndex = 0; 
                SystemAPI.SetComponent(unit, ms);

                unitSpawner.UnitToSpawn = null;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }
}
