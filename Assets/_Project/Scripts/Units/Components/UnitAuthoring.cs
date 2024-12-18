using UnityEngine;
using Unity.Entities;

public class UnitAuthoring : MonoBehaviour
{
    public float MovementSpeed = 5f;

    public class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MovementSpeed { Value = authoring.MovementSpeed });
        }
    }
}

public struct MovementSpeed : IComponentData
{
    public float Value;
}

