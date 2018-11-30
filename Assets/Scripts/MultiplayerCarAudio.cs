using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerCarAudio : MonoBehaviour {

    /*
     * This script is almost the same as CarAudioScript. It only uses 
     * CarControllerMultiplayer instead of CarController. Check CarAudioScript
     * for further comments.
     */

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

    private CarControllerMultiplayer carController;


    void Start()
    {
        carController = GetComponent<CarControllerMultiplayer>();
        StartSound();
    }

    public void StartSound()
    {
        lowAccel = SetUpAudioSource(lowAccelAudio);
        lowDecel = SetUpAudioSource(lowDecelAudio);
        highAccel = SetUpAudioSource(highAccelAudio);
        highDecel = SetUpAudioSource(highDecelAudio);

        soundStarted = true;
    }

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

    void Update()
    {
        if (soundStarted)
        {
            float pitch = CalculatePitch(lowPitchMin, lowPitchMax, carController.SyncaudioRevs);

            lowAccel.pitch = pitch * pitchMultiplier;
            lowDecel.pitch = pitch * pitchMultiplier;
            highAccel.pitch = pitch * pitchMultiplier * highPitchMultiplier;
            highDecel.pitch = pitch * pitchMultiplier * highPitchMultiplier;

            float accelFade = carController.SyncaccelInput;
            float decelFade = 1 - accelFade;
            float highFade = Mathf.InverseLerp(0.2f, 0.8f, carController.SyncaudioRevs);
            float lowFade = 1 - highFade;

            highFade = 1 - ((1 - highFade) * (1 - highFade));
            lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
            accelFade = 1 - ((1 - accelFade) * (1 - accelFade));
            decelFade = 1 - ((1 - decelFade) * (1 - decelFade));

            lowAccel.volume = lowFade * accelFade;
            lowDecel.volume = lowFade = decelFade;
            highAccel.volume = highFade * accelFade;
            highDecel.volume = highFade * decelFade;
        }
    }

    private float CalculatePitch(float Min, float Max, float multiplier)
    {
        return (1.0f - multiplier) * Min + multiplier * Max;
    }
}
