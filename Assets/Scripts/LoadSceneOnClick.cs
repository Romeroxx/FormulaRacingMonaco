using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour {

    // This method is activated by a button
    // It loads the scene designated by the given index
    // 0 == mainmenu
    // 1 == singleplayer
    // 2 == multiplayer
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
