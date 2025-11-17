using UnityEngine;
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
        public Transform triggerWall; // The invisible wall trigger
        public Vector3 spawnPosition; // Where to spawn the sign
        public Vector3 spawnRotation; // Rotation in degrees (X, Y, Z)
        public Vector3 spawnScale; // Scale (X, Y, Z)
        public string locationName; // For debugging (e.g., "Wall 1", "Wall 2")
    }

    [Header("Speed Limit Signs")]
    public List<SpeedLimitSign> speedLimitSigns = new List<SpeedLimitSign>();

    [Header("Spawn Locations")]
    public List<SpawnLocation> spawnLocations = new List<SpawnLocation>();

    private CarController carController;
    private Dictionary<Transform, GameObject> activeSignsByLocation = new Dictionary<Transform, GameObject>();

    void Start()
    {
        Debug.Log("=== SignSpawner Started ===");

        carController = GameObject.Find("CarDriverController").GetComponent<CarController>();

        if (carController == null)
        {
            Debug.LogError("❌ CarController not found! Make sure your car GameObject is named 'CarDriverController'");
            return;
        }

        Debug.Log("✅ CarController found");

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
            Debug.LogWarning("⚠️  No suitable speed limit sign found!");
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
            Debug.Log($"🗑️  Old sign destroyed");
        }

        Debug.Log($"🎯 Spawning {selectedSign.speedLimit} km/h sign at {location.spawnPosition} with rotation {location.spawnRotation} and scale {location.spawnScale}");

        // Spawn the sign at the configured position
        GameObject newSign = Instantiate(selectedSign.signPrefab, location.spawnPosition, Quaternion.Euler(location.spawnRotation));

        // Apply scale
        newSign.transform.localScale = location.spawnScale;

        activeSignsByLocation[location.triggerWall] = newSign;

        Debug.Log($"✅ {selectedSign.speedLimit} km/h sign spawned successfully at {location.spawnPosition}!");
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

// Helper component for each trigger wall
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
            Debug.LogWarning($"⚠️  Collider '{other.name}' doesn't match vehicle detection");
        }
    }
}
