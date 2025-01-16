using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

public class EnemyUnitSpawnerAuthoring : MonoBehaviour
{
    public List<GameObject> UnitPrefabs = new List<GameObject>();
    public List<int> UnitGroupCount = new List<int>();
    public Vector2 SpawnBounds;
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
                    UnitPrefabEntity = GetEntity(authoring.UnitPrefabs[i], TransformUsageFlags.Dynamic),
                    Count = authoring.UnitGroupCount[i]
                });
            }

            AddComponent(entity, new EnemyUnitSpawner
            {
                SpawnBounds = new Unity.Mathematics.float2(authoring.SpawnBounds.x, authoring.SpawnBounds.y),
                Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000)),
                Count = authoring.Count
            });
        }
    }
}

public struct EnemyUnitSpawner : IComponentData
{
    [ReadOnly(true)] public Unity.Mathematics.float2 SpawnBounds;
    public Unity.Mathematics.Random Random;
    public int Count;
}
