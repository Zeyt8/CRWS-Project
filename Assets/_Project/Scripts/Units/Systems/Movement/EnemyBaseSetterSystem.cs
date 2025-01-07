using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
public partial struct EnemyBaseSetterSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<Goal> index, RefRO<LocalTransform> transform) in SystemAPI.Query<RefRO<Goal>, RefRO<LocalTransform>>())
        {
            foreach ((RefRO<LeaderPathfinding> pf, RefRO<TeamData> team, Entity entity) in SystemAPI.Query<RefRO<LeaderPathfinding>, RefRO<TeamData>>().WithNone<EnemyBaseReference>().WithEntityAccess())
            {
                if (index.ValueRO.Index == team.ValueRO.Value)
                {
                    ecb.AddComponent(entity, new EnemyBaseReference
                    {
                        Location = transform.ValueRO.Position
                    });
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

