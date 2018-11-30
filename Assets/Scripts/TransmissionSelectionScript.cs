using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransmissionSelectionScript : MonoBehaviour {

    // This class is attached to a dropdown menu

    public CarController playerCarController;
    private Dropdown dropdown;

    private void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    // This method is activated by selecting something from the dropdown menu
    // It informs the player's car's CarController class about player's chosen
    // gears usage mode
    public void SelectionChange()
    {
        if (dropdown.value == 0) playerCarController.useGears = false;
        else if (dropdown.value == 1) playerCarController.useGears = true;
    }
}
