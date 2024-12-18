using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity agentPrefab;
    private bool isPrefabLoaded = false;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        // Check if the prefab is loaded and spawn agents only once
        if (!isPrefabLoaded)
        {
            TryLoadPrefab();
        }
    }

    void TryLoadPrefab()
    {
        // Query the baked prefab entity from the SubScene
        EntityQuery prefabQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<AgentComponent>());

        if (prefabQuery.CalculateEntityCount() > 0)
        {
            agentPrefab = prefabQuery.GetSingletonEntity();
            isPrefabLoaded = true;

            SpawnAgents(1000); // Spawn agents once the prefab is loaded
        }
        else
        {
            Debug.Log("Prefab not loaded yet, waiting...");
        }

        prefabQuery.Dispose();
    }

    void SpawnAgents(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Entity agent = entityManager.Instantiate(agentPrefab);

            // Set random spawn positions
            float3 spawnPosition = new float3(
                UnityEngine.Random.Range(-10f, 10f),
                0f,
                UnityEngine.Random.Range(-10f, 10f)
            );

            entityManager.SetComponentData(agent, LocalTransform.FromPosition(spawnPosition));
        }

        Debug.Log($"{count} agents spawned.");
    }
}

