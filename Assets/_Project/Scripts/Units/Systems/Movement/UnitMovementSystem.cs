using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(LeaderPathfindingSystem))]
[UpdateAfter(typeof(FollowerPathfindingSystem))]
partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMovementJob job = new UnitMovementJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    private partial struct UnitMovementJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, ref MovementData movement)
        {
            if (!movement.IsMoving) return;

            float3 forward = transform.Forward();
            float3 direction = movement.Direction;
            float desiredVelocity = movement.DesiredVelocity * SpeedBasedOnAngle(forward, direction, 60f);

            movement.CurrentVelocity = MoveTowards(movement.CurrentVelocity, desiredVelocity, DeltaTime * movement.Acceleration);

            float3 movementVector = movement.CurrentVelocity * movement.MovementSpeed * direction;
            transform.Position += movementVector * DeltaTime;

            quaternion targetRotation = quaternion.LookRotationSafe(new float3(direction.x, 0, direction.z), math.up());
            transform.Rotation = math.slerp(transform.Rotation, targetRotation, DeltaTime * movement.TurningSpeed);
        }

        private float SpeedBasedOnAngle(float3 forward, float3 direction, float maxAngle)
        {
            float angle = math.degrees(math.acos(math.clamp(math.dot(forward, direction), -1f, 1f)));

            if (angle > maxAngle)
            {
                return 0f;
            }
            else
            {
                float lerpFactor = 1f - (angle / maxAngle);
                return lerpFactor;
            }
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (math.abs(target - current) <= maxDelta)
            {
                return target;
            }
            return current + math.sign(target - current) * maxDelta;
        }
    }
}
