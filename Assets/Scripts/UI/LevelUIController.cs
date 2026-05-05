using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private Text chickCounterText;
    [SerializeField] private Text levelCounterText;
    [SerializeField] private Text timeCounterText;
    [SerializeField] private Text resultText;

    [System.Serializable]
    public class CounterVisualSettings
    {
        public Color color = new Color(0.39f, 0.26f, 0.13f, 1.0f);
        public Font font;
        public int fontSize = 64;
        public FontStyle fontStyle = FontStyle.Normal;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }

    [Header("Visual Settings")]
    public CounterVisualSettings chickCounterSettings;
    public CounterVisualSettings levelCounterSettings;
    public CounterVisualSettings timeCounterSettings;

    private TextMesh chickMesh;
    private TextMesh levelMesh;
    private TextMesh timeMesh;

    private void Awake()
    {
        // Initialize the meshes
        chickMesh = EnsureCounterMesh("chick counter_0", "ChickCounterText", "Chicks 0/0", chickCounterSettings);
        levelMesh = EnsureCounterMesh("level counter_0", "LevelCounterText", "Level 1", levelCounterSettings);
        timeMesh = EnsureCounterMesh("Time counter_0", "TimeCounterText", "0s", timeCounterSettings);
        
        ApplyAllStyles();
    }

    private void OnValidate()
    {
        // This makes sure your manual settings in the Inspector show up immediately!
        if (Application.isPlaying)
        {
            ApplyAllStyles();
        }
    }

    public void ApplyAllStyles()
    {
        if (chickMesh != null) ApplyVisualSettings(chickMesh, chickCounterSettings);
        if (levelMesh != null) ApplyVisualSettings(levelMesh, levelCounterSettings);
        if (timeMesh != null) ApplyVisualSettings(timeMesh, timeCounterSettings);
    }

    private void ApplyVisualSettings(TextMesh mesh, CounterVisualSettings settings)
    {
        if (mesh == null || settings == null) return;

        // Force the text to match YOUR Inspector settings exactly
        mesh.transform.localPosition = settings.localPosition;
        mesh.transform.localScale = settings.localScale;
        mesh.color = settings.color;
        mesh.fontSize = settings.fontSize;
        mesh.fontStyle = settings.fontStyle;

        if (settings.font != null)
        {
            mesh.font = settings.font;
            MeshRenderer mr = mesh.GetComponent<MeshRenderer>();
            if (mr != null) mr.material = settings.font.material;
        }

        mesh.characterSize = 0.08f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        
        MeshRenderer renderer = mesh.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 20;
        }
    }

    public void SetChicks(int current, int required)
    {
        if (chickMesh == null) 
            chickMesh = EnsureCounterMesh("chick counter_0", "ChickCounterText", "Chicks 0/0", chickCounterSettings);

        string text = $"Chicks {current}/{required}";
        if (chickCounterText != null) chickCounterText.text = text;
        if (chickMesh != null) chickMesh.text = text;
    }

    public void SetLevel(int levelNumber)
    {
        if (levelMesh == null)
            levelMesh = EnsureCounterMesh("level counter_0", "LevelCounterText", "Level 1", levelCounterSettings);

        string text = $"Level {levelNumber}";
        if (levelCounterText != null) levelCounterText.text = text;
        if (levelMesh != null) levelMesh.text = text;
    }

    public void SetTimeSeconds(int seconds)
    {
        if (timeMesh == null)
            timeMesh = EnsureCounterMesh("Time counter_0", "TimeCounterText", "0s", timeCounterSettings);

        string text = $"{Mathf.Max(0, seconds)}s";
        if (timeCounterText != null) timeCounterText.text = text;
        if (timeMesh != null) timeMesh.text = text;
    }

    public void SetResult(string result, string reason)
    {
        if (resultText != null)
        {
            resultText.text = string.IsNullOrEmpty(result) ? "" : (string.IsNullOrEmpty(reason) ? result : $"{result}\n{reason}");
        }
    }

    private TextMesh EnsureCounterMesh(string anchorName, string childName, string defaultText, CounterVisualSettings settings)
    {
        GameObject anchor = GameObject.Find(anchorName);
        if (anchor == null)
        {
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects) { if (obj.name == anchorName) { anchor = obj; break; } }
        }

        if (anchor == null) return null;

        Transform child = anchor.transform.Find(childName);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(childName);
            go.transform.SetParent(anchor.transform);
        }
        else
        {
            go = child.gameObject;
        }

        TextMesh mesh = go.GetComponent<TextMesh>();
        if (mesh == null) mesh = go.AddComponent<TextMesh>();

        ApplyVisualSettings(mesh, settings);

        if (string.IsNullOrEmpty(mesh.text)) mesh.text = defaultText;

        return mesh;
    }
}
