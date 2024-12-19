using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(GoalSetterSystem))]
public partial struct UnitGoalSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Goal>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out Goal goal))
        {
            foreach ((RefRO<GoalFollow> Follower, RefRW<Movement> mov, RefRO<LocalTransform> transform)
                in SystemAPI.Query<RefRO<GoalFollow>, RefRW<Movement>, RefRO<LocalTransform>>())
            {
                mov.ValueRW.DesiredVelocity += math.normalize(Follower.ValueRO.Target - transform.ValueRO.Position);
            }
        }
    }
}