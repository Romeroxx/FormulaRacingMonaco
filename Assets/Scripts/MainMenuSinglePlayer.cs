using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSinglePlayer : MonoBehaviour
{

    // This method is activated by a button
    // It loads the mainmenu scene
    public void GoToMainmenu()
    {

        if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(0);
    }
}
