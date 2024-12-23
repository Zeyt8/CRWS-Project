using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.ComponentModel;

public class UnitAuthoring : MonoBehaviour
{
    [Header("Team")]
    public int Team = 0;
    [Header("Movement")]
    public float MovementSpeed = 5f;
    public float SeparationDistance = 1f;
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
            AddComponent(entity, new TeamData { Value = authoring.Team });
            AddComponent(entity, new MovementData {
                MovementSpeed = authoring.MovementSpeed,
                Target = float3.zero,
                IsMoving = false,
                CurrentVelocity = 0,
                CurrentPathIndex = 0,
                SeparationDistances = new float3(authoring.SeparationDistance, authoring.SeparationDistance * 1.25f, authoring.SeparationDistance * 1.5f)
            });
            AddComponent(entity, new HealthData { Value = authoring.Health });
        }
    }
}

public struct TeamData : IComponentData
{
    [ReadOnly(true)] public int Value;
}

public struct MovementData : IComponentData
{
    [ReadOnly(true)] public float MovementSpeed;
    public float3 Target;
    public bool IsMoving;
    public float CurrentVelocity;
    public int CurrentPathIndex;
    [ReadOnly(true)] public float3 SeparationDistances;
}

public struct HealthData : IComponentData
{
    public float Value;
}

public struct EnemyBaseReference : IComponentData
{
    public float3 Location;
}
