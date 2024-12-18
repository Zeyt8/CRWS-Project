using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class UserAgentManager : MonoBehaviour
{

    private List<int> agentIDs = new List<int>(); // List to store agent ID
    public GameObject agentPrefab; // Prefab for agents
    public BoxCollider goalArea;  // Goal area for agents
    public BoxCollider spawnArea;
    public GameObject floor;

    // Parameters for RVO simulation

    void Start()
    {
        SpawnAgents(1000);
    }

    void SpawnAgents(int numAgents)
    {
        for (int i = 0; i < numAgents; i++)
        {
            // Instantiate agent prefab
            GameObject agent = Instantiate(agentPrefab, GetRandomPositionInSpawnArea(), Quaternion.identity);
            agent.name = "Agent" + i;
            Vector3 goal = GetRandomGoal(goalArea);
            agent.GetComponent<UserObstacleAvoidance>().SetGoal(goal);
        }
    }

    Vector3 GetRandomGoal(BoxCollider goal)
    {
        // Generate a random position within the goal area
        Vector3 randomPosition = new Vector3(
            Random.Range(goal.bounds.min.x, goal.bounds.max.x),
            7.75f,
            Random.Range(goal.bounds.min.z, goal.bounds.max.z)
        );

        return randomPosition;
    }

    Vector3 GetRandomPositionInSpawnArea()
    {
        // Generate a random starting position within the spawn area
        Vector3 randomPosition = new Vector3(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            floor.transform.position.y + 1f,
            Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
        );

        return randomPosition;
    }

}
