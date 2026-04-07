using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoldHarvestTree : MonoBehaviour
{
    [Header("Harvest Settings")]
    public float holdDuration = 3f;

    [Header("Spawn Settings")]
    public GameObject fruitPrefab;
    public Transform fruitSpawnPoint;
    public HarvestUI harvestUI;

    private XRBaseInteractable interactable;
    private Coroutine harvestRoutine;

    private bool isHolding = false;
    private bool harvested = false;

    [Header("UI Settings")]
    public string holdMessage = "Hold trigger to harvest";

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);

        interactable.selectEntered.AddListener(StartHold);
        interactable.selectExited.AddListener(StopHold);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (harvested) return;

        harvestUI.Show(true);
        harvestUI.SetMessage(holdMessage);
        harvestUI.SetProgress(0f);
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        EndHold();
        harvestUI.Show(false);
    }

    private void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
        interactable.selectEntered.RemoveListener(StartHold);
        interactable.selectExited.RemoveListener(StopHold);
    }

    void StartHold(SelectEnterEventArgs args)
    {
        BeginHold();
    }

    void StopHold(SelectExitEventArgs args)
    {
        EndHold();
    }

    public void StartHoldInput()
    {
        BeginHold();
    }

    public void StopHoldInput()
    {
        EndHold();
    }

    private void BeginHold()
    {
        if (harvested || isHolding) return;

        Debug.Log("Harvest started on: " + gameObject.name + ", HarvestUI assigned: " + (harvestUI != null));

        isHolding = true;
        harvestUI.Show(true);
        harvestUI.SetMessage(holdMessage);
        harvestUI.SetProgress(0f);
        harvestRoutine = StartCoroutine(HoldProcess());
    }

    private void EndHold()
    {
        if (!isHolding) return;

        isHolding = false;

        if (harvestRoutine != null)
            StopCoroutine(harvestRoutine);

        harvestUI.SetProgress(0f);
        harvestUI.SetMessage(holdMessage);
        harvestUI.Show(false);
        Debug.Log("Harvest Cancelled");
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
            harvestUI.SetProgress(progress);
            yield return null;
        }

        HarvestComplete();
    }

    void HarvestComplete()
    {
        harvested = true;
        isHolding = false;

        Debug.Log("Fruit Spawned!");

        SpawnFruit();
        harvestUI.Show(false);
        interactable.enabled = false;
    }

    void SpawnFruit()
    {
        GameObject fruit = Instantiate(
            fruitPrefab,
            fruitSpawnPoint.position,
            fruitSpawnPoint.rotation
        );

        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        XRGrabInteractable grab = fruit.GetComponent<XRGrabInteractable>();

        // aktifkan physics langsung
        rb.isKinematic = false;
        rb.useGravity = true;

        grab.enabled = true;
    }


}