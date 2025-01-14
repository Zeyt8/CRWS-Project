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
    [Header("Attack")]
    public float AttackStrength = 5f;
    public float AttackRange = 5f;

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
            AddComponent(entity, new HealthData { Value = authoring.Health });
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
}

public struct EnemyBaseReference : IComponentData
{
    public float3 Location;
}
