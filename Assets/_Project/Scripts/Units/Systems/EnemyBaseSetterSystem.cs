//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Transforms;

//[UpdateInGroup(typeof(MovementSystemGroup))]
//public partial struct GoalSetterSystem : ISystem
//{
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        if (SystemAPI.TryGetSingletonEntity<Goal>(out Entity goal))
//        {
//            LocalTransform goalTransform = SystemAPI.GetComponent<LocalTransform>(goal);
//            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
//            foreach ((RefRW<Movement> mov, Entity entity) in SystemAPI.Query<RefRW<Movement>>().WithNone<GoalFollow>().WithEntityAccess())
//            {
//                ecb.AddComponent(entity, new GoalFollow
//                {
//                    Target = goalTransform.Position
//                });
//            }
//            ecb.Playback(state.EntityManager);
//            ecb.Dispose();
//        }
//    }
//}
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
public partial struct EnemyBaseSetterSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Create an EntityCommandBuffer outside the loops
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        // Collect all goals and their positions
        NativeList<float3> goalPositions = new NativeList<float3>(Allocator.Temp);
        NativeList<int> goalIndices = new NativeList<int>(Allocator.Temp);

        foreach ((RefRO<Goal> index, RefRO<LocalTransform> transform) in SystemAPI.Query<RefRO<Goal>, RefRO<LocalTransform>>())
        {
            LocalTransform goalTransform = SystemAPI.GetComponent<LocalTransform>(goal);
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach ((RefRW<Movement> mov, Entity entity) in SystemAPI.Query<RefRW<Movement>>().WithNone<EnemyBaseReference>().WithEntityAccess())
            {
                ecb.AddComponent(entity, new EnemyBaseReference
                {
                    Location = goalTransform.Position
                });
            }
           
        }

        // Playback and dispose the EntityCommandBuffer
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        // Dispose of the NativeLists
        goalPositions.Dispose();
        goalIndices.Dispose();
    }
}

