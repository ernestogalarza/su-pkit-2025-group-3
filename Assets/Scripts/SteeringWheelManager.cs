using SharpDX.DirectInput;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SteeringWheelManager : MonoBehaviour
{

    public TextMeshProUGUI speedText;  


    private VehicleControls controls;
    private float wheel;
    private float throttle;
    private float brake;

    private void Awake()
    {
        controls = new VehicleControls();
    }

    private void OnEnable()
    {
        controls.Vehicle.Enable();
    }

    private void OnDisable()
    {
        controls.Vehicle.Disable();
    }

    private void Update()
    {
        wheel = controls.Vehicle.Steering.ReadValue<float>();
        throttle = controls.Vehicle.Throttle.ReadValue<float>();
        brake = controls.Vehicle.Brake.ReadValue<float>();
        // Debug.Log($"wheel//: {wheel:F2}");
        //     Debug.Log($"brake//: {brake:F2}");
        //  Debug.Log($"throttle: {throttle:F2}");
        // speedText.text = throttle.ToString();

       // speedText.text = $"{throttle:F1}";
    }

    public float getWheelDirection()
    {
        return wheel;
    }

    public float getThrottle()
    {
        return throttle;
    }

    public float getBrake()
    {
        return brake;
    }



}
