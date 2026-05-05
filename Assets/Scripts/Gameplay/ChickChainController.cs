using System.Collections.Generic;
using UnityEngine;

public class ChickChainController : MonoBehaviour
{
    [SerializeField] private Transform chainRoot;
    [SerializeField] private float segmentSpacing = 0.38f;
    [SerializeField] private float followSpeed = 16f;
    [SerializeField] private float minRecordDistance = 0.03f;

    private readonly List<Transform> followers = new List<Transform>();
    private readonly List<Vector3> history = new List<Vector3>();

    public int Count => followers.Count;

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

    public bool RemoveLastFollower()
    {
        if (followers.Count == 0)
        {
            return false;
        }

        int idx = followers.Count - 1;
        Transform last = followers[idx];
        followers.RemoveAt(idx);

        if (last != null)
        {
            last.gameObject.SetActive(false);
            last.SetParent(null); // Unparent so it doesn't follow the player while hidden
        }

        return true;
    }

    private void RecordHeadPosition()
    {
        Vector3 current = transform.position;
        if (history.Count == 0 || Vector3.Distance(history[0], current) >= minRecordDistance)
        {
            history.Insert(0, current);
        }

        int maxHistory = Mathf.Max(256, followers.Count * 48);
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
            
            // Handle visual orientation
            if (Mathf.Abs(target.x - follower.position.x) > 0.01f)
            {
                SpriteRenderer sr = follower.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.flipX = target.x < follower.position.x;
                }
            }

            follower.position = Vector3.Lerp(follower.position, target, followSpeed * Time.deltaTime);
        }
    }

    public void ResetChain()
    {
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                follower.gameObject.SetActive(false);
                follower.SetParent(null);
            }
        }
        followers.Clear();
        history.Clear();
        history.Add(transform.position);
    }
}
