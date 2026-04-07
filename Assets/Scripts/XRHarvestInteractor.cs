using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRHarvestInteractor : MonoBehaviour
{
    public XRNode controllerNode;

    InputDevice device;
    PalmFruitHarvestable currentFruit;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
    }

    void Update()
    {
        device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed);

        if (pressed && currentFruit != null)
        {
            currentFruit.HoldHarvest(Time.deltaTime);
        }
        else if (currentFruit != null)
        {
            currentFruit.ResetHold();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        currentFruit = other.GetComponentInParent<PalmFruitHarvestable>();
    }

    void OnTriggerExit(Collider other)
    {
        currentFruit = null;
    }
}