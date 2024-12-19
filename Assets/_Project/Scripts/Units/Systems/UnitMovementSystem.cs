using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(UnitAvoidanceSystem))]
partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform, RefRW<Movement> ms) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Movement>>())
        {
            float3 movement = math.normalize(ms.ValueRO.DesiredVelocity) * ms.ValueRO.MovementSpeed;
            localTransform.ValueRW.Position += movement * SystemAPI.Time.DeltaTime;
            localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(ms.ValueRO.DesiredVelocity, math.up());
            ms.ValueRW.IsMoving = math.length(movement) > 0.01f;
            ms.ValueRW.Velocity = math.length(movement);
        }
    }
}
