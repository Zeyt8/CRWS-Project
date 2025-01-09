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
        private const float AvoidanceDistance = 2f;
        private const float BoundsRadius = 0.5f;

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
                dir = math.normalize(dir);
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
                dir += math.normalize(alignmentDir);
                separationDir.y = 0;
                if (!separationDir.Equals(float3.zero))
                {
                    dir += math.normalize(separationDir);
                }
            }

            if (IsHeadingForCollision(PhysicsWorld, selfTransform.Position, BoundsRadius, selfTransform.Forward(), AvoidanceDistance))
            {
                float3 collisionAvoidDir = float3.zero;
                float angleIncrement = math.radians(30);
                for (float angle = 0; angle < math.radians(360); angle += angleIncrement)
                {
                    float3 rayDirection = math.mul(quaternion.AxisAngle(math.up(), angle), selfTransform.Forward());
                    RaycastInput input = new RaycastInput()
                    {
                        Start = selfTransform.Position,
                        End = selfTransform.Position + rayDirection * AvoidanceDistance,
                        Filter = new CollisionFilter()
                            {
                                BelongsTo = 1 << 3,
                                CollidesWith = 1 << 2
                            }
                    };
                    if (PhysicsWorld.CastRay(input))
                    {
                        collisionAvoidDir = rayDirection;
                        break;
                    }
                }
                collisionAvoidDir.y = 0;
                dir += math.normalize(collisionAvoidDir);
            }

            dir = math.normalize(dir);
            movement.DesiredVelocity = 1;
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
            return physicsWorld.SphereCast(center, radius, direction, distance, filter);
        }
    }
}

