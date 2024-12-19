////using Unity.Burst;
////using Unity.Entities;
////using Unity.Transforms;
////using Unity.Mathematics;

////partial struct UnitSpawnerSystem : ISystem
////{
////    [BurstCompile]
////    public void OnCreate(ref SystemState state)
////    {
////        state.RequireForUpdate<UnitSpawner>();
////    }

////    [BurstCompile]
////    public void OnUpdate(ref SystemState state)
////    {
////        if (SystemAPI.TryGetSingleton(out UnitSpawner unitSpawner))
////        {
////            if (unitSpawner.UnitToSpawn.HasValue)
////            {
////                Entity unit = state.EntityManager.Instantiate(unitSpawner.UnitPrefabEntity);
////                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(new float3(0, 0, 0)));
////                Movement ms = SystemAPI.GetComponent<Movement>(unit);
////                ms.Target = new float3(100, 0, 0);
////                SystemAPI.SetComponent(unit, ms);
////                unitSpawner.UnitToSpawn = null;
////                SystemAPI.SetSingleton(unitSpawner);
////            }
////        }
////    }
////}

//using Unity.Burst;
//using Unity.Entities;
//using Unity.Transforms;
//using Unity.Mathematics;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using Unity.Collections;

//partial struct UnitSpawnerSystem : ISystem
//{
//    [BurstCompile]
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<UnitSpawner>();
//    }

//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        if (SystemAPI.TryGetSingleton(out UnitSpawner unitSpawner))
//        {
//            if (unitSpawner.UnitToSpawn.HasValue)
//            {
//                // Spawn the unit
//                Entity unit = state.EntityManager.Instantiate(unitSpawner.UnitPrefabEntity);
//                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(new float3(0, 1.5f, 0)));

//                // Get the movement component
//                Movement ms = SystemAPI.GetComponent<Movement>(unit);

//                // Fetch a random position within a specified collider
//                if (unitSpawner.TargetColliderEntity != Entity.Null)
//                {
//                    float3 randomPoint = GetRandomPointInCollider(unitSpawner.TargetColliderEntity, ref state);
//                    ms.Target = randomPoint;
//                }
//                else
//                {
//                    ms.Target = new float3(100, 0, 0); // Default target if no collider is specified
//                }

//                SystemAPI.SetComponent(unit, ms);

//                // Reset the spawner state
//                unitSpawner.UnitToSpawn = null;
//                SystemAPI.SetSingleton(unitSpawner);
//            }
//        }
//    }

//    private float3 GetRandomPointInCollider(Entity colliderEntity, ref SystemState state)
//    {
//        var entityManager = state.EntityManager;

//        // Ensure the collider entity has a PhysicsCollider component
//        if (entityManager.HasComponent<PhysicsCollider>(colliderEntity))
//        {
//            PhysicsCollider physicsCollider = entityManager.GetComponentData<PhysicsCollider>(colliderEntity);
//            var collider = physicsCollider.Value;

//            // Get the bounds of the collider
//            Aabb aabb = collider.Value.CalculateAabb();

//            // Generate a random point within the bounds of the collider
//            Random random = new Random((uint)UnityEngine.Random.Range(1, 100000));
//            float3 randomPoint;

//            for (int i = 0; i < 10; i++) // Attempt up to 10 times to get a valid point
//            {
//                randomPoint = new float3(
//                    random.NextFloat(aabb.Min.x, aabb.Max.x),
//                    0f,
//                    random.NextFloat(aabb.Min.z, aabb.Max.z)
//                );

//                // Check if the point is within the collider
//                var pointDistanceInput = new PointDistanceInput
//                {
//                    Position = randomPoint,
//                    MaxDistance = 0.01f, // Small threshold for precision
//                    Filter = CollisionFilter.Default
//                };

//                if (collider.Value.CalculateDistance(pointDistanceInput, out _))
//                {
//                    return randomPoint; // Valid point found
//                }
//            }
//        }

//        return float3.zero; // Fallback if no valid point is found
//    }
//}
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSpawner>();
        state.RequireForUpdate<GoalColliderSingleton>(); // Ensure the GoalColliderSingleton exists
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out UnitSpawner unitSpawner))
        {
            if (unitSpawner.UnitToSpawn.HasValue)
            {
                // Spawn the unit
                Entity unit = state.EntityManager.Instantiate(unitSpawner.UnitPrefabEntity);
                SystemAPI.SetComponent(unit, LocalTransform.FromPosition(new float3(0, 1.5f, 0)));

                // Get the movement component
                Movement ms = SystemAPI.GetComponent<Movement>(unit);

                // Access the GoalColliderSingleton
                if (SystemAPI.TryGetSingleton(out GoalColliderSingleton goalCollider))
                {
                    unitSpawner.TargetColliderEntity = goalCollider.GoalColliderEntity;

                    // Fetch a random position within the specified collider
                    if (unitSpawner.TargetColliderEntity != Entity.Null)
                    {
                        float3 randomPoint = GetRandomPointInCollider(unitSpawner.TargetColliderEntity, ref state);
                        ms.Target = randomPoint;
                    }
                }
                else
                {
                    ms.Target = new float3(100, 0, 0); // Default target if no collider is specified
                }

                SystemAPI.SetComponent(unit, ms);

                // Reset the spawner state
                unitSpawner.UnitToSpawn = null;
                SystemAPI.SetSingleton(unitSpawner);
            }
        }
    }

    private float3 GetRandomPointInCollider(Entity colliderEntity, ref SystemState state)
    {
        var entityManager = state.EntityManager;

        if (entityManager.HasComponent<PhysicsCollider>(colliderEntity))
        {
            PhysicsCollider physicsCollider = entityManager.GetComponentData<PhysicsCollider>(colliderEntity);
            var collider = physicsCollider.Value;

            Aabb aabb = collider.Value.CalculateAabb();
            Random random = new Random((uint)UnityEngine.Random.Range(1, 100000));
            float3 randomPoint;

            for (int i = 0; i < 10; i++)
            {
                randomPoint = new float3(
                    random.NextFloat(aabb.Min.x, aabb.Max.x),
                    0f,
                    random.NextFloat(aabb.Min.z, aabb.Max.z)
                );

                var pointDistanceInput = new PointDistanceInput
                {
                    Position = randomPoint,
                    MaxDistance = 0.01f,
                    Filter = CollisionFilter.Default
                };

                if (collider.Value.CalculateDistance(pointDistanceInput, out _))
                {
                    return randomPoint;
                }
            }
        }

        return float3.zero;
    }
}
