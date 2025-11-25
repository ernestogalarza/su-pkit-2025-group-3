using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class AlertManager : MonoBehaviour
{
    private CarController carController;

    [Header("References")]
    public GameObject visualPanel; // visual alert
    public AudioSource alertAudio; // Audio alert
    public SteeringWheelVibration steeringWheelVibration; // vibration component

    [Header("Settings")]
    public float speedThreshold = 60f; // Speed limit threshold
    public float delayBeforeShow = 1f; // Delay before showing alert

    private bool isAlertVisible = false;
    private Coroutine alertCoroutine;
    private int idAlert = 1;
    private DateTime startAlert;
    private DateTime endAlert;
    public TextMeshProUGUI speedLimitText;


    private DataCollectionManager dataCollectionManager;
    // 🔴 NEW: Alert type is now set externally (not random)
    private int typeAlert = 0; // 0 = sound, 1 = vibration, 2 = visual only

    // NEW: Flag to enable/disable the alert system
    private bool isAlertActive = true;

    public void setSpeedAlert(float newSpeed)
    {
        speedThreshold = newSpeed;

        if (speedLimitText != null)
        {
            speedLimitText.text = speedThreshold.ToString("0");
        }
    }

    /// <summary>
    /// 🔴 NEW: Sets the alert modality type
    /// </summary>
    public void SetAlertType(int alertType)
    {
        typeAlert = alertType;
        //Debug.Log($"🎯 Alert type set to: {GetAlertTypeName(alertType)}");
    }

    private string GetAlertTypeName(int type)
    {
        switch (type)
        {
            case 0: return "🔊 SOUND";
            case 1: return "🔔 VIBRATION";
            case 2: return "👁️ VISUAL ONLY";
            default: return "UNKNOWN";
        }
    }

    /// <summary>
    /// Disables the speed alert system (for No Rules signs)
    /// </summary>
    public void DisableSpeedAlert()
    {
        isAlertActive = false;

        // 🔴 NEW: Mark that alert ended due to zone exit
        if (isAlertVisible)
        {
            lastAlertEndReason = AlertEndReason.ZoneExit;
            HideAlert();
        }

        // Stop any pending alert coroutine
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
            alertCoroutine = null;
        }
    }


    /// <summary>
    /// Re-enables the speed alert system
    /// </summary>
    public void EnableSpeedAlert()
    {
        isAlertActive = true;
        // Debug.Log("✅ Speed alerts ENABLED - Speed limit enforcement active");
    }

    void Start()
    {
        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();
        dataCollectionManager = GameObject.Find("DataCollectionManager").GetComponent<DataCollectionManager>();

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
        if (!isAlertActive)
            return;

        if (currentSpeed > speedThreshold + 1f)
        {
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
            {
                // 🔴 NEW: Mark as compliance (speed naturally dropped)
                lastAlertEndReason = AlertEndReason.Compliance;
                HideAlert();
            }
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
        // 🔴 REMOVED: Random alert type generation
        // Now typeAlert is set externally via SetAlertType()

       // Debug.Log($"⚠️ Alert triggered - Type: {GetAlertTypeName(typeAlert)} at {DateTime.Now:HH:mm:ss.fff}");

        // Always show visual panel
        if (visualPanel != null)
            visualPanel.SetActive(true);

        // Type 0: Sound + Visual
        if (alertAudio != null && !alertAudio.isPlaying && typeAlert == 0)
        {
            alertAudio.Play();
          //  Debug.Log("🔊 Alert SOUND enabled");
        }

        // Type 1: Vibration + Visual
        if (steeringWheelVibration != null && typeAlert == 1)
        {
            steeringWheelVibration.StartVibration(steeringWheelVibration.vibrationIntensity);
         //   Debug.Log("🔔 Alert VIBRATION enabled");
        }

        // Type 2: Visual only (no sound, no vibration)
        if (typeAlert == 2)
        {
         //   Debug.Log("👁️ Alert VISUAL ONLY enabled");
        }

        isAlertVisible = true;
    }

    public string getTypeAlert()
    {
        switch (typeAlert)
        {
            case 0:
                return "🔊AUDIO";
            case 1:
                return "🔔VIBRATION";
            case 2:
                return "👁️VISUAL";
        }
        return "NO_ALERT";
    }

    private void HideAlert()
    {
        if (visualPanel != null)
            visualPanel.SetActive(false);

        if (alertAudio != null && alertAudio.isPlaying)
            alertAudio.Stop();

        // Stop steering wheel vibration
        if (steeringWheelVibration != null)
        {
            steeringWheelVibration.StopVibration();
        }

        endAlert = DateTime.Now;
        isAlertVisible = false;

        // 🔴 NEW: Log the end reason
       // dataCollectionManager.setTextOnLog($"🔚 Alert ended - Reason: {GetAlertEndReasonText()}");
        Debug.Log($"🔚 Alert ended - Reason: {GetAlertEndReasonText()}");
    }


    public enum AlertEndReason
    {
        Compliance,      // Speed dropped below threshold
        ZoneExit,        // Entered new zone (speed limit or no rules)
        SystemDisabled   // Alert system disabled
    }

    private AlertEndReason lastAlertEndReason = AlertEndReason.Compliance;

    // NEW: Get why the last alert ended
    public AlertEndReason GetLastAlertEndReason()
    {
        return lastAlertEndReason;
    }

    // NEW: Get human-readable end reason
    public string GetAlertEndReasonText()
    {
        switch (lastAlertEndReason)
        {
            case AlertEndReason.Compliance:
                return "✅ COMPLIED";
            case AlertEndReason.ZoneExit:
                return "⚠️ EXITED ZONE";
            case AlertEndReason.SystemDisabled:
                return "🚫 SYSTEM DISABLED";
            default:
                return "UNKNOWN";
        }
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
