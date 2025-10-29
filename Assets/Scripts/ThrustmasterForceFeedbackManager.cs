using UnityEngine;
using SharpDX.DirectInput;
using System;
using Unity.VisualScripting;

public class ThrustmasterForceFeedbackManager : MonoBehaviour
{
    private DirectInput directInput;
    private Joystick steeringWheel;
    private EffectInfo constantForceEffectInfo;
    private Effect forceEffect;

    [Range(0f, 1f)]
    public float vibrationStrength = 0.5f; // Nivel de vibración (0 = sin fuerza, 1 = máxima fuerza)

    void Start()
    {
        InitializeWheel();
    }

    private void InitializeWheel()
    {
        directInput = new DirectInput();

        // Buscar el volante conectado
        var devices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

        foreach (var deviceInstance in devices)
        {
            Debug.Log($"OBJETO DETECTADO: {deviceInstance.InstanceName}");

            // Puedes ajustar el nombre si tu dispositivo aparece con un nombre distinto
            if (deviceInstance.InstanceName.Contains("TS-XW") || deviceInstance.InstanceName.Contains("B692"))
            {
                steeringWheel = new Joystick(directInput, deviceInstance.InstanceGuid);
                steeringWheel.Acquire();
                Debug.Log($"✅ Volante detectado: {deviceInstance.InstanceName}");
                // 🔍 Aquí colocas la línea que te interesa
                Debug.Log("Axes: " + steeringWheel.Capabilities.AxeCount);

                // También puedes imprimir otros datos útiles
                Debug.Log("Botones: " + steeringWheel.Capabilities.ButtonCount);
                Debug.Log("POVs: " + steeringWheel.Capabilities.PovCount);


                break;
            }
        }

        if (steeringWheel == null)
        {
            Debug.LogError("❌ No se encontró el volante Thrustmaster TS-XW.");
            return;
        }

        foreach (var effectInfo in steeringWheel.GetEffects())
        {
            //Debug.Log("Effect: " + effectInfo.Type);
            Debug.Log($"🎢 Effect GUID: {effectInfo.Guid}, Type: {effectInfo.Type}");

            if (effectInfo.Type.HasFlag(EffectType.ConstantForce))
            {
                constantForceEffectInfo = effectInfo;
                Debug.Log("🎮 ConstantForce disponible y listo para usar.");
                break;
            }
        }

        if (constantForceEffectInfo.Guid == Guid.Empty)
        {
            Debug.LogError("❌ El dispositivo no soporta efectos ConstantForce.");
        }
    }

    void Update()
    {
        StartVibration(0.5f);
    }

    /// <summary>
    /// Activa la vibración del volante con cierta intensidad.
    /// </summary>
    /// <param name="intensity">Entre 0 y 1</param>
    public void StartVibration(float intensity)
    {
        if (steeringWheel == null || constantForceEffectInfo.Guid == Guid.Empty)
            return;

        try
        {
            // Crear el efecto ConstantForce
            var constantForce = new SharpDX.DirectInput.ConstantForce();
            constantForce.Magnitude = (int)(intensity * 10000); // Rango de 0 a 10000

            var effectParams = new EffectParameters
            {
                Duration = int.MaxValue,
                Gain = 10000,
                SamplePeriod = 0,
                TriggerButton = -1,
                TriggerRepeatInterval = int.MaxValue,
                Flags = EffectFlags.ObjectOffsets | EffectFlags.Cartesian,
                StartDelay = 0,
                Parameters = constantForce
            };

            forceEffect = new Effect(steeringWheel, constantForceEffectInfo.Guid, effectParams);
            forceEffect.Start(1);
            Debug.Log($"💥 Vibración iniciada con intensidad {intensity}");
        }
        catch (System.Exception ex)
        {

            Debug.LogError($"⚠️ Error al iniciar vibración: {ex.Message}");
        }
    }

    /// <summary>
    /// Detiene la vibración.
    /// </summary>
    public void StopVibration()
    {
        try
        {
            if (forceEffect != null)
            {
                forceEffect.Stop();
                forceEffect.Dispose();
                forceEffect = null;
                Debug.Log("🛑 Vibración detenida.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"⚠️ Error al detener vibración: {ex.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        StopVibration();
        steeringWheel?.Unacquire();
        steeringWheel?.Dispose();
        directInput?.Dispose();
    }
}