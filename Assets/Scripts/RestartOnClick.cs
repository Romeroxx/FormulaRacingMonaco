using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnClick : MonoBehaviour {

    // This method is activated by a button
    // It restarts the singleplayer scene
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
}
