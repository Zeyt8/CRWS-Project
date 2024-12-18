using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class AgentMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = World.Time.DeltaTime;

        Entities.ForEach((ref LocalTransform transform, in AgentComponent agent) =>
        {
            // Move agent towards the goal
            float3 direction = math.normalize(agent.goal - transform.Position);
            transform.Position += direction * deltaTime * 5f; // Adjust speed as needed
        }).ScheduleParallel();
    }
}
