using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetectCollision : MonoBehaviour
{
    private AlertManager alertManager;
    public float newSpeed;
    public TextMeshProUGUI speedText;
    public RawImage speedLimitSignImage; // Use RawImage instead of Image

    // Speed limit sign textures
    public Texture2D[] signTextures; // Drag your PNG files here
    public float[] correspondingSpeeds; // Enter matching speeds (40, 50, 60, etc.)

    void Start()
    {
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();

        if (speedLimitSignImage == null)
        {
           // Debug.LogError("SpeedLimitSignImage not assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log($"🚀 Change speed Alert To {newSpeed}");
        alertManager.setSpeedAlert(newSpeed);
        speedText.text = $"{newSpeed:F0}";

        DisplaySpeedLimitSign(newSpeed);
    }

    private void DisplaySpeedLimitSign(float speed)
    {
        if (speedLimitSignImage == null)
        {
            Debug.LogError("SpeedLimitSignImage is null!");
            return;
        }

        for (int i = 0; i < correspondingSpeeds.Length; i++)
        {
            if (Mathf.Approximately(correspondingSpeeds[i], speed))
            {
                speedLimitSignImage.texture = signTextures[i]; // RawImage uses .texture
                speedLimitSignImage.gameObject.SetActive(true);
                Debug.Log($"Displaying speed limit sign for {speed} mph");
                return;
            }
        }

        Debug.LogWarning($"No sign texture found for speed {speed}");
        speedLimitSignImage.gameObject.SetActive(false);
    }
}
