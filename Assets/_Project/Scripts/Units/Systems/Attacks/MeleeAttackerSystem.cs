using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<MeleeAttackerData> attack, RefRO<TeamData> team, RefRO<LocalTransform> localTransform, RefRW<AttackerData> attacker, Entity entity) in
            SystemAPI.Query<RefRW<MeleeAttackerData>, RefRO<TeamData>, RefRO<LocalTransform>, RefRW<AttackerData>>().WithEntityAccess())
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
