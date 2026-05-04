using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteClickForwarder : MonoBehaviour, IPointerClickHandler
{
    private Action clickAction;

    public void SetAction(Action action)
    {
        clickAction = action;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerClick("OnPointerClick");
    }

    private void OnMouseDown()
    {
        TriggerClick("OnMouseDown");
    }

    private void TriggerClick(string source)
    {
        Debug.Log($"SpriteClickForwarder: {gameObject.name} clicked via {source}");

        if (clickAction != null)
        {
            clickAction.Invoke();
            return;
        }

        LevelFlowController flow = FindFirstObjectByType<LevelFlowController>();
        if (flow == null)
        {
            Debug.LogWarning("SpriteClickForwarder: LevelFlowController not found");
            return;
        }

        string n = gameObject.name.ToLowerInvariant();
        if (n.Contains("play button"))
        {
            flow.OnPlayClicked();
        }
        else if (n.Contains("next button"))
        {
            flow.OnNextClicked();
        }
        else if (n == "retry" || n.Contains("retry"))
        {
            flow.OnRetryClicked();
        }
    }
}
