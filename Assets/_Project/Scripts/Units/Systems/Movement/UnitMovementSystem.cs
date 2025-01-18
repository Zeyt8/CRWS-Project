using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(LeaderPathfindingSystem))]
[UpdateAfter(typeof(FollowerPathfindingSystem))]
[UpdateAfter(typeof(MeleeAttackerSystem))]
[UpdateAfter(typeof(RangedAttackerSystem))]
partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out Level level))
        {
            if (level.HasStarted)
            {
                UnitMovementJob job = new UnitMovementJob
                {
                    DeltaTime = SystemAPI.Time.DeltaTime
                };
                state.Dependency = job.ScheduleParallel(state.Dependency);
            }
        }
    }

    [BurstCompile]
    private partial struct UnitMovementJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, ref MovementData movement, in AttackerData attacker)
        {
            if (!movement.IsMoving || attacker.IsAttacking) return;

            float3 forward = transform.Forward();
            float3 direction = movement.Direction;
            float desiredVelocity = movement.DesiredVelocity * SpeedBasedOnAngle(forward, direction, 60f);

            movement.CurrentVelocity = MoveTowards(movement.CurrentVelocity, desiredVelocity, DeltaTime * movement.Acceleration);

            float3 movementVector = movement.CurrentVelocity * movement.MovementSpeed * direction;
            transform.Position += movementVector * DeltaTime;

            quaternion targetRotation = quaternion.LookRotationSafe(new float3(direction.x, 0, direction.z), math.up());
            transform.Rotation = math.slerp(transform.Rotation, targetRotation, DeltaTime * movement.TurningSpeed);

            Debug.DrawLine(transform.Position + new float3(0, 0.5f, 0), transform.Position + new float3(0, 0.5f, 0) + movementVector, Color.red);
            Debug.DrawLine(transform.Position + new float3(0, 0.5f, 0), transform.Position + new float3(0, 0.5f, 0) + transform.Forward(), Color.blue);
        }

        private float SpeedBasedOnAngle(float3 forward, float3 direction, float maxAngle)
        {
            float angle = math.degrees(math.acos(math.clamp(math.dot(forward, direction), -1f, 1f)));

            if (angle > maxAngle)
            {
                return 0f;
            }
            else if (angle < 10f)
            {
                return 1f;
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
