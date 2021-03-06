﻿// MoveTo.cs
using UnityEngine;
using UnityEngine.AI;

public class MoveTo : MonoBehaviour
{

    //public Transform goalFromUnity; // Gets updated from SharedAnchorDemoScript
    public GameObject mainCamera;


    public static Transform goal;

    void Start()
    {
        this.transform.localScale = new Vector3(0, 0, 0);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        Debug.Log("NavMesh agent is created");

        //goal = goalFromUnity; // This needs to be changed and use config file
        // Not necessary to set agent's destination here since it is set in Update()
    }
    public void setGoal(Transform newGoal, string gameObjectName)
    {
        if (!gameObjectName.Equals(""))
            Debug.Log("goal is set for " + gameObjectName);

        goal = newGoal; // From newly located spatial anchor.
    }

    void Update()
    {
        //transform.LookAt(goal);

        if ((transform != null) && (goal != null))
        {
            //var offset = transform.position - goal.transform.position; // Look at destination
            var offset = transform.position - goal.transform.position; // Look at destination
            transform.LookAt(transform.position + offset);
            transform.Rotate(80, 0, 0);

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
                            //usersGameObj = mainCamera;
                            //usersGameObj = Camera.main.transform.position;
                            if (Camera.main == null)
                                Debug.Log("usersGameObj is null");
                            else
                            {
                                Transform myRealPosition;
#if !UNITY_EDITOR
                                myRealPosition = Camera.main.transform;
#elif UNITY_EDITOR
                                myRealPosition = mainCamera.transform;
#endif
                                agent.transform.position = myRealPosition.position;
                                Debug.Log("new position" + myRealPosition.position);
                                agent.destination = goal.position;
                            }

                        }
                    }
                }
            }
            else // If not on navmesh, just update the agent's position to the camera's new position and try again. 
            {
                Debug.Log("Agent is not on nav mesh (in update)");
                // Jump back to camera and try to get back on nav mesh
                // Done
                //Debug.Log("navigation complete");
                //usersGameObj = Camera.main.transform.position;
                if (Camera.main == null)
                    Debug.Log("usersGameObj is null");
                else
                {
                    Transform myRealPosition;
#if !UNITY_EDITOR

                    myRealPosition = Camera.main.transform;
#elif UNITY_EDITOR
                    myRealPosition = mainCamera.transform;
#endif

                    agent.transform.position = myRealPosition.position;
                //    Debug.Log("new position" + myRealPosition.position);
                }
            }
        }
    }
}