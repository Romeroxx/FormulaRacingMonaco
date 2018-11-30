using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour {

    /*
     * This script randomizes the start point of audience animation. This makes
     * the crowd more lively because everyone is not doing the same moves at 
     * the same time.
     */

    private Animation audienceAnimation;

	void Start () {
        audienceAnimation = GetComponent<Animation>();

        audienceAnimation["WholeThing"].time = Random.Range(0.0f, audienceAnimation["WholeThing"].length);
	}
	

}
