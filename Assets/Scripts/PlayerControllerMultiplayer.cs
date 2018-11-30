using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerControllerMultiplayer : NetworkBehaviour {
    
    /*
     * This script is used to let the player control their car in multiplayer.
     * It is also used turn of unnecessary GameObjects.
     */

    public CarControllerMultiplayer carController;

    public GameObject startCamera;
    public GameObject respawnpoint;
    public List<Material> materials;
    // Sync this players color with the other players
    [SyncVar] public int colorIndex = 10;
    public string carName = "Player";

    private Canvas textCanvas;
    private Rigidbody rbody;
    private Renderer carRenderer;

	
	void Start () {
        carController = GetComponent<CarControllerMultiplayer>();

        textCanvas = GetComponentInChildren<Canvas>();

        rbody = GetComponent<Rigidbody>();

        // find the respawn point
        respawnpoint = GameObject.FindWithTag("Respawn");

        // turn of the start camera from this client
        startCamera = GameObject.FindWithTag("StartCamera");
        startCamera.GetComponentInChildren<Camera>().enabled = false;

        // turn of the start menu from this client
        Canvas startMenu = GameObject.Find("JoinMenuCanvas").GetComponent<Canvas>();
        startMenu.enabled = false;

        carRenderer = GetComponentInChildren<Renderer>();
        carRenderer.material = materials[colorIndex];

        CmdUpdateGearsOption(carName);
    }


    void FixedUpdate () {

        // Move only if the player has authority over this car.
        if (hasAuthority == false)
        {
            return;
        }

        // Use CarControllerMultiplayer to move the car according to the
        // player's inputs
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        float handbrake = Input.GetAxis("Jump");

        carController.Move(Horizontal, Vertical, Vertical, handbrake);

        if (Input.GetButtonDown("GearUp")) carController.ChangeGear(1);

        if (Input.GetButtonDown("GearDown")) carController.ChangeGear(-1);

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = respawnpoint.transform.position;
            transform.rotation = respawnpoint.transform.rotation;
            rbody.velocity = Vector3.zero;
        }

        textCanvas.enabled = true;

        carRenderer.material = materials[colorIndex];
    }

    // Update the gears option of the server to this player.
    [Command]
    private void CmdUpdateGearsOption(string carName)
    {
        CustomNetworkManager networkManager = FindObjectOfType<CustomNetworkManager>();
        GameObject player = GameObject.Find(carName);
        CarControllerMultiplayer playerCarController = player.GetComponent<CarControllerMultiplayer>();
        playerCarController.useGears = networkManager.useGears;
    }
}
