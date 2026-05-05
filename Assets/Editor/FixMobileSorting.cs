using UnityEngine;
using UnityEditor;

public class FixMobileSorting : EditorWindow
{
    [MenuItem("Tools/Chicken Game/Fix Scene Sorting for Mobile")]
    public static void FixSorting()
    {
        // 1. Fix Backgrounds
        int bgCount = 0;
        SpriteRenderer[] allSR = FindObjectsOfType<SpriteRenderer>(true);
        foreach (var sr in allSR)
        {
            string name = sr.gameObject.name.ToLower();
            if (name.Contains("background"))
            {
                sr.sortingOrder = -10;
                EditorUtility.SetDirty(sr.gameObject);
                bgCount++;
            }
            else if (name.Contains("floor") || name.Contains("platform"))
            {
                sr.sortingOrder = 0;
                EditorUtility.SetDirty(sr.gameObject);
            }
            else if (name.Contains("chick"))
            {
                sr.sortingOrder = 5;
                EditorUtility.SetDirty(sr.gameObject);
            }
            else if (name.Contains("hen") || name.Contains("chicken_character") || sr.gameObject.CompareTag("Player"))
            {
                sr.sortingOrder = 10;
                EditorUtility.SetDirty(sr.gameObject);
            }
        }

        Debug.Log("Sorting Fix Complete! Backgrounds pushed to -10, Floors at 0, Player at 10.");
        EditorUtility.DisplayDialog("Sorting Fix", "Successfully updated sorting orders for mobile visibility!", "OK");
    }
}
