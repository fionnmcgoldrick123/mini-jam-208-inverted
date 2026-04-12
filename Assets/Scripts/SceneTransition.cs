using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    private const string StartTrigger = "Start";
    private const string EndTrigger = "End";

    private static SceneTransition instance;

    [Header("References")]
    [SerializeField] private Animator animator;

    private bool isTransitioning;
    private string pendingSceneName;
    private int pendingBuildIndex = -1;
    private Coroutine endRoutine;
    private Coroutine startRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (animator == null)
            animator = GetComponent<Animator>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BeginStartTransition();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        BeginStartTransition();
    }

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        instance.BeginSceneLoad(sceneName, -1);
    }

    public static void LoadScene(int buildIndex)
    {
        if (instance == null)
        {
            SceneManager.LoadScene(buildIndex);
            return;
        }

        instance.BeginSceneLoad(null, buildIndex);
    }

    public static void ReloadActiveScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(active.name))
            LoadScene(active.name);
        else
            LoadScene(active.buildIndex);
    }

    private void BeginSceneLoad(string sceneName, int buildIndex)
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        pendingSceneName = sceneName;
        pendingBuildIndex = buildIndex;

        if (animator == null)
        {
            ExecuteLoad();
            return;
        }

        animator.ResetTrigger(StartTrigger);
        animator.SetTrigger(EndTrigger);

        if (endRoutine != null)
            StopCoroutine(endRoutine);
        endRoutine = StartCoroutine(WaitForEndThenLoad());
    }

    public void OnEndTransitionComplete()
    {
        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
        }

        ExecuteLoad();
    }

    private IEnumerator WaitForEndThenLoad()
    {
        float elapsed = 0f;
        float maxWait = 1f;

        while (elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        endRoutine = null;
        ExecuteLoad();
    }

    private void BeginStartTransition()
    {
        if (startRoutine != null)
            StopCoroutine(startRoutine);
        startRoutine = StartCoroutine(PlayStartNextFrame());
    }

    private IEnumerator PlayStartNextFrame()
    {
        yield return null;
        startRoutine = null;

        if (animator == null)
        {
            isTransitioning = false;
            yield break;
        }

        animator.Rebind();
        animator.Update(0f);
        animator.ResetTrigger(EndTrigger);
        animator.SetTrigger(StartTrigger);

        isTransitioning = false;
    }

    private void ExecuteLoad()
    {
        if (!string.IsNullOrEmpty(pendingSceneName))
        {
            string sceneName = pendingSceneName;
            pendingSceneName = null;
            pendingBuildIndex = -1;
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (pendingBuildIndex >= 0)
        {
            int idx = pendingBuildIndex;
            pendingBuildIndex = -1;
            SceneManager.LoadScene(idx);
            return;
        }

        isTransitioning = false;
    }
}