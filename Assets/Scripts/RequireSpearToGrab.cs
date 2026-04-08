using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


[RequireComponent(typeof(XRGrabInteractable))]
public class RequireSpearToGrab : MonoBehaviour
{
    [SerializeField]
    private bool dropFruitWhenSpearUnequipped = true;
    [SerializeField]
    private Vector3 stabbedLocalPositionOffset = Vector3.zero;
    [SerializeField]
    private Vector3 stabbedLocalRotationOffset = Vector3.zero;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private FruitStabilizer fruitStabilizer;
    private IXRSelectInteractor selectingInteractor;
    private bool isAttachedToSpear;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        fruitStabilizer = GetComponent<FruitStabilizer>();
    }

    void OnEnable()
    {
        RefreshGrabState();
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);

        if (PlayerEquipment.Instance != null)
        {
            PlayerEquipment.Instance.spearEquippedChanged += OnSpearEquippedChanged;
        }
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);

        if (PlayerEquipment.Instance != null)
            PlayerEquipment.Instance.spearEquippedChanged -= OnSpearEquippedChanged;
    }

    void OnDestroy()
    {
        if (PlayerEquipment.Instance != null)
            PlayerEquipment.Instance.spearEquippedChanged -= OnSpearEquippedChanged;
    }

    void OnSpearEquippedChanged(bool equipped)
    {
        if (!equipped && isAttachedToSpear && dropFruitWhenSpearUnequipped)
            DetachFromSpear();

        RefreshGrabState();
    }

    public void RefreshGrabState()
    {
        if (grabInteractable == null || isAttachedToSpear)
            return;

        bool canGrab = PlayerEquipment.Instance != null && PlayerEquipment.Instance.HasSpearEquipped;
        grabInteractable.enabled = canGrab;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (PlayerEquipment.Instance == null || !PlayerEquipment.Instance.HasSpearEquipped)
            return;

        selectingInteractor = args.interactorObject;
        AttachToSpear();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isAttachedToSpear)
            return;

        DetachFromSpear();
    }

    private void AttachToSpear()
    {
        Transform attachPoint = PlayerEquipment.Instance != null ? PlayerEquipment.Instance.GetFruitAttachPoint() : null;
        if (attachPoint == null)
        {
            Debug.LogWarning("Could not find a spear attach point for the fruit.");
            return;
        }

        if (grabInteractable.isSelected && selectingInteractor != null && grabInteractable.interactionManager != null)
            grabInteractable.interactionManager.SelectExit(selectingInteractor, grabInteractable);

        selectingInteractor = null;
        isAttachedToSpear = true;

        Vector3 currentWorldScale = transform.lossyScale;
        transform.SetParent(attachPoint, false);
        transform.localPosition = stabbedLocalPositionOffset;
        transform.localRotation = Quaternion.Euler(stabbedLocalRotationOffset);
        ApplyWorldScale(currentWorldScale);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (fruitStabilizer != null)
            fruitStabilizer.enabled = false;

        grabInteractable.enabled = false;
    }

    private void DetachFromSpear()
    {
        isAttachedToSpear = false;
        transform.SetParent(null, true);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (fruitStabilizer != null)
            fruitStabilizer.enabled = true;

        RefreshGrabState();
    }

    private void ApplyWorldScale(Vector3 targetWorldScale)
    {
        Transform parent = transform.parent;
        if (parent == null)
        {
            transform.localScale = targetWorldScale;
            return;
        }

        Vector3 parentScale = parent.lossyScale;
        transform.localScale = new Vector3(
            SafeDivide(targetWorldScale.x, parentScale.x),
            SafeDivide(targetWorldScale.y, parentScale.y),
            SafeDivide(targetWorldScale.z, parentScale.z));
    }

    private float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) < 0.0001f ? value : value / divisor;
    }
}
