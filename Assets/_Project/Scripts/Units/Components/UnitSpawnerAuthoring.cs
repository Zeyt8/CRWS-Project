using Unity.Entities;
using UnityEngine;

public class UnitSpawnerAuthoring : MonoBehaviour
{
    public UnitAuthoring UnitPrefab;

    public class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitSpawner
            {
                UnitPrefabEntity = GetEntity(authoring.UnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct UnitSpawner : IComponentData
{
    public Entity UnitPrefabEntity;
}
