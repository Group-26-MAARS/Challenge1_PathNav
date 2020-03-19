// MoveTo.cs
using UnityEngine;
using UnityEngine.AI;

public class MoveTo : MonoBehaviour
{

    //public Transform goalFromUnity; // Gets updated from SharedAnchorDemoScript
    public GameObject mainCamera;


    public static Transform goal;

    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        Debug.Log("NavMesh agent is created");

        //goal = goalFromUnity; // This needs to be changed and use config file
        // Not necessary to set agent's destination here since it is set in Update()
    }
    public void setGoal(Transform newGoal)
    {
        goal = newGoal; // From newly located spatial anchor.
        Debug.Log("goal is set");
    }

    void Update()
    {
        //transform.LookAt(goal);

        if ((transform != null) && (goal != null))
        {
            var offset = transform.position - goal.transform.position; // Look at destination
            transform.LookAt(transform.position + offset);
        }


        GameObject usersGameObj;

        if (goal != null)
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
                            usersGameObj = mainCamera;
                            if (usersGameObj == null)
                                Debug.Log("usersGameObj is null");
                            else
                            {
                                Transform myRealPosition = usersGameObj.transform;
                            //agent.transform.position = mainCamera.transform.position;

                                agent.transform.position = myRealPosition.position;
                                Debug.Log("new position" + myRealPosition.position);
                                agent.destination = goal.position;
                            }

                        }
                    }
                }
            }
            else
            {
                Debug.Log("Agent is not on nav mesh (in update)");
                // Jump back to camera and try to get back on nav mesh
                // Done
                //Debug.Log("navigation complete");
                usersGameObj = mainCamera;
                if (usersGameObj == null)
                    Debug.Log("usersGameObj is null");
                else
                {
                    Transform myRealPosition = usersGameObj.transform;

                    agent.transform.position = myRealPosition.position;
                    Debug.Log("new position" + myRealPosition.position);
                }
            }
        }
    }
}