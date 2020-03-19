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
        if (initialGoalIsSet == false)
        {
            if ((GameObject.Find("arrow").GetComponent<moveTo>() == null) || (GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>() == null))
            {
                print("moveTo is null in Capture Distance");
                return;
            }
            else
                GameObject.Find("arrow").GetComponent<moveTo>().setGoal(GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>().getNext().transform);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (initialGoalIsSet == false)
        {
            if ((GameObject.Find("arrow").GetComponent<moveTo>() == null) || (GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>() == null))
            {
                print("moveTo is null in Capture Distance");
                return;
            }
            else
            {
                GameObject.Find("arrow").GetComponent<moveTo>().setGoal(GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>().getNext().transform);
                initialGoalIsSet = true;
            }
        }
            if (MoveTo.goal) // Current goal from MoveTo
        {
            float dist = Vector3.Distance(MoveTo.goal.transform.position, transform.position);
            print("Distance to goal: " + dist);
            
            if (dist < 1.5) // Made it to current goal. Update Goal
            {
                //GameObject.Find("arrow").GetComponent<moveTo>().setGoal(ListOps.getNext().transform);
                GameObject updatedGoal = GameObject.Find("listOfFlagsGameObj").GetComponent<ListOps>().getNext();
                if (updatedGoal != null)
                    GameObject.Find("arrow").GetComponent<moveTo>().setGoal(updatedGoal.transform);
                else
                {
                    GameObject.Find("arrow").GetComponent<moveTo>().setGoal(null);
                    // Remove Arrow
                    Destroy(GameObject.Find("arrow"));
                    print("All goals have been reached in Path Navigation!");
                }
                //moveTo.goal = ListOps.getNext().transform;
                print("Distance is " + dist + ". Updating goal!");

            }

        }
    }
}
