using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private Text chickCounterText;
    [SerializeField] private Text levelCounterText;
    [SerializeField] private Text timeCounterText;
    [SerializeField] private Text resultText;

    public void SetChicks(int current, int required)
    {
        if (chickCounterText != null)
        {
            chickCounterText.text = $"Chicks: {current}/{required}";
        }
    }

    public void SetLevel(int levelNumber)
    {
        if (levelCounterText != null)
        {
            levelCounterText.text = $"Level: {levelNumber}";
        }
    }

    public void SetTime(string timeText)
    {
        if (timeCounterText != null)
        {
            timeCounterText.text = $"Time: {timeText}";
        }
    }

    public void SetResult(string result, string reason)
    {
        if (resultText != null)
        {
            resultText.text = $"{result} - {reason}";
        }
    }
}
