using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public partial class AgentInteractionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = World.Time.DeltaTime;

        // Create a lookup for enemies' positions
        var enemyPositions = new NativeList<float3>(Allocator.TempJob);

        // Collect all enemy positions
        Entities
            .WithAll<EnemyTag>()
            .ForEach((in LocalTransform enemyTransform) =>
            {
                enemyPositions.Add(enemyTransform.Position);
            }).Run(); // Collect positions on the main thread for simplicity

        // Process agents and check for interaction with enemies
        Entities
            .WithAll<TeamTag>()
            .ForEach((ref AgentComponent agent, in LocalTransform agentTransform) =>
            {
                for (int i = 0; i < enemyPositions.Length; i++)
                {
                    float3 toEnemy = enemyPositions[i] - agentTransform.Position;
                    float distance = math.length(toEnemy);

                    if (distance < 2f) // Example attack range
                    {
                        agent.isAttacking = true;
                        break; // Exit loop after first interaction
                    }
                    else
                    {
                        agent.isAttacking = false;
                    }
                }
            }).ScheduleParallel(); // Parallelize this query

        // Dispose of enemyPositions when done
        Dependency = enemyPositions.Dispose(Dependency);
    }
}
