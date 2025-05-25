using UnityEngine;
using System.Collections.Generic;

public enum LockShapeType
{
    Circle,
    Square,
    Pentagon
}

[System.Serializable]
public class LockStageData
{
    public LockShapeType ExpectedShape;
    public Vector3 TargetCenter = new Vector3(0, 0, 0);
    public float TargetSize;
    public float MatchTolerance = 0.5f;
}

public class LockModel : MonoBehaviour
{
    public LevelView2 view;

    public Transform coordinateRoot; // Центр нової координатної площини

    public List<LockStageData> Stages = new List<LockStageData>();
    private int currentStageIndex = 0;

    void Awake()
    {
        // Ініціалізуємо наші 3 етапи, якщо список порожній
        if (Stages.Count == 0)
        {
            float circleRadius = 1.5f;
            Stages.Add(new LockStageData
            {
                ExpectedShape = LockShapeType.Circle,
                TargetCenter = Vector3.zero, // Центр кола буде в coordinateRoot
                TargetSize = circleRadius, // Радіус
                MatchTolerance = 0.3f
            });
            Stages.Add(new LockStageData
            {
                ExpectedShape = LockShapeType.Square,
                TargetCenter = Vector3.zero, // Центр квадрата буде в coordinateRoot
                TargetSize = circleRadius * Mathf.Sqrt(2), // Сторона вписаного квадрата
                MatchTolerance = 0.4f
            });
            Stages.Add(new LockStageData
            {
                ExpectedShape = LockShapeType.Pentagon,
                TargetCenter = Vector3.zero, // Центр п'ятикутника буде в coordinateRoot
                TargetSize = circleRadius, // Радіус описаного кола = радіус першого кола
                MatchTolerance = 0.5f
            });
        }

        // Перевірка наявності coordinateRoot
        if (coordinateRoot == null)
        {
            Debug.LogError("LockModel: coordinateRoot не призначено! Будь ласка, призначте об'єкт у інспекторі.");
            // Можливо, призначити поточний об'єкт як корінь за замовчуванням, якщо це доцільно
            coordinateRoot = this.transform;
        }
    }

    public int CurrentStageIndex => currentStageIndex;

    public LockStageData GetCurrentStageData()
    {
        if (currentStageIndex >= 0 && currentStageIndex < Stages.Count)
        {
            return Stages[currentStageIndex];
        }
        return null;
    }

    public bool AdvanceToNextStage()
    {
        currentStageIndex++;
        view.SetLevelBackground(currentStageIndex);
        LevelManager.Instance.OnClickToPass();
        if (currentStageIndex == 3 && NextLevel.Instance != null)
        {
            Debug.Log("Досягнуто етапу 3, викликається UnlockNewLevel.");
            NextLevel.Instance.UnlockNewLevel();
        }
        else if (currentStageIndex == 3 && NextLevel.Instance == null)
        {
            Debug.LogError("NextLevel.Instance не знайдено. Переконайтеся, що об'єкт NextLevel існує в сцені та ініціалізований як сінглтон.");
        }

        return currentStageIndex < Stages.Count;
    }

    public bool IsGameFinished()
    {
        return currentStageIndex >= Stages.Count;
    }

    public void ResetGame()
    {
        currentStageIndex = 0;
    }
}