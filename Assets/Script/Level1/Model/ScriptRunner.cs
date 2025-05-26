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
        LET FLOAT pi = 3.1415;
        LET INT steps = 36;
        LET FLOAT angleStep = 2.0 * pi / steps;

        FUNCTION DrawCircle(center, radius) :
            LET INT i = 0;
            LET FLOAT angle = 0.0;
            LET POINT prev = POINT(center.X + COS(0.0) * radius, center.Y + SIN(0.0) * radius);
            DRAWTO(prev);
   
            WHILE (i <= steps) :
                angle = i * angleStep;
                LET FLOAT x = center.X + COS(angle) * radius;
                LET FLOAT y = center.Y + SIN(angle) * radius;
                LET POINT next = POINT(x, y);
                DRAWTO(next);
                i = i + 1;
            ENDWHILE
        ENDFUNCTION

        FUNCTION DrawSquare(center, size) :
            LET FLOAT half = size / 2.0;
    
            // ������ ������ ���
            LET POINT p1 = POINT(center.X + half, center.Y + half);
            // ����� ������ ���
            LET POINT p2 = POINT(center.X + half, center.Y - half);
            // ����� ���� ���
            LET POINT p3 = POINT(center.X - half, center.Y - half);
            // ������ ���� ���
            LET POINT p4 = POINT(center.X - half, center.Y + half);
    
            DRAWTO(p1);
            DRAWTO(p2);
            DRAWTO(p3);
            DRAWTO(p4);
            DRAWTO(p1); // �������� �������
        ENDFUNCTION
        FUNCTION DrawPentagon(center, radius) :
            LET INT sides = 5;
            LET FLOAT angleStep = -2.0 * pi / sides;  // ����� �� ������������ �������
            LET INT i = 0;

            LET FLOAT angle = pi / 2;  // ��������� ���: ������� �����
            LET FLOAT x0 = center.X + COS(angle) * radius;
            LET FLOAT y0 = center.Y + SIN(angle) * radius;
            LET POINT first = POINT(x0, y0);
            LET POINT prev = first;
            DRAWTO(prev);

            i = 1;
            WHILE (i < sides) :
                angle = i * angleStep + pi / 2;  // ����� � ������ �����
                LET FLOAT x = center.X + COS(angle) * radius;
                LET FLOAT y = center.Y + SIN(angle) * radius;
                LET POINT next = POINT(x, y);
                DRAWTO(next);
                prev = next;
                i = i + 1;
            ENDWHILE

            DRAWTO(first); // �������� ����������
        ENDFUNCTION



        COLORRGB(0.0, 0.6, 1.0);  
        STARTDRAW;
        LET POINT center = POINT(0, 0);
        LET FLOAT radius = 15.0;
        DrawCircle(center, radius);
        //LET FLOAT radius = 15.0*SQRT(2);
        //DrawSquare(center, radius);
        //LET FLOAT radius = 15.0;
        //DrawPentagon(center, radius);





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