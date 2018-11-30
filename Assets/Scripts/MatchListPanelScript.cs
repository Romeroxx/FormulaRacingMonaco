using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class MatchListPanelScript : MonoBehaviour {

    /*
     * This script is responsible for updating the join buttons of the
     * MatchListPanel. It uses the matchlist provided by the 
     * CustomNetworkManager to determine the amount of join buttons.
     */

    public GameObject joinButton;

    public void UpdateMatchList(List<MatchInfoSnapshot> matchInfo)
    {
        var buttons = GetComponentsInChildren<JoinButtonScript>();
        // destroy old buttons
        foreach (var button in buttons)
        {
            Destroy(button.gameObject);
        }
        // and create new ones
        foreach (var match in matchInfo)
        {
            var button = Instantiate(joinButton);
            JoinButtonScript buttonScript = button.GetComponent<JoinButtonScript>();
            buttonScript.Initialize(match, transform);
        }
    }


}
