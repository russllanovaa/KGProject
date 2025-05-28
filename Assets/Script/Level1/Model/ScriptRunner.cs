using UnityEngine;
using UnityEngine.UI;

public class ScriptRunner : MonoBehaviour
{
    [Tooltip("���������� ���� ��� Prefab LineDrawer � ���� Project")]
    public GameObject lineDrawerPrefab;

    [Tooltip("���������� ���� ��� Input Field ��� �������� ����")]
    public InputField codeInputField;

    [Tooltip("���������� ���� ���� ������ ���������")]
    public Button executeButton;
    public Transform coordinateRootTransform;
    void Start()
    {
        // ϳ��������� �� ���� ���������� ������
        executeButton.onClick.AddListener(ExecuteUserCode);

        // ����� ������ ������� ���� �� �������������
        codeInputField.text = @"
        
















































































        ";
    }

    public void ExecuteUserCode()
    {
        // ������� �������� �������
        ClearPreviousDrawings();

        // �������� ��� � Input Field
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
        // ��������� � ��������� �� ��'���� LineDrawer
        LineDrawer[] drawers = FindObjectsOfType<LineDrawer>();
        foreach (LineDrawer drawer in drawers)
        {
            Destroy(drawer.gameObject);
        }
    }
}