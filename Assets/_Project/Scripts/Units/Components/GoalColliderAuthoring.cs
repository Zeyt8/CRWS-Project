using Unity.Entities;
using UnityEngine;

public class GoalColliderAuthoring : MonoBehaviour
{
    public class Baker : Baker<GoalColliderAuthoring>
    {
        public override void Bake(GoalColliderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Goal() { Foo = 0 });
        }
    }
}

public struct Goal : IComponentData
{
    public int Foo;
}