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

    public Transform coordinateRoot; // ����� ���� ����������� �������

    public List<LockStageData> Stages = new List<LockStageData>();
    private int currentStageIndex = 0;

    void Awake()
    {
        // ���������� ���� 3 �����, ���� ������ �������
        if (Stages.Count == 0)
        {
            float circleRadius = 1.5f;
            Stages.Add(new LockStageData
            {
                ExpectedShape = LockShapeType.Circle,
                TargetCenter = Vector3.zero, // ����� ���� ���� � coordinateRoot
                TargetSize = circleRadius, // �����
                MatchTolerance = 0.3f
            });
            Stages.Add(new LockStageData
            {
                ExpectedShape = LockShapeType.Square,
                TargetCenter = Vector3.zero, // ����� �������� ���� � coordinateRoot
                TargetSize = circleRadius * Mathf.Sqrt(2), // ������� ��������� ��������
                MatchTolerance = 0.4f
            });
            Stages.Add(new LockStageData
            {
                ExpectedShape = LockShapeType.Pentagon,
                TargetCenter = Vector3.zero, // ����� �'���������� ���� � coordinateRoot
                TargetSize = circleRadius, // ����� ��������� ���� = ����� ������� ����
                MatchTolerance = 0.5f
            });
        }

        // �������� �������� coordinateRoot
        if (coordinateRoot == null)
        {

            // �������, ���������� �������� ��'��� �� ����� �� �������������, ���� �� ��������
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

            NextLevel.Instance.UnlockNewLevel();
        }
        else if (currentStageIndex == 3 && NextLevel.Instance == null)
        {
            Debug.LogError("NextLevel.Instance �� ��������. �������������, �� ��'��� NextLevel ���� � ���� �� ������������� �� �������.");
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