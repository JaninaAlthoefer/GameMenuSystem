using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour {

    //reference to audio source
    private AudioSource audio;
    //flag to play music when in area
    private bool playMusic = false;

	// Use this for initialization
	void Start ()
    {
        //get audio source component
        audio = GetComponent<AudioSource>();	
	}

    //start playing music when player enters trigger
    private void OnTriggerEnter(Collider other)
    {
        //if triggered by player
        if (other.gameObject.tag == "Player")
        {
            //start playing music
            playMusic = true;
        }
    }

    //stop playing music when player exits trigger
    private void OnTriggerExit(Collider other)
    {
        //if triggered by player
        if (other.gameObject.tag == "Player")
        {
            //stop audio playing
            //audio.Stop();
            //stop audio from playing
            playMusic = false;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        //keep playing the music when in area
	    if (playMusic && !audio.isPlaying)
        {
            //plays the clip attached
            audio.Play();
        }
	}
}
