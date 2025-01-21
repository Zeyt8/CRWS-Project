using Unity.Entities;
using UnityEngine;

public class LevelAuthoring : MonoBehaviour
{
    public class Baker : Baker<LevelAuthoring>
    {
        public override void Bake(LevelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Level
            {
                HasStarted = false,
                Lose = false
            });
        }
    }
}

public struct Level : IComponentData
{
    public bool HasStarted;
    public bool Lose;
}
