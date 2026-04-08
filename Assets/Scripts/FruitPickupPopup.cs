using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FruitPickupPopup : MonoBehaviour
{
    public static FruitPickupPopup Instance { get; private set; }

    [Header("Popup UI")]
    public CanvasGroup canvasGroup;
    public Text messageText;
    public float displayDuration = 1.5f;
    public Vector3 localOffset = new Vector3(0f, -0.25f, 1.5f);

    private Coroutine activeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureUiExists();
        SetVisible(false);
    }

    public static void ShowMessage(string message)
    {
        if (Instance == null)
            CreateRuntimeInstance();

        Instance.Show(message);
    }

    public void Show(string message)
    {
        EnsureUiExists();

        if (messageText != null)
            messageText.text = message;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetVisible(true);
        yield return new WaitForSeconds(displayDuration);
        SetVisible(false);
    }

    private void EnsureUiExists()
    {
        if (canvasGroup != null && messageText != null)
            return;

        Transform cam = Camera.main != null ? Camera.main.transform : null;

        GameObject canvasRoot = new GameObject("FruitPickupPopupCanvas");
        canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (cam != null)
            canvas.worldCamera = Camera.main;

        canvasRoot.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400f, 120f);

        if (cam != null)
        {
            canvasRoot.transform.SetParent(cam, false);
            canvasRoot.transform.localPosition = localOffset;
            canvasRoot.transform.localRotation = Quaternion.identity;
            canvasRoot.transform.localScale = Vector3.one * 0.0025f;
        }

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasRoot.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Message");
        textObject.transform.SetParent(panel.transform, false);
        messageText = textObject.AddComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontSize = 36;
        messageText.color = Color.white;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(12f, 8f);
        textRect.offsetMax = new Vector2(-12f, -8f);

        canvasGroup = canvasRoot.AddComponent<CanvasGroup>();
    }

    private void SetVisible(bool state)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = state ? 1f : 0f;
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }

    private static void CreateRuntimeInstance()
    {
        GameObject go = new GameObject("FruitPickupPopup");
        go.AddComponent<FruitPickupPopup>();
    }
}
