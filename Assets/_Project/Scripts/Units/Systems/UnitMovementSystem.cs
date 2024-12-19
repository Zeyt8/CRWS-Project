using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(EnemyBaseSetterSystem))]
partial struct UnitMovementSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRW<LocalTransform> localTransform, RefRW<Movement> movement, RefRO<Team> team, RefRO<EnemyBaseReference> ebr, DynamicBuffer<PathBufferElement> pathBuffer) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<Movement>, RefRO<Team>, RefRO<EnemyBaseReference>, DynamicBuffer<PathBufferElement>>())
        {
            float3 targetPosition = float3.zero;

            // Local Avoidance
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(100, Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                CollidesWith = 1 << 6,
            };
            SystemAPI.GetSingleton<PhysicsWorldSingleton>().OverlapSphere(localTransform.ValueRO.Position, movement.ValueRO.SeparationDistance, ref hits, filter);
            foreach (DistanceHit unit in hits)
            {
                LocalTransform otherUnitTransform = SystemAPI.GetComponent<LocalTransform>(unit.Entity);
                Team otherUnitTeam = SystemAPI.GetComponent<Team>(unit.Entity);
                if (team.ValueRO.Value == otherUnitTeam.Value)
                {
                    float3 awayFromFollower = localTransform.ValueRO.Position - otherUnitTransform.Position;
                    targetPosition += math.normalize(awayFromFollower) * 0.05f;
                }
            }
            hits.Dispose();

            NavMeshUtility.CalculatePath(localTransform.ValueRO.Position, ebr.ValueRO.Location, pathBuffer);

            // Global Planning
            if (movement.ValueRW.CurrentPathIndex >= pathBuffer.Length)
            {
                movement.ValueRW.IsMoving = false;
                movement.ValueRW.Velocity = 0;
                continue;
            }

            targetPosition = pathBuffer[movement.ValueRW.CurrentPathIndex].Position;
            float3 direction = math.normalize(targetPosition - localTransform.ValueRO.Position);
            float3 movementVector = direction * movement.ValueRW.MovementSpeed;

            localTransform.ValueRW.Position += movementVector * SystemAPI.Time.DeltaTime;
            localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());

            if (math.distance(localTransform.ValueRW.Position, targetPosition) < 0.1f)
                movement.ValueRW.CurrentPathIndex++;

            movement.ValueRW.IsMoving = true;
            movement.ValueRW.Velocity = math.length(movementVector);
        }
    }
}
