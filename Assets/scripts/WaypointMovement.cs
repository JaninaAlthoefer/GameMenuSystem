using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMovement : MonoBehaviour {

    //waypoints for the ai to visit in turn
    public Vector3[] waypoints;
    //distance when to switch to next waypoint
    public float closeEnoughDistance;
    //movement speed of the character
    public float movementSpeed;
    //index to provide the current waypoint from
    private int activeWaypoint = 0;
    //get rigidbody to influence movement
    private Rigidbody rBody;

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();

        //activate movement on activation
        //get the direction from active waypoint and own position
        Vector3 direction = (waypoints[activeWaypoint] - transform.position).normalized;
        //use speed, direction and time to manipulate own position
        transform.position += movementSpeed * Time.deltaTime * direction;

		
	}
	
	// Update is called once per frame
	void Update () {
        //get the direction from active waypoint and own position
        Vector3 direction = (waypoints[activeWaypoint]-transform.position).normalized;
        //use speed, direction and time to manipulate own position
        transform.position += movementSpeed * Time.deltaTime * direction;
        // rotate model to face where the character is going
        transform.forward = -direction;

        //if the character is close enough to the active waypoint change waypoint
        if (CloseEnough())
        {
            //change to next waypoint by adding 1 and reducing the number into the array range
            activeWaypoint = (activeWaypoint + 1) % waypoints.Length;
        }
    }

    //function to determine if next waypoint should be activated
    bool CloseEnough()
    {   //if the distance between character and waypoint is smaller than the specified amount
        //enable switching of waypoints
        if (Vector3.Distance(transform.position, waypoints[activeWaypoint]) < closeEnoughDistance)
        {
            //Debug.Log("new waypoint");
            return true;
        }

        //if the distance is greater, do not enable switching
        return false;
    }
}
