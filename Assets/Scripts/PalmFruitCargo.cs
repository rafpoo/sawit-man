using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class PalmFruitCargo : MonoBehaviour
{
    [SerializeField]
    private float deliveryDelayAfterRelease = 0.2f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private float deliveryEnabledTime;

    public bool IsDelivered { get; private set; }

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        deliveryEnabledTime = Time.time;
    }

    void OnEnable()
    {
        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    public bool CanBeDelivered()
    {
        return !IsDelivered
            && Time.time >= deliveryEnabledTime
            && (grabInteractable == null || !grabInteractable.isSelected);
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

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        deliveryEnabledTime = float.PositiveInfinity;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        deliveryEnabledTime = Time.time + deliveryDelayAfterRelease;
    }
}
