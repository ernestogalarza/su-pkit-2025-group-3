using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleController : MonoBehaviour
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoving = !isMoving;


            // Move the vehicle forward based on vertical input
            //carRb.AddForce(transform.forward * Time.deltaTime * speed * forwardInput, ForceMode.Impulse);

            //transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        }

        if (isMoving)
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

    public void UpdateSteerWheel(float horizontalInput)
    {
        // Check if there's any horizontal input (i.e., user is turning the wheel)
        if (horizontalInput != 0f)
        {
            // Calculate the new y-rotation based on input
            float newYRotation = steeringWheel.localEulerAngles.y + horizontalInput * steeringRotationSpeed * Time.deltaTime;

            // Handle angles greater than 270 degrees to ensure smooth transition
            if (newYRotation > 270f)
                newYRotation -= 360f;

            // Ensure the angle is between 60 and 120 degrees (90 ± 30)
            newYRotation = Mathf.Clamp(newYRotation, 60f, 120f);

            // Apply the clamped rotation
            steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x, newYRotation, steeringWheel.localEulerAngles.z);
        }
        else
        {
            // Smoothly return the steering wheel to 90 degrees (the original position) when no input
            float currentY = steeringWheel.localEulerAngles.y;

            // Handle angles greater than 270 degrees to ensure smooth transition
            if (currentY > 270f)
                currentY -= 360f;

            // Gradually return to 90 using Lerp for smooth transition
            float smoothRotation = Mathf.Lerp(currentY, 90f, Time.deltaTime * returnSpeed);

            // Clamp the return value to ensure it's between 60 and 120 degrees
            smoothRotation = Mathf.Clamp(smoothRotation, 60f, 120f);

            // Apply the smoothed rotation
            steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x, smoothRotation, steeringWheel.localEulerAngles.z);
        }
    }

    /*public void UpdateSteerWheel(float horizontalInput)
    {
        if (horizontalInput != 0f)
        {
            // Calculate the new y-rotation based on input
            float newYRotation = steeringWheel.localEulerAngles.y + horizontalInput * steeringRotationSpeed * Time.deltaTime;

            // Ensure the angle is between 60 and 120 degrees (90 ± 30)
            newYRotation = Mathf.Clamp(newYRotation, 60f, 120f);

            steeringWheel.Rotate(Vector3.up, horizontalInput * steeringRotationSpeed * Time.deltaTime);
        }
        else if (horizontalInput == 0f && steeringWheel.localEulerAngles.y > 5f)
        {
            // Smoothly return the steering wheel to the original position when no input
            float currentY = steeringWheel.localEulerAngles.y;

            // Ensure the angle is between 0-360 degrees (as Euler angles can wrap)
            if (currentY > 270f)
                currentY -= 360f;

            // Gradually return to 90 using Lerp, for smooth transition
            float smoothRotation = Mathf.Lerp(currentY, 90f, Time.deltaTime * returnSpeed);

            // Clamp the return value to ensure it's between 60 and 120 degrees
            smoothRotation = Mathf.Clamp(smoothRotation, 60f, 120f);

            // Apply the smoothed rotation
            steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x,
                                                         smoothRotation,
                                                         steeringWheel.localEulerAngles.z);


            //steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x,
            //                                            steeringWheel.localEulerAngles.y,
            //                                            0f);
        } 
    }*/
}