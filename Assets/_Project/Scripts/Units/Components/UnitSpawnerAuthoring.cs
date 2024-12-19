using Unity.Entities;
using UnityEngine;

//public class UnitSpawnerAuthoring : MonoBehaviour
//{
//    public UnitAuthoring UnitPrefab;

//    public class Baker : Baker<UnitSpawnerAuthoring>
//    {
//        public override void Bake(UnitSpawnerAuthoring authoring)
//        {
//            Entity entity = GetEntity(TransformUsageFlags.None);
//            AddComponent(entity, new UnitSpawner
//            {
//                UnitPrefabEntity = GetEntity(authoring.UnitPrefab, TransformUsageFlags.Dynamic),
//                UnitToSpawn = null
//            });
//        }
//    }
//}

public class UnitSpawnerAuthoring : MonoBehaviour
{
    public UnitAuthoring UnitPrefab;

    public class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            // Create an entity for the spawner
            Entity entity = GetEntity(TransformUsageFlags.None);

            // Add the UnitSpawner component with no collider set initially
            AddComponent(entity, new UnitSpawner
            {
                UnitPrefabEntity = GetEntity(authoring.UnitPrefab, TransformUsageFlags.Dynamic),
                UnitToSpawn = null,
                TargetColliderEntity = Entity.Null, // Initially set to null, will be updated by system
            });
        }
    }
}

public struct UnitSpawner : IComponentData
{
    public Entity UnitPrefabEntity;
    public UnitTypes? UnitToSpawn;
    public Entity TargetColliderEntity;
}

