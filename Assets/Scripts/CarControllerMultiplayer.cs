using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

/*
 * This class is used to control the cars in multiplyer. It is very similar to 
 * CarController. The difference between this script and the CarController are
 * the SyncVars used to inform other players about this cars properties.
 * Checkout CarController for more commentation.
 */
public class CarControllerMultiplayer : NetworkBehaviour {

    public List<WheelCollider> wheelColliders;
    public List<GameObject> wheelMeshes;
    public GameObject steeringWheel;
    public Text gearText;
    public Text revsText;
    public Text speedText;

    // synchronizing usage of gears with other clients
    [SyncVar] public bool useGears;

    public float maxReverseTorgue;
    public float maxbrakeTorgue;
    public float maximumAccelTorgue;
    public float maximumSteerAngle;
    public float maxHandbrakeTorgue;
    public float downForce;
    public float slipLimit;
    [Range(0, 1)] public float tractionFix;
    [Range(0, 1)] public float steerHelper;

    private int gear = 1;
    private float currentTorgue;
    public float accelInput;

    // synchronizing accel input with other clients
    [SyncVar] public float SyncaccelInput;
    private float brakeInput;
    private float steering;
    private float steerAngle;
    private float oldRotation;
    private float gearModifier = 1f;
    private float previousGearChange;
    private float Revs = 1500f;
    private float nextStopTime = 0.0f;

    public float audioRevs = 0.1f;
    // synchronizing revs with other clients to have sounds in every car in
    // multiplayer
    [SyncVar] public float SyncaudioRevs = 0.1f;

    private Rigidbody rbody;

    public float currentSpeed { get { return rbody.velocity.magnitude; } }
    public float currentSpeedKmh { get { return rbody.velocity.magnitude * 3.6f; } }

    void Start () {

        rbody = GetComponent<Rigidbody>();

        currentTorgue = maximumAccelTorgue;

        speedText.text = "";
        gearText.text = "1st Gear";
        revsText.text = "";
        revsText.color = Color.green;
	}

    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        float speed = rbody.velocity.magnitude * 3.6f;
        speedText.text = speed.ToString("0.") + " km/h";

        for (int i = 0; i < 4; i++)
        {
            Quaternion rotation;
            Vector3 position;
            wheelColliders[i].GetWorldPose(out position, out rotation);
            wheelMeshes[i].transform.position = position;
            wheelMeshes[i].transform.rotation = rotation;
        }

        steering = Mathf.Clamp(steering, -1, 1);
        accelInput = Mathf.Clamp(accel, 0, 1);
        brakeInput = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        steerAngle = steering * maximumSteerAngle;
        for (int i = 0; i < 2; i++)
        {
            wheelColliders[i].steerAngle = steerAngle;
        }
        Vector3 steeringWheelOldRot = steeringWheel.transform.eulerAngles;
        steeringWheelOldRot.z = -steerAngle;
        steeringWheel.transform.eulerAngles = steeringWheelOldRot;

        SteerHelper();

        if (useGears == false) CalculateCurrentGear();
        CalculateGearsFactor();

        float handbreakTorgue = handbrake * maxHandbrakeTorgue;
        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].brakeTorque = handbreakTorgue;
        }

        ApplyTorgueToWheels( accelInput, brakeInput);

        CalculateRevs();

        StopCarFlying();

        AddDownForce();

        UpdateGearText();

        UpdateSyncVars();
    }

    

    private void ApplyTorgueToWheels(float accel, float brake)
    {
        float Torgue = accel * (currentTorgue / 2f);

        if (useGears) Torgue = Torgue * gearModifier;
        
        for (int i = 2; i < 4; i++)
        {
            wheelColliders[i].motorTorque = Torgue;
        }

        float brakeTorgue = maxbrakeTorgue * brake;

        for (int i = 0; i < 4; i++)
            {
            if ((currentSpeed > 1) && (Vector3.Angle(transform.forward, rbody.velocity) < 50f))
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

    private void StopCarFlying()
    {
        if (transform.eulerAngles.z > 20f && transform.eulerAngles.z < 180f) transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, 19.5f);
        if (transform.eulerAngles.z < 340f && transform.eulerAngles.z > 180f) transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 340.5f);

        int flyingWheelsCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (wheelColliders[i].isGrounded == false) flyingWheelsCount++;
        }

        if (flyingWheelsCount >= 3 && Time.time >= nextStopTime)
        {
            nextStopTime = Time.time + 1f;

            rbody.velocity = new Vector3(0f, 0f, rbody.velocity.z);
        }

    }

    private void AddDownForce()
    {
        rbody.AddForce(-transform.up * downForce * rbody.velocity.magnitude);
    }

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
        if (useGears == false) return;

        if (direction == 1) gear++;
        else if (direction == -1) gear--;

        if (gear < 1) gear = 1;
        else if (gear > 7) gear = 7;
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

    // updating the car's accel input and revs to the other clients.
    private void UpdateSyncVars()
    {
        if (isServer)
        {
            SyncaccelInput = accelInput;
            SyncaudioRevs = audioRevs;
        }
        else CmdUpdateSync(accelInput, audioRevs);
    }

    [Command]
    void CmdUpdateSync(float accel, float audio)
    {
        SyncaccelInput = accel;
        SyncaudioRevs = audio;
    }

}
