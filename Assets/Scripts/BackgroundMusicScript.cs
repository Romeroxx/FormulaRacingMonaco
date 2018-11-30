using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicScript : MonoBehaviour {

    /*
     * This script plays the background music of the game.
     */

    public List<AudioClip> audioClips; 

    private AudioSource audioSource;
	
	void Start () {

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClips[0];
        audioSource.volume = 0.35f;
        audioSource.loop = true;
        audioSource.dopplerLevel = 0;

        audioSource.Play();
    }

    // This method is called if the player changes the value of the 
    // MusicDropDown.
    public void ChangeClip(int index)
    {
        if (index == 1) audioSource.Stop();
        else { 
            audioSource.Stop();
            audioSource.clip = audioClips[index];
            audioSource.Play();
        }
    }
}
