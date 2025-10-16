using SharpDX.DirectInput;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SteeringWheelManager : MonoBehaviour
{

    [Header("Valores de entrada")]
    [Range(-1f, 1f)] public float steering;   // -1 izquierda, 1 derecha
    [Range(0f, 1f)] public float throttle;    // 0 no presionado, 1 fondo
    [Range(0f, 1f)] public float brake;       // 0 no presionado, 1 fondo
    [Range(0f, 1f)] public float clutch;      // opcional

    private Gamepad wheelDevice; // TS-XW aparecerá como un dispositivo tipo Gamepad o Joystick
    private UnityEngine.InputSystem.Joystick joystick;
   // public TextMeshProUGUI speedText;
    private float steerValue;
    private DirectInput directInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joystick = UnityEngine.InputSystem.Joystick.current;

        if (joystick == null)
        {
            Debug.LogWarning("⚠️ No se detectó ningún volante o joystick. Conéctalo antes de ejecutar Unity.");
            return;
        }

        Debug.Log($" Dispositivo detectado: {joystick.displayName}");

        // Mostrar todos los ejes disponibles (te ayuda a saber qué eje usa cada pedal)
        foreach (var control in joystick.allControls)
        {
            if (control is AxisControl axis)
                Debug.Log($"Eje detectado → {axis.name}");
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (joystick == null) return;

        // Intentar leer el eje X (rotación del volante)
         steerValue = 0f;

        var control = joystick.TryGetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("x");
        if (control != null)
        {
            //ConstantForce
            steerValue = control.ReadValue();
        }
        else if (joystick.stick != null)
        {
            steerValue = joystick.stick.x.ReadValue();
           // speedText.text = steerValue.ToString();
        }

        // Si giras el timón hacia la izquierda o derecha, mostrar en consola
        if (steerValue < -0.1f)
        {
            Debug.Log($"↩️ Girando a la IZQUIERDA ({steerValue:F2})");
           // speedText.text = steerValue.ToString();
                }
        else if (steerValue > 0.1f)
        {
            Debug.Log($"↪️ Girando a la DERECHA ({steerValue:F2})");
          //  speedText.text = steerValue.ToString();
        }
    }


    public float getStaringWheelDirection() {
        return steerValue;
    } 

    // Intenta leer un eje usando varios nombres posibles
    float ReadAxis(params string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            var control = joystick.TryGetChildControl<AxisControl>(name);
            if (control != null)
                return control.ReadValue();
        }
        return 0f;
    }

    // Convierte el valor del pedal (-1 a 1) → (0 a 1)
    float NormalizePedal(float value)
    {
        return Mathf.InverseLerp(1f, -1f, value);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 300, 30), $"Steering: {steering:F2}");
        GUI.Label(new Rect(20, 40, 300, 30), $"Throttle: {throttle:F2}");
        GUI.Label(new Rect(20, 60, 300, 30), $"Brake: {brake:F2}");
        GUI.Label(new Rect(20, 80, 300, 30), $"Clutch: {clutch:F2}");
    }



}
