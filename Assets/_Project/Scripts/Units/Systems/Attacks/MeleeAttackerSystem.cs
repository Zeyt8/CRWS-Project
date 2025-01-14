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
        foreach ((RefRW<AttackerData> attacker, RefRO<MeleeAttackerData> meleeAttacker, RefRO<TeamData> team, RefRO<LocalTransform> localTransform, Entity entity) in
            SystemAPI.Query<RefRW<AttackerData>, RefRO<MeleeAttackerData>, RefRO<TeamData>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            if (attacker.ValueRO.Timer >= attacker.ValueRO.Cooldown)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
                CollisionFilter filter = new CollisionFilter()
                {
                    BelongsTo = 1 << 3,
                    CollidesWith = 1 << 0
                };
                SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.ValueRO.Position, meleeAttacker.ValueRO.Range, ref hits, filter);
                if (hits.Length > 1)
                {
                    foreach (DistanceHit unit in hits)
                    {
                        int otherUnitTeam = SystemAPI.GetComponent<TeamData>(unit.Entity).Value;
                        if (unit.Entity != entity && team.ValueRO.Value != otherUnitTeam)
                        {
                            HealthData otherUnitHealth = SystemAPI.GetComponent<HealthData>(unit.Entity);
                            otherUnitHealth.Value -= meleeAttacker.ValueRO.Damage;
                            SystemAPI.SetComponent(unit.Entity, otherUnitHealth);
                            attacker.ValueRW.Timer -= attacker.ValueRO.Cooldown;
                            attacker.ValueRW.IsAttacking = true;
                            attacker.ValueRW.AttackAnimTrigger = true;
                            break;
                        }
                    }
                }
                hits.Dispose();
            }
            else if (attacker.ValueRO.Timer >= attacker.ValueRO.AttackDuration)
            {
                attacker.ValueRW.IsAttacking = false;
            }
            attacker.ValueRW.Timer += SystemAPI.Time.DeltaTime;
        }
    }
}
