using Latios.Mimic.Addons.Mecanim;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
partial struct UnitAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitAnimationJob job = new UnitAnimationJob();
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    partial struct UnitAnimationJob : IJobEntity
    {
        public void Execute(MecanimAspect anim, in MovementData mov, ref AttackerData attacker)
        {
            anim.SetBool(0, mov.IsMoving);
            anim.SetFloat(2, mov.CurrentVelocity);
            if (attacker.AttackAnimTrigger)
            {
                anim.SetTrigger(3);
                attacker.AttackAnimTrigger = false;
            }
        }
    }
}
