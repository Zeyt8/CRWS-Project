using Unity.Entities;
using UnityEngine;

public class MeleeAttackerDataAuthoring : MonoBehaviour
{
    public float Damage;
    public float Range;
    public float Cooldown;
    public float AttackDuration;
    public float AggroRange;
    public DamageType AttackType;

    public class Baker : Baker<MeleeAttackerDataAuthoring>
    {
        public override void Bake(MeleeAttackerDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttackerData
            {
                Damage = authoring.Damage,
                Range = authoring.Range
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

public struct MeleeAttackerData : IComponentData
{
    public float Damage;
    public float Range;
}
