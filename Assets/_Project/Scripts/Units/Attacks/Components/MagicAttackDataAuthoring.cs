using Unity.Entities;
using UnityEngine;

public class MagicAttackDataAuthoring : MonoBehaviour
{
    public ProjectileInstaceAuthoring Projectile;
    public float Range;
    public float Cooldown;
    public float Timer;
    public bool IsAttacking;

    public class Baker : Baker<MagicAttackDataAuthoring>
    {
        public override void Bake(MagicAttackDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MagicAttackData
            {
                Projectile = GetEntity(authoring.Projectile, TransformUsageFlags.Dynamic),
                Range = authoring.Range,
                Cooldown = authoring.Cooldown,
                Timer = 0,
                IsAttacking = false
            });
        }
    }
}

public struct MagicAttackData : IComponentData
{
    public Entity Projectile;
    public float Range;
    public float Cooldown;
    public float Timer;
    public bool IsAttacking;
}
