using System;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class LevelManager : MonoBehaviour
{
    public static Action<int, int> UnitCountUpdated { get; set; } = delegate { };

    private EntityManager _entityManager;
    private float _timeSinceLastUpdate;

    private void Awake()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (_timeSinceLastUpdate > 0)
        {
            _timeSinceLastUpdate -= Time.deltaTime;
            return;
        }
        _timeSinceLastUpdate = 1f;
        int alliedUnits = 0;
        int enemyUnits = 0;

        EntityQuery query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TeamData>());
        using (NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob))
        {
            foreach (var entity in entities)
            {
                TeamData teamData = _entityManager.GetComponentData<TeamData>(entity);
                if (teamData.Value == 0)
                {
                    alliedUnits++;
                }
                else if (teamData.Value == 1)
                {
                    enemyUnits++;
                }
            }
        }

        UnitCountUpdated(alliedUnits, enemyUnits);

        query.Dispose();

        if (enemyUnits == 0)
        {
            Debug.Log("Allied units win!");
        }
        else if (alliedUnits == 0)
        {
            Debug.Log("Enemy units win!");
        }
    }
}
