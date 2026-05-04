using System;
using UnityEngine;

public class LevelSession : MonoBehaviour
{
    public static LevelSession Current { get; private set; }

    [SerializeField] private LevelUIController ui;
    [SerializeField] private GameTimer timer;

    private int collectedChicks;
    private int minRequiredChicks;
    private bool levelEnded;

    public event Action<bool, string> OnLevelEnded;

    private void Awake()
    {
        Current = this;
        if (timer == null)
        {
            timer = GetComponent<GameTimer>();
        }

        if (ui == null)
        {
            ui = FindFirstObjectByType<LevelUIController>();
        }
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }

    public void Initialize(LevelData levelData)
    {
        collectedChicks = 0;
        minRequiredChicks = Mathf.Max(0, levelData.minChicksRequired);
        levelEnded = false;

        if (ui != null)
        {
            ui.SetLevel(levelData.levelNumber);
            ui.SetChicks(collectedChicks, minRequiredChicks);
        }

        if (timer == null)
        {
            Debug.LogWarning("LevelSession has no GameTimer reference.");
            return;
        }

        timer.OnTimeChanged -= HandleTimeChanged;
        timer.OnTimeExpired -= HandleTimeExpired;
        timer.OnTimeChanged += HandleTimeChanged;
        timer.OnTimeExpired += HandleTimeExpired;
        timer.Initialize(levelData.levelTime);
    }

    public void RegisterChickCollected(ChickCollectible _)
    {
        if (levelEnded)
        {
            return;
        }

        collectedChicks++;
        if (ui != null)
        {
            ui.SetChicks(collectedChicks, minRequiredChicks);
        }
    }

    public void TryCompleteLevel()
    {
        if (levelEnded)
        {
            return;
        }

        if (timer != null && timer.RemainingTime <= 0f)
        {
            EndLevel(false, "Time up");
            return;
        }

        if (collectedChicks >= minRequiredChicks)
        {
            EndLevel(true, "Level complete");
        }
        else
        {
            EndLevel(false, "Not enough chicks");
        }
    }

    private void HandleTimeChanged(float time)
    {
        if (ui != null)
        {
            ui.SetTime(GameTimer.FormatMMSS(time));
        }
    }

    private void HandleTimeExpired()
    {
        EndLevel(false, "Time up");
    }

    private void EndLevel(bool won, string reason)
    {
        if (levelEnded)
        {
            return;
        }

        levelEnded = true;
        if (timer != null)
        {
            timer.StopTimer();
        }

        if (ui != null)
        {
            ui.SetResult(won ? "WIN" : "LOSE", reason);
        }
        OnLevelEnded?.Invoke(won, reason);
        Debug.Log($"Level ended. Won: {won}. Reason: {reason}");
    }
}
