using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private LevelData levelToLoad;
    [SerializeField] private LevelData[] levelSequence;
    [SerializeField] private int currentLevelIndex;

    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private LevelSession levelSession;
    [SerializeField] private Transform runtimeRoot;
    [SerializeField] private Transform platformsRoot;
    [SerializeField] private Transform chicksRoot;
    [SerializeField] private Transform actorsRoot;

    [Header("Prefabs")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject chickPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject basketPrefab;

    public bool CanLoadConfiguredLevel => levelToLoad != null && levelSession != null && platformPrefab != null && chickPrefab != null && enemyPrefab != null && basketPrefab != null;

    private void Awake()
    {
        AutoWire();
        EnsureHierarchy();
    }

    private void Start()
    {
        if (levelToLoad == null && levelSequence != null && levelSequence.Length > 0)
        {
            levelToLoad = levelSequence[Mathf.Clamp(currentLevelIndex, 0, levelSequence.Length - 1)];
        }

        if (levelToLoad != null && levelSession != null)
        {
            levelSession.Initialize(levelToLoad);
        }
    }

    [ContextMenu("Load Configured Level")]
    public void LoadConfiguredLevel()
    {
        AutoWire();
        if (levelToLoad != null)
        {
            LoadLevel(levelToLoad);
        }
    }

    public void LoadNextLevel()
    {
        if (levelSequence == null || levelSequence.Length == 0)
        {
            LoadConfiguredLevel();
            return;
        }

        currentLevelIndex = (currentLevelIndex + 1) % levelSequence.Length;
        levelToLoad = levelSequence[currentLevelIndex];
        LoadConfiguredLevel();
    }

    public void LoadLevel(LevelData levelData)
    {
        AutoWire();
        if (levelData == null || levelSession == null || platformPrefab == null || chickPrefab == null || enemyPrefab == null || basketPrefab == null)
        {
            Debug.LogError("LevelLoader is missing required references.");
            return;
        }

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        EnsureHierarchy();
        ClearRuntimeObjects();

        SpawnPlatforms(levelData);
        SpawnChicks(levelData);
        SpawnEnemy(levelData);
        SpawnBasket(levelData);

        if (player != null)
        {
            player.position = levelData.playerStartPosition;
            player.gameObject.SetActive(true);

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }

            if (player.GetComponent<ChickChainController>() == null)
            {
                player.gameObject.AddComponent<ChickChainController>();
            }
            if (player.GetComponent<ScreenWrap2D>() == null)
            {
                player.gameObject.AddComponent<ScreenWrap2D>();
            }
            Debug.Log($"LevelLoader: Player reset to {levelData.playerStartPosition}");
        }
        else
        {
            Debug.LogWarning("LevelLoader: Player reference is missing. Could not reset player.");
        }

        levelSession.Initialize(levelData);
        Debug.Log($"LevelLoader: Loaded level {levelData.levelNumber}");
    }

    private void AutoWire()
    {
        if (levelSession == null)
        {
            levelSession = GetComponent<LevelSession>();
        }

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }
    }

    private void SpawnPlatforms(LevelData levelData)
    {
        foreach (Vector2 pos in levelData.platformPositions)
        {
            GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity, platformsRoot);
            BoxCollider2D col = platform.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = platform.AddComponent<BoxCollider2D>();
            }
            col.isTrigger = false;
        }
    }

    private void SpawnChicks(LevelData levelData)
    {
        foreach (Vector2 pos in levelData.chickPositions)
        {
            GameObject chick = Instantiate(chickPrefab, pos, Quaternion.identity, chicksRoot);
            CircleCollider2D col = chick.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = chick.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;

            if (chick.GetComponent<ChickCollectible>() == null)
            {
                chick.AddComponent<ChickCollectible>();
            }
        }
    }

    private void SpawnEnemy(LevelData levelData)
    {
        GameObject enemy = Instantiate(enemyPrefab, levelData.enemyPosition, Quaternion.identity, actorsRoot);
        CapsuleCollider2D cap = enemy.GetComponent<CapsuleCollider2D>();
        if (cap == null)
        {
            cap = enemy.AddComponent<CapsuleCollider2D>();
        }
        cap.isTrigger = true;

        if (enemy.GetComponent<EnemyHazard>() == null)
        {
            enemy.AddComponent<EnemyHazard>();
        }
    }

    private void SpawnBasket(LevelData levelData)
    {
        GameObject basket = Instantiate(basketPrefab, levelData.basketPosition, Quaternion.identity, actorsRoot);
        BoxCollider2D col = basket.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = basket.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;

        if (basket.GetComponent<BasketGoal>() == null)
        {
            basket.AddComponent<BasketGoal>();
        }
    }

    private void ClearRuntimeObjects()
    {
        ClearChildren(platformsRoot);
        ClearChildren(chicksRoot);
        ClearChildren(actorsRoot);
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void EnsureHierarchy()
    {
        if (runtimeRoot == null)
        {
            runtimeRoot = EnsureChild(transform, "RuntimeLevel");
        }

        if (platformsRoot == null)
        {
            platformsRoot = EnsureChild(runtimeRoot, "Platforms");
        }

        if (chicksRoot == null)
        {
            chicksRoot = EnsureChild(runtimeRoot, "Chicks");
        }

        if (actorsRoot == null)
        {
            actorsRoot = EnsureChild(runtimeRoot, "Actors");
        }
    }

    private static Transform EnsureChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing;
        }

        GameObject go = new GameObject(childName);
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }
}
