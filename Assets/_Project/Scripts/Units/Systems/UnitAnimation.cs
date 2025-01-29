using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct UnitAnimationSystem : ISystem
{
    void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimDbRefData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitAnimationJob job = new UnitAnimationJob()
        {
            Db = SystemAPI.GetSingleton<AnimDbRefData>()
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    partial struct UnitAnimationJob : IJobEntity
    {
        [ReadOnly] public AnimDbRefData Db;

        [BurstCompile]
        public void Execute(ref AnimationCmdData cmd, in MovementData mov, ref AttackerData attacker, in AnimationStateData state)
        {
            /*anim.SetBool(0, mov.IsMoving);
                anim.SetFloat(2, mov.CurrentVelocity);
                if (attacker.AttackAnimTrigger)
                {
                    anim.SetTrigger(3);
                    attacker.AttackAnimTrigger = false;
                }*/
        }

    }
}
