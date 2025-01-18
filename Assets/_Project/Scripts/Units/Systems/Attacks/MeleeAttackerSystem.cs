using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(LeaderPathfindingSystem))]
[UpdateAfter(typeof(FollowerPathfindingSystem))]
partial struct MeleeAttackerSystem : ISystem
{
    private ComponentLookup<TeamData> _teams;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _teams = state.GetComponentLookup<TeamData>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _teams.Update(ref state);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        MeleeAttacker job = new MeleeAttacker
        {
            ECB = ecb.AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            PhysicsWorld = physicsWorld,
            Teams = _teams
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct MeleeAttacker : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float DeltaTime;
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        [ReadOnly] public ComponentLookup<TeamData> Teams;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref AttackerData attacker, in MeleeAttackerData meleeAttacker, in TeamData team, ref LocalTransform localTransform, Entity entity)
        {
            if (attacker.Timer >= attacker.Cooldown)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
                CollisionFilter filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1 << 0
                };
                PhysicsWorld.OverlapSphere(localTransform.Position, meleeAttacker.Range, ref hits, filter);
                if (hits.Length > 1)
                {
                    foreach (DistanceHit unit in hits)
                    {
                        if (!Teams.HasComponent(unit.Entity))
                            continue;
                        int otherUnitTeam = Teams.GetRefRO(unit.Entity).ValueRO.Value;
                        if (unit.Entity != entity && team.Value != otherUnitTeam)
                        {
                            float3 dir = unit.Position - localTransform.Position;
                            dir.y = 0;
                            localTransform.Rotation = quaternion.LookRotationSafe(dir, math.up());
                            ECB.AddComponent(entityInQueryIndex, unit.Entity, new DamageInstanceData
                            {
                                Value = meleeAttacker.Damage,
                                Type = attacker.AttackType
                            });
                            attacker.Timer = 0;
                            attacker.IsAttacking = true;
                            attacker.AttackAnimTrigger = true;
                            break;
                        }
                    }
                }
                hits.Dispose();
            }
            if (attacker.Timer >= attacker.AttackDuration)
            {
                attacker.IsAttacking = false;
            }
            attacker.Timer += DeltaTime;
        }
    }
}
