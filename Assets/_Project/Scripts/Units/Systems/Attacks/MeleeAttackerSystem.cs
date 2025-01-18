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
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<AttackerData> attacker, RefRO<MeleeAttackerData> meleeAttacker, RefRO<TeamData> team, RefRW<LocalTransform> localTransform, Entity entity) in
            SystemAPI.Query<RefRW<AttackerData>, RefRO<MeleeAttackerData>, RefRO<TeamData>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (attacker.ValueRO.Timer >= attacker.ValueRO.Cooldown)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
                CollisionFilter filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1 << 0
                };
                SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.ValueRO.Position, meleeAttacker.ValueRO.Range, ref hits, filter);
                if (hits.Length > 1)
                {
                    foreach (DistanceHit unit in hits)
                    {
                        if (!SystemAPI.HasComponent<TeamData>(unit.Entity))
                            continue;
                        int otherUnitTeam = SystemAPI.GetComponent<TeamData>(unit.Entity).Value;
                        if (unit.Entity != entity && team.ValueRO.Value != otherUnitTeam)
                        {
                            float3 dir = unit.Position - localTransform.ValueRO.Position;
                            dir.y = 0;
                            localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(dir, math.up());
                            state.EntityManager.AddComponentData(unit.Entity, new DamageInstanceData
                            {
                                Value = meleeAttacker.ValueRO.Damage,
                                Type = attacker.ValueRO.AttackType
                            });
                            attacker.ValueRW.Timer = 0;
                            attacker.ValueRW.IsAttacking = true;
                            attacker.ValueRW.AttackAnimTrigger = true;
                            break;
                        }
                    }
                }
                hits.Dispose();
            }
            if (attacker.ValueRO.Timer >= attacker.ValueRO.AttackDuration)
            {
                attacker.ValueRW.IsAttacking = false;
            }
            attacker.ValueRW.Timer += SystemAPI.Time.DeltaTime;
        }
    }
}
