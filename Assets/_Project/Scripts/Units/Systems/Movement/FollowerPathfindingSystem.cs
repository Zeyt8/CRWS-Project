using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(LeaderPathfindingSystem))]
partial struct FollowerPathfindingSystem : ISystem
{
    private ComponentLookup<LeaderPathfinding> _leaders;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _leaders = state.GetComponentLookup<LeaderPathfinding>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _leaders.Update(ref state);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new FollowerPathfindingJob
        {
            PhysicsWorld = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            Leaders = _leaders
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    private partial struct FollowerPathfindingJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LeaderPathfinding> Leaders;

        public void Execute(ref LocalTransform transform, in FollowerPathfinding pf, ref MovementData movement, in TeamData team, Entity entity)
        {
            float3 leaderPosition = Leaders.GetRefRO(pf.Leader).ValueRO.CurrentPosition;
            float3 targetPosition = leaderPosition + pf.FormationOffset;

            /*NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1 << 0,
                CollidesWith = 1 << 0
            };*/

            // If units kinda close, move slower
            /*PhysicsWorld.OverlapSphere(transform.Position, pf.SeparationDistances.z, ref hits, filter);
            if (hits.Length > 1)
            {
                movement.DesiredVelocity = 0.25f;
            }*/

            // If even closer, just stop
            /*PhysicsWorld.OverlapSphere(transform.Position, pf.SeparationDistances.y, ref hits, filter);
            if (hits.Length > 1)
            {
                movement.DesiredVelocity = 0;
            }*/

            //PhysicsWorld.OverlapSphere(transform.Position, pf.SeparationDistances.x, ref hits, filter);

            // TODO: Boids

            if (math.distance(transform.Position, targetPosition) < 0.01f)
            {
                movement.IsMoving = false;
                movement.DesiredVelocity = 0;
                return;
            }
            movement.DesiredVelocity = 1;
            movement.Direction = math.normalize(targetPosition - transform.Position);
            movement.IsMoving = true;

            //hits.Dispose();
        }
    }
}

