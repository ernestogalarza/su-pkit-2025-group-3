using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario para usar Text o TextMeshProUGUI
using TMPro;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
public class CarController : MonoBehaviour
{

    public bool handlerKeyboard = true;
    public float acceleration = 20f;     // km/h por segundo
    public float brakeForce = 30f;       // km/h por segundo
    public float maxSpeed = 100f;        // km/h
    public float turnSpeed = 60f;        // grados por segundo
    public float drag = 0.98f;           // fricción
    private float currentSpeed = 0f;     // km/h
    private Rigidbody rb;

    private Rigidbody carRb;
    public TextMeshProUGUI speedText;               // Referencia al texto en la UI (opcional)

    private SteeringWheelManager steeringWheelManager;
    private float wheelDirection = 0f;

    private VehicleControls controls;
    private float wheel;
    private float throttle;




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
         HandleMovement();
    }

    private void acceleratorArrowKey(float value) {

        if (value > 0)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
    }

    private void acceleratorSteeringWheel(float value)
    {

            currentSpeed += acceleration * Time.deltaTime * -value;

            if(currentSpeed<0) currentSpeed= 0;
    }

    void HandleMovement()
    {
        //Throttle
         speedText.text = steeringWheelManager.getThrottle().ToString();
         wheelDirection = steeringWheelManager.getWheelDirection();

        float throttle = steeringWheelManager.getThrottle();

        Debug.Log($"throttle: {throttle:F2}");

        if (handlerKeyboard)
        {
            acceleratorArrowKey(throttle);

        }
        else
        {
            acceleratorSteeringWheel(throttle);
        }
       


        // ↓ Frenar / retroceder
        if (Input.GetKey(KeyCode.DownArrow))
        {
            currentSpeed -= brakeForce * Time.deltaTime;
        } 

        // Limitar velocidad (permitiendo un poco en reversa)
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed / 2, maxSpeed);

        // ← / → Girar
        float turn = 0f;
        turn = wheelDirection;
        // Aplicar rotación
        transform.Rotate(0f, turn, 0f);

        // Convertir km/h → m/s (1 km/h = 0.27778 m/s)
        float speedInMetersPerSec = currentSpeed * 0.27778f;

        // Mover el coche
        rb.MovePosition(rb.position + transform.forward * speedInMetersPerSec * Time.deltaTime);

        // Desaceleración natural
        currentSpeed *= drag;
    }

    
    
}
