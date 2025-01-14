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

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _transforms = state.GetComponentLookup<LocalTransform>(true);
        _leaders = state.GetComponentLookup<LeaderPathfinding>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _transforms.Update(ref state);
        _leaders.Update(ref state);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new FollowerPathfindingJob
        {
            PhysicsWorld = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            Transforms = _transforms,
            Leaders = _leaders
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    private partial struct FollowerPathfindingJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> Transforms;
        [ReadOnly] public ComponentLookup<LeaderPathfinding> Leaders;
        private const float AvoidanceDistance = 3f; //lenght of ray cast (both thick and others)
        private const float BoundsRadius = 3f; //how thick the first one is, (might need to increase)
        private const float TargetWeight = 1f;
        private const float UnitAvoidanceWeight = 1f;
        private const float UnitAlignmentWeight = 1f;
        private const float TerrainAvoidanceWeight = 5f;

        public void Execute(in LocalTransform selfTransform, in FollowerPathfinding pf, ref MovementData movement, in TeamData team, Entity entity)
        {
            LeaderPathfinding leader = Leaders.GetRefRO(pf.Leader).ValueRO;
            float3 leaderPosition = Transforms.GetRefRO(pf.Leader).ValueRO.Position;
            float3 targetPosition = leaderPosition + pf.FormationOffset;
            
            float3 dir = targetPosition - selfTransform.Position;
            if (math.length(dir) < 0.01f)
            {
                if (!leader.IsMoving)
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

            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1 << 3,
                CollidesWith = 1 << 0
            };

            PhysicsWorld.OverlapSphere(selfTransform.Position, pf.ViewRadius, ref hits, filter);
            if (hits.Length > 1)
            {
                float3 alignmentDir = float3.zero;
                float3 separationDir = float3.zero;

                foreach (DistanceHit hit in hits)
                {
                    if (hit.Entity != entity)
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
                float3 collisionAvoidDir = float3.zero;
                float angleIncrement = 10f; // probably change this, not too small for optimization
                bool castHit = false;
                for (float angle = 0; angle <= 180; angle += angleIncrement) // Change to 90
                {
                    float realAngle = angle;

                    if(realAngle > 90)
                    {
                        realAngle = 90 - realAngle;
                    }
                    float3 rayDirection = math.mul(quaternion.AxisAngle(math.up(), math.radians(realAngle)), selfTransform.Forward());
                    
                    RaycastInput input = new RaycastInput()
                    {
                        Start = selfTransform.Position + new float3(0f, 1f, 0f),
                        End = selfTransform.Position + rayDirection * AvoidanceDistance + new float3(0f, 1f, 0f),
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = 1 << 3,
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
                if (!collisionAvoidDir.Equals(float3.zero) && castHit)
                {
                   dir += math.normalize(collisionAvoidDir) * TerrainAvoidanceWeight;
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

