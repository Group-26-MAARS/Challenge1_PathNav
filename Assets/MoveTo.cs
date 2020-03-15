// MoveTo.cs
using UnityEngine;
using UnityEngine.AI;

public class MoveTo : MonoBehaviour
{

    public Transform goal;
    public Transform initial;

    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
       
        // Not necessary to set agent's destination here since it is set in Update()
    }

    void Update()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent.isOnNavMesh)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        // Done
                        //Debug.Log("navigation complete");
                        agent.transform.position = initial.position;
                        agent.destination = goal.position;

                    }
                }
            }
            else
            {
                Debug.Log("Agent is not on nav mesh (in update)");
            }
        }

    }
}