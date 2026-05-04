using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private Text chickCounterText;
    [SerializeField] private Text levelCounterText;
    [SerializeField] private Text timeCounterText;
    [SerializeField] private Text resultText;

    private TextMesh chickMesh;
    private TextMesh levelMesh;
    private TextMesh timeMesh;

    private void Awake()
    {
        // Using your exact settings for the chick counter with default 0/0
        chickMesh = EnsureCounterMesh("chick counter_0", "ChickCounterText", new Vector3(0.76f, -0.57f, -0.1f), new Vector3(2f, 2f, 1f), "Chicks 0/0");
        
        // Using your exact settings for the level counter
        levelMesh = EnsureCounterMesh("level counter_0", "LevelCounterText", new Vector3(0.2f, -0.18f, -0.1f), new Vector3(2f, 2f, 1f), "Level 1");

        // Using your exact settings for the time counter (Higher scale to compensate for the small parent)
        timeMesh = EnsureCounterMesh("Time counter_0", "TimeCounterText", new Vector3(2.13f, -0.58f, -0.1f), new Vector3(3f, 3f, 1f), "0s");
    }

    public void SetChicks(int current, int required)
    {
        if (chickMesh == null) 
            chickMesh = EnsureCounterMesh("chick counter_0", "ChickCounterText", new Vector3(0.76f, -0.57f, -0.1f), new Vector3(2f, 2f, 1f), "Chicks 0/0");

        string text = $"Chicks {current}/{required}";
        if (chickCounterText != null)
        {
            chickCounterText.text = text;
        }
        if (chickMesh != null)
        {
            chickMesh.text = text;
        }
    }

    public void SetLevel(int levelNumber)
    {
        if (levelMesh == null)
            levelMesh = EnsureCounterMesh("level counter_0", "LevelCounterText", new Vector3(0f, 0f, -0.1f), Vector3.one, "Level 1");

        string text = $"Level {levelNumber}";
        if (levelCounterText != null)
        {
            levelCounterText.text = text;
        }
        if (levelMesh != null)
        {
            levelMesh.text = text;
        }
    }

    public void SetTimeSeconds(int seconds)
    {
        if (timeMesh == null)
            timeMesh = EnsureCounterMesh("Time counter_0", "TimeCounterText", new Vector3(0f, 0f, -0.1f), Vector3.one, "0s");

        string text = $"{Mathf.Max(0, seconds)}s";
        if (timeCounterText != null)
        {
            timeCounterText.text = text;
        }
        if (timeMesh != null)
        {
            timeMesh.text = text;
        }
    }

    public void SetResult(string result, string reason)
    {
        if (resultText != null)
        {
            if (string.IsNullOrEmpty(result))
            {
                resultText.text = string.Empty;
            }
            else
            {
                resultText.text = string.IsNullOrEmpty(reason) ? result : $"{result}\n{reason}";
            }
        }
    }

    private static TextMesh EnsureCounterMesh(string anchorName, string childName, Vector3 localOffset, Vector3 localScale, string defaultText)
    {
        // More robust search: GameObject.Find only finds active objects.
        // Let's search all objects to be sure.
        GameObject anchor = GameObject.Find(anchorName);
        if (anchor == null)
        {
            // Fallback: search by tag or type if name fails
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == anchorName)
                {
                    anchor = obj;
                    break;
                }
            }
        }

        if (anchor == null)
        {
            Debug.LogWarning($"LevelUIController: Could not find anchor '{anchorName}' in the scene!");
            return null;
        }

        Transform child = anchor.transform.Find(childName);
        GameObject go;
        bool isNew = false;
        if (child == null)
        {
            go = new GameObject(childName);
            go.transform.SetParent(anchor.transform);
            isNew = true;
            Debug.Log($"LevelUIController: Created new text object '{childName}' under '{anchorName}'");
        }
        else
        {
            go = child.gameObject;
        }

        // Force exact position and scale from parameters
        go.transform.localPosition = localOffset;
        go.transform.localScale = localScale;

        TextMesh mesh = go.GetComponent<TextMesh>();
        if (mesh == null)
        {
            mesh = go.AddComponent<TextMesh>();
        }

        // Set default text if it was just created or if it's currently empty
        if (isNew || string.IsNullOrEmpty(mesh.text))
        {
            mesh.text = defaultText;
        }

        // Apply visual settings for high quality and centering
        mesh.fontSize = 64;
        mesh.characterSize = 0.08f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        
        // Set Color to Dark Brown
        mesh.color = new Color(0.39f, 0.26f, 0.13f, 1.0f);

        // Fix Visibility: Ensure it renders on top of the sprite panel
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 20;
        }

        return mesh;
    }
}
