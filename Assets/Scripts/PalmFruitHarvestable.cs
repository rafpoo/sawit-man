using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PalmFruitHarvestable : MonoBehaviour
{
    public float holdTimeRequired = 2f;

    float holdTimer = 0f;
    bool isHarvested = false;

    Rigidbody rb;
    XRGrabInteractable grab;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();

        grab.enabled = false; // belum bisa diambil
    }

    public void HoldHarvest(float deltaTime)
    {
        if (isHarvested) return;

        holdTimer += deltaTime;

        if (holdTimer >= holdTimeRequired)
        {
            Harvest();
        }
    }

    public void ResetHold()
    {
        holdTimer = 0f;
    }

    void Harvest()
    {
        isHarvested = true;

        // aktifkan physics
        rb.isKinematic = false;

        // lepas dari pohon
        transform.parent = null;

        // aktifkan grab XR
        grab.enabled = true;
    }
}