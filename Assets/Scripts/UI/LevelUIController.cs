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
        chickMesh = EnsureCounterMesh("chick counter_0", "ChickCounterText", new Vector3(0f, 0f, -0.2f));
        levelMesh = EnsureCounterMesh("level counter_0", "LevelCounterText", new Vector3(0f, 0f, -0.2f));
        timeMesh = EnsureCounterMesh("Time counter_0", "TimeCounterText", new Vector3(0f, 0f, -0.2f));
    }

    public void SetChicks(int current, int required)
    {
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

    private static TextMesh EnsureCounterMesh(string anchorName, string childName, Vector3 localOffset)
    {
        GameObject anchor = GameObject.Find(anchorName);
        if (anchor == null)
        {
            return null;
        }

        Transform child = anchor.transform.Find(childName);
        if (child == null)
        {
            GameObject go = new GameObject(childName);
            go.transform.SetParent(anchor.transform);
            go.transform.localPosition = localOffset;
            go.transform.localScale = Vector3.one * 0.2f;
            child = go.transform;
        }

        TextMesh mesh = child.GetComponent<TextMesh>();
        if (mesh == null)
        {
            mesh = child.gameObject.AddComponent<TextMesh>();
        }

        mesh.fontSize = 64;
        mesh.characterSize = 0.15f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = Color.black;

        return mesh;
    }
}
