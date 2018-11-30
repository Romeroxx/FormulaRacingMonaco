using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class JoinButtonScript : MonoBehaviour {

    private Text buttonText;
    private MatchInfoSnapshot match;

    private void Awake()
    {
        buttonText = GetComponentInChildren<Text>();
        GetComponent<Button>().onClick.AddListener(JoinMatch);
    }

    // This method is called by another class
    // It creates Join game -buttons that can be used to join online games
    public void Initialize(MatchInfoSnapshot match, Transform joinButtonPanelTransform)
    {
        this.match = match;
        buttonText.text = match.name;
        transform.SetParent(joinButtonPanelTransform);
    }

    // This method is activated by a button
    private void JoinMatch()
    {
        FindObjectOfType<CustomNetworkManager>().JoinMatch(match);
    }
}
