using Unity.Entities;
using Unity.Mathematics;

public class AgentBaker : Baker<AgentAuthoring>
{
    public override void Bake(AgentAuthoring authoring)
    {
        // Add the ECS component and set its data
        AddComponent(new AgentComponent
        {
            health = authoring.health,
            attackStrength = authoring.attackStrength,
            goal = (float3)authoring.goal,
            isAttacking = false // Default value
        });
    }
}
