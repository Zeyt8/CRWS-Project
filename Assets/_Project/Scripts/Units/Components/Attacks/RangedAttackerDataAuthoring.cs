using Unity.Entities;
using UnityEngine;

public class RangedAttackerDataAuthoring : MonoBehaviour
{
    public ProjectileInstaceAuthoring Projectile;
    public float Range;
    public float Cooldown;
    public float AttackDuration;
    public float AggroRange;
    public DamageType AttackType;

    public class Baker : Baker<RangedAttackerDataAuthoring>
    {
        public override void Bake(RangedAttackerDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RangedAttackerData
            {
                Projectile = GetEntity(authoring.Projectile, TransformUsageFlags.Dynamic),
                Range = authoring.Range,
            });
            AddComponent(entity, new AttackerData
            {
                Cooldown = authoring.Cooldown,
                Timer = 0,
                AttackDuration = authoring.AttackDuration,
                AggroRange = authoring.AggroRange,
                AttackType = authoring.AttackType,
                IsAttacking = false,
                AttackAnimTrigger = false
            });
        }
    }
}

public struct RangedAttackerData : IComponentData
{
    public Entity Projectile;
    public float Range;
}
