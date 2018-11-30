using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    /*
     * This script is responsible of moving the camera.
     * It also enables the free look feature
     */

    public Transform Player;
    public float cameraMoveSpeed;
    public float cameraRotationSpeed;
    public float freecameraSensitivityX;
    public float freecameraSensitivityY;
    public float freecameraRotationSpeed;

    public bool firstPerson;

    private bool freecamera = false;
    private float cameraModeChangeTime = 0f;

     private Rigidbody rbodyPlayer;

    void Start () {

        if (Player == null) return;

        rbodyPlayer = Player.GetComponent<Rigidbody>();

	}
	
	
	void FixedUpdate () {

        FollowTarget(Time.deltaTime);
	}

    // This method moves the camera after the player.
    // It also monitors the input on the 'freeCamera'-button and changes the
    // camera mode accordingly.
    void FollowTarget(float deltaTime)
    {
        if (!(deltaTime > 0)) return;

        var playerForward = Player.forward;
        var playerUp = Player.up;

        // Change camera mode if needed
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

        // if freeCamera mode is active turn the camera according to the
        // player's mouse movement.
        if (freecamera)
        {
            if (firstPerson == false)
            {
                // smoothly move camera according to the player's position
                transform.position = Vector3.Lerp(transform.position, Player.position
                        + rbodyPlayer.velocity * 0.02f, deltaTime * cameraMoveSpeed);
            }

            // get mouse input
            float mouseX = freecameraSensitivityX * Input.GetAxis("Mouse X");
            float mouseY = -freecameraSensitivityY * Input.GetAxis("Mouse Y");

            float currentX = transform.eulerAngles.x;
            float currentY = transform.eulerAngles.y;

            float nextX = 0.0f;
            float nextY = 0.0f;

            // calculate next angle
            nextY = currentY + mouseX;
            nextX = currentX + mouseY;

            Quaternion rotation = Quaternion.Euler(nextX, nextY, 0.0f);

            // smoothly move the camera to the new position
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                                        deltaTime * freecameraRotationSpeed);
        }
        else
        {
            // if the game is in first-person mode do nothing
            if (firstPerson) { }

            else
            {
                // smoothly move camera according to the player's position
                transform.position = Vector3.Lerp(transform.position, Player.position,
                                                deltaTime * cameraMoveSpeed);

                // calculate next angle
                var rotation = Quaternion.LookRotation(playerForward, playerUp);

                // smoothly move the camera to the new position
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                                                deltaTime * cameraRotationSpeed);
            }
        }
    }
}
