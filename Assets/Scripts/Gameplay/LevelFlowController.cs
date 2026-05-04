using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFlowController : MonoBehaviour
{
    private static bool autoResumeGameplayAfterReload;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private PlayerMover2D playerMover;

    [Header("Screens")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject retryButton;

    private bool hasStarted;

    private void Awake()
    {
        if (levelLoader == null) levelLoader = FindFirstObjectByType<LevelLoader>();
        if (levelSession == null) levelSession = FindFirstObjectByType<LevelSession>();
        if (playerMover == null) playerMover = FindFirstObjectByType<PlayerMover2D>();

        AutoFindScreens();

        if (levelSession != null)
        {
            levelSession.OnLevelEnded += HandleLevelEnded;
        }

        BindButton(playButton, OnPlayClicked);
        BindButton(nextButton, OnNextClicked);
        BindButton(retryButton, OnRetryClicked);
    }

    private void Start()
    {
        if (autoResumeGameplayAfterReload)
        {
            autoResumeGameplayAfterReload = false;
            OnPlayClicked();
            return;
        }

        ShowStart();
    }

    private void OnDestroy()
    {
        if (levelSession != null)
        {
            levelSession.OnLevelEnded -= HandleLevelEnded;
        }
    }

    public void ShowStart()
    {
        SetActive(startScreen, true);
        SetActive(playButton, true);
        SetActive(winScreen, false);
        SetActive(nextButton, false);
        SetActive(loseScreen, false);
        SetActive(retryButton, false);
        SetGameplayActive(false);
    }

    public void OnPlayClicked()
    {
        Debug.Log("LevelFlowController: OnPlayClicked fired");

        hasStarted = true;
        SetActive(startScreen, false);
        SetActive(playButton, false);
        SetActive(winScreen, false);
        SetActive(nextButton, false);
        SetActive(loseScreen, false);
        SetActive(retryButton, false);

        if (levelLoader != null && levelLoader.CanLoadConfiguredLevel)
        {
            levelLoader.LoadConfiguredLevel();
        }
        else
        {
            Debug.LogWarning("LevelFlowController: Loader config missing, continuing with current scene state.");
        }

        SetGameplayActive(true);
    }

    public void OnNextClicked()
    {
        if (!hasStarted)
        {
            return;
        }

        if (levelLoader != null)
        {
            levelLoader.LoadNextLevel();
        }
        SetActive(winScreen, false);
        SetActive(nextButton, false);
        SetActive(loseScreen, false);
        SetActive(retryButton, false);
        SetGameplayActive(true);
    }

    public void OnRetryClicked()
    {
        Debug.Log("LevelFlowController: OnRetryClicked fired");

        if (levelLoader != null && levelLoader.CanLoadConfiguredLevel)
        {
            levelLoader.LoadConfiguredLevel();
        }
        else
        {
            Debug.LogWarning("LevelFlowController: Loader config missing on retry, reloading scene.");
            autoResumeGameplayAfterReload = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        SetActive(winScreen, false);
        SetActive(nextButton, false);
        SetActive(loseScreen, false);
        SetActive(retryButton, false);
        SetGameplayActive(true);
    }

    private void HandleLevelEnded(bool won, string _)
    {
        SetGameplayActive(false);
        SetActive(winScreen, won);
        SetActive(nextButton, won);
        SetActive(loseScreen, !won);
        SetActive(retryButton, !won);
    }

    private void SetGameplayActive(bool active)
    {
        if (playerMover != null)
        {
            playerMover.IsInputEnabled = active;
        }

        EnemyPingPong[] enemies = FindObjectsByType<EnemyPingPong>(FindObjectsSortMode.None);
        foreach (EnemyPingPong enemy in enemies)
        {
            enemy.enabled = active;
        }
    }

    private void AutoFindScreens()
    {
        if (startScreen == null) startScreen = FindByNameContains("Start image 1");
        if (playButton == null) playButton = FindByNameContains("Play Button");
        if (winScreen == null) winScreen = FindByNameContains("you win");
        if (nextButton == null) nextButton = FindByNameContains("Next button");
        if (loseScreen == null) loseScreen = FindByNameContains("you lose");
        if (retryButton == null) retryButton = FindByNameContains("retry");
    }

    private static GameObject FindByNameContains(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
        {
            return null;
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform found = FindInChildren(root.transform, fragment);
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindInChildren(Transform root, string fragment)
    {
        if (root.name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindInChildren(root.GetChild(i), fragment);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }

    private static void BindButton(GameObject target, Action onClick)
    {
        if (target == null || onClick == null)
        {
            return;
        }

        SpriteClickForwarder forwarder = target.GetComponent<SpriteClickForwarder>();
        if (forwarder == null)
        {
            forwarder = target.AddComponent<SpriteClickForwarder>();
        }

        forwarder.SetAction(onClick);

        Collider2D col = target.GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D box = target.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                box.size = sr.sprite.bounds.size;
            }
            box.isTrigger = true;
        }
    }
}
