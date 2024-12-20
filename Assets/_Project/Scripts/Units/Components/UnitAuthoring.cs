using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

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
                DesiredVelocity = 0,
                CurrentVelocity = 0,
                CurrentPathIndex = 0,
                SeparationDistance = authoring.SeparationDistance
            });
            AddComponent(entity, new HealthData { Value = authoring.Health });
        }
    }
}

public struct TeamData : IComponentData
{
    public int Value;
}

public struct MovementData : IComponentData
{
    public float MovementSpeed;
    public float3 Target;
    public bool IsMoving;
    public float DesiredVelocity;
    public float CurrentVelocity;
    public int CurrentPathIndex;
    public float SeparationDistance;
}

public struct HealthData : IComponentData
{
    public float Value;
}

public struct EnemyBaseReference : IComponentData
{
    public float3 Location;
}
