using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders (FrontLeft, FrontRight, RearLeft, RearRight)")]
    public WheelCollider wcFL;
    public WheelCollider wcFR;
    public WheelCollider wcRL;
    public WheelCollider wcRR;

    [Header("Wheel Meshes")]
    public Transform wFL;
    public Transform wFR;
    public Transform wRL;
    public Transform wRR;

    [Header("Center of Mass")]
    public Transform centerOfMass;

    [Header("Car Physics Settings")]
    public float maxMotorTorque = 1500f;   // Torque applied to drive wheels
    public float maxSteerAngle = 30f;      // Maximum steering angle in degrees
    public float maxBrakeTorque = 3000f;   // Brake torque

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass != null)
        {
            // Set Rigidbody center of mass relative to the Rigidbody transform
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }

    }

    void Update()
    {
        // Input handling is done in Update()
        // You can extend this later for better input systems
    }

    void FixedUpdate()
    {
        // Read inputs from legacy Input Manager
        float steer = Input.GetAxis("Horizontal");   // A/D or Left/Right
        float throttle = Input.GetAxis("Vertical");  // W/S or Up/Down
        bool braking = Input.GetKey(KeyCode.Space);  // Space bar for brake

        // Calculate motor torque & steering angle
        float motor = maxMotorTorque * throttle;
        float steerAngle = maxSteerAngle * steer;
        float brake = braking ? maxBrakeTorque : 0f;

        // Apply steering to front wheels
        if (wcFL != null) wcFL.steerAngle = steerAngle;
        if (wcFR != null) wcFR.steerAngle = steerAngle;

        // Apply motor torque to front wheels
        if (wcFL != null) wcFL.motorTorque = motor;
        if (wcFR != null) wcFR.motorTorque = motor;

        // Apply brake torque to all wheels
        if (wcFL != null) wcFL.brakeTorque = brake;
        if (wcFR != null) wcFR.brakeTorque = brake;
        if (wcRL != null) wcRL.brakeTorque = brake;
        if (wcRR != null) wcRR.brakeTorque = brake;

        // Update the wheel meshes' position and rotation to match colliders
        UpdateWheelPose(wcFL, wFL);
        UpdateWheelPose(wcFR, wFR);
        UpdateWheelPose(wcRL, wRL);
        UpdateWheelPose(wcRR, wRR);

        Debug.Log($"Throttle: {throttle}, MotorTorque: {motor}");

    }

    // Helper method to sync WheelCollider pose with visual wheel
    void UpdateWheelPose(WheelCollider collider, Transform wheel)
    {
        if (collider == null || wheel == null) return;
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }
}
