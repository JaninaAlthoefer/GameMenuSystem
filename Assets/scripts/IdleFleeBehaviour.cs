using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleFleeBehaviour : MonoBehaviour {
    //the state the AI is actively in
    private AIState activeState = AIState.IDLE;
    //distance when changing target positions
    public float closeEnoughDistance = 1f;
    //original spawn position to use in AI
    private Vector3 spawnPos;
    //max radius for idle movement positions
    public float maxIdleMovementRadius;
    //target position to move to
    private Vector3 targetPosition;
    //movement speed
    public float movementSpeed = 0.5f;
    //gameobject to flee from
    private GameObject dangerObject;

    //nav mesh agent for better pathfinding
    private UnityEngine.AI.NavMeshAgent navAgent;

    
	// Use this for initialization
	void Start () {
        //init spawn position when game starts
        spawnPos = transform.position;
        //first position to go to
        targetPosition = spawnPos;
        //navagent reference to use in pathfinding
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        
	}
	
	// Update is called once per frame
	void Update () {
        //small state machine between idle and flee
        if (activeState == AIState.IDLE)
        {
            //if idle see if target position needs changing
            if (closeEnough(transform.position, targetPosition))
            {
                //new position computed through angle and radius around spawnpoint
                float angle = Random.Range(0f, 360f);
                float radius = Random.Range(0f, maxIdleMovementRadius);
                Vector3 target = spawnPos +  radius * new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0, -Mathf.Cos(Mathf.Deg2Rad * angle));
                //make target height same as character position height
                target.y = transform.position.y;
                targetPosition = target;
            } 
        }
        else
        {
            //if fleeing and object to flee from is set
            if (dangerObject != null)
            {
                //get direction in opposite direction from object to flee from
                targetPosition = transform.position - dangerObject.transform.position;
                //make target height character height
                targetPosition.y = transform.position.y;
            }
        }

        //get direction from targetposition and current position
        Vector3 direction = (targetPosition - transform.position).normalized;
        //set navagent to move character to target position
        navAgent.SetDestination(targetPosition);
        //should rotate the charactre toward direction moving in, seems to not work ?
        transform.forward = direction;

	}

    //determines whether the aI is close enough to point
    bool closeEnough(Vector3 position, Vector3 target)
    {
        //make sure target is same height as ai character
        target.y = position.y;

        //if distance between character and target position is less than specified
        if (Vector3.Distance(position, target) < closeEnoughDistance)
        {
            //change target positions
            //Debug.Log("new pos");
            return true;
        }
        else
        { 
            //don't change target positions
            return false;
        }
    }

    //when perception trigger is entered
    private void OnTriggerEnter(Collider other)
    {
        //if object that triggered event is player
        if (other.gameObject.tag == "Player")
        {
            //set object to flee from to player
            dangerObject = other.gameObject;
            //switch to fleeing state
            switchStates(false);
        }
    }

    //when perception trigger is exited
    private void OnTriggerExit(Collider other)
    {
        //if object that laft trigger is player
        if (other.gameObject.tag == "Player")
        {
            //switch to idle state
            switchStates(true);
            //reset object to flee from
            dangerObject = null;
        }
    }

    //function to switch between both AI states
    void switchStates( bool idle)
    {
        //if flag is set for idle state set active state to idle state
        if (idle)
        {
            //Debug.Log("idle");
            //set to idle state
            activeState = AIState.IDLE;
        }
        else // otherwise
        { 
            //Debug.Log("flee");
            //set active state to flee state
            activeState = AIState.FLEE;
        }
    }
}
 
//states for this AI to possibly be in
enum AIState
{
    //simple flee and idle states
    FLEE, IDLE
}