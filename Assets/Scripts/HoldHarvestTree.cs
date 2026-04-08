using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HoldHarvestTree : MonoBehaviour
{
    [Header("Harvest Settings")]
    public float holdDuration = 3f;
    public bool requireSpearToHarvest = true;
    public string spearRequiredMessage = "Press the grip button to equip a spear to harvest";

    [Header("Spawn Settings")]
    public GameObject fruitPrefab;
    public Transform fruitSpawnPoint;
    public HarvestUI harvestUI;
    public Vector3 spawnLocalOffset = Vector3.zero;
    public float releaseDelay = 0.1f;

    private GameObject[] onTreeFruitObjects;

    private XRBaseInteractable interactable;
    private Coroutine harvestRoutine;

    private bool isHolding = false;
    private bool harvested = false;

    public bool IsHarvested => harvested;

    [Header("UI Settings")]
    public string holdMessage = "Hold trigger to harvest";

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (fruitSpawnPoint != null && fruitSpawnPoint.childCount > 0)
        {
            onTreeFruitObjects = new GameObject[fruitSpawnPoint.childCount];
            for (int i = 0; i < fruitSpawnPoint.childCount; i++)
            {
                onTreeFruitObjects[i] = fruitSpawnPoint.GetChild(i).gameObject;
            }
        }
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
        harvestUI.SetMessage(GetHarvestMessage());
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

        if (!CanHarvest())
        {
            harvestUI.Show(true);
            harvestUI.SetMessage(spearRequiredMessage);
            harvestUI.SetProgress(0f);
            return;
        }

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

        if (onTreeFruitObjects != null)
        {
            foreach (var fruit in onTreeFruitObjects)
            {
                if (fruit != null)
                    fruit.SetActive(false);
            }
        }

        Debug.Log("Fruit Spawned!");

        SpawnFruit();
        harvestUI.Show(false);
        interactable.enabled = false;
    }

    void SpawnFruit()
    {
        GameObject fruit = Instantiate(fruitPrefab);
        fruit.transform.SetParent(fruitSpawnPoint, false);
        fruit.transform.localPosition = spawnLocalOffset;
        fruit.transform.localRotation = Quaternion.identity;
        fruit.transform.SetParent(null, true);

        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        XRGrabInteractable grab = fruit.GetComponent<XRGrabInteractable>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;

            FruitStabilizer stabilizer = fruit.GetComponent<FruitStabilizer>();
            if (stabilizer == null)
                fruit.AddComponent<FruitStabilizer>();
            else
                stabilizer.enabled = true;
        }

        if (grab != null)
        {
            grab.enabled = true;
        }

        if (fruit.GetComponent<PalmFruitCargo>() == null)
            fruit.AddComponent<PalmFruitCargo>();

        var requireSpear = fruit.GetComponent<RequireSpearToGrab>();
        if (requireSpear != null)
        {
            requireSpear.RefreshGrabState();
        }

        if (rb != null)
            StartCoroutine(ReleaseFruit(rb));
    }

    private IEnumerator ReleaseFruit(Rigidbody rb)
    {
        if (releaseDelay > 0f)
            yield return new WaitForSeconds(releaseDelay);

        if (rb == null)
            yield break;

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private bool CanHarvest()
    {
        return !requireSpearToHarvest || PlayerEquipment.Instance == null || PlayerEquipment.Instance.HasSpearEquipped;
    }

    private string GetHarvestMessage()
    {
        if (requireSpearToHarvest && PlayerEquipment.Instance != null && !PlayerEquipment.Instance.HasSpearEquipped)
            return spearRequiredMessage;

        return holdMessage;
    }
}
