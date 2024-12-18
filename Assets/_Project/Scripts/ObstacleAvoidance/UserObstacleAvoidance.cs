using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.AI;
using UnityEngine.Rendering;


public class UserObstacleAvoidance : MonoBehaviour {

    public float separationDistance = 1f;
    private NavMeshAgent agent;
    private Vector3 goal;
    public float health;
    public float attackStrength;
    public bool isAttacking;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (goal != Vector3.zero)
        {
            agent.SetDestination(goal);
        }
        Collider[] nearbyFollowers = Physics.OverlapSphere(transform.position, separationDistance);
        foreach (Collider col in nearbyFollowers)
        {
            if (col.gameObject != this.gameObject && col.CompareTag("TeamMate"))
            {
                Vector3 awayFromFollower = transform.position - col.transform.position;
                agent.velocity += awayFromFollower.normalized * 0.05f;
            
            }
            if(col.gameObject != this.gameObject && col.CompareTag("Enemy"))
            {
                agent.velocity = Vector3.zero;
                isAttacking = true;
                col.gameObject.GetComponent<UserObstacleAvoidance>().isAttacking = true;
                if (health > col.gameObject.GetComponent<UserObstacleAvoidance>().attackStrength)
                {
                    Destroy(col.gameObject);
                    isAttacking = false;
                }
                else
                    Destroy(this.gameObject);
            }
            if (col.gameObject != null)
                isAttacking = false;
        }
    }

    public void SetGoal(Vector3 goalSet)
    {
        goal = goalSet;
    }

}
    

