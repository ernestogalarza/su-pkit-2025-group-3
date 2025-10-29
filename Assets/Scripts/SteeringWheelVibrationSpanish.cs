using System;
using System.Runtime.InteropServices;
using SharpDX.DirectInput;
using UnityEngine;

public class SteeringWheelVibration : MonoBehaviour
{
    private DirectInput directInput;
    private Joystick steeringWheel;
    private EffectInfo constantForceEffect;
    private Effect effect;

    void Start()
    {
        try
        {
            directInput = new DirectInput();

            // Buscar dispositivos de tipo Driving (volante)
            var devices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Driving, DeviceEnumerationFlags.AllDevices);
            if (devices.Count == 0)
            {
                Debug.LogWarning("⚠️ No se detectaron volantes en el sistema.");
                return;
            }

            steeringWheel = new Joystick(directInput, devices[0].InstanceGuid);
            Debug.Log($"✅ Volante detectado: {devices[0].InstanceName}");

            // Configurar modo de acceso exclusivo (necesario para FFB)
            steeringWheel.SetCooperativeLevel(GetUnityWindowHandle(),
                CooperativeLevel.Foreground | CooperativeLevel.Exclusive);

            steeringWheel.Properties.BufferSize = 128;
            steeringWheel.Acquire();

            Debug.Log($"Axes: {steeringWheel.Capabilities.AxeCount}, Buttons: {steeringWheel.Capabilities.ButtonCount}, POVs: {steeringWheel.Capabilities.PovCount}");

            // Buscar efecto de fuerza constante (FFB)
            foreach (var eff in steeringWheel.GetEffects())
            {
                Debug.Log($"🎢 Effect GUID: {eff.Guid}, Type: {eff.Type}");
                if (eff.Guid == EffectGuid.ConstantForce)
                {
                    constantForceEffect = eff;
                    Debug.Log($"✅ ConstantForce effect disponible: {eff.Guid}");
                    break;
                }
            }

            if (constantForceEffect.Guid == Guid.Empty)
            {
                Debug.LogWarning("⚠️ El dispositivo no soporta efectos ConstantForce.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error al inicializar el volante: {ex.Message}");
        }

        Debug.Log("ℹ️ SteeringWheelVibration está deshabilitado en el editor (solo funciona en build).");

    }
    void Update()
    {
        StartVibration(1f);
    }

    /// <summary>
    /// Inicia la vibración (solo en build)
    /// </summary>
    public void StartVibration(float intensity)
    {
        if (steeringWheel == null || constantForceEffect.Guid == Guid.Empty)
        {
            Debug.LogWarning("⚠️ No hay volante o efecto disponible para vibrar.");
            return;
        }

        try
        {
            int magnitude = Mathf.Clamp((int)(intensity * 10000), 0, 10000);
            var direction = new int[] { 0 };

            var constantForce = new SharpDX.DirectInput.ConstantForce { Magnitude = magnitude };

            var parameters = new EffectParameters
            {
                Flags = EffectFlags.Cartesian , // ⚡ clave
                Duration = int.MaxValue,
                Gain = 10000,
                SamplePeriod = 0,
                TriggerButton = -1,
                TriggerRepeatInterval = int.MaxValue,
                StartDelay = 0,
                Axes = new int[] { 0 },      // eje 0
                Directions = new int[] { 0 },// dirección 0
                Parameters = constantForce
            };

            effect = new Effect(steeringWheel, constantForceEffect.Guid, parameters);
            effect.Start(1);
            Debug.Log($"✅ Vibración iniciada con intensidad {intensity:F2}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"⚠️ Error al iniciar vibración: {ex.Message}");
        }

        Debug.Log("🧩 Vibración ignorada (solo funciona en build).");

    }

    /// <summary>
    /// Detiene la vibración actual (solo en build)
    /// </summary>
    public void StopVibration()
    {
        try
        {
            effect?.Stop();
            Debug.Log("🛑 Vibración detenida.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Error al detener vibración: {ex.Message}");
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
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Error al liberar recursos: {ex.Message}");
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private IntPtr GetUnityWindowHandle()
    {
        return GetActiveWindow();
    }

}
