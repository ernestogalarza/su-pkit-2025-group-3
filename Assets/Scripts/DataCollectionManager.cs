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


    void Start()
    {
        steeringWheelManager = GameObject.Find("SteeringWheelManager").GetComponent<SteeringWheelManager>();
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!alertManager.getIsAlertVisible()) {
            throttleTemporal = steeringWheelManager.getThrottle();
        }


       
        if (alertManager.getIsAlertVisible() && !isLogStart)
        {
            idTemporal = alertManager.getIdAlert();

            Debug.Log($" ID: {alertManager.getIdAlert()} - ✅ start {alertManager.getTimeStartAlert().ToString("HH:mm:ss.fff")}");
            isLogStart = true;



        }
        else if (alertManager.getIsAlertVisible()==false && idTemporal == alertManager.getIdAlert() && idTemporal>0)
        {

            TimeSpan diff = alertManager.getTimeEndAlert() - alertManager.getTimeStartAlert();
            TimeSpan diffReaction = alertManager.getTimeEndAlert() - reactionStart;


            Debug.Log($" ID: {alertManager.getIdAlert()} - 🚨 total alert time {diff.ToString(@"hh\:mm\:ss\.fff")} -  end {alertManager.getTimeEndAlert().ToString("HH:mm:ss.fff")}");
            Debug.Log($" ID: {alertManager.getIdAlert()} - 🛑 total reaction time {diffReaction.ToString(@"hh\:mm\:ss\.fff")}");
            alertManager.setIdAlert(alertManager.getIdAlert() + 1);

            isLogStart = false;
            isReactionStart = false;

        }


        if (alertManager.getIsAlertVisible() && throttleTemporal < steeringWheelManager.getThrottle() && !isReactionStart) {

            reactionStart = DateTime.Now;

            Debug.Log($" ID: {alertManager.getIdAlert()} - ⚠️Reaction start {DateTime.Now.ToString("HH:mm:ss.fff")}");
            isReactionStart = true;
        }
    }
}
