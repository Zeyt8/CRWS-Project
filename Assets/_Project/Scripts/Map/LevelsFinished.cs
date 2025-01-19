using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsFinished", menuName = "Scriptable Objects/LevelsFinished")]
public class LevelsFinished : ScriptableObject
{
    private bool[] _regionsDefeated = new bool[4];
    public void LevelCompleted(int index)
    {
        _regionsDefeated[index] = true;
    }

    public bool IsLevelCompleted(int index)
    {
        return _regionsDefeated[index];
    }

    [ContextMenu("reset")]
    public void Reset()
    {
        _regionsDefeated = new bool[4];
    }
}
