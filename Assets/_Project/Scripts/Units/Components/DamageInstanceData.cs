using Unity.Entities;

public struct DamageInstanceData : IComponentData
{
    public float Value;
    public DamageType Type;
}

public enum DamageType
{
    Slash,
    Pierce,
    Blunt,
    Magic
}
