using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCapper : MonoBehaviour {

    /*
     * This script caps the game's FPS to 60
     */

	void Awake () {

        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 60;

	}
	
}
