using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PalmFruitCargo : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    public bool IsDelivered { get; private set; }

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    public bool CanBeDelivered()
    {
        return !IsDelivered && (grabInteractable == null || !grabInteractable.isSelected);
    }

    public void Deliver()
    {
        if (IsDelivered)
            return;

        IsDelivered = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Destroy(gameObject);
    }
}
