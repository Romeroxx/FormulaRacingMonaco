using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenuScript : MonoBehaviour {

    public CustomNetworkManager networkManager;

    // This method is activated by clicking an appropriate button in game
    // The method disconnects the player from any online games and loads the mainmenu scene
	public void GoToMainmenu()
    {
        networkManager.Disconnect();

        if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(0);
    }
}
