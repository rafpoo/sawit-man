using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [Header("Tree Targeting")]
    public LayerMask treeLayer = ~0;
    public float maxDistance = 20f;

    private Transform cam;
    private CanvasGroup canvasGroup;
    private bool isVisible;

    void Start()
    {
        // cari kamera XR utama
        cam = Camera.main?.transform;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetVisible(false);
    }

    void LateUpdate()
    {
        if (cam == null) return;

        transform.LookAt(transform.position + cam.forward);

        bool shouldShow = false;
        Ray ray = new Ray(cam.position, cam.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, treeLayer))
        {
            if (hit.collider.GetComponentInParent<HoldHarvestTree>() != null)
                shouldShow = true;
        }

        SetVisible(shouldShow);
    }

    void SetVisible(bool state)
    {
        if (isVisible == state) return;

        isVisible = state;
        canvasGroup.alpha = state ? 1f : 0f;
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }
}