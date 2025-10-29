using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.DirectInput;
using UnityEngine;

public class SteeringWheelVibration : MonoBehaviour
{
    private DirectInput directInput;
    private Joystick steeringWheel;
    private Effect currentEffect;
    private int[] actuatorAxes;

    [Header("Vibration Settings")]
    public float vibrationIntensity = 0.7f; // 0.0 to 1.0
    public float vibrationFrequency = 50f; // Hz (vibration speed)
    public EffectType effectType = EffectType.Square; // Type of vibration

    private bool isVibrating = false;

    public enum EffectType
    {
        Sine,
        Square,
        Triangle,
        SawtoothUp,
        SawtoothDown
    }

    void Start()
    {
        InitializeSteeringWheel();
    }

    void InitializeSteeringWheel()
    {
        try
        {
            directInput = new DirectInput();
            var devices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Driving, DeviceEnumerationFlags.AllDevices);
            
            if (devices.Count == 0)
            {
                Debug.LogWarning("⚠️ No steering wheels detected in the system.");
                return;
            }

            steeringWheel = new Joystick(directInput, devices[0].InstanceGuid);
            Debug.Log($"✅ Steering wheel detected: {devices[0].InstanceName}");

            steeringWheel.SetCooperativeLevel(GetUnityWindowHandle(), CooperativeLevel.Background | CooperativeLevel.Exclusive);
            steeringWheel.Properties.BufferSize = 128;
            steeringWheel.Acquire();

            var actuatorList = new List<int>();
            foreach (var obj in steeringWheel.GetObjects())
            {
                if (obj.ObjectId.Flags.HasFlag(DeviceObjectTypeFlags.ForceFeedbackActuator))
                {
                    actuatorList.Add((int)obj.ObjectId);
                    Debug.Log($"🎯 Found force feedback actuator: {obj.Name} (ID: {obj.ObjectId})");
                }
            }
            actuatorAxes = actuatorList.ToArray();

            if (actuatorAxes.Length == 0)
            {
                Debug.LogWarning("⚠️ No force feedback actuators found on this device.");
                return;
            }

            Debug.Log("✅ Force feedback initialization complete!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error initializing the steering wheel: {ex.Message}\nStack: {ex.StackTrace}");
        }
    }

    public void StartVibration(float intensity)
    {
        if (steeringWheel == null)
        {
            Debug.LogWarning("⚠️ Steering wheel not initialized.");
            return;
        }

        if (actuatorAxes == null || actuatorAxes.Length == 0)
        {
            Debug.LogWarning("⚠️ No force feedback actuators available.");
            return;
        }

        if (isVibrating)
        {
            Debug.Log("🔄 Already vibrating, restarting with new parameters...");
            StopVibration();
        }

        try
        {
            Guid effectGuid = GetEffectGuid(effectType);
            
            int magnitude = Mathf.Clamp((int)(intensity * 10000), 0, 10000);
            Debug.Log($"🎮 Creating {effectType} vibration with magnitude: {magnitude}, frequency: {vibrationFrequency} Hz");

            var directions = new int[actuatorAxes.Length];
            for (int i = 0; i < directions.Length; i++)
                directions[i] = 0;

            // Create periodic effect (for vibration)
            var periodicEffect = new PeriodicForce
            {
                Magnitude = magnitude,
                Offset = 0,
                Phase = 0,
                Period = (int)(1000000 / vibrationFrequency) // Convert Hz to microseconds
            };

            var parameters = new EffectParameters
            {
                Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds,
                Duration = int.MaxValue, // Infinite duration until stopped
                Gain = 10000,
                SamplePeriod = 0,
                TriggerButton = -1,
                TriggerRepeatInterval = 0,
                StartDelay = 0,
                Axes = actuatorAxes,
                Directions = directions,
                Parameters = periodicEffect
            };

            currentEffect?.Dispose();
            currentEffect = new Effect(steeringWheel, effectGuid, parameters);
            currentEffect.Start(1);
            isVibrating = true;
            Debug.Log($"✅ {effectType} vibration started successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error starting vibration: {ex.Message}\nStack: {ex.StackTrace}");
        }
    }

    private Guid GetEffectGuid(EffectType type)
    {
        switch (type)
        {
            case EffectType.Sine:
                return EffectGuid.Sine;
            case EffectType.Square:
                return EffectGuid.Square;
            case EffectType.Triangle:
                return EffectGuid.Triangle;
            case EffectType.SawtoothUp:
                return EffectGuid.SawtoothUp;
            case EffectType.SawtoothDown:
                return EffectGuid.SawtoothDown;
            default:
                return EffectGuid.Square;
        }
    }

    public void StopVibration()
    {
        try
        {
            if (currentEffect != null)
            {
                currentEffect.Stop();
                currentEffect.Dispose();
                currentEffect = null;
                Debug.Log("🛑 Vibration stopped.");
            }
            isVibrating = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Error stopping vibration: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        try
        {
            StopVibration();
            steeringWheel?.Unacquire();
            steeringWheel?.Dispose();
            directInput?.Dispose();
            Debug.Log("🧹 DirectInput resources cleaned up.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Error releasing resources: {ex.Message}");
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private IntPtr GetUnityWindowHandle() => GetActiveWindow();
}
