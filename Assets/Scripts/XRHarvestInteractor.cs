using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class XRHarvestInteractor : MonoBehaviour
{
    public XRNode controllerNode;
    public Transform rayOrigin;
    public LayerMask treeLayer = ~0;
    public float maxDistance = 10f;

    UnityEngine.XR.InputDevice device;
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

        // Try XR input first, fall back to mouse input for simulator testing
        bool pressed = false;

        if (device.isValid)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out pressed);
        }
        else
        {
            if (Mouse.current != null)
            {
                pressed = Mouse.current.leftButton.IsPressed();
            }
        }

        UpdateTarget();

        if (currentTree != null)
        {
            if (pressed)
            {
                currentTree.StartHoldInput();
            }
            else
            {
                currentTree.StopHoldInput();
            }
        }
    }

    void UpdateTarget()
    {
        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance, Color.green);

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance, treeLayer))
        {
            HoldHarvestTree tree = hit.collider.GetComponentInParent<HoldHarvestTree>();
            if (tree != null && tree != currentTree)
            {
                currentTree = tree;
            }
        }
        else if (currentTree != null)
        {
            currentTree.StopHoldInput();
            currentTree = null;
        }
    }
}