using UnityEngine;
using System.Collections;
using System;

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
    private int idAlert = 1;
    private DateTime startAlert;
    private DateTime endAlert;

    // NEW: Flag to enable/disable the alert system
    private bool isAlertActive = true;

    public void setSpeedAlert(float newSpeed)
    {
        speedThreshold = newSpeed;
    }

    /// <summary>
    /// Disables the speed alert system (for No Rules signs)
    /// </summary>
    public void DisableSpeedAlert()
    {
        isAlertActive = false;

        // Hide current alert if visible
        if (isAlertVisible)
        {
            HideAlert();
        }

        // Stop any pending alert coroutine
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
            alertCoroutine = null;
        }

        Debug.Log("🚫 Speed alerts DISABLED - No speed limit enforcement");
    }

    /// <summary>
    /// Re-enables the speed alert system
    /// </summary>
    public void EnableSpeedAlert()
    {
        isAlertActive = true;
        Debug.Log("✅ Speed alerts ENABLED - Speed limit enforcement active");
    }

    void Start()
    {
        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();

        // Verify references
        if (steeringWheelVibration == null)
        {
            // Debug.LogWarning("⚠️ SteeringWheelVibration reference is not assigned in AlertManager!");
        }
    }

    void Update()
    {
        // Only run alert logic if system is active
        if (!isAlertActive)
            return;

        float currentSpeed = carController.getCurrentSpeed();
        UpdateAlert(currentSpeed);
    }

    public void UpdateAlert(float currentSpeed)
    {
        // Only check speed if alerts are enabled
        if (!isAlertActive)
            return;

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
        // Debug.Log($"🚀 Coroutine iniciada a tiempo {Time.time}");
        yield return new WaitForSeconds(delayBeforeShow);
        // Debug.Log($"⏰ Mostrando alerta a tiempo {Time.time}");
        ShowAlert();
        startAlert = DateTime.Now;
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
            // Debug.Log("🚨 Alerta activada: velocidad superior a límite + vibración iniciada");
        }
        else
        {
            // Debug.LogWarning("⚠️ SteeringWheelVibration component not assigned!");
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
            // Debug.Log("✅ Alerta desactivada: velocidad dentro del rango + vibración detenida");
        }

        // idAlert++;
        endAlert = DateTime.Now;
        isAlertVisible = false;
    }

    public bool getIsAlertVisible()
    {
        return isAlertVisible;
    }

    public int getIdAlert()
    {
        return idAlert;
    }

    public DateTime getTimeStartAlert()
    {
        return startAlert;
    }

    public DateTime getTimeEndAlert()
    {
        return endAlert;
    }

    public void setIdAlert(int id)
    {
        idAlert = id;
    }

    public bool IsAlertSystemActive()
    {
        return isAlertActive;
    }
}
