using TMPro;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    private AlertManager alertManager;
    public float newSpeed;
    public TextMeshProUGUI speedText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"🚀 Change speed Alert To  {newSpeed}");
        alertManager.setSpeedAlert(newSpeed);
        speedText.text = $"{newSpeed:F0}";
    }


}
