using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using moveTo = MoveTo;


public class CaptureDistance : MonoBehaviour
{
    bool initialGoalIsSet;
    // Start is called before the first frame update
    void Start()
    {
        // Comment this out when using on real device
        beginNavigation();
    }

    void beginNavigation() // Called either from Start (if just using Unity) or from UI button
    {
        if (initialGoalIsSet == false)
        {
            if ((GameObject.Find("arrow").GetComponent<moveTo>() == null) || (GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>() == null))
            {
                print("arrow or flag list are null in Capture Distance");
                return;
            }
            else // Otherwise, get first flag from the list and set it as destination
            {
                GameObject myGameObject = GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>().getNext();
                GameObject.Find("arrow").GetComponent<moveTo>().setGoal(myGameObject.transform, myGameObject.name);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (MoveTo.goal) // Current goal from MoveTo
        {
            float dist = Vector3.Distance(MoveTo.goal.transform.position, transform.position);
            //print("Distance to goal: " + dist);

            if (dist < 1.5) // Made it to current goal. Update Goal
            {
                //GameObject.Find("arrow").GetComponent<moveTo>().setGoal(ListOps.getNext().transform);
                GameObject updatedGoal = GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>().getNext();
                if (updatedGoal != null)
                    GameObject.Find("arrow").GetComponent<moveTo>().setGoal(updatedGoal.transform, updatedGoal.name);
                else
                {
                    GameObject.Find("arrow").GetComponent<moveTo>().setGoal(null, "");
                    // Remove Arrow
                    Destroy(GameObject.Find("arrow"));
                    print("All goals have been reached in Path Navigation!");

                    // This is when next phase of Challenge 1 needs to be called
                }
                //moveTo.goal = ListOps.getNext().transform;
                print("Distance is " + dist + ". Updating goal!");

            }

        }
    }
}
