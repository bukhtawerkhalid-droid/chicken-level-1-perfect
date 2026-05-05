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

    public bool CanLoadConfiguredLevel => levelToLoad != null && levelSession != null;

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
        if (levelData == null || levelSession == null)
        {
            Debug.LogError("LevelLoader is missing required level data or session reference.");
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

        if (platformPrefab != null) SpawnPlatforms(levelData);
        if (chickPrefab != null) SpawnChicks(levelData);
        if (enemyPrefab != null) SpawnEnemies(levelData);
        if (basketPrefab != null) SpawnBasket(levelData);

        ResetManualObjects(levelData);

        if (player != null)
        {
            player.position = levelData.playerStartPosition;
            player.rotation = Quaternion.identity; // Snap back to upright
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
            
            // Fix: Explicitly set sorting order for visibility on mobile
            SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 0;
            }

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

    private void SpawnEnemies(LevelData levelData)
    {
        foreach (Vector2 pos in levelData.enemyPositions)
        {
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, actorsRoot);
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
        if (platformPrefab != null) ClearChildren(platformsRoot);
        if (chickPrefab != null) ClearChildren(chicksRoot);
        if (enemyPrefab != null) ClearChildren(actorsRoot);
        // Note: basketPrefab check not strictly needed if it's in actorsRoot, 
        // but adding logic to clear actorsRoot only if we spawn something.
        if (enemyPrefab == null && basketPrefab != null) ClearChildren(actorsRoot);
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

    private void ResetManualObjects(LevelData levelData)
    {
        // Reset the player's chain FIRST so it releases the followers
        ChickChainController chain = FindFirstObjectByType<ChickChainController>();
        if (chain != null)
        {
            chain.ResetChain();
        }

        // Reset all chicks in the scene (manual or spawned)
        ChickCollectible[] allChicks = FindObjectsByType<ChickCollectible>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        int requiredCount = levelData.chickPositions.Count;
        // If we are using manual chicks and have no positions in LevelData, 
        // maybe use a default or show all? 
        // But usually LevelData should dictate the count.
        
        for (int i = 0; i < allChicks.Length; i++)
        {
            var chick = allChicks[i];
            chick.ResetCollectible();
            
            // If this chick is extra (more than what the current level needs), hide it!
            if (i >= requiredCount)
            {
                chick.gameObject.SetActive(false);
            }
            else
            {
                // Only move them if we are NOT on Level 1, 
                // because Level 1 is usually the "default scene setup"
                if (levelData.levelNumber > 1)
                {
                    chick.transform.position = levelData.chickPositions[i];
                }
            }
        }
    }
}
