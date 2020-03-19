using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshGen : MonoBehaviour
{
    public NavMeshSurface surface;
    private bool navMeshIsCreated = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(buildNav());
    }

    // Update is called once per frame
    void Update()
    {
        // // Initially create Nav Mesh
        if ((!navMeshIsCreated))
        {
            surface.BuildNavMesh();
            navMeshIsCreated = true;
            Debug.Log("Building Navmesh first time");

        }

    }
    IEnumerator buildNav()
    {
        while (true)
        {
            yield return new WaitForSeconds(Convert.ToSingle(5.0)); // approx 1.5s

            surface.BuildNavMesh();
            Debug.Log("Rebulding Navmesh");
        }

    }
}