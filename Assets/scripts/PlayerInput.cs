using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    //reference to the pause screen thats supposed to pop up when pause button is pressed
    public GameObject pauseScreen;
    //reference to camera to get movement directions from
    public Transform playerCamera;
    //player movement speed, in editor uses a slider in the range between 0-10
    [Range(0, 10)]
    public float movementSpeed = 3;
    
    //reference to the script of the pausemenu to set the paused flag 
    //paused flag is used to keep menu popping up when pause button is pressed
    private MenuScript menuScript;

    //flag to indicate jump button was pressed, to heighten response time
    private bool jumpPressed;
    //flag to see if horizontal or vertical axis buttons were pressed
    private bool hPressed, vPressed;
    //flag to see if the button to pick up items was pressed, held or released
    private bool pickupPressed, pickupHeld, pickupReleased;
    //flag to see if the button to spawn cubes was pressed, held or released
    private bool spawnPressed, spawnHeld, spawnReleased;
    
    //force to make the player character jump with
    [Range(0, 10)]
    public float jumpForce = 5f;
    //flags for jumping, to restrain infinite jumps
    private bool isGrounded = true;
    //reference to characters rigidbody to affect physics while jumping
    private Rigidbody rBody;
    //animator to change the animation
    private Animator anim;

    //distance for the raycast to check if the player can pick up an item that's in front of them
    [Range(0, 20)]
    public float itemCheckDistance = 1f;
    //speed for item to move around the screen
    [Range(0,2)]
    public float itemSpeed = 1f;
    [Range(0, 5f)]
    public float itemPosOffset = 1f;
    //gameobject to be picked up and moved, null if nothing is moved or nothing can be picked up immediately
    private GameObject item = null;
    //keep 2D offset for spawning items
    private float itemOffsetX = 0, itemOffsetY = 0;

    //flag to enable player to spawn items
    private bool canSpawnItems = false;
    //delay for destroying the spawned cubes
    [Range(10,30)]
    public float stepCubeDelay = 10f;
    //image on hud for spawner 
    public GameObject spawnerHUDImage;
    //reference to prefabs for spawning the cubes
    public GameObject stepCubeTranslucent, stepCubeFinal;
    //reference for the interim cube to be moved and destroyed
    private GameObject interimCube;
    //position of the interim cube and later actual cube
    private Vector3 interimCubePos;

    ////audio source to play audio from for picking up items and spawning items and for footsteps
    //private AudioSource feedbackAS, footstepAS;
    ////audio mixer group that contains the sfx
    //public UnityEngine.Audio.AudioMixerGroup sfxMixer;
    ////audio mixer snapshots to switch inbetween when spawning and not spawning items and pause game
    //public UnityEngine.Audio.AudioMixerSnapshot defaultSnap, spawnSnap, pausedSnap;
    ////audio clips to be used for pickup (press, hold, release) and footstep sfx
    //public AudioClip[] pickupClips, footstepClips;
    ////audio clips to be used for spawning
    //public AudioClip spawningClip;

    ////amount of butterflies collected, this usually would be in a different script, but this is a mess anyways
    //private int numberButterflies = 0;
    ////reference to HUD text of numbers for butterflies gotten
    //public UnityEngine.UI.Text butterfliesText;
    ////reference to textbox in middle of canvas
    //public UnityEngine.UI.Text infoText;

    //global reference for computed moving direction
    private Vector3 directionN = Vector3.zero;

    // Use this for initialization
    void Start () {
        //get the reference to the menuscript from the gameobject
        menuScript = GameObject.FindGameObjectWithTag("MenuObject").GetComponent<MenuScript>() ;
        //get rigidbody for physics manipulation of the player character
        rBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        ////get two audio sources to play sounds from
        //feedbackAS = gameObject.AddComponent<AudioSource>();
        //feedbackAS.outputAudioMixerGroup = sfxMixer;
        //footstepAS = gameObject.AddComponent<AudioSource>();
        //footstepAS.outputAudioMixerGroup = sfxMixer;

        //butterfliesText.text = "0 / " + ApplicationModel.maxButterflies;
        ////show text for 2 seconds
        //StartCoroutine(showInfoText("Where am I?\n\nWhy can I pick up some rocks?", 2));
    }
	
	// Update is called once per frame
	void Update ()
    { 
        //set all button press flags in update
        jumpPressed = Input.GetButtonDown("Jump");
        hPressed = Input.GetButton("Horizontal");
        vPressed = Input.GetButton("Vertical");
        pickupPressed = Input.GetButtonDown("Item Pickup");
        pickupHeld = Input.GetButton("Item Pickup");
        pickupReleased = Input.GetButtonUp("Item Pickup");
        spawnPressed = Input.GetButtonDown("Spawn Item");
        spawnHeld = Input.GetButton("Spawn Item");
        spawnReleased = Input.GetButtonUp("Spawn Item");

        if (Input.GetKeyDown(KeyCode.Y))
            canSpawnItems = true;
        //pause game when pause button is pressed
        //show pause menu
        if (Input.GetButtonDown("Pause"))
        {
            if (!pauseScreen.activeSelf && !menuScript.GetPaused())
            {
                //pause game
                Time.timeScale = 0;
                //show pause menu screen
                pauseScreen.SetActive(true);
                //set game paused flag
                menuScript.SetPaused(true);
                //lower all volumes
                //pausedSnap.TransitionTo(0.1f);
            }
            else
            {
                //hide menu
                menuScript.DisableMenu();
                //unpause game
                Time.timeScale = 1;
                //set pausegame flag
                pauseScreen.SetActive(false);
                //reset sounds do default values
                //defaultSnap.TransitionTo(0f);
            }
        }

        //start of moving items around
        if (pickupPressed)
        {
            //play random picking up sound effect
            //int index = UnityEngine.Random.Range(0,pickupClips.Length-2);
            //feedbackAS.PlayOneShot(pickupClips[index]);
            //Debug.Log("press");
            //check if there is an item to be picked up in front of the player 
            //GetPickupItemStatus();
        }
        //end moving items around
        else if (pickupReleased)
        {
            //Debug.Log("release");
            //enable gravity (apparently this throws a nullreference error but it still works so I'm just gonna keep it)
            item.GetComponent<Rigidbody>().useGravity = true;
            //set item to null and release from movement
            item = null;
        }

        //start spawning item
        if (spawnPressed && canSpawnItems)
        {
            //lowpass filter all sounds not sfx by applying snapshot to audiomixer
            //spawnSnap.TransitionTo(0f);

            //Debug.Log("spawn press");
            //get location to spawn the interim cube in 
            interimCubePos = transform.position + itemPosOffset * transform.forward;
            interimCubePos.y += 0.8f;
            //spawn the translucent cube at set loaction
            interimCube = Instantiate(stepCubeTranslucent, interimCubePos, Quaternion.identity);
        }
        //spawning item, end
        else if (spawnReleased && canSpawnItems)
        {
            //reset audiomixer from lowpass filters
            //defaultSnap.TransitionTo(0f);
            //feedbackAS.PlayOneShot(spawningClip);

            //Debug.Log("spawn release");
            //destroy translucent cube and reset reference
            Destroy(interimCube);
            interimCube = null;
            itemOffsetX = 0;
            itemOffsetY = 0;

            //spawn cube and give delay on destruction, no actual reference for the object needed afterwards
            GameObject gObj = Instantiate(stepCubeFinal, interimCubePos, Quaternion.identity);
            //delay destruction of cube by x seconds
            Destroy(gObj, stepCubeDelay);
        }

    }

    //FixedUpdate is called in fixed intervalls if need be more than once before the next frame
    //accomodates the physics evaluation needed for collisions and rigidbody in general
    private void FixedUpdate()
    {
        //Movement Input for player
        Vector3 camForward = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up); //since camera can be at an angle, project movement onto plane that has normal vector.up
        Vector3 camRight = playerCamera.transform.right;

        //normalised vector to update position with
        //Vector3 directionN = Vector3.zero;

        if (hPressed || vPressed)
        {
            //if player is not spawning items, move player 
            if (!spawnHeld)
            {
                //get amount of left/right & forward/backward buttons are pressed
                //use the amount to influence character movement vectors
                Vector3 goRight = Input.GetAxis("Horizontal") * camRight;
                Vector3 goAhead = Input.GetAxis("Vertical") * camForward;

                //Debug.Log(goRight + "   " + goAhead);

                //normalise direction to get constant speed in all directions
                directionN = (goRight + goAhead).normalized;

                //apply movement to player position with movement speed and deltatime to achieve smooth momenet across 
                //multiple performances
                transform.position += movementSpeed * Time.fixedDeltaTime * directionN;
                //rotate player character to match camera rotation
                transform.rotation = Quaternion.Euler(0, playerCamera.rotation.eulerAngles.y, 0);
            }   
            
        }
        else
        {
            directionN = Vector3.zero;
        }

        //if player is on ground and jump is pressed
        if (isGrounded && jumpPressed)
        {   
            //use movement direction and jumpforce to give upward movement to the character
            rBody.velocity = new Vector3(directionN.x, jumpForce, directionN.z);
            //player character is not on ground anymore
            isGrounded = false;
        }

        
        //move items around if button is held
        //if (pickupHeld)
        //{
        //    //move the picked up item when the player moves
        //    if (item != null)
        //    {
        //        //repeat dragging sound to signify dragging an item
        //        if (!feedbackAS.isPlaying)
        //        {
        //            feedbackAS.PlayOneShot(pickupClips[4]);
        //        }
        //        //get a new position to change the items position values
        //        Vector3 itemTrans = new Vector3(transform.position.x, transform.position.y + 0.8f, transform.position.z);
        //        //add a little offset in front of the character to make it look like something was picked up
        //        item.transform.position = itemTrans + itemPosOffset * transform.forward;
        //    }
        //}

        ////while choosing spot to spawn item
        //if (spawnHeld)
        //{
        //    //move placeholder cube to determine spawn location
        //    if (interimCube != null && canSpawnItems)
        //    {
        //        //get amount of left/right & forward/backward buttons are pressed
        //        //use the amount to influence character movement vectors
        //        float goRight = Input.GetAxis("Horizontal");
        //        float goUp = Input.GetAxis("Vertical");
        //        float goFurther = Input.GetAxis("Camera Vertical");

        //        //normalize to average speed
        //        //Vector3 cubeDirection = (goRight + goUp).normalized;
        //        itemOffsetX += goRight * itemSpeed * Time.deltaTime;
        //        itemOffsetY += goUp * itemSpeed * Time.deltaTime;
        //        itemPosOffset += goFurther * itemSpeed * Time.deltaTime;
        //        //rotate and move cube position in regards to player 2D
        //        interimCubePos = transform.position + itemPosOffset * transform.forward + itemOffsetX * transform.right + itemOffsetY * transform.up;
        //        //adjust for pivot point of player character, gives floaty movement if += offset is used to complete cmputation 
        //        //needs to be done here
        //        interimCubePos.y += 0.8f;

        //        //update translucent cube position
        //        interimCube.transform.position = interimCubePos;
        //    }
        //}

        //update animation
        UpdateAnimator();
    }

    //general onCollisionEnter function for all collisions
    private void OnCollisionEnter(Collision collision)
    {
        //is colliding with objects tagged as ground
        if (collision.gameObject.tag == "Ground")
        {
            //reset flag to give back ability to jump
            //Debug.Log("OnGround");
            isGrounded = true;
            rBody.velocity = Vector3.zero;
        }
        //if velocity is 0 or downward and landing on movable object, land
        else if (collision.gameObject.tag == "Pickup" && rBody.velocity.y <= 0)
        {
            //reset flag to give back ability to jump
            isGrounded = true;
            rBody.velocity = Vector3.zero;
        }
        //if the player collects a butterfly
        else if (collision.gameObject.tag == "Collectible")
        {
            //increase number gotten
            //increaseButterfliesGotten();
            //destroy collectible object
            Destroy(collision.gameObject);
        }
        //if the player reaches the spawner, enable item spawning
        else if (collision.gameObject.tag == "Spawner")
        {
            //show text for 2 seconds
            //StartCoroutine(showInfoText("Press Shift to spawn items", 2));
            //show the HUD image for the spawning ability
            spawnerHUDImage.SetActive(true);
            //set the flag to enable the player to spawn items
            canSpawnItems = true;
            Destroy(collision.gameObject);
        }
        //exit level if exit is collided with
        else if (collision.gameObject.tag == "Exit")
        {
            //show text for 2 seconds
            //StartCoroutine(showInfoText("Thanks for playing this demo", 2));

            //reset butterflies gotten
            //numberButterflies = 0;
            //reset spawning items ability
            canSpawnItems = false;
            spawnerHUDImage.SetActive(false);
            
            //load main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
            
    }

    //increase the number of butterflies gotten and display
    //private void increaseButterfliesGotten()
    //{
    //    //increase number
    //    numberButterflies++;
    //    //update HUD
    //    butterfliesText.text = numberButterflies + " / " + ApplicationModel.maxButterflies;
    //}

    //used from the ThirdPersonCharacter in Unity Standard Assets, removed a lot of animations and functionality since they
    //aren't implemented in this game control scheme
    //the proposed character model an animations from CA1 weren't importing correctly so the standard asset third person controller
    //was chosen to be the easily accessible replacement
    void UpdateAnimator()
    {
        //Debug.Log(directionN);

        //get forward movement speed using the same calculations as before but
        // increased in value to match animation to movement speed
        float forward = 20f * (movementSpeed * Time.deltaTime * directionN).magnitude;
        
        // update the animator parameters for forward movement in the blend weight for ground movement
        anim.SetFloat("Forward", forward, 0.1f, Time.deltaTime);
        // update the animator parameter for jumping behaviour, used to switch between airborne and ground movement
        anim.SetBool("OnGround", isGrounded);

        //if the character is in the air, update the animator to set the appropriate airborne animation
        if (!isGrounded)
        {
            //set the float value to use as blend weight in the animator
            anim.SetFloat("Jump", rBody.velocity.y);
        }

        //play footstep sounds if player is on ground, moving forward and a sound isn't already playing
        //if (isGrounded && !footstepAS.isPlaying && forward > 0)
        //{
        //    //get random index of sound clip array
        //    int index = UnityEngine.Random.Range(0, footstepClips.Length-1);
        //    //play sound at random index once
        //    footstepAS.PlayOneShot(footstepClips[index]);
        //}
    }

    //uses a raycast to determine whether the player character is on the ground and able to jump
    void GetPickupItemStatus()
    {
        //reference to information produced if raycast hit an object
        RaycastHit info;
        //adjust raycast position from bottom of character
        Vector3 transpos = transform.position;
        transpos.y += 0.8f;

        Debug.DrawLine(transpos, transpos + (transform.forward * itemCheckDistance), Color.red, 20f, false);

        //uses the bool return of the raycast to see if the raycast hit something in front of the player character
        if (Physics.Raycast(transpos, transform.forward, out info, itemCheckDistance))
        {
            if (info.collider.tag == "Pickup")
            {
                //reference found item as item to pickup
                item = info.collider.gameObject;
                //disable gravity to keep object from falling through floor
                item.GetComponent<Rigidbody>().useGravity = false;
                //Debug.Log(item);
            }

        }
    }
    
    //get flag value for player being able to spawn items
    //used in savegame
    public bool getCanSpawn()
    {
        return this.canSpawnItems;
    }

    //set the canSpawn flag after loading
    public void setCanSpawn(bool spawning)
    {
        canSpawnItems = spawning;
    }

    //set the HUD image for the spawner item to active/inactive
    //used when loading the game
    public void SetSpawnerImage(bool active)
    {
        spawnerHUDImage.SetActive(active);
    }

    //shows text in textfield for secs seconds
    //IEnumerator showInfoText (string text, int secs)
    //{
    //    //set text to the textfield
    //    //
    //    infoText.text = text;
    //    //do nothing for x amount of seconds
    //    yield return new WaitForSeconds(secs);
    //    //reset textfield to nothing
    //    //infoText.enabled = false;
    //    infoText.text = "";
    //}
}
