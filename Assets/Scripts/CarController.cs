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

    private void acceleratorArrowKey(float value) {

        if (value > 0)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
    }

    private void acceleratorSteeringWheel(float value)
    {

            currentSpeed += acceleration*Time.deltaTime * -value;

            if(currentSpeed<0) currentSpeed= 0;
    }


    void speedWithSteeringWheel() {

        // Leer valores del volante y pedales
        steeringInput = steeringWheelManager.getWheelDirection(); // -1 izquierda, 1 derecha
        throttleInput = steeringWheelManager.getThrottle();       // -1 presionado, 1 libre
        brakeInput = steeringWheelManager.getBrake();             // -1 presionado, 1 libre

        // Normalizar acelerador: -1 (presionado) → 1 (acelera al máximo)
        float throttle = Mathf.InverseLerp(1f, -1f, throttleInput);

        // Freno invertido: 0 presionado → 1, 1 sin presionar → 0
        float brake = 1f - brakeInput;

        // Aplicar aceleración
        currentSpeed += throttle * acceleration * Time.deltaTime;

        // Aplicar freno
        currentSpeed -= brake * brakeForce * Time.deltaTime;

        // Desaceleración por fricción cuando no se pisa throttle ni brake
        if (throttle <= 0f && brake <= 0f && currentSpeed > 0f)
        {
            currentSpeed -= friction * Time.deltaTime;
        }

        // Limitar velocidad
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // Mover el coche hacia adelante
        transform.Translate(Vector3.forward * (currentSpeed / 3.6f) * Time.deltaTime);

        // Aplicar giro
        float turn = steeringInput * turnSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, turn);

        // Actualizar velocímetro
        if (speedometerNeedle != null)
        {
            float needleZ = Mathf.Lerp(minRotationZ, maxRotationZ, currentSpeed / maxSpeedometerSpeed);
            Vector3 rotation = speedometerNeedle.localEulerAngles;
            rotation.z = needleZ;
            speedometerNeedle.localEulerAngles = rotation;
        }

        // Mostrar datos en consola
        Debug.Log($"Speed: {currentSpeed:F1} km/h | Throttle: {throttle:F2} | Brake: {brake:F2} | Wheel: {steeringInput:F2}");

        speedText.text = $"{currentSpeed:F1} km/h";
    }


    
    
}
