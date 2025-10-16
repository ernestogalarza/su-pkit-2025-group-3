using UnityEngine;
using SharpDX.DirectInput;

public class SimpleFFB : MonoBehaviour
{
    private DirectInput directInput;
    private Joystick steeringWheel;
    private Effect constantForceEffect;

    void Start()
    {
        directInput = new DirectInput();

        foreach (var device in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
        {
            if (device.InstanceName.ToLower().Contains("b692"))
            {
                steeringWheel = new Joystick(directInput, device.InstanceGuid);
                steeringWheel.Acquire();
                Debug.Log("✅ Volante Thrustmaster detectado y listo.");

                // Crear efecto constante (vibración) con magnitud inicial 6000
                var effectParams = new EffectParameters
                {
                    Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets,
                    Duration = int.MaxValue,
                    Gain = 10000,
                    Axes = new int[] { 0 },
                    Directions = new int[] { 0 },
                    StartDelay = 0,
                    SamplePeriod = 0,
                    TriggerButton = -1,
                    TriggerRepeatInterval = int.MaxValue,
                    Parameters = new SharpDX.DirectInput.ConstantForce { Magnitude = 6000 }
                };

                constantForceEffect = new Effect(steeringWheel, EffectGuid.ConstantForce, effectParams);

                break;
            }
        }

        if (steeringWheel == null)
            Debug.LogWarning("⚠️ No se detectó el volante. Conéctalo antes de ejecutar el build.");
    }

    void Update()
    {
        if (steeringWheel == null) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            constantForceEffect.Start(1); // inicia vibración
            Debug.Log("💥 Vibración activada");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            constantForceEffect.Stop(); // detiene vibración
            Debug.Log("🛑 Vibración detenida");
        }
    }

    void OnDestroy()
    {
        steeringWheel?.Unacquire();
        directInput?.Dispose();
    }
}
