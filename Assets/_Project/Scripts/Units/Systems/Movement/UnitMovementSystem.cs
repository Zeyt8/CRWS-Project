using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(EnemyBaseSetterSystem))]
partial struct UnitMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRW<LocalTransform> localTransform, RefRW<MovementData> movement, RefRO<TeamData> team, RefRO<EnemyBaseReference> ebr, DynamicBuffer<PathBufferElement> pathBuffer, Entity entity) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, RefRO<TeamData>, RefRO<EnemyBaseReference>, DynamicBuffer<PathBufferElement>>().WithEntityAccess())
        {
            float3 targetPosition = localTransform.ValueRO.Position;

            // Local Avoidance
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1 << 0,
                CollidesWith = 1 << 0
            };
            SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.ValueRO.Position, movement.ValueRO.SeparationDistance, ref hits, filter);
            // If any units nearby, move slower
            // If they are on the same team, move away from them
            if (hits.Length > 1)
            {
                foreach (DistanceHit unit in hits)
                {
                    LocalTransform otherUnitTransform = SystemAPI.GetComponent<LocalTransform>(unit.Entity);
                    TeamData otherUnitTeam = SystemAPI.GetComponent<TeamData>(unit.Entity);
                    if (team.ValueRO.Value == otherUnitTeam.Value && unit.Entity != entity)
                    {
                        float3 awayFromFollower = localTransform.ValueRO.Position - otherUnitTransform.Position;
                        targetPosition += math.normalize(awayFromFollower);
                    }
                }
                movement.ValueRW.DesiredVelocity = 0.25f;
            }
            else // If no units nearby, move at full speed towards the base
            {
                targetPosition = ebr.ValueRO.Location;
                movement.ValueRW.DesiredVelocity = 0.5f;
            }
            hits.Dispose();
            targetPosition.y = 0;

            // Recalculate path if target has changed
            if (!movement.ValueRO.Target.Equals(targetPosition))
            {
                NavMeshUtility.CalculatePath(localTransform.ValueRO.Position, targetPosition, pathBuffer);
                movement.ValueRW.CurrentPathIndex = 1;
            }

            // Traverse path
            if (movement.ValueRW.CurrentPathIndex >= pathBuffer.Length)
            {
                movement.ValueRW.IsMoving = false;
                movement.ValueRW.DesiredVelocity = 0;
                continue;
            }

            movement.ValueRW.CurrentVelocity = math.lerp(movement.ValueRO.CurrentVelocity, movement.ValueRO.DesiredVelocity, SystemAPI.Time.DeltaTime * 10);
            float3 movementTarget = pathBuffer[movement.ValueRO.CurrentPathIndex].Position;
            float3 direction = math.normalize(movementTarget - localTransform.ValueRO.Position);
            float3 movementVector = direction * movement.ValueRO.CurrentVelocity * movement.ValueRO.MovementSpeed;

            localTransform.ValueRW.Position += movementVector * SystemAPI.Time.DeltaTime;

            quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime);

            if (math.distance(localTransform.ValueRO.Position, movementTarget) < 0.01f)
                movement.ValueRW.CurrentPathIndex++;

            movement.ValueRW.IsMoving = true;
            movement.ValueRW.Target = targetPosition;
        }
    }
}
