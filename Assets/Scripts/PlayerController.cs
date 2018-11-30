using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public CarController carController;

	
	void Awake () {
        carController = GetComponent<CarController>();
	}

    // This method reads the player's inputs and moves the player's car according
    // to those inputs. The player's car is moved by the CarController class and 
    // this method only calls the Move-method of that class. 
    // It also checks if the player wants to change gears. Gear changing is also
    // handled by the CarController class.
	void FixedUpdate () {

        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        float handbrake = Input.GetAxis("Jump");

        carController.Move(Horizontal, Vertical, Vertical, handbrake);

        if (Input.GetButtonDown("GearUp")) carController.ChangeGear(1);

        if (Input.GetButtonDown("GearDown")) carController.ChangeGear(-1);

    }
}
