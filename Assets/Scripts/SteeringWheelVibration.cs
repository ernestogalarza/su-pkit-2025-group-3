using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.DirectInput;
using UnityEngine;

public class SteeringWheelVibration : MonoBehaviour
{
    private DirectInput directInput;
    private Joystick steeringWheel;
    private EffectInfo constantForceEffect;
    private Effect currentEffect;
    private int[] actuatorAxes;

    [Header("Speed Limit Settings")]
    public float speedLimit = 60f;
    public float vibrationIntensity = 0.7f; // 0.0 to 1.0
    public float vibrationDuration = 0.5f; // seconds

    private Rigidbody vehicleRigidbody;
    private bool isOverSpeedLimit = false;
    private bool isVibrating = false;

    void Start()
    {
        vehicleRigidbody = GetComponent<Rigidbody>();

#if !UNITY_EDITOR
        InitializeSteeringWheel();
#else
        Debug.Log("ℹ️ SteeringWheelVibration is disabled in the editor (only works in build).");
#endif
    }

    void InitializeSteeringWheel()
    {
        try
        {
            directInput = new DirectInput();

            // Search for Driving devices (steering wheel)
            var devices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Driving, DeviceEnumerationFlags.AllDevices);

            if (devices.Count == 0)
            {
                Debug.LogWarning("⚠️ No steering wheels detected in the system.");
                return;
            }

            steeringWheel = new Joystick(directInput, devices[0].InstanceGuid);
            Debug.Log($"✅ Steering wheel detected: {devices[0].InstanceName}");

            // Configure exclusive access (necessary for FFB)
            steeringWheel.SetCooperativeLevel(GetUnityWindowHandle(),
                CooperativeLevel.Background | CooperativeLevel.Exclusive);

            steeringWheel.Properties.BufferSize = 128;
            steeringWheel.Acquire();

            Debug.Log($"Axes: {steeringWheel.Capabilities.AxeCount}, Buttons: {steeringWheel.Capabilities.ButtonCount}, POVs: {steeringWheel.Capabilities.PovCount}");

            // Get force feedback actuator axes
            var actuatorList = new List<int>();
            foreach (var obj in steeringWheel.GetObjects())
            {
                if (obj.ObjectId.Flags.HasFlag(DeviceObjectTypeFlags.ForceFeedbackActuator))
                {
                    actuatorList.Add((int)obj.ObjectId);
                    Debug.Log($"🎯 FFB Actuator found: {obj.Name}, ObjectId: {obj.ObjectId}");
                }
            }

            actuatorAxes = actuatorList.ToArray();

            if (actuatorAxes.Length == 0)
            {
                Debug.LogWarning("⚠️ No force feedback actuators found on this device.");
                return;
            }

            // Find ConstantForce effect
            foreach (var eff in steeringWheel.GetEffects())
            {
                Debug.Log($"🎢 Effect GUID: {eff.Guid}, Type: {eff.Type}");

                if (eff.Guid == EffectGuid.ConstantForce)
                {
                    constantForceEffect = eff;
                    Debug.Log($"✅ ConstantForce effect available: {eff.Guid}");
                    break;
                }
            }

            if (constantForceEffect.Guid == Guid.Empty)
            {
                Debug.LogWarning("⚠️ The device does not support ConstantForce effects.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error initializing the steering wheel: {ex.Message}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartVibration(1f); // Max intensity vibration
        }
    }


    /// <summary>
    /// Starts the vibration (only in build)
    /// </summary>
    public void StartVibration(float intensity)
    {

        if (steeringWheel == null || constantForceEffect.Guid == Guid.Empty || actuatorAxes == null || actuatorAxes.Length == 0)
        {
            Debug.LogWarning("⚠️ No steering wheel or effect available for vibration.");
            return;
        }

        if (isVibrating)
        {
            StopVibration(); // Stop previous effect first
        }

        try
        {
            int magnitude = Mathf.Clamp((int)(intensity * 10000), 0, 10000);

            // Create direction array matching the number of actuator axes
            var directions = new int[actuatorAxes.Length];
            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = 0; // Direction value (0 = neutral, can be adjusted)
            }

            var constantForce = new SharpDX.DirectInput.ConstantForce { Magnitude = magnitude };

            var parameters = new EffectParameters
            {
                Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds, // Use ObjectIds, not ObjectOffsets
                Duration = (int)(vibrationDuration * 1000000), // Convert to microseconds
                Gain = 10000,
                SamplePeriod = 0,
                TriggerButton = -1,
                TriggerRepeatInterval = 0,
                StartDelay = 0,
                Axes = actuatorAxes, // Use actual actuator axes
                Directions = directions, // Must match axes length
                Parameters = constantForce
            };

            // Dispose previous effect if exists
            currentEffect?.Dispose();

            // Create and start new effect
            currentEffect = new Effect(steeringWheel, constantForceEffect.Guid, parameters);
            currentEffect.Start(1);

            isVibrating = true;
            Debug.Log($"✅ Vibration started with intensity {intensity:F2}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"⚠️ Error starting vibration: {ex.Message}\nStack: {ex.StackTrace}");
        }

        Debug.Log("🧩 Vibration ignored (only works in build).");

    }

    /// <summary>
    /// Stops the current vibration (only in build)
    /// </summary>
    public void StopVibration()
    {
#if !UNITY_EDITOR
        try
        {
            if (currentEffect != null)
            {
                currentEffect.Stop();
                currentEffect.Dispose();
                currentEffect = null;
            }
            
            isVibrating = false;
            Debug.Log("🛑 Vibration stopped.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Error stopping vibration: {ex.Message}");
        }
#endif
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR
        try
        {
            StopVibration();
            steeringWheel?.Unacquire();
            steeringWheel?.Dispose();
            directInput?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Error releasing resources: {ex.Message}");
        }
#endif
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private IntPtr GetUnityWindowHandle()
    {
        return GetActiveWindow();
    }
}
