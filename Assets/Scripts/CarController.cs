using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 * This class is used to control the cars in the game.
 * It handles almost everything that affects moving the cars.
 * This same calss is used to move the AI cars and the player's car.
 */
public class CarController : MonoBehaviour {

    public List<WheelCollider> wheelColliders;
    public List<GameObject> wheelMeshes;
    public GameObject steeringWheel;

    public Text gearText;
    public Text revsText;
    public Text speedText;

    public bool useGears = false;

    public float maxReverseTorgue;
    public float maxbrakeTorgue;
    public float maximumAccelTorgue;
    public float maximumSteerAngle;
    public float maxHandbrakeTorgue;
    public float downForce;
    public float slipLimit;
    [Range(0, 1)] public float tractionFix;
    [Range(0, 1)] public float steerHelper;
    public float accelInput;
    public float audioRevs = 0.1f;

    private int gear = 1;
    private float currentTorgue;
    private float brakeInput;
    private float steering;
    private float steerAngle;
    private float oldRotation;
    private float gearModifier = 1f;
    private float previousGearChange;
    private float Revs = 1500f;
    private float nextStopTime = 0.0f;

    private Rigidbody rbody;

    public float currentSpeed { get { return rbody.velocity.magnitude; } }
    public float currentSpeedKmh { get { return rbody.velocity.magnitude * 3.6f; } }

    void Start () {

        rbody = GetComponent<Rigidbody>();

        currentTorgue = maximumAccelTorgue;

        speedText.text = "";
        gearText.text = "";
        revsText.text = "";
        revsText.color = Color.green;
	}

    // This method is used to move the car
    // The car's movement is created by using Unity's WheelColliders
    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        float speed = rbody.velocity.magnitude * 3.6f;
        speedText.text = speed.ToString("0.") + " km/h";

        // This section makes the wheelMeshes that the player can see ingame to
        // rotate according to the cars movement. This is made by rotating the
        // WheelMeshes to the same rotations with the WheelColliders.
        for (int i = 0; i < 4; i++)
        {
            Quaternion rotation;
            Vector3 position;
            wheelColliders[i].GetWorldPose(out position, out rotation);

            wheelMeshes[i].transform.position = position;
            wheelMeshes[i].transform.rotation = rotation;
        }

        // Mathf.clamp() makes sure that the parameter values are not too big 
        // or too small
        steering = Mathf.Clamp(steering, -1, 1);
        accelInput = Mathf.Clamp(accel, 0, 1);
        brakeInput = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        // Setting the forward WheelColliders to steering angle affected by the
        // steering parameter.
        steerAngle = steering * maximumSteerAngle;
        for (int i = 0; i < 2; i++)
        {
            wheelColliders[i].steerAngle = steerAngle;
        }

        // Setting the steeringwheel of the car to match the rotation of the
        // forward WheelColliders
        Vector3 steeringWheelOldRot = steeringWheel.transform.eulerAngles;
        steeringWheelOldRot.z = -steerAngle;
        steeringWheel.transform.eulerAngles = steeringWheelOldRot;

        // Apply steer helper
        SteerHelper();

        // if the car uses automatic gears this method calculates which gear is
        // in use at the moment
        if (useGears == false) CalculateCurrentGear();

        // calculating how the current gear affects acceleration
        CalculateGearsFactor();

