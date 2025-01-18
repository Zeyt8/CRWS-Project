using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(DamageSystemGroup))]
[UpdateAfter(typeof(DamageSystem))]
partial struct HealthSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        HealthJob job = new HealthJob
        {
            ECB = ecb.AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime
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
        public float DeltaTime;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref HealthData health, Entity entity)
        {
            if (health.Value <= 0)
            {
                ECB.DestroyEntity(entityInQueryIndex, entity);
            }
            health.Value += health.Regen * DeltaTime;
        }
    }
}
