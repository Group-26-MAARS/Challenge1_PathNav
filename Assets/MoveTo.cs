// MoveTo.cs
using UnityEngine;
using UnityEngine.AI;

public class MoveTo : MonoBehaviour
{

    public Transform goalFromUnity; // Gets updated from SharedAnchorDemoScript
    public Transform initial;

    public static Transform goal;

    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        goal = goalFromUnity; // This needs to be changed and use config file
        // Not necessary to set agent's destination here since it is set in Update()
    }
    public static void setGoal(Transform newGoal)
    {
        goal = newGoal; // From newly located spatial anchor
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
        }
        else
        {
            Debug.Log("Agent is not on nav mesh (in update)");
        }

    }
}