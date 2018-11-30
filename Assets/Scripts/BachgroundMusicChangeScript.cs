using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BachgroundMusicChangeScript : MonoBehaviour {

    /*
     * This script lets the player change the background music of the game.
     */

    public BackgroundMusicScript backgroundMusic;

    private Dropdown dropdown;

	void Start () {
        dropdown = GetComponent<Dropdown>();
	}

    public void AudioSelectionChange()
    {
        // check BackgroundMusicScript for details
        backgroundMusic.ChangeClip(dropdown.value);
    }
}
