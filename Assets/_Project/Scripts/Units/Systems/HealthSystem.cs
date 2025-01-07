using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct HealthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        HealthJob job = new HealthJob
        {
            ECB = ecb.AsParallelWriter()
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct HealthJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex, in HealthData health, Entity entity)
        {
            if (health.Value <= 0)
            {
                ECB.DestroyEntity(entityInQueryIndex, entity);
            }
        }
    }
}
