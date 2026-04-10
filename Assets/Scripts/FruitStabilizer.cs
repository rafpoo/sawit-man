using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FruitStabilizer : MonoBehaviour
{
    [SerializeField]
    private float linearDampingMultiplier = 0.98f;
    [SerializeField]
    private float angularDampingMultiplier = 0.95f;
    [SerializeField]
    private float sleepVelocityThreshold = 0.08f;
    [SerializeField]
    private float sleepAngularVelocityThreshold = 0.08f;
    [SerializeField]
    private float settleDelay = 0.75f;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private float spawnedAtTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        spawnedAtTime = Time.time;
    }

    void FixedUpdate()
    {
        if (rb == null || rb.isKinematic)
            return;

        if (grabInteractable != null && grabInteractable.isSelected)
            return;

        if (Time.time - spawnedAtTime < settleDelay)
            return;

        rb.linearVelocity *= linearDampingMultiplier;
        rb.angularVelocity *= angularDampingMultiplier;

        if (rb.linearVelocity.sqrMagnitude <= sleepVelocityThreshold * sleepVelocityThreshold &&
            rb.angularVelocity.sqrMagnitude <= sleepAngularVelocityThreshold * sleepAngularVelocityThreshold)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            enabled = false;
        }
    }
}
