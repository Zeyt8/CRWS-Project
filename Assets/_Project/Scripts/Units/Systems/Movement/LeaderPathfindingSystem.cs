using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
partial struct LeaderPathfindingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRO<LocalTransform> transform, RefRW<LeaderPathfinding> pf, RefRW<MovementData> movement, RefRO<EnemyBaseReference> ebr, DynamicBuffer<PathBufferElement> pathBuffer, RefRO<TeamData> team, RefRO<AttackerData> attacker) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<LeaderPathfinding>, RefRW<MovementData>, RefRO<EnemyBaseReference>, DynamicBuffer<PathBufferElement>, RefRO<TeamData>, RefRO<AttackerData>>())
        {
            float3 targetPosition = ebr.ValueRO.Location;
            movement.ValueRW.DesiredVelocity = 0.4f;

            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << 0
            };
            SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(transform.ValueRO.Position, attacker.ValueRO.AggroRange, ref hits, filter);
            if (hits.Length > 1)
            {
                float minDistance = float.MaxValue;
                foreach (DistanceHit hit in hits)
                {
                    if (!SystemAPI.HasComponent<TeamData>(hit.Entity))
                        continue;
                    TeamData otherTeam = SystemAPI.GetComponent<TeamData>(hit.Entity);
                    if (otherTeam.Value != team.ValueRO.Value)
                    {
                        float dist = math.distance(transform.ValueRO.Position, hit.Position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            targetPosition = hit.Position;
                            movement.ValueRW.DesiredVelocity = 1f;
                        }
                    }
                }
            }

            // Recalculate path if target has changed
            if (!pf.ValueRO.Target.Equals(targetPosition))
            {
                Debug.Log("Path buffer recalculating");
                pf.ValueRW.Target = targetPosition;
                NavMeshUtility.CalculatePath(transform.ValueRO.Position, targetPosition, pathBuffer);
                pf.ValueRW.CurrentPathIndex = 1;
            }

            // Traverse path
            if (pf.ValueRW.CurrentPathIndex >= pathBuffer.Length)
            {
                pf.ValueRW.IsMoving = false;
                movement.ValueRW.IsMoving = false;
                movement.ValueRW.DesiredVelocity = 0;
                continue;
            }

            float3 movementTarget = pathBuffer[pf.ValueRO.CurrentPathIndex].Position;
            movementTarget.y = NavMeshUtility.SampleHeight(movementTarget);
            float3 direction = math.normalize(movementTarget - transform.ValueRO.Position);

            if (math.distance(transform.ValueRO.Position, movementTarget) < 0.1f)
                pf.ValueRW.CurrentPathIndex++;

            movement.ValueRW.Direction = direction;
            pf.ValueRW.IsMoving = true;
            movement.ValueRW.IsMoving = true;

            Debug.DrawLine(transform.ValueRO.Position, movementTarget, Color.cyan);
        }
    }
}
