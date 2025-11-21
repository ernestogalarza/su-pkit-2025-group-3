using System;
using TMPro;
using UnityEngine;

public class DataCollectionManager : MonoBehaviour
{
    private SteeringWheelManager steeringWheelManager;
    private AlertManager alertManager;

    private DateTime reactionStart;
    private DateTime spawnSignStart;

    private int idTemporal = 0;

    private bool isLogStart = false;
    private bool isReactionStart = false;

    private float throttleTemporal;
    private float speedSpawnSign;

    private bool spawnSign = false;


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

        if (!spawnSign) {
            speedSpawnSign = steeringWheelManager.getThrottle();
        }


        if (spawnSign && speedSpawnSign < steeringWheelManager.getThrottle())
        {
            Debug.Log($"⚠️Reaction spawn sign: {DateTime.Now.ToString("HH:mm:ss.fff")}");


            spawnSign = false;
        }



        if (alertManager.getIsAlertVisible() && !isLogStart)
        {
            idTemporal = alertManager.getIdAlert();

            Debug.Log($" ID: {alertManager.getIdAlert()} - TYPE: {alertManager.getTypeAlert()} - ✅ start {alertManager.getTimeStartAlert().ToString("HH:mm:ss.fff")}");
            isLogStart = true;

        }
        else if (alertManager.getIsAlertVisible()==false && idTemporal == alertManager.getIdAlert() && idTemporal>0 && isLogStart)
        {

            TimeSpan diff = alertManager.getTimeEndAlert() - alertManager.getTimeStartAlert();
            TimeSpan diffReaction = alertManager.getTimeEndAlert() - reactionStart;


            Debug.Log($" ID: {alertManager.getIdAlert()} - 🚨 end alert: {alertManager.getTimeEndAlert().ToString("HH:mm:ss.fff")} - duration: {diff.ToString(@"hh\:mm\:ss\.fff")} ");

            if (isReactionStart) {
                 Debug.Log($" ID: {alertManager.getIdAlert()} - 🛑  reaction duration: {diffReaction.ToString(@"hh\:mm\:ss\.fff")}");
            }


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

    public void setSpawnSignAlert(bool value) {
        this.spawnSign = value;
    }
}
