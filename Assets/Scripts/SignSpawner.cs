using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SignSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpeedLimitSign
    {
        public int speedLimit; // e.g., 50, 60, 70, 80, 90, 100, 110, 120, 130
        public GameObject signPrefab; // The 3D model/prefab for this speed limit
    }

    [System.Serializable]
    public class SpawnLocation
    {
        public Transform triggerWall; // The initial invisible wall trigger
        [Space]
        [Header("Sign Spawn Settings")]
        public Vector3 signSpawnPosition; // Where to spawn the sign
        public Vector3 signSpawnRotation; // Rotation in degrees (X, Y, Z)
        public Vector3 signSpawnScale; // Scale (X, Y, Z)
        [Space]
        [Header("Alert Wall Spawn Settings")]
        public Vector3 alertWallSpawnPosition; // Where to spawn the alert trigger wall
        public Vector3 alertWallSpawnRotation; // Rotation in degrees (X, Y, Z)
        public Vector3 alertWallSpawnScale; // Scale (X, Y, Z)
        [Space]
        public string locationName; // For debugging (e.g., "Wall 1", "Wall 2")
    }

    [Header("Speed Limit Signs")]
    public List<SpeedLimitSign> speedLimitSigns = new List<SpeedLimitSign>();

    [Header("Spawn Locations")]
    public List<SpawnLocation> spawnLocations = new List<SpawnLocation>();

    [Header("Alert Wall Prefab")]
    public GameObject alertWallPrefab; // Prefab for the invisible alert trigger wall

    [Header("Alert Settings")]
    public float alertDelay = 0.2f; // Delay before triggering alert after passing wall

    private CarController carController;
    private AlertManager alertManager;
    private Dictionary<Transform, GameObject> activeSignsByLocation = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, GameObject> activeAlertWallsByLocation = new Dictionary<Transform, GameObject>();

    void Start()
    {
        Debug.Log("=== SignSpawner Started ===");

        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();
        alertManager = GameObject.Find("AlertManager").GetComponent<AlertManager>();

        if (carController == null)
        {
            Debug.LogError("❌ CarController not found! Make sure your car GameObject is named 'CarDriverController'");
            return;
        }

        if (alertManager == null)
        {
            Debug.LogError("❌ AlertManager not found!");
            return;
        }

        Debug.Log("✅ CarController and AlertManager found");

        if (speedLimitSigns.Count == 0)
        {
            Debug.LogError("❌ No speed limit signs configured!");
            return;
        }

        Debug.Log($"✅ {speedLimitSigns.Count} speed limit signs configured");

        // Validate all signs have prefabs
        foreach (SpeedLimitSign sign in speedLimitSigns)
        {
            if (sign.signPrefab == null)
                Debug.LogError($"❌ Speed limit sign {sign.speedLimit} km/h has no prefab assigned!");
            else
                Debug.Log($"✅ Speed limit sign {sign.speedLimit} km/h assigned");
        }

        if (alertWallPrefab == null)
        {
            Debug.LogWarning("⚠️ Alert wall prefab not assigned! Creating default cube.");
        }

        if (spawnLocations.Count == 0)
        {
            Debug.LogError("❌ No spawn locations configured!");
            return;
        }

        Debug.Log($"✅ {spawnLocations.Count} spawn locations configured");

        // Attach trigger scripts to each wall
        foreach (SpawnLocation location in spawnLocations)
        {
            if (location.triggerWall == null)
            {
                Debug.LogError($"❌ Trigger wall not assigned in spawn location!");
                continue;
            }

            Debug.Log($"📍 Setting up trigger: {location.locationName}");

            // Check if trigger has collider
            Collider triggerCollider = location.triggerWall.GetComponent<Collider>();
            if (triggerCollider == null)
            {
                Debug.LogError($"❌ {location.locationName} has NO collider! Add a Collider component.");
                continue;
            }

            if (!triggerCollider.isTrigger)
            {
                Debug.LogError($"❌ {location.locationName} collider is NOT a trigger! Check 'Is Trigger' checkbox.");
                continue;
            }

            Debug.Log($"✅ {location.locationName} has valid trigger collider");

            TriggerWallDetector detector = location.triggerWall.GetComponent<TriggerWallDetector>();
            if (detector == null)
            {
                detector = location.triggerWall.gameObject.AddComponent<TriggerWallDetector>();
                Debug.Log($"✅ Added TriggerWallDetector to {location.locationName}");
            }
            detector.SetSpawner(this, location);
        }
    }

    public void OnWallTriggerEnter(SpawnLocation location)
    {
        Debug.Log($"🚗 Car passed {location.locationName}!");

        if (carController == null)
        {
            Debug.LogError("❌ CarController is null in OnWallTriggerEnter!");
            return;
        }

        float currentSpeed = carController.getCurrentSpeed();
        Debug.Log($"📊 Current speed: {currentSpeed:F1} km/h");

        // Calculate target speed: 30 km/h lower than current speed
        int targetSpeedLimit = Mathf.RoundToInt(currentSpeed) - 30;
        Debug.Log($"🎯 Target speed limit: {targetSpeedLimit} km/h (current - 30)");

        // Find the closest sign that matches or is below the target speed
        SpeedLimitSign selectedSign = FindBestSpeedLimitSign(targetSpeedLimit);

        if (selectedSign == null)
        {
            Debug.LogWarning("⚠️ No suitable speed limit sign found!");
            return;
        }

        if (selectedSign.signPrefab == null)
        {
            Debug.LogError($"❌ Speed limit sign {selectedSign.speedLimit} has no prefab assigned!");
            return;
        }

        // Destroy previous sign at this location if exists
        if (activeSignsByLocation.ContainsKey(location.triggerWall))
        {
            Destroy(activeSignsByLocation[location.triggerWall]);
            activeSignsByLocation.Remove(location.triggerWall);
            Debug.Log($"🗑️ Old sign destroyed");
        }

        // Destroy previous alert wall at this location if exists
        if (activeAlertWallsByLocation.ContainsKey(location.triggerWall))
        {
            Destroy(activeAlertWallsByLocation[location.triggerWall]);
            activeAlertWallsByLocation.Remove(location.triggerWall);
            Debug.Log($"🗑️ Old alert wall destroyed");
        }

        Debug.Log($"🎯 Spawning {selectedSign.speedLimit} km/h sign at {location.signSpawnPosition}");

        // Spawn the sign at the configured position
        GameObject newSign = Instantiate(selectedSign.signPrefab, location.signSpawnPosition, Quaternion.Euler(location.signSpawnRotation));
        newSign.transform.localScale = location.signSpawnScale;
        activeSignsByLocation[location.triggerWall] = newSign;

        Debug.Log($"✅ {selectedSign.speedLimit} km/h sign spawned successfully!");

        // Spawn the alert trigger wall
        SpawnAlertWall(location, selectedSign.speedLimit);
    }

    private void SpawnAlertWall(SpawnLocation location, int speedLimit)
    {
        GameObject alertWall;

        if (alertWallPrefab != null)
        {
            // Use the prefab
            alertWall = Instantiate(alertWallPrefab, location.alertWallSpawnPosition, Quaternion.Euler(location.alertWallSpawnRotation));
        }
        else
        {
            // Create a default cube if no prefab assigned
            alertWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            alertWall.transform.position = location.alertWallSpawnPosition;
            alertWall.transform.rotation = Quaternion.Euler(location.alertWallSpawnRotation);

            // Make it invisible (remove mesh renderer)
            MeshRenderer renderer = alertWall.GetComponent<MeshRenderer>();
            if (renderer != null)
                Destroy(renderer);
        }

        alertWall.transform.localScale = location.alertWallSpawnScale;
        alertWall.name = $"AlertWall_{speedLimit}kmh_{location.locationName}";

        // Ensure it has a trigger collider
        Collider collider = alertWall.GetComponent<Collider>();
        if (collider == null)
        {
            collider = alertWall.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;

        // Add the alert trigger detector component
        AlertWallDetector alertDetector = alertWall.AddComponent<AlertWallDetector>();
        alertDetector.SetAlertSettings(carController, alertManager, speedLimit, alertDelay);

        activeAlertWallsByLocation[location.triggerWall] = alertWall;

        Debug.Log($"✅ Alert wall spawned at {location.alertWallSpawnPosition} with speed limit {speedLimit} km/h");
    }

    /// <summary>
    /// Finds the best speed limit sign based on the target speed.
    /// Returns the highest speed limit that is <= target speed.
    /// If no speed is <= target, returns the lowest available speed limit.
    /// </summary>
    private SpeedLimitSign FindBestSpeedLimitSign(int targetSpeed)
    {
        SpeedLimitSign bestSign = null;

        // Find the highest speed limit that is <= target speed
        foreach (SpeedLimitSign sign in speedLimitSigns)
        {
            if (sign.speedLimit <= targetSpeed)
            {
                if (bestSign == null || sign.speedLimit > bestSign.speedLimit)
                {
                    bestSign = sign;
                }
            }
        }

        // If no sign found (target is too low), return the lowest speed limit sign
        if (bestSign == null)
        {
            foreach (SpeedLimitSign sign in speedLimitSigns)
            {
                if (bestSign == null || sign.speedLimit < bestSign.speedLimit)
                {
                    bestSign = sign;
                }
            }
        }

        return bestSign;
    }
}

