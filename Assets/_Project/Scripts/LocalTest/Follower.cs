using UnityEngine;
using UnityEngine.AI;

public class Follower : MonoBehaviour
{
    public Transform leader;
    public float followDistance = 3f;
    public float separationDistance = 2f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // following
        if (leader != null)
        {
            Vector3 targetPosition = leader.position - (leader.forward * followDistance);
            agent.SetDestination(targetPosition);
        }

        // seperation
        Collider[] nearbyFollowers = Physics.OverlapSphere(transform.position, separationDistance);
        foreach (Collider col in nearbyFollowers)
        {
            if (col.gameObject != this.gameObject && col.CompareTag("Follower"))
            {
                Vector3 awayFromFollower = transform.position - col.transform.position;
                agent.velocity += awayFromFollower.normalized * 0.05f;
            }
        }
    }
}
