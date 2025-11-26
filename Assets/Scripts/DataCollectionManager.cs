using System;
using TMPro;
using UnityEngine;

public class DataCollectionManager : MonoBehaviour
{
    private SteeringWheelManager steeringWheelManager;
    private AlertManager alertManager;
    private DateTime reactionStart;
    private int idTemporal = 0;
    private bool isLogStart = false;
    private bool isReactionStart = false;
    private float throttleTemporal;
    private int speedLimit;
    private CarController carController;

    private string textOnLog;

    //public TextMeshProUGUI speedText;

    void Start()
    {
        steeringWheelManager = GameObject.Find("SteeringWheelManager").GetComponent<SteeringWheelManager>();
        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();
        textOnLog = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Save baseline throttle BEFORE alert appears
        if (!alertManager.getIsAlertVisible())
        {
            throttleTemporal = steeringWheelManager.getThrottle();
        }

        // LOG: Alert starts
        if (alertManager.getIsAlertVisible() && !isLogStart)
        {
            idTemporal = alertManager.getIdAlert();

            float currentSpeed = carController != null ? carController.getCurrentSpeed() : 0f;

            Debug.Log($"ID: {alertManager.getIdAlert()} - ✅ startAlert {alertManager.getTimeStartAlert().ToString("HH:mm:ss.fff")} - TYPE: {alertManager.getTypeAlert()} - 🎯SPEED: {speedLimit} - INITIAL_SPEED: {currentSpeed:F1}");
            isLogStart = true;
        }

        //speedText.text = $"tmp: {throttleTemporal} - now: {steeringWheelManager.getThrottle()}";

        // DETECT REACTION: Driver reduces throttle (FIXED CONDITION)
        if (alertManager.getIsAlertVisible() &&
            throttleTemporal < steeringWheelManager.getThrottle() &&
            !isReactionStart)
        {
            reactionStart = DateTime.Now;
            Debug.Log($" ID: {alertManager.getIdAlert()} - ⚠️Reaction start {DateTime.Now.ToString("HH:mm:ss.fff")}");
            isReactionStart = true;
        }

        // LOG: Alert ends
        else if (alertManager.getIsAlertVisible() == false &&
                 idTemporal == alertManager.getIdAlert() &&
                 idTemporal > 0 &&
                 isLogStart)
        {
            TimeSpan diff = alertManager.getTimeEndAlert() - alertManager.getTimeStartAlert();
            TimeSpan diffReaction = alertManager.getTimeEndAlert() - reactionStart;

            //Debug.Log($" ID: {alertManager.getIdAlert()} - 🛑endAlert: {alertManager.getTimeEndAlert().ToString("HH:mm:ss.fff")} - duration: {diff.ToString(@"hh\:mm\:ss\.fff")} ");

            if (isReactionStart)
            {
                Debug.Log($" ID: {alertManager.getIdAlert()} - 🛑endAlert: {alertManager.getTimeEndAlert().ToString("HH:mm:ss.fff")} - duration: {diff.ToString(@"hh\:mm\:ss\.fff")} - ⚠️ reaction duration: {diffReaction.ToString(@"hh\:mm\:ss\.fff")}");
            }
            else {

                Debug.Log($" ID: {alertManager.getIdAlert()} - 🛑endAlert: {alertManager.getTimeEndAlert().ToString("HH:mm:ss.fff")} - duration: {diff.ToString(@"hh\:mm\:ss\.fff")} ");
            }

            alertManager.setIdAlert(alertManager.getIdAlert() + 1);
            isLogStart = false;
            isReactionStart = false;
        }



        if (!String.IsNullOrEmpty(textOnLog)) {
            Debug.Log(textOnLog);
            textOnLog = null;
        }

    }

    // 🔴 ADD THIS: Dummy method so SignSpawner doesn't break
    public void setSpawnSignAlert(bool value)
    {
        // Not used - kept for compatibility with SignSpawner
    }

    public void setSpeedLimit(int speed) {
        speedLimit = speed;
    }

    public void setTextOnLog(string text) {
        textOnLog = text;
    }
}
