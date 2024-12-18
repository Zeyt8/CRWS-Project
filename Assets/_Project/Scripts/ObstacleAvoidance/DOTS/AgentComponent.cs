using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public struct AgentComponent : IComponentData
{
    public float3 goal;
    public float health;
    public float attackStrength;
    public bool isAttacking;
}

public struct MovementComponent : IComponentData
{
    public float3 velocity;
}

public struct TeamTag : IComponentData { }
public struct EnemyTag : IComponentData { }