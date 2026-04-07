using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRHarvestInteractor : MonoBehaviour
{
    public XRNode controllerNode;
    public Transform rayOrigin;
    public LayerMask treeLayer = ~0;
    public float maxDistance = 10f;

    InputDevice device;
    HoldHarvestTree currentTree;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);

        if (rayOrigin == null)
            rayOrigin = transform;
    }

    void Update()
    {
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(controllerNode);

        device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed);
        UpdateTarget();

        if (currentTree != null)
        {
            if (pressed)
                currentTree.StartHoldInput();
            else
                currentTree.StopHoldInput();
        }
    }

    void UpdateTarget()
    {
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance, treeLayer))
        {
            currentTree = hit.collider.GetComponentInParent<HoldHarvestTree>();
        }
        else if (currentTree != null)
        {
            currentTree.StopHoldInput();
            currentTree = null;
        }
    }
}