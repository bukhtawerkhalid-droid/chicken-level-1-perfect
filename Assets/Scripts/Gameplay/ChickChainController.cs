using System.Collections.Generic;
using UnityEngine;

public class ChickChainController : MonoBehaviour
{
    [SerializeField] private Transform chainRoot;
    [SerializeField] private float segmentSpacing = 0.75f;
    [SerializeField] private float followSpeed = 14f;
    [SerializeField] private float minRecordDistance = 0.05f;

    private readonly List<Transform> followers = new List<Transform>();
    private readonly List<Vector3> history = new List<Vector3>();

    private void Awake()
    {
        if (chainRoot == null)
        {
            chainRoot = transform;
        }

        history.Clear();
        history.Add(transform.position);
    }

    private void LateUpdate()
    {
        RecordHeadPosition();
        UpdateFollowers();
    }

    public void AttachChick(Transform chick)
    {
        if (chick == null || followers.Contains(chick))
        {
            return;
        }

        chick.SetParent(chainRoot);
        followers.Add(chick);

        if (chick.GetComponent<ScreenWrap2D>() == null)
        {
            chick.gameObject.AddComponent<ScreenWrap2D>();
        }
    }

    private void RecordHeadPosition()
    {
        Vector3 current = transform.position;
        if (history.Count == 0 || Vector3.Distance(history[0], current) >= minRecordDistance)
        {
            history.Insert(0, current);
        }

        int maxHistory = Mathf.Max(256, followers.Count * 40);
        if (history.Count > maxHistory)
        {
            history.RemoveRange(maxHistory, history.Count - maxHistory);
        }
    }

    private void UpdateFollowers()
    {
        int pointsPerSegment = Mathf.Max(1, Mathf.RoundToInt(segmentSpacing / Mathf.Max(minRecordDistance, 0.01f)));

        for (int i = 0; i < followers.Count; i++)
        {
            Transform follower = followers[i];
            if (follower == null)
            {
                continue;
            }

            int historyIndex = Mathf.Min((i + 1) * pointsPerSegment, history.Count - 1);
            Vector3 target = history[historyIndex];
            follower.position = Vector3.Lerp(follower.position, target, followSpeed * Time.deltaTime);
        }
    }
}
