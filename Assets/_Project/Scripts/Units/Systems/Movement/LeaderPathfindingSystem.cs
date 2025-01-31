using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using Pathfinding.Aspects;
using Pathfinding.Components;
using UnityEngine.Experimental.AI;

[UpdateInGroup(typeof(MovementSystemGroup))]
partial struct LeaderPathfindingSystem : ISystem
{
    private ComponentLookup<TeamData> _teams;
    private BufferLookup<PathBuffer> _pathBuffers;
    private PathfinderAspect.Lookup _pathfinderLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _teams = state.GetComponentLookup<TeamData>(true);
        _pathBuffers = state.GetBufferLookup<PathBuffer>(false);
        _pathfinderLookup = new PathfinderAspect.Lookup(ref state);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out Level level))
        {
            if (!level.HasStarted)
                return;
        }
        _teams.Update(ref state);
        _pathBuffers.Update(ref state);
        _pathfinderLookup.Update(ref state);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new LeaderPathfindingJob
        {
            PhysicsWorld = physicsWorld,
            Teams = _teams,
            PathBuffers = _pathBuffers,
            PathfinderLookup = _pathfinderLookup
        };

        state.Dependency = job.Schedule(state.Dependency);
        NavMeshWorld.GetDefaultWorld().AddDependency(state.Dependency);
        state.Dependency.Complete();
    }

    [BurstCompile]
    private partial struct LeaderPathfindingJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        [ReadOnly] public ComponentLookup<TeamData> Teams;
        [ReadOnly] public BufferLookup<PathBuffer> PathBuffers;
        public PathfinderAspect.Lookup PathfinderLookup;

        public void Execute(in LocalTransform transform, ref LeaderPathfinding pf, ref MovementData movement, in EnemyBaseReference ebr, in TeamData team, in AttackerData attacker, Entity entity)
        {
            float3 targetPosition = ebr.Location;
            movement.DesiredVelocity = 0.4f;

            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << 0
            };
            PhysicsWorld.OverlapSphere(transform.Position, attacker.AggroRange, ref hits, filter);
            if (hits.Length > 1)
            {
                float minDistance = float.MaxValue;
                foreach (DistanceHit hit in hits)
                {
                    if (!Teams.HasComponent(hit.Entity))
                        continue;
                    TeamData otherTeam = Teams.GetRefRO(hit.Entity).ValueRO;
                    if (otherTeam.Value != team.Value)
                    {
                        float dist = math.distance(transform.Position, hit.Position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            targetPosition = hit.Position;
                            movement.DesiredVelocity = 1f;
                        }
                    }
                }
            }

            // Recalculate path if target has changed
            if (!pf.Target.Equals(targetPosition))
            {
                var pathfinderAspect = PathfinderLookup[entity];
                pathfinderAspect.FindPath(transform.Position, targetPosition);
                pf.CurrentPathIndex = 1;
            }

            var pathBuffer = PathBuffers[entity];

            // Traverse path
            if (pf.CurrentPathIndex >= pathBuffer.Length)
            {
                pf.IsMoving = false;
                movement.IsMoving = false;
                movement.DesiredVelocity = 0;
                return;
            }

            float3 movementTarget = pathBuffer[pf.CurrentPathIndex].position;
            //movementTarget.y = NavMeshUtility.SampleHeight(movementTarget);
            movementTarget.y = transform.Position.y;
            float3 direction = math.normalize(movementTarget - transform.Position);

            if (math.distance(transform.Position, movementTarget) < 0.1f)
                pf.CurrentPathIndex++;

            movement.Direction = direction;
            pf.IsMoving = true;
            movement.IsMoving = true;

            Debug.DrawLine(transform.Position, movementTarget, Color.cyan);
        }
    }
}
