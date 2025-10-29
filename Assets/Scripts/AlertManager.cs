using UnityEngine;
using System.Collections;

public class AlertManager : MonoBehaviour
{
    private CarController carController;

    [Header("References")]
    public GameObject visualPanel; // Panel a mostrar u ocultar
    public AudioSource alertAudio; // Audio de alerta
    public SteeringWheelVibration steeringWheelVibration; // Referencia al componente de vibración

    [Header("Settings")]
    public float speedThreshold = 60f; // Límite de velocidad para mostrar alerta
    public float delayBeforeShow = 1f; // Retraso en segundos antes de mostrar

    private bool isAlertVisible = false;
    private Coroutine alertCoroutine;

    void Start()
    {
        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();
        
        // Verify references
        if (steeringWheelVibration == null)
        {
            Debug.LogWarning("⚠️ SteeringWheelVibration reference is not assigned in AlertManager!");
        }
    }

    void Update()
    {
        float currentSpeed = carController.getCurrentSpeed();
        UpdateAlert(currentSpeed);
    }

    public void UpdateAlert(float currentSpeed)
    {
        if (currentSpeed > speedThreshold + 1f)
        {
            // Si supera el límite y no se ha mostrado aún, iniciar temporizador
            if (!isAlertVisible && alertCoroutine == null)
            {
                alertCoroutine = StartCoroutine(ShowAfterDelay());
            }
        }
        else if (currentSpeed < speedThreshold - 2f)
        {
            if (alertCoroutine != null)
            {
                StopCoroutine(alertCoroutine);
                alertCoroutine = null;
            }
            if (isAlertVisible)
                HideAlert();
        }
    }

    private IEnumerator ShowAfterDelay()
    {
        Debug.Log($"🚀 Coroutine iniciada a tiempo {Time.time}");
        yield return new WaitForSeconds(delayBeforeShow);
        Debug.Log($"⏰ Mostrando alerta a tiempo {Time.time}");
        ShowAlert();
        alertCoroutine = null;
    }

    private void ShowAlert()
    {
        if (visualPanel != null)
            visualPanel.SetActive(true);
        
        if (alertAudio != null && !alertAudio.isPlaying)
            alertAudio.Play();

        // Activar vibración del volante
        if (steeringWheelVibration != null)
        {
            steeringWheelVibration.StartVibration(steeringWheelVibration.vibrationIntensity);
            Debug.Log("🚨 Alerta activada: velocidad superior a límite + vibración iniciada");
        }
        else
        {
            Debug.LogWarning("⚠️ SteeringWheelVibration component not assigned!");
        }

        isAlertVisible = true;
    }

    private void HideAlert()
    {
        if (visualPanel != null)
            visualPanel.SetActive(false);
        
        if (alertAudio != null && alertAudio.isPlaying)
            alertAudio.Stop();

        // Detener vibración del volante
        if (steeringWheelVibration != null)
        {
            steeringWheelVibration.StopVibration();
            Debug.Log("✅ Alerta desactivada: velocidad dentro del rango + vibración detenida");
        }

        isAlertVisible = false;
    }
}
