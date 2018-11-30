using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    /*
     * This script is used to calculate the player's lap times, placement, 
     * current lap and completion of the race.
     */

    public Text timerText;
    public Text bestlap;
    public Text startTimeText;
    [SerializeField] private Text playerPlacement;
    [SerializeField] private Text lapCounter;
    public PlayerController playerController;
    public List<CarAIScript> AIcontrollers;
    [SerializeField] private List<Transform> targets;

    public GameObject player;

    public float passedTime = 0f;
    public bool inMenu = true;

    private float time;
    private float previousLapTime = 0f;
    private float bestLapTime = 0f;
 
    private bool lapFinished = false;
    private bool playerActive = false;
    private bool startTimeActive = true;
    private bool gameEnded = false;

    private int currentPlayerLap = 1;
    private int targetIndex = 0;
    private Transform currentPlayerTarget;


    void Start () {

        timerText.text = "";
        bestlap.text = "00 : 00";
        playerPlacement.text = "";
        lapCounter.text = "Lap: 1/5";
        currentPlayerTarget = targets[targetIndex];
	}
	
	
	void FixedUpdate () {

        if (playerActive == false) ActivatePlayer();

        if (inMenu == false && startTimeActive) SetStartTimer();

        checkCheckpoints();

        // Updating clock and the player's placement on every frame.
        if (playerActive)
        {
            updateClock();
            updatePlayerPlacement();
        }

        // The game is set to end when the player has finished 5 laps
        if (currentPlayerLap == 6 && !gameEnded)
        {
            SetGameEnd();
        }
	}

    // This method ends the and displays the player's placement
    private void SetGameEnd()
    {
        lapCounter.text = "Lap: 5/5";

        startTimeText.text = "Your placement: " + playerPlacement.text;

        playerController.enabled = false;

        foreach (var AiController in AIcontrollers)
        {
            AiController.enabled = false;
        }

        gameEnded = true;
    }

    // This method calculates how many AIs are ahead of the player and then
    // displays the player's placement accordingly.
    private void updatePlayerPlacement()
    {
        List<int> AIsOnSameLapOrAheadOfPlayer = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            if (AIcontrollers[i].currentLap >= currentPlayerLap) AIsOnSameLapOrAheadOfPlayer.Add(i);
        }

        int AIsAheadOfPlayer = 1;

        float distanceToTarget = (currentPlayerTarget.position - player.transform.position).sqrMagnitude;

        foreach (var AI in AIsOnSameLapOrAheadOfPlayer)
        {
            // AI ahead in checkpoints
            if (AIcontrollers[AI].targetIndex > targetIndex) AIsAheadOfPlayer++;

            // AI ahead on laps
            else if (AIcontrollers[AI].currentLap > currentPlayerLap) AIsAheadOfPlayer++;

            // AI closer to the next checkpoint
            else if (AIcontrollers[AI].targetIndex == targetIndex)
            {
                float AIdistanceToTarget = (targets[AIcontrollers[AI].targetIndex].position
                                        - AIcontrollers[AI].transform.position).sqrMagnitude;

                if (AIdistanceToTarget < distanceToTarget) AIsAheadOfPlayer++;
            }
        }

        if (AIsAheadOfPlayer == 1) playerPlacement.text = "1st";
        else if (AIsAheadOfPlayer == 2) playerPlacement.text = "2nd";
        else if (AIsAheadOfPlayer == 3) playerPlacement.text = "3rd";
        else playerPlacement.text = AIsAheadOfPlayer.ToString("0") + "th";
    }

    // This method displays numbers counting down from 3 to 1 and displays 'GO'
    // when the counter reaches 0.
    private void SetStartTimer()
    {
        if (Time.time - passedTime < -2) startTimeText.text = "3";
        else if (Time.time - passedTime < -1) startTimeText.text = "2";
        else if (Time.time - passedTime < 0) startTimeText.text = "1";
        else if (Time.time - passedTime < 2)
        {
            startTimeText.text = "GO!";
        }
        else
        {
            startTimeText.text = "";
            startTimeActive = false;
        }
    }
    
    // This method lets the player start driving when 3 seconds have passed 
    // after the player has pressed 'PLAY' in the start menu. It also
    // activates the AI cars.
    private void ActivatePlayer()
    {
        if (inMenu) return;

        if (Time.time - passedTime >= 0f)
        {
            playerActive = true;
            playerController.enabled = true;

            foreach (var controller in AIcontrollers)
            {
                controller.driving = true;
                controller.startDrivingTime = Time.time;
            }
        }
    }

    // This method updates the player's current checkpoint and also updates 
    // the player's current lap.
    void checkCheckpoints()
    {

        Vector3 carPos = player.transform.position;

        if ((carPos.x > (currentPlayerTarget.position.x - 7.5f) && carPos.x < (currentPlayerTarget.position.x + 7.5f)) &&
            (carPos.y > (currentPlayerTarget.position.y - 3) && carPos.y < (currentPlayerTarget.position.y + 3f)) &&
            (carPos.z > (currentPlayerTarget.position.z - 7.5f) && carPos.z < (currentPlayerTarget.position.z + 7.5f))) targetIndex++;


        if (targetIndex >= 68)
        {
            currentPlayerLap++;
            lapFinished = true;
            targetIndex = 0;
            lapCounter.text = "Lap: " + currentPlayerLap.ToString("0") + "/5";
        }

        currentPlayerTarget = targets[targetIndex];
    }

    // This method updates the player's lap timer and records the player's best
    // lap time
    void updateClock()
    {
        time = Time.time - passedTime;

        if (lapFinished)
        {
            previousLapTime = time;
            passedTime += time;
            time = 0;
            lapFinished = false;

            if (bestLapTime == 0) bestLapTime = previousLapTime;

            else { if (bestLapTime > previousLapTime) bestLapTime = previousLapTime; }

            float helpNumber2 = bestLapTime % 60;
            float minutes2 = (bestLapTime - helpNumber2) / 60;
            float seconds2 = bestLapTime - minutes2 * 60;
            bestlap.text = minutes2.ToString("00.") + " : " + seconds2.ToString("00.");
        } 

        float helpNumber = time % 60;
        float minutes = (time - helpNumber) / 60;
        float seconds = time - minutes * 60;
        timerText.text = minutes.ToString("00.") + " : " + seconds.ToString("00.");
    }

}
