using Latios.Mimic.Addons.Mecanim;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[BurstCompile]
partial struct UnitAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new UnitAnimationJob();
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    partial struct UnitAnimationJob : IJobEntity
    {
        public void Execute(MecanimAspect anim, in MovementData mov)
        {
            anim.SetBool("IsMoving", mov.IsMoving);
            anim.SetFloat("Velocity", mov.CurrentVelocity);
        }
    }
}
