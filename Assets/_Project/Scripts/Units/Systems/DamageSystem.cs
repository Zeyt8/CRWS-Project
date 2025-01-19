using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(DamageSystemGroup))]
partial struct DamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        DamageJob job = new DamageJob
        {
            ECB = ecb.AsParallelWriter(),
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct DamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex, in DamageInstanceData damage, ref HealthData health, in ArmourData armour, Entity entity)
        {
            switch (damage.Type)
            {
                case DamageType.Slash:
                    health.Value -= damage.Value * (1 - armour.SlashResistance);
                    break;
                case DamageType.Pierce:
                    health.Value -= damage.Value * (1 - armour.PierceResistance);
                    break;
                case DamageType.Blunt:
                    health.Value -= damage.Value * (1 - armour.BluntResistance);
                    break;
                case DamageType.Magic:
                    health.Value -= damage.Value * (1 - armour.MagicResistance);
                    break;
            }
            ECB.RemoveComponent<DamageInstanceData>(entityInQueryIndex, entity);
        }
    }
}
