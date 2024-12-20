using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

public class EnemyUnitSpawnerAuthoring : MonoBehaviour
{
    public List<UnitAuthoring> UnitPrefabs = new List<UnitAuthoring>();
    public float SpawnWidth;
    public float SpawnLength;
    public int Count;

    public class Baker : Baker<EnemyUnitSpawnerAuthoring>
    {
        public override void Bake(EnemyUnitSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            DynamicBuffer<UnitPrefabBufferElement> buffer = AddBuffer<UnitPrefabBufferElement>(entity);

            for (int i = 0; i < authoring.UnitPrefabs.Count; i++)
            {
                buffer.Add(new UnitPrefabBufferElement
                {
                    UnitPrefabEntity = GetEntity(authoring.UnitPrefabs[i], TransformUsageFlags.Dynamic)
                });
            }

            AddComponent(entity, new EnemyUnitSpawner
            {
                SpawnWidth = authoring.SpawnWidth,
                SpawnLength = authoring.SpawnLength,
                Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000)),
                Count = authoring.Count
            });
        }
    }
}

[InternalBufferCapacity(10)] // Adjust the capacity as needed
public struct EnemyUnitPrefabBufferElement : IBufferElementData
{
    public Entity EnemyUnitPrefabEntity;
}

public struct EnemyUnitSpawner : IComponentData
{
    public float SpawnWidth;
    public float SpawnLength;
    public Unity.Mathematics.Random Random;
    public int Count;
}
