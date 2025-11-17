using UnityEngine;
using System.Collections.Generic;

public class SignSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnLocation
    {
        public Transform triggerWall; // The invisible wall trigger
        public Vector3 spawnPosition; // Where to spawn the sign
        public Vector3 spawnRotation; // Rotation in degrees (X, Y, Z)
        public Vector3 spawnScale; // Scale (X, Y, Z)
        public string locationName; // For debugging (e.g., "Wall 1", "Wall 2")
    }

    [Header("Sign Prefabs")]
    public GameObject maxSpeedSign; // Sign for speed > 100 km/h
    public GameObject minSpeedSign; // Sign for speed <= 100 km/h

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

        if (maxSpeedSign == null)
            Debug.LogError("❌ maxSpeedSign prefab not assigned!");
        else
            Debug.Log("✅ maxSpeedSign prefab assigned");

        if (minSpeedSign == null)
            Debug.LogError("❌ minSpeedSign prefab not assigned!");
        else
            Debug.Log("✅ minSpeedSign prefab assigned");

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

        // Destroy previous sign at this location if exists
        if (activeSignsByLocation.ContainsKey(location.triggerWall))
        {
            Destroy(activeSignsByLocation[location.triggerWall]);
            activeSignsByLocation.Remove(location.triggerWall);
            Debug.Log($"🗑️  Old sign destroyed");
        }

        // Determine which sign to spawn
        GameObject signPrefab = currentSpeed > 100f ? maxSpeedSign : minSpeedSign;
        string signType = currentSpeed > 100f ? "MAX 120" : "MIN 120";

        if (signPrefab == null)
        {
            Debug.LogError($"❌ Sign prefab is NULL! Cannot spawn.");
            return;
        }

        Debug.Log($"🎯 Spawning {signType} sign at {location.spawnPosition} with rotation {location.spawnRotation} and scale {location.spawnScale}");

        // Spawn the sign at the configured position
        GameObject newSign = Instantiate(signPrefab, location.spawnPosition, Quaternion.Euler(location.spawnRotation));

        // Apply scale
        newSign.transform.localScale = location.spawnScale;

        activeSignsByLocation[location.triggerWall] = newSign;

        Debug.Log($"✅ {signType} sign spawned successfully at {location.spawnPosition}!");
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
