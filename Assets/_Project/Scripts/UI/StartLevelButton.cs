using Unity.Entities;
using UnityEngine;

public class StartLevelButton : MonoBehaviour
{
    public void StartLevel()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(Level));

        Entity levelEntity = query.GetSingletonEntity();
        Level level = entityManager.GetComponentData<Level>(levelEntity);

        level.HasStarted = true;
        entityManager.SetComponentData(levelEntity, level);
    }
}
