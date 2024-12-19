using Unity.Entities;
using UnityEngine;

public class UnitSpawnerAuthoring : MonoBehaviour
{
    public UnitAuthoring UnitPrefab;
    public float SpawnWidth;
    public float SpawnLength;

    public class Baker : Baker<UnitSpawnerAuthoring>
    {
        public override void Bake(UnitSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new UnitSpawner
            {
                UnitPrefabEntity = GetEntity(authoring.UnitPrefab, TransformUsageFlags.Dynamic),
                UnitToSpawn = null,
                SpawnWidth = authoring.SpawnWidth,
                SpawnLength = authoring.SpawnLength,
                Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000))
            });
        }
    }
}

public struct UnitSpawner : IComponentData
{
    public Entity UnitPrefabEntity;
    public UnitTypes? UnitToSpawn;
    public float SpawnWidth;
    public float SpawnLength;
    public Unity.Mathematics.Random Random;
}

