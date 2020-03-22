using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using moveTo = MoveTo;

public class ListOps : MonoBehaviour
{
    //public GameObject[] flags;

    public List<GameObject> flags;
    bool itemsAdded;
    int currentFlagNbr;


    // This script contains logic for the game objects (flags) that are used in path navigation
    // It also contains logic for timer. For now, after every x seconds, the arrow will transition from one
    // flag to the next.

    // Start is called before the first frame update
    void Start()
    {
        itemsAdded = false;
        flags = new List<GameObject>();

        // Load actual flag game objects into flags

        // THIS WILL FAIL IF the first one isn't found!!!


    }
    public void addFlag(GameObject newFlag)
    {
        if (newFlag != null)
        {
            flags.Add(newFlag);
            Debug.Log("********************************************added at location: " + newFlag.transform.position + ". Size is now" + flags.Count);

        }
    }
    public GameObject getNext()
    {
        if ((flags != null) && (flags.Count > 0))
        {
            GameObject newFlag = flags[0];
            flags.RemoveAt(0);
            return newFlag;
        }
        else
        {
            return null;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
