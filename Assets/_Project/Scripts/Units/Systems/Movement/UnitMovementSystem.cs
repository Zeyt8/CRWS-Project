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
        foreach (
            (RefRW<LocalTransform> transform, RefRW<MovementData> movement) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>>())
        {
            if (!movement.ValueRO.IsMoving) continue;

            float3 forward = transform.ValueRO.Forward();
            float3 direction = movement.ValueRO.Direction;
            float desiredVelocity = movement.ValueRO.DesiredVelocity * SpeedBasedOnAngle(forward, direction, 60f);

            movement.ValueRW.CurrentVelocity = MoveTowards(movement.ValueRW.CurrentVelocity, desiredVelocity, SystemAPI.Time.DeltaTime * movement.ValueRO.Acceleration);

            //float3 movementVector = movement.ValueRO.CurrentVelocity * movement.ValueRO.MovementSpeed * forward;
            float3 movementVector = movement.ValueRO.CurrentVelocity * movement.ValueRO.MovementSpeed * direction;
            transform.ValueRW.Position += movementVector * SystemAPI.Time.DeltaTime;

            quaternion targetRotation = quaternion.LookRotationSafe(new float3(direction.x, 0, direction.z), math.up());
            transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * movement.ValueRO.TurningSpeed);
        }
    }

    [BurstCompile]
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

    [BurstCompile]
    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (math.abs(target - current) <= maxDelta)
        {
            return target;
        }
        return current + math.sign(target - current) * maxDelta;
    }

}
