using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario para usar Text o TextMeshProUGUI
using TMPro;
public class CarController : MonoBehaviour
{

//public float speed = 5.0f;
//public float turnSpeed = 20.0f;
//public float horizontalInput;
//public float forwardInput;

  //  public bool isMoving = false;

   // public Transform steeringWheel;
  //  public float steeringLimitAngle = 100f;
  //  public float steeringRotationSpeed = 60f;
  //  public float returnSpeed = 2f;

  
  public float acceleration = 20f;     // km/h por segundo
  public float brakeForce = 30f;       // km/h por segundo
  public float maxSpeed = 100f;        // km/h
  public float turnSpeed = 60f;        // grados por segundo
  public float drag = 0.98f;           // fricción
//  private Rigidbody rb;
    private float currentSpeed = 0f;     // km/h
    private Rigidbody rb;

    private Rigidbody carRb;
    public TextMeshProUGUI speedText;               // Referencia al texto en la UI (opcional)

    private SteeringWheelManager steeringWheelManager;
    private float wheelDirection = 0f;


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
      //  UpdateSpeedDisplay();
    }
    
    void HandleMovement()
    {
        speedText.text = steeringWheelManager.getStaringWheelDirection().ToString();

        wheelDirection = steeringWheelManager.getStaringWheelDirection();

        // ↑ Acelerar
        if (Input.GetKey(KeyCode.UpArrow))
        {
            currentSpeed += acceleration * Time.deltaTime;
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
        /*
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            turn = -turnSpeed * Time.deltaTime;
           // speedText.text = turn.ToString();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            turn = turnSpeed * Time.deltaTime;

         //  speedText.text = turn.ToString();
        } */

        // Aplicar rotación
        transform.Rotate(0f, turn, 0f);

        // Convertir km/h → m/s (1 km/h = 0.27778 m/s)
        float speedInMetersPerSec = currentSpeed * 0.27778f;

        // Mover el coche
        rb.MovePosition(rb.position + transform.forward * speedInMetersPerSec * Time.deltaTime);

        // Desaceleración natural
        currentSpeed *= drag;
    }

    void UpdateSpeedDisplay()
    {
        if (speedText != null)
        {
            float shownSpeed = Mathf.Abs(currentSpeed);
            speedText.text = shownSpeed.ToString("F1") + " km/h";
        }
    }
    
    
    
}
