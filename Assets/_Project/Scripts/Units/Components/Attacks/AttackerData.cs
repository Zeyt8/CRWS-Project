using Unity.Entities;

public struct AttackerData : IComponentData
{
    public float Cooldown;
    public float Timer;
    public float AttackDuration;
    public float AggroRange;
    public DamageType AttackType;
    public bool IsAttacking;
    public bool AttackAnimTrigger;
}
