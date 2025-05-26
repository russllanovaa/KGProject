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
    
            // Верхній правий кут
            LET POINT p1 = POINT(center.X + half, center.Y + half);
            // Нижній правий кут
            LET POINT p2 = POINT(center.X + half, center.Y - half);
            // Нижній лівий кут
            LET POINT p3 = POINT(center.X - half, center.Y - half);
            // Верхній лівий кут
            LET POINT p4 = POINT(center.X - half, center.Y + half);
    
            DRAWTO(p1);
            DRAWTO(p2);
            DRAWTO(p3);
            DRAWTO(p4);
            DRAWTO(p1); // Замикаємо квадрат
        ENDFUNCTION
        FUNCTION DrawPentagon(center, radius) :
            LET INT sides = 5;
            LET FLOAT angleStep = -2.0 * pi / sides;  // обхід за годинниковою стрілкою
            LET INT i = 0;

            LET FLOAT angle = pi / 2;  // стартовий кут: вершина вгору
            LET FLOAT x0 = center.X + COS(angle) * radius;
            LET FLOAT y0 = center.Y + SIN(angle) * radius;
            LET POINT first = POINT(x0, y0);
            LET POINT prev = first;
            DRAWTO(prev);

            i = 1;
            WHILE (i < sides) :
                angle = i * angleStep + pi / 2;  // обхід зі зсувом угору
                LET FLOAT x = center.X + COS(angle) * radius;
                LET FLOAT y = center.Y + SIN(angle) * radius;
                LET POINT next = POINT(x, y);
                DRAWTO(next);
                prev = next;
                i = i + 1;
            ENDWHILE

            DRAWTO(first); // замикаємо п’ятикутник
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