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
public partial struct GoalSetterSystem : ISystem
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
            goalPositions.Add(transform.ValueRO.Position);
            goalIndices.Add(index.ValueRO.Index);

        }
        

        // Query entities with Movement and Team, but without GoalFollow
        foreach ((RefRW<Movement> mov, RefRO<Team> team, Entity entity)
                 in SystemAPI.Query<RefRW<Movement>, RefRO<Team>>().WithNone<GoalFollow>().WithEntityAccess())
        {
            // Determine the target goal based on the team value
            float3 targetPosition = float3.zero;
            bool goalFound = false;
            
            for (int i = 0; i < goalIndices.Length; i++)
            {
                if (goalIndices[i] == team.ValueRO.Value)
                {
                    targetPosition = goalPositions[i];
                    goalFound = true;
                    break;
                }
            }

            if (goalFound)
            {
                // Add GoalFollow component with the selected target
                ecb.AddComponent(entity, new GoalFollow
                {
                    Target = targetPosition
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

