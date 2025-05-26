using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class LockHoleView : MonoBehaviour
{

    private LineRenderer lr;
    private const int SEGMENTS = 60; // Сегменти для кола

    void Awake()
    {

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.loop = true;
        lr.widthMultiplier = 0.01f; // Ціль трохи товща
        // Використовуйте матеріал, який добре видно, наприклад, Unlit/Color
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;
        lr.useWorldSpace = false; // Малюємо відносно об'єкта
        lr.sortingOrder = 10; // Вище за Canvas (який має 0)
          }
    private void Start()
    {
         }

    public void DrawShape(LockStageData data)
    {
        if (data == null)
        {
            Clear();
            return;
        }
        transform.localPosition = data.TargetCenter; // Має бути Vector3.zero
       

        switch (data.ExpectedShape)
        {
            case LockShapeType.Circle:
                DrawCircleInternal(data.TargetSize);
                break;
            case LockShapeType.Square:
                DrawSquareInternal(data.TargetSize);
                break;
            case LockShapeType.Pentagon:
                DrawPentagonInternal(data.TargetSize);
                break;
        }
        gameObject.SetActive(true);
    }

    private void DrawCircleInternal(float radius)
    {
        lr.positionCount = SEGMENTS + 1;
        for (int i = 0; i <= SEGMENTS; i++)
        {
            float angle = 2 * Mathf.PI * i / SEGMENTS;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            lr.SetPosition(i, pos);
        }
    }

    private void DrawSquareInternal(float size)
    {
        lr.positionCount = 5;
        float half = size / 2f;
        lr.SetPosition(0, new Vector3(-half, -half, 0));
        lr.SetPosition(1, new Vector3(-half, half, 0));
        lr.SetPosition(2, new Vector3(half, half, 0));
        lr.SetPosition(3, new Vector3(half, -half, 0));
        lr.SetPosition(4, new Vector3(-half, -half, 0));
    }

    private void DrawPentagonInternal(float radius)
    {
        lr.positionCount = 6;
        for (int i = 0; i < 6; i++)
        {
            // Зміна напрямку: від 5 до 0 — за годинниковою стрілкою
            float angle = -2 * Mathf.PI * i / 5 + Mathf.PI / 2; // +90° — чітко вгору
            Vector3 point = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            lr.SetPosition(i, point);
        }
    }

    public void Clear()
    {
        lr.positionCount = 0;
        gameObject.SetActive(false);
    }
}

