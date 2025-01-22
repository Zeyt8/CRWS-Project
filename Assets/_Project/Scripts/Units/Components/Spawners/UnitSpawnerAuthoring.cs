using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class UnitSpawnerAuthoring : MonoBehaviour
{
    public List<GameObject> UnitPrefabs = new List<GameObject>();
    public List<int> UnitGroupCount = new List<int>();

    public class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            DynamicBuffer<UnitPrefabBufferElement> buffer = AddBuffer<UnitPrefabBufferElement>(entity);

            for (int i = 0; i < authoring.UnitPrefabs.Count; i++)
            {
                buffer.Add(new UnitPrefabBufferElement
                {
                    UnitPrefabEntity = GetEntity(authoring.UnitPrefabs[i], TransformUsageFlags.Dynamic),
                    Count = authoring.UnitGroupCount[i]
                });
            }

            AddComponent(entity, new UnitSpawner
            {
                UnitToSpawn = UnitTypes.TwoHandedSword,
                SpawnPosition = null,
            });
        }
    }
}

[InternalBufferCapacity(12)]
public struct UnitPrefabBufferElement : IBufferElementData
{
    public Entity UnitPrefabEntity;
    public int Count;
}

public struct UnitSpawner : IComponentData
{
    public UnitTypes UnitToSpawn;
    public float3? SpawnPosition;
}