        // Applying the usage of handbrake to the WheelColliders
        float handbreakTorgue = handbrake * maxHandbrakeTorgue;
        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].brakeTorque = handbreakTorgue;
        }

        // Applying acceleration/braking to the WheelColliders
        ApplyTorgueToWheels( accelInput, brakeInput);

        // Calculating the amount of revs
        CalculateRevs();

        // prevents the car from flying
        StopCarFlying();

        // adds additional force to keep the car on the road
        AddDownForce();

        // updating player texts
        UpdateGearText();
    }

    // Applies acceleration/braking to the WheelColliders
    private void ApplyTorgueToWheels(float accel, float brake)
    {
        float Torgue = accel * (currentTorgue / 2f);

        // if the player is using manual gear control multiply the motorTorgue
        // with the current gear modifier.
        if (useGears) Torgue = Torgue * gearModifier;
        
        for (int i = 2; i < 4; i++)
        {
            wheelColliders[i].motorTorque = Torgue;
        }

        float brakeTorgue = maxbrakeTorgue * brake;

        for (int i = 0; i < 4; i++)
        {
            if ((currentSpeed > 1f) && (Vector3.Angle(transform.forward, rbody.velocity) < 50f))
            {
                if (brakeTorgue > 10f) wheelColliders[i].brakeTorque = brakeTorgue;
            }
            else
            {
                if (brake > 0)
                {
                    wheelColliders[i].brakeTorque = 0f;
                    wheelColliders[i].motorTorque = -maxReverseTorgue * brake;
                }
            }
        }
    }

    // This method prevents the cars from flying and going upside down.
    private void StopCarFlying()
    {
        if (transform.eulerAngles.z > 30f && transform.eulerAngles.z < 180f)
            transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, 19.5f);

        if (transform.eulerAngles.z < 330f && transform.eulerAngles.z > 180f)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 340.5f);

        int flyingWheelsCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (wheelColliders[i].isGrounded == false) flyingWheelsCount++;
        }

        if (flyingWheelsCount == 4 && Time.time >= nextStopTime)
        {
            nextStopTime = Time.time + 2f;

            rbody.velocity = new Vector3(0f, 0f, rbody.velocity.z);
        }

    }

    private void AddDownForce()
    {
        rbody.AddForce(-transform.up * downForce * rbody.velocity.magnitude);
    }

    // This method could be used to help controlling the cars tracktion, but it
    // is unused for the time being
    private void TractionControl()
    {
        WheelHit wheelHit;

        for (int i = 2; i < 4; i++)
        {
            wheelColliders[i].GetGroundHit(out wheelHit);

            AdjustAccelTorgue(wheelHit.forwardSlip);
        }
    }

    private void AdjustAccelTorgue(float slip)
    {
        if (slip > slipLimit && currentTorgue > 0)
        {
            currentTorgue -= 10 * tractionFix;
        }
        else
        {
            currentTorgue += 10 * tractionFix;

            if (currentTorgue > maximumAccelTorgue)
            {
                currentTorgue = maximumAccelTorgue;
            }
        }
    }

    // This method helps to steer the car by adjusting the car's velocity
    // accorgin to the direction the car is turning to.
    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelHit;
            wheelColliders[i].GetGroundHit(out wheelHit);
            if (wheelHit.normal == Vector3.zero) return;
        }

        if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnAdjust = (transform.eulerAngles.y - oldRotation) * steerHelper;
            Quaternion velocityRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            rbody.velocity = velocityRotation * rbody.velocity;
        }
        oldRotation = transform.eulerAngles.y;
    }

    // This method is used if the player has chosen teh automatic gears mode.
    // It calculates the current gear from the player's speed.
    private void CalculateCurrentGear()
    {
        if (currentSpeedKmh < 9f) gear = 1;
        else if (currentSpeedKmh >= 9f && currentSpeedKmh < 33f) gear = 2;
        else if (currentSpeedKmh >= 33f && currentSpeedKmh < 77f) gear = 3;
        else if (currentSpeedKmh >= 77f && currentSpeedKmh < 125f) gear = 4;
        else if (currentSpeedKmh >= 125f && currentSpeedKmh < 182f) gear = 5;
        else if (currentSpeedKmh >= 182f && currentSpeedKmh < 245f) gear = 6;
        else if (currentSpeedKmh >= 245f) gear = 7;
    }

    // This method calculates the  current gaer modifier which effects the 
    // car's motor torgue. Every gear has their own speed area where tehy give
    // the most torgue and another speed area where they only give half the 
    // torgue. Also if the car moves too fats or too slowly for the current
    // gear the gear modifier is set to 0. Blue revs text indicates too slow
    // speed and red indicates too fast speed.
    private void CalculateGearsFactor()
    {
        float curSpeed = currentSpeedKmh;
        if (gear == 1)
        {
            if (curSpeed <= 10f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if (curSpeed > 10f && curSpeed <= 15f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.red;
            }
            else gearModifier = 0f;
        }
        else if (gear == 2)
        {
            if (curSpeed <= 35f && curSpeed >= 8f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if ((curSpeed > 35f && curSpeed <= 40f))
            {
                gearModifier = 0.5f;
                revsText.color = Color.red;
            }
            else if (curSpeed >= 5f && curSpeed < 8f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.blue;
            }
            else gearModifier = 0f;
        }
        else if (gear == 3)
        {
            if (curSpeed <= 80f && curSpeed >= 31f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if ((curSpeed >= 25f && curSpeed < 31f))
            {
                gearModifier = 0.5f;
                revsText.color = Color.blue;
            }
            else if ((curSpeed > 80f && curSpeed <= 90f)) 
            {
                gearModifier = 0.5f;
                revsText.color = Color.red;
            }
            else gearModifier = 0f;
        }
        else if (gear == 4)
        {
            if (curSpeed <= 130f && curSpeed >= 74f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if (curSpeed >= 65f && curSpeed < 74f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.blue;
            }
            else if (curSpeed > 130f && curSpeed <= 140f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.red;
            }
            else gearModifier = 0f;
        }
        else if (gear == 5)
        {
            if (curSpeed <= 190f && curSpeed >= 120f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if (curSpeed >= 100f && curSpeed < 120f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.blue;
            }
            else if (curSpeed > 190f && curSpeed <= 200f) 
            {
                gearModifier = 0.5f;
                revsText.color = Color.red;
            }
            else gearModifier = 0f;
        }
        else if (gear == 6)
        {
            if (curSpeed <= 250f && curSpeed >= 175f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if  (curSpeed >= 150f && curSpeed < 175f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.blue;
            }
            else if (curSpeed > 250f && curSpeed <= 260f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.red;
            }
            else gearModifier = 0f;
        }
        else if (gear == 7)
        {
            if (curSpeed >= 240f)
            {
                gearModifier = 1f;
                revsText.color = Color.green;
            }
            else if (curSpeed > 200f && curSpeed <= 240f)
            {
                gearModifier = 0.5f;
                revsText.color = Color.blue;
            }
            else gearModifier = 0f;
        }
        else gearModifier = 0f;
    }

    public void ChangeGear(int direction)
    {

        if (direction == 1) gear++;
        else if (direction == -1) gear--;

        if (gear < 1) gear = 1;
        else if (gear > 7) gear = 7;

        gearText.text = gear.ToString("0");
    }

    private void CalculateRevs()
    {
        Revs = 1500f;

        if (gear == 1)
        {
            Revs += currentSpeedKmh * 566f;
        }
        else if (gear == 2)
        {
            Revs += (currentSpeedKmh - 5f) * 242f;
        }
        else if (gear == 3)
        {
            Revs += (currentSpeedKmh - 25f) * 130f;
        }
        else if (gear == 4)
        {
            Revs += (currentSpeedKmh - 65f) * 113f;
        }
        else if (gear == 5)
        {
            Revs += (currentSpeedKmh - 100f) * 85f;
        }
        else if (gear == 6)
        {
            Revs += (currentSpeedKmh - 150f) * 77f;
        }
        else if (gear == 7)
        {
            Revs += (currentSpeedKmh - 200f) * 60f;
        }

        if (Revs > 10000f)
        {
            Revs = 10000f;
            revsText.color = Color.red;
        }
        else if (Revs < 1500f)
        {
            Revs = 1500f;
            revsText.color = Color.blue;
        }

        audioRevs = (Revs - 1499f) / 8500f;

        revsText.text = Revs.ToString("0000") + " rpm";

    }

    private void UpdateGearText()
    {
        if (gear == 1) gearText.text = "1st Gear";
        else if (gear == 2) gearText.text = "2nd Gear";
        else if (gear == 3) gearText.text = "3rd Gear";
        else gearText.text = gear.ToString("0") + "th Gear";
    }
}
