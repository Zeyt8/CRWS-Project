using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct UnitAnimationSystem : ISystem
{
    private ComponentLookup<AnimationStateData> _animationStateLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _animationStateLookup = state.GetComponentLookup<AnimationStateData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        _animationStateLookup.Update(ref state);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        UnitAnimationJob job = new UnitAnimationJob()
        {
            AnimationStateLookup = _animationStateLookup,
            ECB = ecb.AsParallelWriter()
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    partial struct UnitAnimationJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<AnimationStateData> AnimationStateLookup;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex, in MovementData mov, ref AttackerData attacker, DynamicBuffer<Child> children)
        {
            foreach (Child child in children)
            {
                Entity childEntity = child.Value;
                if (!AnimationStateLookup.HasComponent(childEntity))
                {
                    continue;
                }
                AnimationStateData animState = AnimationStateLookup.GetRefRO(childEntity).ValueRO;

                AnimationCmdData cmdData = new AnimationCmdData();
                cmdData.Speed = 1;
                if (attacker.AttackAnimTrigger)
                {
                    cmdData.Cmd = AnimationCmd.PlayOnce;
                    cmdData.ClipIndex = (byte)TwoHanded.Attack1;
                    attacker.AttackAnimTrigger = false;
                }
                else if (mov.IsMoving)
                {
                    cmdData.Cmd = AnimationCmd.SetPlayForever;
                    if (mov.CurrentVelocity > 0.66f)
                    {
                        cmdData.ClipIndex = (byte)TwoHanded.Run;
                    }
                    else if (mov.CurrentVelocity > 0.33f)
                    {
                        cmdData.ClipIndex = (byte)TwoHanded.Walk;
                    }
                    else
                    {
                        cmdData.ClipIndex = (byte)TwoHanded.WalkSlow;
                    }
                }
                else
                {
                    cmdData.Cmd = AnimationCmd.SetPlayForever;
                    cmdData.ClipIndex = (byte)TwoHanded.Idle;
                }
                if (cmdData.ClipIndex != animState.CurrentClipIndex)
                {
                    ECB.SetComponent(entityInQueryIndex, childEntity, cmdData);
                }
            }
        }
    }
}
