using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct FollowerPathfindingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRW<LocalTransform> transform, RefRO<FollowerPathfinding> pf, RefRW<MovementData> movement, RefRO<TeamData> team, Entity entity) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRO<FollowerPathfinding>, RefRW<MovementData>, RefRO<TeamData>>().WithEntityAccess())
        {
            float3 leaderPosition = SystemAPI.GetComponent<LocalTransform>(pf.ValueRO.Leader).Position;
            float3 targetPosition = leaderPosition + pf.ValueRO.FormationOffset;

            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1 << 0,
                CollidesWith = 1 << 0
            };

            movement.ValueRW.DesiredVelocity = 1;

            // If units kinda close, move slower
            /*SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(transform.ValueRO.Position, pf.ValueRO.SeparationDistances.z, ref hits, filter);
            if (hits.Length > 1)
            {
                movement.ValueRW.DesiredVelocity = 0.25f;
            }*/

            // If even closer, just stop
            /*SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(transform.ValueRO.Position, pf.ValueRO.SeparationDistances.y, ref hits, filter);
            if (hits.Length > 1)
            {
                movement.ValueRW.DesiredVelocity = 0;
            }*/

            //SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(transform.ValueRO.Position, pf.ValueRO.SeparationDistances.x, ref hits, filter);
            
            // TODO: Boids
            
            if (math.distance(transform.ValueRO.Position, targetPosition) < 0.01f)
            {
                movement.ValueRW.IsMoving = false;
                movement.ValueRW.DesiredVelocity = 0;
                continue;
            }
            movement.ValueRW.Direction = math.normalize(targetPosition - transform.ValueRO.Position);
            movement.ValueRW.IsMoving = true;

            //hits.Dispose();
        }
    }
}
