using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaySinglePlayer : MonoBehaviour {

    public Canvas inGameCanvas;
    public Camera startCamera;

    public GameObject playerCamera3rd;
    public GameObject playerCamera1st;

    public List<Dropdown> dropdowns;
    public GameController controller;

    public bool firstPerson = false;
    public GameObject startMenuCanvas;

    public List<CarAudioScript> carAudios;

    // This method is activated by a button
    // It disables the startmenu and the camera which provided the background
    // for the startmenu. Then it activates 1st- or 3rd-person player camera
    // depending on the player's choices. It also activates the ingame menu
    // and starts the audio effects of the formula cars.
    public void StartTheGame()
    {
        startMenuCanvas.SetActive(false);
        inGameCanvas.enabled = true;
        startCamera.enabled = false;

        foreach (var dropdown in dropdowns)
        {
            dropdown.enabled = false;
        }

        if (firstPerson) playerCamera1st.SetActive(true);
        else playerCamera3rd.SetActive(true);

        controller.passedTime = Time.time + 3f;
        controller.inMenu = false;

        foreach (var carAudio in carAudios)
        {
            carAudio.StartSound();
        }
    }
}
