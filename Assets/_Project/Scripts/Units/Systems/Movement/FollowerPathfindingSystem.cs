using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(LeaderPathfindingSystem))]
partial struct FollowerPathfindingSystem : ISystem
{
    private ComponentLookup<LocalTransform> _transforms;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _transforms = state.GetComponentLookup<LocalTransform>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _transforms.Update(ref state);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var job = new FollowerPathfindingJob
        {
            PhysicsWorld = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            Transforms = _transforms
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    private partial struct FollowerPathfindingJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> Transforms;
        private const float AvoidanceDistance = 5f; //lenght of ray cast (both thick and others)
        private const float BoundsRadius = 3f; //how thick the first one is, (might need to increase)
        private const float TargetWeight = 1f;
        private const float UnitAvoidanceWeight = 5f;
        private const float UnitAlignmentWeight = 1f;
        private const float TerrainAvoidanceWeight = 15f;
        private const float oppositeAvoidForce = 10f;
        public void Execute(in FollowerPathfinding pf, ref MovementData movement, in TeamData team, Entity entity)
        {
            LocalTransform selfTransform = Transforms.GetRefRO(entity).ValueRO;
            float3 leaderPosition = Transforms.GetRefRO(pf.Leader).ValueRO.Position;
            float3 targetPosition = leaderPosition + pf.FormationOffset;
            
            float3 dir = targetPosition - selfTransform.Position;
            if (math.length(dir) < 0.01f)
            {
                movement.IsMoving = false;
                movement.DesiredVelocity = 0;
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

            //if (IsHeadingForCollision(PhysicsWorld, selfTransform.Position + new float3(0f, 1f, 0f), BoundsRadius, selfTransform.Forward(), AvoidanceDistance))
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
                            BelongsTo = 1 << 3,
                            CollidesWith = 1 << 2
                        }
                    };

                    Debug.DrawLine(selfTransform.Position + new float3(0f, 1f, 0f), selfTransform.Position + rayDirection * AvoidanceDistance + new float3(0f, 1f, 0f));

                    if (PhysicsWorld.CastRay(input, out Unity.Physics.RaycastHit hit))
                    {
                        float candidateDistance = math.distance(selfTransform.Position, hit.Position);
                        Debug.DrawLine(selfTransform.Position + new float3(0f, 1f, 0f), hit.Position, Color.green);
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
                    }
                    else if (maxDistance != float.MaxValue) 
                    {
                        collisionAvoidDir = rayDirection;
                        maxDistance = float.MaxValue;
                    }
                        
                }


                collisionAvoidDir.y = 0;
                oppositeAvoidForceDir.y = 0;
                if (!collisionAvoidDir.Equals(float3.zero) && castHit && !oppositeAvoidForceDir.Equals(float3.zero))
                {
                   dir += math.normalize(collisionAvoidDir) * TerrainAvoidanceWeight - math.normalize(oppositeAvoidForceDir) * oppositeAvoidForce;
                   //Debug.Log("Direction :: line 155 :: " + dir);
                }
            }

            dir = math.normalize(dir);
            movement.Direction = dir;
            movement.IsMoving = true;
            hits.Dispose();
        }

        private bool IsHeadingForCollision(PhysicsWorldSingleton physicsWorld, float3 center, float radius, float3 direction, float distance)
        {
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1 << 0,
                CollidesWith = 1 << 2
            };
            Debug.DrawLine(center, center + direction * distance, Color.red);
            return physicsWorld.SphereCast(center, radius, direction, distance, filter);
        }
    }
}

