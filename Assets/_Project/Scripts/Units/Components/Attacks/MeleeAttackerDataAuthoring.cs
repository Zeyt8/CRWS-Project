using Unity.Entities;
using UnityEngine;

public class MeleeAttackerDataAuthoring : MonoBehaviour
{
    public float Range;
    public float Cooldown;
    public float Timer;
    public bool IsAttacking;

    public class Baker : Baker<MeleeAttackerDataAuthoring>
    {
        public override void Bake(MeleeAttackerDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttackerData
            {
                Range = authoring.Range,
                Cooldown = authoring.Cooldown,
                Timer = 0
            });
        }
    }
}

public struct MeleeAttackerData : IComponentData
{
    public float Range;
    public float Cooldown;
    public float Timer;
}
