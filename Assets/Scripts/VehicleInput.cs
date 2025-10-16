using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    private VehicleControls controls;
    public TextMeshProUGUI speedText;
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
        //  brake = controls.Vehicle.Brake.ReadValue<float>();
        Debug.Log($"Wheel: {wheel:F2}");
        speedText.text = throttle.ToString();
    }

    public float getWheelDirection() {
        return wheel;
    }

    public float getThrottle()
    {
        return throttle;
    }

    public float getBrake()
    {
        return wheel;
    }
}
