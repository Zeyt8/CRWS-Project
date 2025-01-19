using Unity.Entities;
using UnityEngine;

public class ProjectileInstaceAuthoring : MonoBehaviour
{
    public float Damage;
    public DamageType DamageType;
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
                DamageType = src.DamageType,
                Speed = src.Speed,
                Lifetime = src.Lifetime
            });
        }
    }
}

public struct ProjectileInstanceData : IComponentData
{
    public float Damage;
    public DamageType DamageType;
    public float Speed;
    public float Lifetime;
}
