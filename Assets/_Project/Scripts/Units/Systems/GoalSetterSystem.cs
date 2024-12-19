using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
public partial struct GoalSetterSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingletonEntity<Goal>(out Entity goal))
        {
            LocalTransform goalTransform = SystemAPI.GetComponent<LocalTransform>(goal);
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach ((RefRW<Movement> mov, Entity entity) in SystemAPI.Query<RefRW<Movement>>().WithNone<GoalFollow>().WithEntityAccess())
            {
                ecb.AddComponent(entity, new GoalFollow
                {
                    Target = goalTransform.Position
                });
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
