using UnityEngine;
using System.Collections;

public class LockController : MonoBehaviour
{
    public static LockController Instance { get; private set; }

    public LockModel Model;
    public LockHoleView HoleView;
    public MessageView MessageView;
    public FeedbackView FeedbackView;

    // Це теж потрібно зробити дочірнім до coordinateRoot, якщо воно малюється в тому ж просторі
    public GameObject UserFigurePrefab; 

    void Awake()
    {
        // Реалізація Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    void Start()
    {

        if (Model.coordinateRoot != null && HoleView.transform.parent != Model.coordinateRoot)
        {
            HoleView.transform.SetParent(Model.coordinateRoot);
            HoleView.transform.localPosition = Vector3.zero; 
            HoleView.transform.localRotation = Quaternion.identity;
            HoleView.transform.localScale = Vector3.one;
        }
        StartNewGame();
    }
    public void StartNewGame()
    {
        Model.ResetGame();
        SetupCurrentStage();
    }

    private void SetupCurrentStage()
    {
        LockStageData currentData = Model.GetCurrentStageData();

        if (currentData != null)
        {

            HoleView.DrawShape(currentData);
            MessageView.ShowMessage($"Stage {Model.CurrentStageIndex + 1}: Draw {currentData.ExpectedShape}", Color.black);
             }
        else
        {

            HoleView.Clear();
            MessageView.ShowMessage("All locks have been opened!", Color.green);
            Debug.Log("Game finished");
        }
    }

    public void SubmitUserFigure(LineRenderer userDrawnFigure)
    {

        if (Model.IsGameFinished())
        {
            Debug.Log("Game already finished");
            Destroy(userDrawnFigure.gameObject);
            return;
        }

        LockStageData currentData = Model.GetCurrentStageData();

        LineRenderer transformedUserFigure = userDrawnFigure; 

        if (transformedUserFigure.transform.parent != Model.coordinateRoot)
        {
            transformedUserFigure.transform.SetParent(Model.coordinateRoot);
            transformedUserFigure.transform.localPosition = Vector3.zero;
            transformedUserFigure.transform.localRotation = Quaternion.identity;
            transformedUserFigure.transform.localScale = Vector3.one;
        }

        bool isMatch = GeometryHelper.CheckMatch(transformedUserFigure, currentData);

        if (isMatch)
        {
            SoundManager.Instance.PlaySound2D("sublevel_passed");
            StartCoroutine(FeedbackView.AnimateSuccess(transformedUserFigure.gameObject, currentData.TargetCenter, currentData.TargetSize));
        }
        else
        {
            SoundManager.Instance.PlaySound2D("sublevel_failed");
            StartCoroutine(FeedbackView.AnimateFailure(transformedUserFigure.gameObject, "Shape doesn't fit!"));
        }

    }

    public void OnSuccessAnimationComplete()
    {
        Model.AdvanceToNextStage();
        SetupCurrentStage();
    }
}