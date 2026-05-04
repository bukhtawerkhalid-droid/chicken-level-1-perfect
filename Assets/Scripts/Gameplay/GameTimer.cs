using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float remainingTime;
    [SerializeField] private bool isRunning;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    public event Action<float> OnTimeChanged;
    public event Action OnTimeExpired;

    public void Initialize(float seconds)
    {
        remainingTime = Mathf.Max(0f, seconds);
        isRunning = true;
        OnTimeChanged?.Invoke(remainingTime);
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            OnTimeChanged?.Invoke(remainingTime);
            OnTimeExpired?.Invoke();
            return;
        }

        OnTimeChanged?.Invoke(remainingTime);
    }

    public static string FormatMMSS(float seconds)
    {
        int clamped = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int minutes = clamped / 60;
        int secs = clamped % 60;
        return $"{minutes:00}:{secs:00}";
    }
}
