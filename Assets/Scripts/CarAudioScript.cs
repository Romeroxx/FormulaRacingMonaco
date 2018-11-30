using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAudioScript : MonoBehaviour {

    /*
     * This script creates and handles the car's audio.
     * The car's audio is created by mixing together four different engine 
     * sounds and controlling their volume and pitch according to the car's
     * revs.
     */

    // Clips used to create a real-like car audio
    public AudioClip lowAccelAudio;
    public AudioClip lowDecelAudio;
    public AudioClip highAccelAudio;
    public AudioClip highDecelAudio;

    public float lowPitchMin = 1f;
    public float lowPitchMax = 6f;
    public float pitchMultiplier = 1f;
    public float highPitchMultiplier = 0.25f;

    private AudioSource lowAccel;
    private AudioSource lowDecel; 
    private AudioSource highAccel;
    private AudioSource highDecel;

    private bool soundStarted = false;

    private CarController carController;

    void Start ()
    {
        carController = GetComponent<CarController>();
    }

    // This method creates the audio sources that are used to play the audio.
    public void StartSound()
    {
        lowAccel = SetUpAudioSource(lowAccelAudio);
        lowDecel = SetUpAudioSource(lowDecelAudio);
        highAccel = SetUpAudioSource(highAccelAudio);
        highDecel = SetUpAudioSource(highDecelAudio);

        soundStarted = true;
    }

    // This method sets up one audio source
    private AudioSource SetUpAudioSource(AudioClip clip)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = 0;
        audioSource.loop = true;

        audioSource.spatialBlend = 1.0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 100f;

        audioSource.minDistance = 4;
        audioSource.dopplerLevel = 1;

        audioSource.Play();
        return audioSource;
    }

    // This method controls the loudness and pitch of the car's audio 
    // according to the car's revs. The CarController class provides the revs
    // of the car.
    void Update ()
    {
		if (soundStarted)
        {
            float pitch = CalculatePitch(lowPitchMin, lowPitchMax, carController.audioRevs);

            lowAccel.pitch = pitch * pitchMultiplier;
            lowDecel.pitch = pitch * pitchMultiplier;
            highAccel.pitch = pitch * pitchMultiplier * highPitchMultiplier;
            highDecel.pitch = pitch * pitchMultiplier * highPitchMultiplier;

            float accelFade = carController.accelInput;
            float decelFade = 1 - accelFade;
            float highFade = Mathf.InverseLerp(0.2f, 0.8f, carController.audioRevs);
            float lowFade = 1 - highFade;

            highFade = 1 - ((1 - highFade) * (1 - highFade));
            lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
            accelFade = 1 - ((1 - accelFade) * (1 - accelFade));
            decelFade = 1 - ((1 - decelFade) * (1 - decelFade));

            lowAccel.volume = lowFade * accelFade;
            lowDecel.volume = lowFade = decelFade;
            highAccel.volume = highFade * accelFade;
            highDecel.volume = highFade * decelFade;

            lowAccel.dopplerLevel = 1;
            lowDecel.dopplerLevel = 1;
            highAccel.dopplerLevel = 1;
            highDecel.dopplerLevel = 1;
        }
	}

    // This method calculates a suitable pitch for the audio
    private float CalculatePitch(float Min, float Max, float multiplier)
    {
        return (1.0f - multiplier) * Min + multiplier * Max;
    }
}
