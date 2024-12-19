using Unity.Entities;
using Unity.Physics;

public partial struct GoalColliderInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GoalColliderEnemy>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Find the goal collider entity in the scene
        Entity goalColliderEntity = Entity.Null;
        foreach (var (collider, entity) in SystemAPI.Query<PhysicsCollider>().WithEntityAccess().WithAll<GoalColliderEnemy>())
        {
            goalColliderEntity = entity;
            break; // Assuming there's only one goal collider
        }

        // Now update all UnitSpawner entities with the found goal collider
        if (goalColliderEntity != Entity.Null)
        {
            // Find all entities with the UnitSpawner component and set their TargetColliderEntity
            foreach (var unitSpawnerEntity in SystemAPI.Query<RefRW<UnitSpawner>>())
            {
                // Access and modify the UnitSpawner component data
                unitSpawnerEntity.ValueRW.TargetColliderEntity = goalColliderEntity;
            }
        }

        // Optionally, disable the system if you only want this to run once
        state.Enabled = false;
    }
}



public struct GoalColliderSingleton : IComponentData
{
    public Entity GoalColliderEntity;
}

public struct GoalColliderEnemy : IComponentData
{

}