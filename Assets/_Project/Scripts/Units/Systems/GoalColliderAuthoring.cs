using Unity.Entities;
using UnityEngine;

public class GoalColliderAuthoring : MonoBehaviour
{
    public class Baker : Baker<GoalColliderAuthoring>
    {
        public override void Bake(GoalColliderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            // Add the GoalColliderEnemy component to the goal collider entity
            AddComponent(entity, new GoalColliderEnemy());
        }
    }
}