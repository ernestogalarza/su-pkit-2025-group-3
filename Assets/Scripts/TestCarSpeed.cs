using UnityEngine;

using UnityEngine.InputSystem;
public class TestCarSpeed : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var device in InputSystem.devices)
        {
            Debug.Log($"Device: {device.displayName}, Layout: {device.layout}, Description: {device.description}");
        }
    
    }

    // Update is called once per frame
    void Update()
    {
     //   Debug.Log("Script ejecutándose correctamente");
    }
}
