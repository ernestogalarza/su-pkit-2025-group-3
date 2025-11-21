using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario para usar Text o TextMeshProUGUI
using TMPro;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
public class CarController : MonoBehaviour
{

    public bool handlerKeyboard = true;

    [Header("Car Settings")]
    public float acceleration = 50f;     // km/h por segundo
    public float brakeForce = 100f;      // km/h por segundo
    public float friction = 10f;         // km/h por segundo cuando no se pisa nada
    public float maxSpeed = 180f;        // km/h
    public float turnSpeed = 50f;        // grados por segundo


    public Transform speedometerNeedle;      // Aguja del velocímetro


    private float currentSpeed = 0f;     // km/h
    private Rigidbody rb;

    private Rigidbody carRb;
    public TextMeshProUGUI speedText;               // Referencia al texto en la UI (opcional)

    private SteeringWheelManager steeringWheelManager;



    private float steeringInput;
    private float throttleInput;
    private float brakeInput;

    private float wheelDirection;


    [Header("Speedometer Settings")]
    public float minSpeed = 0f;          // km/h
    public float maxSpeedometerSpeed = 240f;  // km/h del velocímetro
    public float minRotationZ = 0f;      // rotación z a 0 km/h
    public float maxRotationZ = -280f;   // rotación z a 240 km/h





    // Start is called before the first frame update
    void Start()
    {
        steeringWheelManager = GameObject.Find("SteeringWheelManager").GetComponent<SteeringWheelManager>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
    }
    

    // Update is called once per frame
    void Update()
    {
        // HandleMovement();
        speedWithSteeringWheel();
    }


    void speedWithSteeringWheel()
    {
        steeringInput = steeringWheelManager.getWheelDirection();
        throttleInput = steeringWheelManager.getThrottle();
        brakeInput = steeringWheelManager.getBrake();

        float throttle = Mathf.InverseLerp(1f, -1f, throttleInput);
        float brake = 1f - brakeInput;

        // Calculate target speed
        currentSpeed += throttle * acceleration * Time.deltaTime;
        currentSpeed -= brake * brakeForce * Time.deltaTime;

        if (throttle <= 0f && brake <= 0f && currentSpeed > 0f)
        {
            currentSpeed -= friction * Time.deltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // Apply velocity to Rigidbody
        Vector3 targetVelocity = transform.forward * (currentSpeed / 3.6f);
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Steering
        float turn = steeringInput * turnSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, turn);

        // Get ACTUAL speed from Rigidbody
        float actualSpeed = rb.linearVelocity.magnitude * 3.6f;

        // Update speedometer
        if (speedometerNeedle != null)
        {

            // Clamp speed to speedometer range (0 to 240 km/h)
            actualSpeed = Mathf.Clamp(actualSpeed, 0f, 240f);

            // Map speed directly to rotation
            // Formula: rotation = minRotation + (speed / maxSpeed) * (maxRotation - minRotation)
            float speedRatio = actualSpeed / 240f; // 0 to 1 range
            float needleZ = minRotationZ + speedRatio * (maxRotationZ - minRotationZ);

            speedometerNeedle.localEulerAngles = new Vector3(0, 0, needleZ);
        }

        speedText.text = $"{actualSpeed:F1}";

    }



    public float getCurrentSpeed() {
        return currentSpeed;
    }

    
    
}
