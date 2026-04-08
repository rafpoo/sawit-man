using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerEquipment : MonoBehaviour
{
    private enum SpearTipAxis
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ,
        AutoPositiveLongest,
        AutoNegativeLongest,
    }

    public static PlayerEquipment Instance { get; private set; }

    [Header("Equipment")]
    [SerializeField]
    private bool hasSpearEquipped = false;
    [SerializeField]
    private GameObject spearPrefab;
    [SerializeField]
    private Transform spearParentTransform;
    [SerializeField]
    private XRNode equipControllerNode = XRNode.RightHand;
    [SerializeField]
    private Vector3 equippedSpearScale = new Vector3(50f, 50f, 50f);
    [SerializeField]
    private Key keyboardEquipFallbackKey = Key.F;
    [SerializeField]
    private string spearTipTransformName = "FruitAttachPoint";
    [SerializeField]
    private SpearTipAxis spearTipAxis = SpearTipAxis.PositiveY;

    public bool HasSpearEquipped => hasSpearEquipped;
    public Transform EquippedSpearTransform => spearInstance != null ? spearInstance.transform : null;

    private GameObject spearInstance;
    private Transform spearFruitAttachPoint;
    private UnityEngine.XR.InputDevice device;

    public event System.Action<bool> spearEquippedChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (spearInstance != null)
            Destroy(spearInstance);
    }

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(equipControllerNode);
        ResolveSpearParentTransform();
    }

    void Update()
    {
        bool pressed = false;

        // Try XR controller grip button first
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(equipControllerNode);

        if (device.isValid)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out pressed);
        }
        else if (Keyboard.current != null)
        {
            // Fallback to keyboard for testing in editor
            pressed = Keyboard.current[keyboardEquipFallbackKey].wasPressedThisFrame;
            if (pressed)
                Debug.Log("Equip button pressed (keyboard fallback: " + keyboardEquipFallbackKey + ")");
        }

        if (pressed)
        {
            Debug.Log("Equip toggle triggered!");
            ToggleSpearEquipped();
        }
    }

    public void ToggleSpearEquipped()
    {
        SetSpearEquipped(!hasSpearEquipped);
    }

    public void SetSpearEquipped(bool equipped)
    {
        if (hasSpearEquipped == equipped)
            return;

        hasSpearEquipped = equipped;

        if (equipped)
        {
            EquipSpear();
        }
        else
        {
            UnequipSpear();
        }

        spearEquippedChanged?.Invoke(hasSpearEquipped);
        Debug.Log("Spear equipped: " + hasSpearEquipped);
    }

    private void EquipSpear()
    {
        if (spearPrefab == null)
        {
            Debug.LogWarning("Spear prefab is not assigned!");
            hasSpearEquipped = false;
            return;
        }

        Transform parent = ResolveSpearParentTransform();
        if (parent == null)
        {
            Debug.LogWarning("Could not find a hand/controller transform for the equipped spear. Falling back to PlayerEquipment transform.");
            parent = transform;
        }

        spearInstance = Instantiate(spearPrefab, parent);
        spearInstance.name = "Spear (Equipped)";

        // Reset local position and rotation to make it visible at parent location
        spearInstance.transform.localPosition = Vector3.zero;
        spearInstance.transform.localRotation = Quaternion.identity;
        spearInstance.transform.localScale = equippedSpearScale;
        spearFruitAttachPoint = ResolveFruitAttachPoint();

        // Ensure all renderers are enabled
        MeshRenderer[] renderers = spearInstance.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        Rigidbody rb = spearInstance.GetComponent<Rigidbody>();
        Collider col = spearInstance.GetComponent<Collider>();

        if (rb != null)
            rb.isKinematic = true;

        if (col != null)
            col.enabled = false;

        Debug.Log("Spear equipped! Instance: " + spearInstance.name + " at parent: " + parent.name + " with " + renderers.Length + " renderers");
    }

    private Transform ResolveSpearParentTransform()
    {
        if (spearParentTransform != null)
            return spearParentTransform;

        spearParentTransform = FindControllerTransform();
        return spearParentTransform;
    }

    private Transform FindControllerTransform()
    {
        string[] preferredNames = equipControllerNode == XRNode.LeftHand
            ? new[] { "Left Controller", "LeftHand", "Left Hand" }
            : new[] { "Right Controller", "RightHand", "Right Hand" };

        Transform[] allTransforms = FindObjectsOfType<Transform>(true);

        foreach (string preferredName in preferredNames)
        {
            foreach (Transform candidate in allTransforms)
            {
                if (candidate.name == preferredName)
                    return candidate;
            }
        }

        foreach (Transform candidate in allTransforms)
        {
            string lowerName = candidate.name.ToLowerInvariant();
            bool matchesHand = equipControllerNode == XRNode.LeftHand
                ? lowerName.Contains("left")
                : lowerName.Contains("right");

            if (matchesHand && (lowerName.Contains("controller") || lowerName.Contains("hand")))
                return candidate;
        }

        return null;
    }

    public Transform GetFruitAttachPoint()
    {
        if (spearInstance == null)
            return null;

        if (spearFruitAttachPoint == null)
            spearFruitAttachPoint = ResolveFruitAttachPoint();

        return spearFruitAttachPoint;
    }

    private Transform ResolveFruitAttachPoint()
    {
        if (spearInstance == null)
            return null;

        Transform namedTip = FindChildRecursive(spearInstance.transform, spearTipTransformName);
        if (namedTip != null)
            return namedTip;

        Transform attachPoint = new GameObject("FruitAttachPoint").transform;
        attachPoint.SetParent(spearInstance.transform, false);
        attachPoint.localPosition = GetAutoDetectedTipLocalPosition(spearInstance.transform);
        attachPoint.localRotation = Quaternion.identity;
        attachPoint.localScale = Vector3.one;
        return attachPoint;
    }

    private Transform FindChildRecursive(Transform root, string targetName)
    {
        if (root.name == targetName)
            return root;

        foreach (Transform child in root)
        {
            Transform result = FindChildRecursive(child, targetName);
            if (result != null)
                return result;
        }

        return null;
    }

    private Vector3 GetAutoDetectedTipLocalPosition(Transform spearRoot)
    {
        Renderer[] renderers = spearRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return Vector3.zero;

        bool hasBounds = false;
        Bounds localBounds = default;

        foreach (Renderer renderer in renderers)
        {
            Bounds worldBounds = renderer.bounds;
            Vector3 center = spearRoot.InverseTransformPoint(worldBounds.center);
            Vector3 extents = worldBounds.extents;

            Vector3 axisX = spearRoot.InverseTransformVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = spearRoot.InverseTransformVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = spearRoot.InverseTransformVector(new Vector3(0f, 0f, extents.z));
            Vector3 localExtents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));

            Bounds rendererLocalBounds = new Bounds(center, localExtents * 2f);

            if (!hasBounds)
            {
                localBounds = rendererLocalBounds;
                hasBounds = true;
            }
            else
            {
                localBounds.Encapsulate(rendererLocalBounds.min);
                localBounds.Encapsulate(rendererLocalBounds.max);
            }
        }

        Vector3 ext = localBounds.extents;
        Vector3 tip = localBounds.center;

        switch (spearTipAxis)
        {
            case SpearTipAxis.PositiveX:
                tip += Vector3.right * ext.x;
                break;
            case SpearTipAxis.NegativeX:
                tip += Vector3.left * ext.x;
                break;
            case SpearTipAxis.PositiveY:
                tip += Vector3.up * ext.y;
                break;
            case SpearTipAxis.NegativeY:
                tip += Vector3.down * ext.y;
                break;
            case SpearTipAxis.PositiveZ:
                tip += Vector3.forward * ext.z;
                break;
            case SpearTipAxis.NegativeZ:
                tip += Vector3.back * ext.z;
                break;
            case SpearTipAxis.AutoNegativeLongest:
                if (ext.z >= ext.x && ext.z >= ext.y)
                    tip += Vector3.back * ext.z;
                else if (ext.y >= ext.x)
                    tip += Vector3.down * ext.y;
                else
                    tip += Vector3.left * ext.x;
                break;
            default:
                if (ext.z >= ext.x && ext.z >= ext.y)
                    tip += Vector3.forward * ext.z;
                else if (ext.y >= ext.x)
                    tip += Vector3.up * ext.y;
                else
                    tip += Vector3.right * ext.x;
                break;
        }

        return tip;
    }

    private void UnequipSpear()
    {
        if (spearInstance != null)
        {
            Destroy(spearInstance);
            spearInstance = null;
            spearFruitAttachPoint = null;
        }

        Debug.Log("Spear unequipped!");
    }
}
