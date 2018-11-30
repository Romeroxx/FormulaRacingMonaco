using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectionScript : MonoBehaviour {

    /*
     * This script is used to change the player's car's color according to the
     * player's choice. This script is used in multiplayer
     */

    public int materialIndex = 5;

    public Renderer startSceneCar;

    public List<Material> materials;

    private Dropdown dropdown;

    private void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    public void ColorSelectionChange()
    {
        // Updating the start scene's car's color to show the player a review
        // of their color.
        startSceneCar.material = materials[dropdown.value];

        materialIndex = dropdown.value;
    }
}
