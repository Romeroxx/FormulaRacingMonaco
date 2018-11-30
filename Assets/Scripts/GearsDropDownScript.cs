using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GearsDropDownScript : MonoBehaviour {

    /*
     * This script allows the player to changes the gears option of the game.
     */

    public CustomNetworkManager networkManager;

    private Dropdown dropdown;
	
	void Start () {
        dropdown = GetComponent<Dropdown>();
	}

    public void ChangeGearOption()
    {
        if (dropdown.value == 0) networkManager.useGears = false;
        else if (dropdown.value == 1) networkManager.useGears = true;
    }
}
