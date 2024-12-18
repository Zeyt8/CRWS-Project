using Unity.Entities;
using UnityEngine;

public class UnitSpawnerAuthoring : MonoBehaviour
{
    public UnitAuthoring UnitPrefab;

    public class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new UnitSpawner
            {
                UnitPrefabEntity = GetEntity(authoring.UnitPrefab, TransformUsageFlags.Dynamic),
                UnitToSpawn = null
            });
        }
    }
}

public struct UnitSpawner : IComponentData
{
    public Entity UnitPrefabEntity;
    public UnitTypes? UnitToSpawn;
}
