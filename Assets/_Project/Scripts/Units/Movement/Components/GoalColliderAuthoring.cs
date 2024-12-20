using Unity.Entities;
using UnityEngine;

public class GoalColliderAuthoring : MonoBehaviour
{
    public int IndexId;

    public class Baker : Baker<GoalColliderAuthoring>
    {
        public override void Bake(GoalColliderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Goal() { Index = authoring.IndexId });
        }
    }
}

public struct Goal : IComponentData
{
    public int Index;
}