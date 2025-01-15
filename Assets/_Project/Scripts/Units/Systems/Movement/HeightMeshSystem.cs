using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct HeightMeshSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<LocalTransform> transform in SystemAPI.Query<RefRW<LocalTransform>>().WithPresent<MovementData>())
        {
            transform.ValueRW.Position.y = NavMeshUtility.SampleHeight(transform.ValueRO.Position);
        }
    }
}
