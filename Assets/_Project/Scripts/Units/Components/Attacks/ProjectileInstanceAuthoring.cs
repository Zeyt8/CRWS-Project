using Unity.Entities;
using UnityEngine;

public class ProjectileInstaceAuthoring : MonoBehaviour
{
    public float Damage;
    public float Speed;
    public float Lifetime;

    public class Baker : Baker<ProjectileInstaceAuthoring>
    {
        public override void Bake(ProjectileInstaceAuthoring src)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ProjectileInstanceData()
            {
                Damage = src.Damage,
                Speed = src.Speed,
                Lifetime = src.Lifetime
            });
        }
    }
}

public struct ProjectileInstanceData : IComponentData
{
    public float Damage;
    public float Speed;
    public float Lifetime;
}
