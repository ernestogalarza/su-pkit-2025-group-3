using UnityEngine;

public class CarControllerCustom : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Pointers")]
    public Transform speedPointer;   // car-speedpointer
    public Transform rpmPointer;     // car-rmppointer (if needed)

    [Header("Settings")]
    public float maxMotorTorque = 500f;
    public float maxSteeringAngle = 30f;
    public float brakeTorque = 300f;
    public Rigidbody rb;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steer = maxSteeringAngle * Input.GetAxis("Horizontal");

        // Apply motor torque
        rearLeftCollider.motorTorque  = motor;
        rearRightCollider.motorTorque = motor;

        // Apply steering
        frontLeftCollider.steerAngle  = steer;
        frontRightCollider.steerAngle = steer;

        // Apply brake when reversing or stopping
        if (Input.GetAxis("Vertical") < 0)
        {
            rearLeftCollider.brakeTorque  = brakeTorque;
            rearRightCollider.brakeTorque = brakeTorque;
        }
        else
        {
            rearLeftCollider.brakeTorque  = 0;
            rearRightCollider.brakeTorque = 0;
        }

        UpdateWheelVisual(frontLeftCollider, frontLeftMesh);
        UpdateWheelVisual(frontRightCollider, frontRightMesh);
        UpdateWheelVisual(rearLeftCollider, rearLeftMesh);
        UpdateWheelVisual(rearRightCollider, rearRightMesh);

        UpdatePointers();
    }

    void UpdateWheelVisual(WheelCollider col, Transform mesh)
    {
         // 1. Get the world position & rotation from the collider
    Vector3 pos;
    Quaternion rot;
    col.GetWorldPose(out pos, out rot);

    // 2. Apply world position
    mesh.position = pos;

    // 3. Extract the steering angle (col.steerAngle) into a yaw rotation
    float steer = col.steerAngle;
    Quaternion steerRotation = Quaternion.Euler(0f, steer, 0f);

    // 4. Combine the collider’s roll/pitch rotation (rot) with yaw steerRotation
    mesh.rotation = rot * steerRotation;
    }

    void UpdatePointers()
    {
        if (speedPointer != null)
        {
            float speed = rb.linearVelocity.magnitude * 3.6f; // km/h
            // Map speed to pointer rotation (0–240 km/h → 0–270°)
            float speedAngle = Mathf.Clamp(speed / 240f, 0f, 1f) * 270f;
            speedPointer.localRotation = Quaternion.Euler(0, 0, -speedAngle);
        }
        if (rpmPointer != null)
        {
            // Example RPM mapping (0–8000 RPM → 0–270°)
            float rpm = Mathf.Clamp01(Mathf.Abs(rb.angularVelocity.y) / 8000f);
            float rpmAngle = rpm * 270f;
            rpmPointer.localRotation = Quaternion.Euler(0, 0, -rpmAngle);
        }
    }
}
