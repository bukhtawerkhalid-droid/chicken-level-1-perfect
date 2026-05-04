using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Chicken/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    public int levelNumber = 1;

    [Header("Spawn Positions")]
    public List<Vector2> platformPositions = new List<Vector2>();
    public List<Vector2> chickPositions = new List<Vector2>();
    public Vector2 playerStartPosition;
    public Vector2 basketPosition;
    public Vector2 enemyPosition;

    [Header("Rules")]
    public int minChicksRequired = 1;
    public float levelTime = 60f;
}
