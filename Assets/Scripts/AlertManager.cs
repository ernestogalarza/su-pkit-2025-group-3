using UnityEngine;
using System.Collections;

public class AlertManager : MonoBehaviour
{


    private CarController carController;

    [Header("References")]
    public GameObject visualPanel;  // Panel a mostrar u ocultar
    public AudioSource alertAudio;   // Audio de alerta

    [Header("Settings")]
    public float speedThreshold = 60f;   // Límite de velocidad para mostrar alerta
    public float delayBeforeShow = 1f;   // Retraso en segundos antes de mostrar

    private bool isAlertVisible = false;
    private Coroutine alertCoroutine;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        float currentSpeed = carController.getCurrentSpeed();
       // Debug.Log($"Velocidad: {currentSpeed:F1} km/h");

        UpdateAlert(carController.getCurrentSpeed());
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


        isAlertVisible = true;
        Debug.Log("Alerta activada: velocidad superior a límite");
    }

    private void HideAlert()
    {
        if (visualPanel != null)
            visualPanel.SetActive(false);

        if (alertAudio != null && alertAudio.isPlaying)
            alertAudio.Stop();

        isAlertVisible = false;
        Debug.Log("✅ Alerta desactivada: velocidad dentro del rango");
    }
}
