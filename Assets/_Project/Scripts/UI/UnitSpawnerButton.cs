using UnityEngine;
using Unity.Entities;

public class UnitSpawnerButton : MonoBehaviour
{
    #region Public Methods
    public void SpawnUnit(int unitType)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(UnitSpawner));

        Entity unitSpawnerEntity = query.GetSingletonEntity();
        UnitSpawner unitSpawner = entityManager.GetComponentData<UnitSpawner>(unitSpawnerEntity);

        unitSpawner.UnitToSpawn = (UnitTypes)unitType;
        entityManager.SetComponentData(unitSpawnerEntity, unitSpawner);
    }
    #endregion
}
