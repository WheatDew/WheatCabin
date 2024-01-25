using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Demo_XRHand : MonoBehaviour
{
    public InputDeviceCharacteristics DeviceCharacteristics;
    public List<GameObject> ControllerPrefabs;
    private InputDevice TargetDevice;

    private GameObject SpawnedController;
    private Animator SpawnedHandAnimator;

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        if (!TargetDevice.isValid)
        {
            TryInitialize();
        }
        else
        {
            SpawnedController.SetActive(true);
        }
    }

    private void TryInitialize()
    {
        List<InputDevice> Devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(DeviceCharacteristics, Devices);

        if (Devices.Count > 0)
        {
            TargetDevice = Devices[0];
            GameObject prefab = ControllerPrefabs.Find(controller => controller.name == TargetDevice.name);

            if (prefab)
            {
                SpawnedController = Instantiate(prefab, transform);
            }
            else
            {
                SpawnedController = Instantiate(ControllerPrefabs[0], transform);
            }
        }
    }
}