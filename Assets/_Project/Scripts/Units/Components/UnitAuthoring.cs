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
            AddComponent(entity, new Team { Value = authoring.Team });
            AddComponent(entity, new Movement {
                MovementSpeed = authoring.MovementSpeed,
                Target = float3.zero,
                DesiredVelocity = float3.zero,
                IsMoving = false
            });
            AddComponent(entity, new Avoidance { SeparationDistance = authoring.SeparationDistance });
            AddComponent(entity, new Health { Value = authoring.Health });
            AddComponent(entity, new AttackData { Strength = authoring.AttackStrength, Range = authoring.AttackRange, IsAttacking = false });
        }
    }
}

public struct Team : IComponentData
{
    public int Value;
}

public struct Movement : IComponentData
{
    public float MovementSpeed;
    public float3 Target;
    public float3 DesiredVelocity;
    public bool IsMoving;
    public float Velocity;
}

public struct Avoidance : IComponentData
{
    public float SeparationDistance;
}

public struct Health : IComponentData
{
    public float Value;
}

public struct AttackData : IComponentData
{
    public float Strength;
    public float Range;
    public bool IsAttacking;
}
