using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct TargetReachSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        NativeReference<bool> jobResult = new NativeReference<bool>(Allocator.TempJob);
        EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

        TargetReachJob job = new TargetReachJob
        {
            ParallelWriter = buffer.AsParallelWriter(),
            Result = jobResult,
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();
        if (SystemAPI.TryGetSingleton<Level>(out var level))
        {
            level.Lose |= jobResult.Value;
            SystemAPI.SetSingleton<Level>(level);
        }
            
        jobResult.Dispose();
        buffer.Playback(state.EntityManager);
        buffer.Dispose();
    }

    [BurstCompile]
    private partial struct TargetReachJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [NativeDisableParallelForRestriction] public NativeReference<bool> Result;
        public void Execute([EntityIndexInQuery]int entityInQueryIndex, in EnemyBaseReference ebr, in LocalTransform transform, in TeamData teamData, Entity entity)
        {
            float3 editedPosition = transform.Position;

            editedPosition.y = 0f;


            if (math.distance(editedPosition, ebr.Location) < 2f)
            {
                if (teamData.Value == 1)
                {
                    Result.Value = true;
                    UnityEngine.Debug.Log("Reached end");
                }
                
                ParallelWriter.DestroyEntity(entityInQueryIndex, entity);
            }
        }
    }
}
