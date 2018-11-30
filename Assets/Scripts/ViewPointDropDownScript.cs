using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewPointDropDownScript : MonoBehaviour {

    // This class is attached to a dropdown menu

    public PlaySinglePlayer launchGameScript;
    private Dropdown dropdown;

	void Start ()
    {
        dropdown = GetComponent<Dropdown>();
	}

    // This method is activated by selecting something from the dropdown menu
    // It informs the PlaySinglePlayer class about the player's chosen
    // camera mode
    public void ViewPointSelectionChange()
    {
        if (dropdown.value == 1) launchGameScript.firstPerson = false;
        else if (dropdown.value == 0) launchGameScript.firstPerson = true;

    }

}
