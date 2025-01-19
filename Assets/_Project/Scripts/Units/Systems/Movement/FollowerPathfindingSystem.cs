using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(LeaderPathfindingSystem))]
partial struct FollowerPathfindingSystem : ISystem
{
    private ComponentLookup<LocalTransform> _transforms;
    private ComponentLookup<LeaderPathfinding> _leaders;
    private ComponentLookup<TeamData> _teams;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _transforms = state.GetComponentLookup<LocalTransform>(true);
        _leaders = state.GetComponentLookup<LeaderPathfinding>(true);
        _teams = state.GetComponentLookup<TeamData>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _transforms.Update(ref state);
        _leaders.Update(ref state);
        _teams.Update(ref state);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new FollowerPathfindingJob
        {
            PhysicsWorld = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            Transforms = _transforms,
            Leaders = _leaders,
            Teams = _teams
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
    }

    [BurstCompile]
    private partial struct FollowerPathfindingJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> Transforms;
        [ReadOnly] public ComponentLookup<LeaderPathfinding> Leaders;
        [ReadOnly] public ComponentLookup<TeamData> Teams;
        private const float AvoidanceDistance = 5f; //lenght of ray cast (both thick and others)
        private const float BoundsRadius = 3f; //how thick the first one is, (might need to increase)
        private const float TargetWeight = 1f;
        private const float UnitAvoidanceWeight = 5f;
        private const float UnitAlignmentWeight = 1f;
        private const float TerrainAvoidanceWeight = 15f;
        private const float oppositeAvoidForce = 10f;

        public void Execute(in LocalTransform selfTransform, in FollowerPathfinding pf, ref MovementData movement, in TeamData team, in EnemyBaseReference ebr, Entity entity)
        {
            float3 targetPosition;
            LeaderPathfinding? leader = null;
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << 0
            };
            PhysicsWorld.OverlapSphere(selfTransform.Position, pf.ViewRadius, ref hits, filter);
            if (Leaders.HasComponent(pf.Leader))
            {
                leader = Leaders.GetRefRO(pf.Leader).ValueRO;
                float3 leaderPosition = Transforms.GetRefRO(pf.Leader).ValueRO.Position;
                quaternion leaderRotation = Transforms.GetRefRO(pf.Leader).ValueRO.Rotation;
                float3 rotatedFormationOffset = math.rotate(leaderRotation, pf.FormationOffset);
                targetPosition = leaderPosition + rotatedFormationOffset;
            }
            else
            {
                targetPosition = ebr.Location;
            }
            bool chargingEnemy = false;
            if (hits.Length > 1)
            {
                float minDistance = float.MaxValue;
                foreach (DistanceHit hit in hits)
                {
                    if (!Teams.HasComponent(hit.Entity))
                        continue;
                    TeamData otherTeam = Teams.GetRefRO(hit.Entity).ValueRO;
                    if (team.Value != otherTeam.Value)
                    {
                        float dist = math.distance(selfTransform.Position, hit.Position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            targetPosition = hit.Position;
                            movement.DesiredVelocity = 1f;
                            chargingEnemy = true;
                        }
                    }
                }
            }

            float3 dir = targetPosition - selfTransform.Position;
            if (math.length(dir) < 0.01f)
            {
                if ((leader.HasValue && !leader.Value.IsMoving) || chargingEnemy)
                {
                    movement.IsMoving = false;
                    movement.DesiredVelocity = 0;
                }
                return;
            }
            else
            {
                dir = math.normalize(dir) * TargetWeight;
            }

            if (hits.Length > 1)
            {
                float3 alignmentDir = float3.zero;
                float3 separationDir = float3.zero;

                foreach (DistanceHit hit in hits)
                {
                    if (!Teams.HasComponent(hit.Entity))
                        continue;
                    TeamData otherTeam = Teams.GetRefRO(hit.Entity).ValueRO;
                    if (hit.Entity != entity && team.Value == otherTeam.Value)
                    {
                        LocalTransform otherTransform = Transforms.GetRefRO(hit.Entity).ValueRO;
                        float3 offset = otherTransform.Position - selfTransform.Position;
                        float sqrDst = math.lengthsq(offset);

                        alignmentDir += otherTransform.Forward();

                        if (sqrDst < pf.AvoidanceRadius * pf.AvoidanceRadius)
                        {
                            separationDir -= offset / sqrDst;
                        }
                    }
                }

                alignmentDir /= (hits.Length - 1);

                alignmentDir.y = 0;
                dir += math.normalize(alignmentDir) * UnitAlignmentWeight;
                separationDir.y = 0;
                if (!separationDir.Equals(float3.zero))
                {
                    dir += math.normalize(separationDir) * UnitAvoidanceWeight;
                }
            }

            {
                float maxDistance = 0;
                float minDistance = float.MaxValue;
                float3 collisionAvoidDir = float3.zero;
                float3 oppositeAvoidForceDir = float3.zero;
                
                float angle = 0;
                bool castHit = false;
                float angleIncrement = 10.0f;
                for (int step = 0; step <= 18; ++step)
                {
                    angleIncrement *= -1;
                    angle += angleIncrement * step;


                    float3 rayDirection = math.mul(quaternion.AxisAngle(math.up(), math.radians(angle)), math.normalize(targetPosition - selfTransform.Position));
                    
                    RaycastInput input = new RaycastInput()
                    {
                        Start = selfTransform.Position + new float3(0f, 1f, 0f),
                        End = selfTransform.Position + rayDirection * AvoidanceDistance + new float3(0f, 1f, 0f),
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = ~0u,
                            CollidesWith = 1 << 2
                        }
                    };

                    if (PhysicsWorld.CastRay(input, out Unity.Physics.RaycastHit hit))
                    {
                        float candidateDistance = math.distance(selfTransform.Position, hit.Position);
                        castHit = true;
                        if (candidateDistance > maxDistance)
                        {
                            maxDistance = candidateDistance;
                            collisionAvoidDir = rayDirection;
                        }
                        if(candidateDistance < minDistance )
                        {
                            minDistance = candidateDistance;
                            oppositeAvoidForceDir = rayDirection;
                        }
                        Debug.DrawLine(selfTransform.Position + new float3(0f, 1f, 0f), hit.Position, Color.green);
                    }
                    else if (maxDistance != float.MaxValue) 
                    {
                        collisionAvoidDir = rayDirection;
                        maxDistance = float.MaxValue;
                        Debug.DrawLine(selfTransform.Position + new float3(0f, 1f, 0f), selfTransform.Position + new float3(0f, 1f, 0f) + rayDirection * AvoidanceDistance, Color.white);
                    }
                        
                }

                collisionAvoidDir.y = 0;
                oppositeAvoidForceDir.y = 0;
                if (!collisionAvoidDir.Equals(float3.zero) && castHit && !oppositeAvoidForceDir.Equals(float3.zero))
                {
                   dir += math.normalize(collisionAvoidDir) * TerrainAvoidanceWeight - math.normalize(oppositeAvoidForceDir) * oppositeAvoidForce;
                }
            }

            dir = math.normalize(dir);
            float distance = math.distance(selfTransform.Position, targetPosition);
            movement.DesiredVelocity = 0.4f + math.min(distance, 6) / 10;
            movement.Direction = dir;
            movement.IsMoving = true;
            hits.Dispose();

            Debug.DrawLine(selfTransform.Position, targetPosition, Color.cyan);
        }
    }
}

