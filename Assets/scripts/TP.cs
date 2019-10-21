using UnityEngine;
using System.Collections;

public class TP : MonoBehaviour {

    //save spawn position to move character to that position when out of playing area
    private Vector3 spawnPosition;

	// Use this for initialization
	void Start ()
    {
        //set spawn position as position to teleport to
        Transform spawnTransform = transform;
        //modify position so that player lands on ground instead of just reappearing
        spawnPosition = new Vector3(spawnTransform.position.x, spawnTransform.position.y + 2, spawnTransform.position.z);
	}
    
    //called when entering trigger
    private void OnTriggerEnter(Collider other)
    {
        //if colliding with trigger that has tag "Death"
        if (other.gameObject.tag == "Death")
        {
            //respawn
            respawnPlayer();
        }

    }

    //called when exited any trigger
    private void OnTriggerExit(Collider other)
    {
        //if colliding with trigger that has tag "Death"
        if (other.gameObject.tag == "Death")
        {
            //respawn
            respawnPlayer();
        }
    }

    //teleport player to starting position
    //intentionally left out the rotation or changing isGrounded for more "dreamlike" effect
    void respawnPlayer()
    { 
        //reset player position and rotation
        transform.position = spawnPosition;
    }
}
