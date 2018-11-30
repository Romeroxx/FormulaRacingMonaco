using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHidingScript : MonoBehaviour {

    public GameObject inGameMenu;
    private bool menuActive = false;

    // This method hides/reveals the ingame menu if escape is clicked
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuActive)
            {
                menuActive = false;
                inGameMenu.SetActive(false);
            }
            else
            {
                menuActive = true;
                inGameMenu.SetActive(true);
            }
        }
    }
}
