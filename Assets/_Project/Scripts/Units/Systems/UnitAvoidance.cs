using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(UnitGoalSystem))]
partial struct UnitAvoidanceSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRO<LocalTransform> localTransform, RefRW<Movement> ms, RefRO<Avoidance> avoidance, RefRO<Team> team) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<Movement>, RefRO<Avoidance>, RefRO<Team>>())
        {
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                CollidesWith = 1 << 6,
            };
            SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.ValueRO.Position, avoidance.ValueRO.SeparationDistance, ref hits, filter);
            foreach (DistanceHit unit in hits)
            {
                LocalTransform otherUnitTransform = SystemAPI.GetComponent<LocalTransform>(unit.Entity);
                Team otherUnitTeam = SystemAPI.GetComponent<Team>(unit.Entity);
                if (team.ValueRO.Value == otherUnitTeam.Value)
                {
                    float3 awayFromFollower = localTransform.ValueRO.Position - otherUnitTransform.Position;
                    ms.ValueRW.DesiredVelocity += math.normalize(awayFromFollower) * 0.05f;
                }
            }
            hits.Dispose();
        }
    }
}
