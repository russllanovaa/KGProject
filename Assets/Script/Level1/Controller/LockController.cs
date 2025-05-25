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

        if (Model == null) Debug.LogError("LockController: LockModel не призначено!");
        if (HoleView == null) Debug.LogError("LockController: LockHoleView не призначено!");
        if (MessageView == null) Debug.LogError("LockController: MessageView не призначено!");
        if (FeedbackView == null) Debug.LogError("LockController: FeedbackView не призначено!");
        if (UserFigurePrefab == null) Debug.LogError("LockController: UserFigurePrefab не призначено! Будь ласка, призначте префаб з LineRenderer.");
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
            MessageView.ShowMessage($"Етап {Model.CurrentStageIndex + 1}: Намалюйте {currentData.ExpectedShape}", Color.white);
            Debug.Log($"Етап {Model.CurrentStageIndex + 1}: Очікується {currentData.ExpectedShape}, Розмір={currentData.TargetSize}, Центр={currentData.TargetCenter}");
        }
        else
        {

            HoleView.Clear();
            MessageView.ShowMessage("✅ Усі замки відкрито!", Color.green);
            Debug.Log("Гра закінчена!");
        }
    }

    public void SubmitUserFigure(LineRenderer userDrawnFigure)
    {
        if (Model.IsGameFinished())
        {
            Debug.Log("Гра вже закінчена, спробу не оброблено.");
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
            StartCoroutine(FeedbackView.AnimateSuccess(transformedUserFigure.gameObject, currentData.TargetCenter, currentData.TargetSize));
        }
        else
        {
            StartCoroutine(FeedbackView.AnimateFailure(transformedUserFigure.gameObject, "Фігура не підійшла!"));
        }

    }

    public void OnSuccessAnimationComplete()
    {
        Model.AdvanceToNextStage();
        SetupCurrentStage();
    }
}