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
            foreach ((RefRO<TeamData> team, RefRO<LocalTransform> entityTransform, Entity entity) in SystemAPI.Query<RefRO<TeamData>, RefRO<LocalTransform>>().WithNone<EnemyBaseReference>().WithEntityAccess())
            {
                if (index.ValueRO.Index == team.ValueRO.Value)
                {
                    ecb.AddComponent(entity, new EnemyBaseReference
                    {
                        Location = new Unity.Mathematics.float3(entityTransform.ValueRO.Position.x, transform.ValueRO.Position.y, transform.ValueRO.Position.z)
                        
                    });
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

