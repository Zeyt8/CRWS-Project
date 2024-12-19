using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform, RefRW<Movement> movement, DynamicBuffer<PathBufferElement> pathBuffer)
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Movement>, DynamicBuffer<PathBufferElement>>())
        {
            if (movement.ValueRW.CurrentPathIndex >= pathBuffer.Length)
            {
                movement.ValueRW.IsMoving = false;
                movement.ValueRW.Velocity = 0;
                continue;
            }

            float3 targetPosition = pathBuffer[movement.ValueRW.CurrentPathIndex].Position;
            float3 direction = math.normalize(targetPosition - localTransform.ValueRW.Position);
            float3 movementVector = direction * movement.ValueRW.MovementSpeed * SystemAPI.Time.DeltaTime;

            localTransform.ValueRW.Position += movementVector;
            localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());

            if (math.distance(localTransform.ValueRW.Position, targetPosition) < 0.1f)
                movement.ValueRW.CurrentPathIndex++;

            movement.ValueRW.IsMoving = true;
            movement.ValueRW.Velocity = math.length(movementVector);
        }
    }
}
