using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoldHarvestXR : MonoBehaviour
{
    [Header("Harvest Settings")]
    public float holdDuration = 3f;

    [Header("References")]
    public Rigidbody fruitRigidbody;
    public XRGrabInteractable grabInteractable;

    private XRBaseInteractable interactable;
    private Coroutine harvestRoutine;

    private bool isHarvested = false;
    private bool isHolding = false;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        // buah belum jatuh
        fruitRigidbody.useGravity = false;
        fruitRigidbody.isKinematic = true;

        // belum bisa diambil
        grabInteractable.enabled = false;
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(StartHold);
        interactable.selectExited.AddListener(StopHold);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(StartHold);
        interactable.selectExited.RemoveListener(StopHold);
    }

    void StartHold(SelectEnterEventArgs args)
    {
        if (isHarvested) return;

        isHolding = true;
        harvestRoutine = StartCoroutine(HoldProcess());
    }

    void StopHold(SelectExitEventArgs args)
    {
        isHolding = false;

        if (harvestRoutine != null)
            StopCoroutine(harvestRoutine);

        Debug.Log("Harvest cancelled");
    }

    IEnumerator HoldProcess()
    {
        float timer = 0f;

        while (timer < holdDuration)
        {
            if (!isHolding)
                yield break;

            timer += Time.deltaTime;

            float progress = timer / holdDuration;
            Debug.Log("Harvest Progress: " + Mathf.Round(progress * 100) + "%");

            yield return null;
        }

        HarvestComplete();
    }

    void HarvestComplete()
    {
        isHarvested = true;

        Debug.Log("Harvest Complete!");

        // lepas dari pohon
        transform.parent = null;

        // aktifkan physics
        fruitRigidbody.isKinematic = false;
        fruitRigidbody.useGravity = true;

        // sekarang bisa di-grab
        grabInteractable.enabled = true;
    }
}