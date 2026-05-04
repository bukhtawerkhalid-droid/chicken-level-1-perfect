using UnityEngine;

public class BasketGoal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        LevelSession.Current?.TryCompleteLevel();
    }
}
