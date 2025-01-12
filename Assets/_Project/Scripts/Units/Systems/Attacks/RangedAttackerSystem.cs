using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct RangedAttackerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<RangedAttackerData> attack, RefRO<TeamData> team, RefRO<LocalTransform> localTransform, RefRW<AttackerData> attacker, Entity entity) in
            SystemAPI.Query<RefRW<RangedAttackerData>, RefRO<TeamData>, RefRO<LocalTransform>, RefRW<AttackerData>>().WithEntityAccess())
        {
            if (attack.ValueRO.Timer >= attack.ValueRO.Cooldown)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
                CollisionFilter filter = new CollisionFilter()
                {
                    BelongsTo = 1 << 3,
                    CollidesWith = 1 << 0
                };
                SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.ValueRO.Position, attack.ValueRO.Range, ref hits, filter);
                if (hits.Length > 1)
                {
                    foreach (DistanceHit unit in hits)
                    {
                        int otherUnitTeam = SystemAPI.GetComponent<TeamData>(unit.Entity).Value;
                        if (unit.Entity != entity && team.ValueRO.Value != otherUnitTeam)
                        {
                            Entity projectile = state.EntityManager.Instantiate(attack.ValueRO.Projectile);
                            float3 forwardPosition = localTransform.ValueRO.Position + localTransform.ValueRO.Forward() + math.up() * 1.5f;
                            quaternion rotation = quaternion.LookRotationSafe(localTransform.ValueRO.Forward(), math.up());
                            state.EntityManager.SetComponentData(projectile, LocalTransform.FromPositionRotation(forwardPosition, rotation));
                            attack.ValueRW.Timer -= attack.ValueRO.Cooldown;
                            attacker.ValueRW.IsAttacking = true;
                        }
                    }
                }
                hits.Dispose();
            }
            else if (attack.ValueRO.Timer >= 0.5f)
            {
                attacker.ValueRW.IsAttacking = false;
            }
            attack.ValueRW.Timer += SystemAPI.Time.DeltaTime;
        }
    }
}
