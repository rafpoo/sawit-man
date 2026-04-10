using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartInstructionPopup : MonoBehaviour
{
    private static GameStartInstructionPopup instance;
    private static bool hasShownThisSession;

    [SerializeField]
    private float showDelay = 0.5f;
    [SerializeField]
    private float autoHideDelay = 10f;
    [SerializeField]
    private Vector3 localOffset = new Vector3(0f, 0f, 1.8f);
    [SerializeField]
    private Vector2 panelSize = new Vector2(720f, 340f);

    private CanvasGroup canvasGroup;
    private Text titleText;
    private Text bodyText;
    private Coroutine showRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnLoad()
    {
        if (hasShownThisSession)
            return;

        if (instance == null)
        {
            GameObject go = new GameObject("GameStartInstructionPopup");
            instance = go.AddComponent<GameStartInstructionPopup>();
        }

        instance.BeginShow();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (canvasGroup == null || canvasGroup.alpha <= 0f)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureUiExists();
        AttachToCamera();
    }

    private void BeginShow()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        while (Camera.main == null)
            yield return null;

        EnsureUiExists();
        AttachToCamera();
        yield return new WaitForSeconds(showDelay);

        titleText.text = "Cara Bermain";
        bodyText.text =
            "F: equip atau unequip spear\n" +
            "Tahan trigger untuk harvest pohon sawit\n" +
            "XR Device Simulator: G = grip, T = trigger\n" +
            "Dekatkan buah ke ujung spear untuk menusuk\n" +
            "Ambil buah dari spear dengan tangan lain\n" +
            "Lempar buah ke bak truck\n\n" +
            "Tekan Space, Enter, atau Esc untuk menutup";

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        hasShownThisSession = true;

        yield return new WaitForSeconds(autoHideDelay);
        Hide();
    }

    private void Hide()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void EnsureUiExists()
    {
        if (canvasGroup != null && titleText != null && bodyText != null)
            return;

        GameObject canvasRoot = new GameObject("InstructionCanvas");
        canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasRoot.AddComponent<GraphicRaycaster>();
        canvasGroup = canvasRoot.AddComponent<CanvasGroup>();

        RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelSize;
        canvasGroup.alpha = 0f;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasRoot.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(panel.transform, false);
        titleText = titleObject.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 40;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = Color.white;
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 60f);
        titleRect.anchoredPosition = new Vector2(0f, -16f);

        GameObject bodyObject = new GameObject("Body");
        bodyObject.transform.SetParent(panel.transform, false);
        bodyText = bodyObject.AddComponent<Text>();
        bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bodyText.fontSize = 26;
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.color = Color.white;
        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(28f, 28f);
        bodyRect.offsetMax = new Vector2(-28f, -80f);
    }

    private void AttachToCamera()
    {
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam == null || canvasGroup == null)
            return;

        Transform canvasTransform = canvasGroup.transform;
        canvasTransform.SetParent(cam, false);
        canvasTransform.localPosition = localOffset;
        canvasTransform.localRotation = Quaternion.identity;
        canvasTransform.localScale = Vector3.one * 0.0018f;
    }
}
