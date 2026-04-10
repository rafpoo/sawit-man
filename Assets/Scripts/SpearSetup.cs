using UnityEngine;

public class SpearSetup : MonoBehaviour
{
    [ContextMenu("Setup Spear Prefab")]
    void SetupSpearPrefab()
    {
        // This method can be called from the context menu in Unity
        // It will ensure the spear has proper components for visibility

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = true;
            Debug.Log("Enabled renderer: " + renderer.gameObject.name);
        }

        // Ensure transform is visible
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log("Spear setup complete!");
    }
}