// Helper component for the initial trigger walls
public class TriggerWallDetector : MonoBehaviour
{
    private SignSpawner spawner;
    private SignSpawner.SpawnLocation location;

    public void SetSpawner(SignSpawner signSpawner, SignSpawner.SpawnLocation spawnLocation)
    {
        spawner = signSpawner;
        location = spawnLocation;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔔 Trigger collision detected on {location.locationName} with: {other.name} (Tag: {other.tag})");

        // Try to find CarController in the collider or its parents
        CarController carController = other.GetComponent<CarController>();
        if (carController == null)
            carController = other.GetComponentInParent<CarController>();

        // Also check by collider name (your car uses "ColliderBody")
        if (carController != null || other.name == "ColliderBody")
        {
            Debug.Log("✅ Vehicle detected!");
            spawner.OnWallTriggerEnter(location);
        }
        else
        {
            Debug.LogWarning($"⚠️ Collider '{other.name}' doesn't match vehicle detection");
        }
    }
}

// Component for dynamically spawned alert trigger walls
public class AlertWallDetector : MonoBehaviour
{
    private CarController carController;
    private AlertManager alertManager;
    private int speedLimit;
    private float alertDelay;

    public void SetAlertSettings(CarController car, AlertManager alert, int limit, float delay)
    {
        carController = car;
        alertManager = alert;
        speedLimit = limit;
        alertDelay = delay;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🚨 Alert wall triggered by: {other.name}");

        // Check if it's the car
        CarController car = other.GetComponent<CarController>();
        if (car == null)
            car = other.GetComponentInParent<CarController>();

        if (car != null || other.name == "ColliderBody")
        {
            Debug.Log($"✅ Car detected! Setting speed threshold to {speedLimit} km/h with {alertDelay}s delay");

            // Update the alert manager's speed threshold
            StartCoroutine(UpdateAlertAfterDelay());
        }
    }

    private IEnumerator UpdateAlertAfterDelay()
    {
        yield return new WaitForSeconds(alertDelay);

        if (alertManager != null)
        {
            alertManager.setSpeedAlert(speedLimit);
            Debug.Log($"⚠️ Alert threshold updated to {speedLimit} km/h after {alertDelay}s delay");
        }
    }
}
