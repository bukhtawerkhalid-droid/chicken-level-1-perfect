using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LevelBuilderWindow : EditorWindow
{
    private string assetName = "Level_01";
    private string saveFolder = "Assets/LevelData";
    private int levelNumber = 1;
    private int minChicksRequired = 1;
    private float levelTime = 60f;

    [MenuItem("Tools/Level Builder")]
    public static void ShowWindow()
    {
        GetWindow<LevelBuilderWindow>("Level Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Capture Current Scene To LevelData", EditorStyles.boldLabel);

        assetName = EditorGUILayout.TextField("Level Name", assetName);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
        levelNumber = EditorGUILayout.IntField("Level Number", levelNumber);
        minChicksRequired = EditorGUILayout.IntField("Min Chicks Required", minChicksRequired);
        levelTime = EditorGUILayout.FloatField("Level Time (s)", levelTime);

        GUILayout.Space(8f);

        if (GUILayout.Button("Capture Current Scene"))
        {
            CaptureCurrentScene();
        }
    }

    private void CaptureCurrentScene()
    {
        EnsureFolder(saveFolder);

        var data = CreateInstance<LevelData>();
        data.levelNumber = Mathf.Max(1, levelNumber);
        data.minChicksRequired = Mathf.Max(0, minChicksRequired);
        data.levelTime = Mathf.Max(1f, levelTime);

        data.platformPositions = FindPositionsByPrefix("Floor");
        data.chickPositions = FindPositionsByPrefix("Chick");

        data.playerStartPosition = FindPlayerPosition();
        data.basketPosition = FindSinglePosition("Exit_basket_0", "Basket");
        
        // Capture all enemies (supporting both "Enemy" and "Cat" prefixes)
        data.enemyPositions = FindPositionsByPrefix("Enemy");
        data.enemyPositions.AddRange(FindPositionsByPrefix("Cat"));

        string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{assetName}.asset");
        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;
        Debug.Log($"LevelData created: {path}");
    }

    private static List<Vector2> FindPositionsByPrefix(string prefix)
    {
        return Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(t => t.name.StartsWith(prefix) && !t.CompareTag("Player") && !t.name.Contains("character"))
            .Select(t => (Vector2)t.position)
            .ToList();
    }

    private static Vector2 FindSinglePosition(string primaryName, string fallbackPrefix)
    {
        Transform exact = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.name == primaryName);

        if (exact != null)
        {
            return exact.position;
        }

        Transform fallback = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.name.StartsWith(fallbackPrefix));

        return fallback != null ? (Vector2)fallback.position : Vector2.zero;
    }

    private static Vector2 FindPlayerPosition()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in roots)
        {
            if (go.CompareTag("Player"))
            {
                return go.transform.position;
            }
        }

        Transform fallback = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.name.StartsWith("Chicken_character"));
        return fallback != null ? (Vector2)fallback.position : Vector2.zero;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
