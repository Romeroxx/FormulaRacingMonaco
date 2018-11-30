using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIScript : MonoBehaviour {

    /*
     * This script is responsible for moving the AI cars. The AI cars are 
     * supposed to drive around track. They do this by following checkpoints
     * that are already set on the track. The angle between the checkpoints'
     * transforms' forward vectors determines the AI car's speed. If the angle
     * is big the AI slows down a lot and tries to turn it's transform's 
     * forward vector to point to the same direction as the next checkpoint's
     * forward vector.
     *      The AI doesn't know how to avoid the player or other AIs. Instead
     * there is random perlin noise that alters the AI car's course randomly.
     */

    public List<Transform> targets;
    [Range(0, 1)] public float cautiosSpeedMultiplier = 0.05f;
    public float cautiousMaxAngle = 50f;
    public float angularVelocityMultiplier = 30f;
    public float wanderFluctuateSpeed = 0.1f;
    public float wanderDistance = 3f;
    public float accelSensityvity = 0.05f;
    public float breakSensitivity = 1f;
    public float steerSensitivity = 0.05f;
    public float reversingTime = 2.5f;
    public bool driving = false;
    public float startDrivingTime = 0f;

    private float desiredSpeed = 80f;
    private float maxSpeed = 80f;
    private float RandomPerlinCoordinate;
    private float reverseSteering = 0f;
    private float rotationFixingTime = 0f;

    public int targetIndex = 0;
    public int currentLap = 1;

    private Transform currentTarget;
    private CarController carController;
    private Rigidbody rbody;

	void Start () {

        carController = GetComponent<CarController>();

        rbody = GetComponent<Rigidbody>();

        currentTarget = targets[targetIndex];

        // Creating random perlin noise to randomise the AI's movement.
        RandomPerlinCoordinate = UnityEngine.Random.value * 100;
    }
	
    // This method calculates the acceleration, brake and steering floats
    // needed to move a car with the CarController. It also makes the car to
    // reverse if needed. 
	void FixedUpdate ()
    {
        if (driving == false) return;

        // reversing for the set time period
        if (rotationFixingTime > Time.time)
        {
            carController.Move(reverseSteering, -1f, -1f, 0f);
        }
        else
        {

            Vector3 forward = transform.forward;
            if (rbody.velocity.magnitude > 8) forward = rbody.velocity;

            // comparing the AI's transform's forward vector to the next
            // target's forward vector
            float nextCornerAngle = Vector3.Angle(currentTarget.forward, forward);

            float currentSpinningAngle = rbody.angularVelocity.magnitude * angularVelocityMultiplier;

            // Calculating an appropriate cautiousness multiplier for the AI's
            // speed
            float requiredCautiousness = Mathf.InverseLerp(0, cautiousMaxAngle,
                                         Mathf.Max(currentSpinningAngle, nextCornerAngle));

            // calculates a desired speed for the AI car
            desiredSpeed = Mathf.Lerp(maxSpeed, maxSpeed * cautiosSpeedMultiplier, requiredCautiousness);

            Vector3 offsetedTargetPos = currentTarget.position;

            // Adding a random perlin noise factor to the next target's 
            // position which randomises the AI's movement.
            offsetedTargetPos += currentTarget.right *
                (Mathf.PerlinNoise(Time.time * wanderFluctuateSpeed, 
                RandomPerlinCoordinate) * 2 - 1) * wanderDistance;


            float accelBrakeSensitivity = accelSensityvity;

            if (desiredSpeed < carController.currentSpeed) accelBrakeSensitivity = breakSensitivity;

            // Calculating an appropriate float to represent the AI's need to
            // accelerate or brake.
            float accelBrake = Mathf.Clamp((desiredSpeed - carController.currentSpeed)
                                                * accelBrakeSensitivity, -1, 1);

            Vector3 localTarget = transform.InverseTransformPoint(offsetedTargetPos);

            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            // Calculating an appropriate float to represent the AI's steering
            // needs.
            float steer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1);

            float angleToTarget = Quaternion.Angle(transform.rotation, currentTarget.rotation);

            // if the AI is not moving although it should be try to reverse for
            // a moment and then try to move forward again
            if (rbody.velocity.magnitude < 0.25f && Time.time > (startDrivingTime + 1f) && Time.time > rotationFixingTime + 1f)
            {
                rotationFixingTime = Time.time + reversingTime;

                if (steer >= 0) reverseSteering = -1;
                else reverseSteering = 1;
                accelBrake = -1;
            } 

            // using CarController to move the AI cars
            carController.Move(steer, accelBrake, accelBrake, 0f);
        }

        UpdateTarget();
	}

    // This method updates the AI's current target checkpoint.
    private void UpdateTarget()
    {
        Vector3 carPos = transform.position;

        if ((carPos.x > (currentTarget.position.x - 7) && carPos.x < (currentTarget.position.x + 7)) &&
            (carPos.y > (currentTarget.position.y - 3) && carPos.y < (currentTarget.position.y + 3)) &&
            (carPos.z > (currentTarget.position.z - 7) && carPos.z < (currentTarget.position.z + 7)))    targetIndex++;

        // Checkpoint 38 is in the start the chicane and the AIs need to be 
        // extra careful
        if (targetIndex == 38)
        {
            cautiosSpeedMultiplier = 0.1f;
        }

        // restoring normal cautiousness
        if (targetIndex == 42)
        {
            cautiosSpeedMultiplier = 0.24f;
        }

        // AI is on the finishing line so add +1 lap
        if (targetIndex >= 68)
        {
            currentLap += 1;
            targetIndex = 0;
        }

        currentTarget = targets[targetIndex];
    }
}
