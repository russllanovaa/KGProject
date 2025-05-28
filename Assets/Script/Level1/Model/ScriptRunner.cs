using UnityEngine;
using UnityEngine.UI;

public class ScriptRunner : MonoBehaviour
{
    [Tooltip("Перетягніть сюди ваш Prefab LineDrawer з вікна Project")]
    public GameObject lineDrawerPrefab;

    [Tooltip("Перетягніть сюди ваш Input Field для введення коду")]
    public InputField codeInputField;

    [Tooltip("Перетягніть сюди вашу кнопку виконання")]
    public Button executeButton;
    public Transform coordinateRootTransform;
    void Start()
    {
        // Підписуємось на подію натискання кнопки
        executeButton.onClick.AddListener(ExecuteUserCode);

        // Можна додати приклад коду за замовчуванням
        codeInputField.text = @"
        
















































































        ";
    }

    public void ExecuteUserCode()
    {
        // Очищаємо попередні малюнки
        ClearPreviousDrawings();

        // Отримуємо код з Input Field
        string userScript = codeInputField.text;

        if (string.IsNullOrEmpty(userScript))
        {
            Debug.Log("There's no code");
            return;
        }

        Debug.Log("--- Using your code ---");
        Debug.Log(userScript);

        try
        {
            UnityCanvasDrawingEngine engine = new UnityCanvasDrawingEngine(lineDrawerPrefab, coordinateRootTransform);
            GraphicsLangRunner.RunScript(userScript, engine);

            LineDrawer lastDrawer = engine.GetLastActiveLineDrawer();

            if (lastDrawer != null && LockController.Instance != null)
            {
                LineRenderer lr = lastDrawer.GetLineRenderer();
                if (lr != null && lr.positionCount > 0)
                {

                    LockController.Instance.SubmitUserFigure(lr);
                }
                else
                {

                    if (lr != null) Destroy(lr.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("Nothing was drawn");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error code: {e.Message}");
        }
    }

    private void ClearPreviousDrawings()
    {
        // Знаходимо і видаляємо всі об'єкти LineDrawer
        LineDrawer[] drawers = FindObjectsOfType<LineDrawer>();
        foreach (LineDrawer drawer in drawers)
        {
            Destroy(drawer.gameObject);
        }
    }
}