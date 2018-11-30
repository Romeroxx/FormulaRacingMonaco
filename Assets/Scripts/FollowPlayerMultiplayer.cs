using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FollowPlayerMultiplayer : NetworkBehaviour {

    /*
     * This script is responsible for moving the camera behind the player.
     * It is very similar to FollowPlayer-script and most of the commentation 
     * is done in that script.
     */

    public GameObject Player;

    public string playerGameObjectName = "Player";

    public float cameraMoveSpeed;
    public float cameraRotationSpeed;
    public float freecameraSensitivityX;
    public float freecameraSensitivityY;
    public float freecameraRotationSpeed;

    public bool firstPerson;

    private bool freecamera = false;
    private float cameraModeChangeTime = 0f;

    private Camera playerCamera;
    private AudioListener playerAudioListener;
    private Rigidbody rbodyPlayer;

    void Start () {

        // Finding the right player from the multiplayer scebe.
        Player = GameObject.Find(playerGameObjectName);
        if (Player == null) { }
        else
        {
            rbodyPlayer = Player.GetComponent<Rigidbody>();
        }

        playerCamera = GetComponentInChildren<Camera>();

        playerAudioListener = GetComponentInChildren<AudioListener>();
	}
	
	
	void FixedUpdate () {

        // return if the the client doesn't have authority over this object.
        if (hasAuthority == false) return;

        // if player not yet found search for player
        if (Player == null)
        {
            Player = GameObject.Find(playerGameObjectName);
            rbodyPlayer = Player.GetComponent<Rigidbody>();
        }
        else
        {
            FollowTarget(Time.deltaTime);
        }
	}

    void FollowTarget(float deltaTime)
    {
        // enabling the player's main camera and it's audio listener
        playerCamera.enabled = true;
        playerAudioListener.enabled = true;

        if (!(deltaTime > 0)) return;

        var playerForward = Player.transform.forward;
        var playerUp = Player.transform.up;

        if (Input.GetButtonDown("FreeCamera"))
        {
            if (Time.time - cameraModeChangeTime > 0.5f)
            {
                cameraModeChangeTime = Time.time;
                if (freecamera)
                {
                    freecamera = false;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    freecamera = true;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }


        if (freecamera)
        {
            transform.position = Vector3.Lerp(transform.position, Player.transform.position + rbodyPlayer.velocity * 0.02f,
                                              deltaTime * cameraMoveSpeed);

            float mouseX = freecameraSensitivityX * Input.GetAxis("Mouse X");
            float mouseY = -freecameraSensitivityY * Input.GetAxis("Mouse Y");

            float currentX = transform.eulerAngles.x;
            float currentY = transform.eulerAngles.y;

            float nextX = 0.0f;
            float nextY = 0.0f;

            nextY = currentY + mouseX;
            nextX = currentX + mouseY;

            Quaternion rotation = Quaternion.Euler(nextX, nextY, 0.0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, deltaTime * freecameraRotationSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, Player.transform.position,
                                            deltaTime * cameraMoveSpeed);

            var rotation = Quaternion.LookRotation(playerForward, playerUp);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, deltaTime * cameraRotationSpeed);
        }
    }
}
