using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct UnitAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        UnitAnimationJob job = new UnitAnimationJob()
        {
            ECB = ecb.AsParallelWriter()
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    partial struct UnitAnimationJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int entityInQueryIndex, in MovementData mov, ref AttackerData attacker, DynamicBuffer<Child> children)
        {
            foreach (Child child in children)
            {
                Entity childEntity = child.Value;

                if (mov.IsMoving)
                {
                    ECB.SetComponent(entityInQueryIndex, childEntity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.SetPlayForever,
                        ClipIndex = (byte)TwoHanded.Walk
                    });
                }
                else
                {
                    ECB.SetComponent(entityInQueryIndex, childEntity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.SetPlayForever,
                        ClipIndex = (byte)TwoHanded.Idle
                    });
                }

                // Your animation logic here
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
}
