using UnityEngine;
using System.Collections;

public class NoRulesWall : MonoBehaviour
{
    [Header("References")]
    private AlertManager alertManager;

    [Header("Settings")]
    public float alertDelay = 0.2f; // Delay before removing alert
    public string noRulesSignName = "NoRulesSign"; // For debugging

    void Start()
    {
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();

        if (alertManager == null)
        {
            Debug.LogError("❌ AlertManager not found!");
            return;
        }

        // Ensure this object has a trigger collider
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;

        Debug.Log($"✅ NoRulesWall initialized at {transform.position}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🚫 No Rules wall collision detected with: {other.name}");

        // Check if it's the car
        CarController carController = other.GetComponent<CarController>();
        if (carController == null)
            carController = other.GetComponentInParent<CarController>();

        if (carController != null || other.name == "ColliderBody")
        {
            Debug.Log("✅ Car detected on No Rules wall!");

            // Disable the alert after delay
            StartCoroutine(DisableAlertAfterDelay());
        }
    }

    private IEnumerator DisableAlertAfterDelay()
    {
        yield return new WaitForSeconds(alertDelay);

        if (alertManager != null)
        {
            alertManager.DisableSpeedAlert();
            Debug.Log($"🚗 Speed alerts disabled! Car can drive any speed.");
        }
    }
}
