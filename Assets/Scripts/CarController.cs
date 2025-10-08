using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{

    public float speed = 5.0f;
    public float turnSpeed = 20.0f;
    public float horizontalInput;
    public float forwardInput;

    public bool isMoving = false;

    public Transform steeringWheel;
    public float steeringLimitAngle = 100f;
    public float steeringRotationSpeed = 60f;
    public float returnSpeed = 2f;




    private Rigidbody carRb;

    // Start is called before the first frame update
    void Start()
    {
        Input.
        carRb = GetComponent<Rigidbody>();
        carRb.drag = 2f; // You can adjust this value
        carRb.angularDrag = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        UpdateSteerWheel(horizontalInput);

        if(Input.GetKeyDown(KeyCode.Space))   
        {
            isMoving = !isMoving;


            // Move the vehicle forward based on vertical input
            //carRb.AddForce(transform.forward * Time.deltaTime * speed * forwardInput, ForceMode.Impulse);
            
            //transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        }

        if(isMoving)
        {
            //transform.Translate(Vector3.forward * Time.deltaTime * speed);
            carRb.velocity = transform.forward * speed;
        }
        else
        {
            carRb.velocity = Vector3.zero;
        }
        transform.Rotate(Vector3.up, turnSpeed * horizontalInput * Time.deltaTime);

    }

    private void UpdateSteerWheel(float horizontalInput)
    {
        if (horizontalInput != 0f)
        {
            steeringWheel.Rotate(Vector3.forward, -horizontalInput * steeringRotationSpeed * Time.deltaTime);
        }
        else if (horizontalInput == 0f && steeringWheel.localEulerAngles.z > 5f)
        {
            // Smoothly return the steering wheel to the original position when no input
            float currentZ = steeringWheel.localEulerAngles.z;
            // Ensure the angle is between 0-360 degrees (as Euler angles can wrap)
            if (currentZ > 180f)
                currentZ -= 360f;
            // Gradually return to 0 using Lerp, for smooth transition
            float smoothRotation = Mathf.Lerp(currentZ, 0f, Time.deltaTime * returnSpeed);
            steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x,
                                                         steeringWheel.localEulerAngles.y,
                                                         smoothRotation);


            //steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x,
            //                                            steeringWheel.localEulerAngles.y,
            //                                            0f);
        }
    }
}
