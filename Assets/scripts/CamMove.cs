using UnityEngine;
using System.Collections;

public class CamMove : MonoBehaviour {

    //player character to orbit around
    public GameObject camTarget;
    // radius for camera to rotate around target
    public float radiusTarget;
    //angles to determine rotation around Y and X axis
    private float azim = 0.0f, elev = 40.0f;

	// Use this for initialization
	void Start ()
    {
        //minimum radius
        if (radiusTarget < 1.0f)
            radiusTarget = 1.0f;

        //set starting angles for camera orbit
        float azimAngle = Mathf.Deg2Rad * (azim - 90.0f);
        float elevAngle = Mathf.Deg2Rad * (elev - 20.0f);

        //starting position of player character
        Vector3 playerPos = camTarget.transform.position;

        //new position of camera depending on radius, angles and player position
        Vector3 camPos = new Vector3(radiusTarget * Mathf.Cos(azimAngle), radiusTarget * Mathf.Sin(elevAngle), radiusTarget * Mathf.Sin(azimAngle));

        //set position and rotation via lookat target
        transform.position = camPos;
        transform.LookAt(playerPos);
        
	}
	
	// LateUpdate is called once per frame, after the Update() function
	void LateUpdate ()
    {
        //update the angels for the camera orbit
        UpdateCamPosition();

        //adjust angles for camera rotation
        float azimAngle = Mathf.Deg2Rad * (azim - 90.0f);
        float elevAngle = Mathf.Deg2Rad * (elev - 20.0f);
        //get player position to update camera position
        Vector3 playerPos = camTarget.transform.position;
        //get the difference for the camera to adjust by
        Vector3 deltaCamPos = new Vector3(radiusTarget * Mathf.Cos(azimAngle), radiusTarget * Mathf.Sin(elevAngle), radiusTarget * Mathf.Sin(azimAngle));
        //adjust camera position by the difference
        Vector3 camPos = playerPos + deltaCamPos;
        //set new position for camera
        transform.position = camPos;

        //adjusts camera position if there is a wall inbetween camera and target
        //WallHitAdjust(); unused, jerking movement when hitting something even though player character is set to ignore raycasts

        //set new rotation using the lookat target function
        transform.LookAt(playerPos + Vector3.up);

        //adjust target rotation when camera moves
        Vector3 rotationTarget = transform.rotation.eulerAngles;
        rotationTarget.x = camTarget.transform.rotation.x;
        rotationTarget.z = camTarget.transform.rotation.z;
        camTarget.transform.rotation = Quaternion.Euler(rotationTarget);

	}

    //function to update the angle values used in the camera orbit updating
    void UpdateCamPosition()
    {
        //get the input of the player to change the angles by
        float h = Input.GetAxis("Camera Horizontal");
        float v = Input.GetAxis("Camera Vertical");
        
        //add amount of input to angles 
        azim += 100 * h * Time.deltaTime;
        elev += 100 * v * Time.deltaTime;

        //if y rotation is complete, start anew
        if (azim > 360.0f)
            azim = 0.0f;

        //clamp x rotation
        elev = Mathf.Clamp(elev, 0, 110);
    }

    //use raycast to determine if there is something blocking the path
    void WallHitAdjust()
    {
        //info about possible hits
        RaycastHit hit;
        //vector direction to use with the raycast
        Vector3 direction = transform.position - camTarget.transform.position;

        //do raycast from player to camera
        if (Physics.Raycast(camTarget.transform.position, direction, out hit, direction.magnitude))
        {
            //new position if raycast hit obstruction
            transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
    }
}
