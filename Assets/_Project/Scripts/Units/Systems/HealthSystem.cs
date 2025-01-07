using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct HealthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        foreach ((RefRO<HealthData> health, Entity entity) in SystemAPI.Query<RefRO<HealthData>>().WithEntityAccess())
        {
            if (health.ValueRO.Value <= 0)
            {
                ecb.DestroyEntity(entity);
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();*/
    }
}
