using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchNameInputFieldscript : MonoBehaviour {

    /*
     * This script updates the player's input from the MatchNameInputField to 
     * the CustomNetworkManager.
     */

    public GameObject networkManager;
    private Text playerText;
    private CustomNetworkManager networkScript;

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager");

        networkScript = networkManager.GetComponent<CustomNetworkManager>();

        playerText = GetComponentInChildren<Text>();
    }

    public void ChangeMatchName()
    {
        string matchName = playerText.text;
        networkScript.MatchName = matchName;
    }
}
