using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.ComponentModel;

public class UnitAuthoring : MonoBehaviour
{
    [Header("Movement")]
    public float MovementSpeed = 5f;
    public float TurningSpeed = 1f;
    public float Acceleration = 10f;
    [Header("Health")]
    public float Health = 5f;
    public float Regen = 0f;
    [Header("Armour")]
    [Range(0, 1)] public float SlashResistance = 0;
    [Range(0, 1)] public float PierceResistance = 0;
    [Range(0, 1)] public float BluntResistance = 0;
    [Range(0, 1)] public float MagicResistance = 0;

    public class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MovementData {
                MovementSpeed = authoring.MovementSpeed,
                TurningSpeed = authoring.TurningSpeed,
                Acceleration = authoring.Acceleration,
                Direction = float3.zero,
                IsMoving = false,
                DesiredVelocity = 0,
                CurrentVelocity = 0,
            });
            AddComponent(entity, new HealthData
            {
                Value = authoring.Health,
                Regen = authoring.Regen
            });
            AddComponent(entity, new ArmourData
            {
                SlashResistance = authoring.SlashResistance,
                PierceResistance = authoring.PierceResistance,
                BluntResistance = authoring.BluntResistance,
                MagicResistance = authoring.MagicResistance
            });
        }
    }
}

public struct TeamData : IComponentData
{
    [ReadOnly(true)] public int Value;
}

public struct LeaderPathfinding : IComponentData
{
    public int CurrentPathIndex;
    public float3 Target;
    public bool IsMoving;
}

public struct FollowerPathfinding : IComponentData
{
    [ReadOnly(true)] public float ViewRadius;
    [ReadOnly(true)] public float AvoidanceRadius;
    [ReadOnly(true)] public Entity Leader;
    [ReadOnly(true)] public float3 FormationOffset;
}

public struct MovementData : IComponentData
{
    [ReadOnly(true)] public float MovementSpeed;
    [ReadOnly(true)] public float TurningSpeed;
    [ReadOnly(true)] public float Acceleration;
    public float3 Direction;
    public bool IsMoving;
    public float DesiredVelocity;
    public float CurrentVelocity;
}

public struct HealthData : IComponentData
{
    public float Value;
    public float Regen;
}

public struct EnemyBaseReference : IComponentData
{
    public float3 Location;
}

public struct UnitTypeData : IComponentData
{
    public UnitTypes Value;
}

public struct ArmourData : IComponentData
{
    public float SlashResistance;
    public float PierceResistance;
    public float BluntResistance;
    public float MagicResistance;
}
