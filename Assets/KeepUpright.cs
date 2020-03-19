using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUpright : MonoBehaviour
{
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Quaternion q = Quaternion.FromToRotation(transform.up, vector3.up) * transform.rotation;

        //transform.rotation = Quaternion.identity;

        transform.rotation = Quaternion.LookRotation(
        Vector3.ProjectOnPlane(Vector3.up, hit.normal),
        hit.normal
        );
    }
}
