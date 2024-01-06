using UnityEngine;

[CreateAssetMenu(menuName = "PuzzleConfig", fileName = "PuzzleConfig", order = 0)]
public class PuzzleConfig : ScriptableObject
{
    public int width;
    public int height;
    public int scoreToComplete;
    public int timeToComplete;
    public PuzzleObjectConfig[] puzzleObjects;
}