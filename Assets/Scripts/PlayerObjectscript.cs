using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerObjectscript : NetworkBehaviour {

    /*
     * This script initializes the player in multiplayer. It is called when the
     * PlayerObject is created by the NetworkManager.
     */

    public GameObject playerPrefab;
    public GameObject playerCameraPrefab;

    private ColorSelectionScript colorSelector;
    private string playerGOName = "Player";
    private int colorIndex = 12;


    void Start () {

        if (isLocalPlayer == false) return;

        // Setting a random name to the player's GameObject to find it later.
        playerGOName += Random.Range(0, 100);

        while (GameObject.Find(playerGOName) != null)
        {
            playerGOName = "Player" + Random.Range(0, 100);
        }

        // Setting the player selected color to the car.
        colorSelector = FindObjectOfType<ColorSelectionScript>();
        if (colorSelector == null)
        {
            colorIndex = 12;
        }
        else
        {
            colorIndex = colorSelector.materialIndex;
        }

        CmdSpawnPlayer(playerGOName, colorIndex);
	}

    // This Command tells the host to spawn the new player.
    [Command]
    void CmdSpawnPlayer(string Name, int color)
    {
        GameObject player = Instantiate(playerPrefab);
        GameObject playerCamera = Instantiate(playerCameraPrefab);

        // setting the given parameters to the players car and camera
        player.name = Name;
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;

        var playerScript = player.GetComponent<PlayerControllerMultiplayer>();
        playerScript.colorIndex = color;
        playerScript.carName = Name;

        var cameraScript = playerCamera.GetComponent<FollowPlayerMultiplayer>();

        cameraScript.playerGameObjectName = Name;

        // Spawning the player to the clients
        NetworkServer.SpawnWithClientAuthority(player, connectionToClient);
        NetworkServer.SpawnWithClientAuthority(playerCamera, connectionToClient);

        RpcNameChange(player, playerCamera, Name, color);
    }

    // This method tells the clients about the new player
    [ClientRpc]
    void RpcNameChange(GameObject player, GameObject playerCamera, string Name, int color) 
    {
        player.name = Name;
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;

        var playerScript = player.GetComponent<PlayerControllerMultiplayer>();
        playerScript.colorIndex = color;
        playerScript.carName = Name;

        var cameraScript = playerCamera.GetComponent<FollowPlayerMultiplayer>();

        cameraScript.playerGameObjectName = Name;
    }

}
