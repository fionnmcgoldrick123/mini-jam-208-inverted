using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunTimerManager : MonoBehaviour
{
    public static RunTimerManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool autoCreateText = true;
    [SerializeField] private string labelText = "TIME";
    [SerializeField] private bool showLabel = true;
    [SerializeField] private Vector2 anchoredPosition = new Vector2(-24f, -24f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;

    [Header("Format")]
    [SerializeField] private string timeFormat = "{0:0.000}";

    private float elapsedTime;
    private bool isFrozen;
    private Canvas timerCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        EnsureTimerText();
        ResetTimerInternal();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void Update()
    {
        if (isFrozen)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        ResetTimerInternal();
    }

    public static void ResetTimer()
    {
        EnsureInstance();

        if (Instance != null)
            Instance.ResetTimerInternal();
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
            return;

        RunTimerManager existingTimer = FindFirstObjectByType<RunTimerManager>(FindObjectsInactive.Include);
        if (existingTimer != null)
        {
            Instance = existingTimer;
            return;
        }

        new GameObject(nameof(RunTimerManager)).AddComponent<RunTimerManager>();
    }

    private void ResetTimerInternal()
    {
        elapsedTime = 0f;
        isFrozen = false;
        UpdateTimerText();
    }

    public static void FreezeTimer()
    {
        EnsureInstance();

        if (Instance != null)
            Instance.FreezeTimerInternal();
    }

    private void FreezeTimerInternal()
    {
        isFrozen = true;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        string timeString = string.Format(timeFormat, elapsedTime);
        timerText.text = showLabel ? $"{labelText}\n{timeString}" : timeString;
    }

    private void EnsureTimerText()
    {
        if (timerText != null)
            return;

        if (!autoCreateText)
            return;

        EnsureCanvas();

        GameObject textObject = new GameObject("TimerText");
        textObject.transform.SetParent(timerCanvas.transform, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(220f, 72f);

        timerText = textObject.AddComponent<TextMeshProUGUI>();
        timerText.alignment = TextAlignmentOptions.TopRight;
        timerText.fontSize = fontSize;
        timerText.color = textColor;
        timerText.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            timerText.font = TMP_Settings.defaultFontAsset;
    }

    private void EnsureCanvas()
    {
        if (timerCanvas != null)
            return;

        GameObject canvasObject = new GameObject("LevelTimerCanvas");
        canvasObject.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasObject);

        timerCanvas = canvasObject.AddComponent<Canvas>();
        timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        timerCanvas.sortingOrder = 1000;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
    }

    public static void EnsureTimer()
    {
        EnsureInstance();
    }
}