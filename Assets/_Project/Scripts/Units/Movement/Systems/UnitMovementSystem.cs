using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static UnityEngine.EventSystems.EventTrigger;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(EnemyBaseSetterSystem))]
partial struct UnitMovementSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRW<LocalTransform> localTransform, RefRW<MovementData> movement, RefRO<TeamData> team, RefRO<EnemyBaseReference> ebr, DynamicBuffer<PathBufferElement> pathBuffer, Entity entity) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>, RefRO<TeamData>, RefRO<EnemyBaseReference>, DynamicBuffer<PathBufferElement>>().WithEntityAccess())
        {
            float desiredVelocity = 0.5f;
            float3 targetPosition = localTransform.ValueRO.Position;

            (desiredVelocity, targetPosition) = LocalAvoidance(ref state, localTransform.ValueRO, movement.ValueRO, team.ValueRO.Value, ebr.ValueRO, entity);

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
                desiredVelocity = 0;
                continue;
            }

            float3 movementTarget = pathBuffer[movement.ValueRO.CurrentPathIndex].Position;
            float3 direction = math.normalize(movementTarget - localTransform.ValueRO.Position);

            float3 forward = localTransform.ValueRO.Forward();
            desiredVelocity *= SpeedBasedOnAngle(forward, direction, 60f);

            if (movement.ValueRO.CurrentVelocity < desiredVelocity)
            {
                movement.ValueRW.CurrentVelocity += SystemAPI.Time.DeltaTime * 10;
            }
            else if (movement.ValueRO.CurrentVelocity > desiredVelocity)
            {
                movement.ValueRW.CurrentVelocity -= SystemAPI.Time.DeltaTime * 10;
            }

            float3 movementVector = movement.ValueRO.CurrentVelocity * movement.ValueRO.MovementSpeed * forward;
            localTransform.ValueRW.Position += movementVector * SystemAPI.Time.DeltaTime;

            quaternion targetRotation = quaternion.LookRotationSafe(new float3(direction.x, 0, direction.z), math.up());
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * 1);

            if (math.distance(localTransform.ValueRO.Position, movementTarget) < 0.01f)
                movement.ValueRW.CurrentPathIndex++;

            movement.ValueRW.IsMoving = true;
            movement.ValueRW.Target = targetPosition;
        }
    }

    private (float desiredVelocity, float3 targetLocation) LocalAvoidance(ref SystemState state, LocalTransform localTransform, MovementData movement, int team, EnemyBaseReference ebr, Entity entity)
    {
        float desiredVelocity = 0.5f;
        float3 targetPosition = float3.zero;
        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
        CollisionFilter filter = new CollisionFilter()
        {
            BelongsTo = 1 << 0,
            CollidesWith = 1 << 0
        };

        // If units kinda close, move slower
        SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.Position, movement.SeparationDistances.z, ref hits, filter);
        if (hits.Length > 1)
        {
            desiredVelocity = 0.25f;
        }

        // If even closer, just stop
        SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.Position, movement.SeparationDistances.y, ref hits, filter);
        if (hits.Length > 1)
        {
            desiredVelocity = 0;
        }

        SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.Position, movement.SeparationDistances.x, ref hits, filter);
        // If even closer, move away from the unit
        if (hits.Length > 1)
        {
            desiredVelocity = 0.25f;
            foreach (DistanceHit unit in hits)
            {
                LocalTransform otherUnitTransform = SystemAPI.GetComponent<LocalTransform>(unit.Entity);
                TeamData otherUnitTeam = SystemAPI.GetComponent<TeamData>(unit.Entity);
                if (team == otherUnitTeam.Value && unit.Entity != entity)
                {
                    float3 awayFromFollower = localTransform.Position - otherUnitTransform.Position;
                    targetPosition += math.normalize(awayFromFollower);
                }
            }
        }
        else // If no units nearby, move at full speed towards the base
        {
            targetPosition = ebr.Location;
        }
        hits.Dispose();

        return (desiredVelocity, targetPosition);
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
