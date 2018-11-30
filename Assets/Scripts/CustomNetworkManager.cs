using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

/*
 * This is a script is derived from unity's NetworkManager. The custom part of 
 * this script implements a possibility to use a custom join menu. It also
 * determines the gears option of the game.
 */
public class CustomNetworkManager : NetworkManager {

    // default name can be altered by the input field in the multiplayer menu
    public string MatchName = "Race";
    public bool useGears = false;
    public InputField inputField;

    private bool isHosting = false;
    private bool isClient = false;

    private MatchInfo hostedMatchInfo;
    private MatchListPanelScript matchListPanelScript;

    private float nextRefreshTime = 0f;

    private void Start()
    {
       GameObject matchListPanel = GameObject.Find("MenuPanel");

        matchListPanelScript = matchListPanel.GetComponent<MatchListPanelScript>();
    }

    // Creates a new game using the NetworkManager and servers provided by 
    // unity.
    public void StartHosting()
    {
        StartMatchMaker();
        matchMaker.CreateMatch(MatchName, 8, true, "", "", "", 0, 0, MatchCreated);
    }

    private void MatchCreated(bool success, string extendedInfo, MatchInfo responseData)
    {
        hostedMatchInfo = responseData;
        base.StartHost(responseData);
        isHosting = true;
    }

    private void Update()
    {
        if (Time.time > nextRefreshTime) RefreshMatchList();
    }

    // Refreshing the match list every 3rd second
    private void RefreshMatchList()
    {
        nextRefreshTime = Time.time + 3f;

        if (matchMaker == null)
            StartMatchMaker();

        // using the NetworkManager to create the matchlist
        matchMaker.ListMatches(0, 10, "", false, 0, 0, HandleMatchList);

    }

    // Updating the match list's data to the match list panel
    private void HandleMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        matchListPanelScript.UpdateMatchList(responseData);
    }

    // uses the NetworkManager to join a match
    public void JoinMatch(MatchInfoSnapshot match)
    {
        if (matchMaker == null)
            StartMatchMaker();

        inputField.enabled = false;

        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, MatchJoined);
    }

    private void MatchJoined(bool success, string extendedInfo, MatchInfo responseData)
    {
        StartClient(responseData);
        isClient = true;
    }

    // uses the NetworkManager to disconnect from a server
    public void Disconnect()
    {
        if (isHosting) StopHosting();
        else if (isClient)
        {
            StopClient();
            StopMatchMaker();
            isClient = false;
        }
    }

    // uses Network Manager to stop hosting
    private void StopHosting()
    {
        matchMaker.DestroyMatch(hostedMatchInfo.networkId, hostedMatchInfo.domain, MatchDeleted);
        StopHost();
        StopMatchMaker();
        isHosting = false;
    }

    private void MatchDeleted(bool success, string extendedInfo)
    {   
    }
}
