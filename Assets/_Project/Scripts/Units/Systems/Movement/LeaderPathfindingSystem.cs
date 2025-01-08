using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(MovementSystemGroup))]
partial struct LeaderPathfindingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRO<LocalTransform> transform, RefRW<LeaderPathfinding> pf, RefRW<MovementData> movement, RefRO<EnemyBaseReference> ebr, DynamicBuffer<PathBufferElement> pathBuffer) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<LeaderPathfinding>, RefRW<MovementData>, RefRO<EnemyBaseReference>, DynamicBuffer<PathBufferElement>>())
        {
            float3 targetPosition = ebr.ValueRO.Location;

            // Recalculate path if target has changed
            if (!pf.ValueRO.Target.Equals(targetPosition))
            {
                pf.ValueRW.Target = targetPosition;
                NavMeshUtility.CalculatePath(transform.ValueRO.Position, targetPosition, pathBuffer);
                pf.ValueRW.CurrentPathIndex = 1;
            }

            // Traverse path
            if (pf.ValueRW.CurrentPathIndex >= pathBuffer.Length)
            {
                movement.ValueRW.IsMoving = false;
                movement.ValueRW.DesiredVelocity = 0;
                continue;
            }

            float3 movementTarget = pathBuffer[pf.ValueRO.CurrentPathIndex].Position;
            float3 direction = math.normalize(movementTarget - transform.ValueRO.Position);

            if (math.distance(transform.ValueRO.Position, movementTarget) < 0.01f)
                pf.ValueRW.CurrentPathIndex++;

            movement.ValueRW.Direction = direction;
            movement.ValueRW.IsMoving = true;
            movement.ValueRW.DesiredVelocity = 1;
        }
    }
}
