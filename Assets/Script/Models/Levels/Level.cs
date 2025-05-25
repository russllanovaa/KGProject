using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;

public class LevelData
{
    public int CurrentLevel { get; private set; } = 1;
    public int Progress { get; private set; } = 1;

    public void PassLevel()
    {
        Progress++;
    }

    public void SetCurrentLevel(int level)
    {
        CurrentLevel = level;
    }
}
