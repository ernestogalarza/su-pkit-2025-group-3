using System;
using UnityEngine;

public class DataCollectionManager : MonoBehaviour
{
    private SteeringWheelManager steeringWheelManager;
    private AlertManager alertManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private DateTime startAlert;
    private DateTime endAlert;
    private DateTime reactionStart;

    private int idTemporal = 0;

    private bool isLogStart = false;
    private bool isReactionStart = false;

    private float throttleTemporal;


    void Start()
    {
        steeringWheelManager = GameObject.Find("SteeringWheelManager").GetComponent<SteeringWheelManager>();
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();
    }

    // Update is called once per frame
    void Update()
    {
        throttleTemporal = steeringWheelManager.getThrottle();
        if (alertManager.getIsAlertVisible() && !isLogStart)
        {
            idTemporal = alertManager.getIdAlert();

            Debug.Log($" ID: {alertManager.getIdAlert()} - ✅ start {alertManager.getTimeStartAlert().ToString("HH:mm:ss")}");
            isLogStart = true;

            if (throttleTemporal != steeringWheelManager.getThrottle()) {
                Debug.Log($" ID: {alertManager.getIdAlert()} - ✅ ⚠️Reaction start {DateTime.Now.ToString("HH:mm:ss")}");
            }


        }
        else if (alertManager.getIsAlertVisible()==false && idTemporal == alertManager.getIdAlert() && idTemporal>0)
        {

            TimeSpan diff = alertManager.getTimeEndAlert() - alertManager.getTimeStartAlert();


            Debug.Log($" ID: {alertManager.getIdAlert()} - 🛑 end {alertManager.getTimeEndAlert().ToString("HH:mm:ss")}");
            Debug.Log($" ID: {alertManager.getIdAlert()} - 🚨 total alert time {diff.ToString(@"hh\:mm\:ss")}");
            alertManager.setIdAlert(alertManager.getIdAlert() + 1);

            isLogStart = false;
            isReactionStart = false;

        }


        if (alertManager.getIsAlertVisible() && throttleTemporal != steeringWheelManager.getThrottle() && !isReactionStart) {
            Debug.Log($" ID: {alertManager.getIdAlert()} - ✅ ⚠️Reaction start {DateTime.Now.ToString("HH:mm:ss")}");
            isReactionStart = true;
        }
    }
}
