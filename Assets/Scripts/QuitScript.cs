using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitScript : MonoBehaviour {

    /*
     * This script disconnects from any servers and quits the game
     */

    public CustomNetworkManager networkManager;

    public void Quit()
    {
        networkManager.Disconnect();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
}
