using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GeometryHelper
{
    private const float POINT_MERGE_DISTANCE_SQR = 0.1f * 0.1f; // Відстань для злиття точок

    /// <summary>
    /// Отримує "чистий" список точок з LineRenderer, видаляючи дублікати.
    /// </summary>
    private static List<Vector3> GetCleanPoints(LineRenderer lr)
    {
        List<Vector3> cleanPoints = new List<Vector3>();
        if (lr.positionCount == 0) return cleanPoints;

        Vector3[] rawPoints = new Vector3[lr.positionCount];
        lr.GetPositions(rawPoints);

        // Додаємо першу точку
        cleanPoints.Add(rawPoints[0]);

        // Додаємо наступні, якщо вони не надто близькі до попередньої
        for (int i = 1; i < rawPoints.Length; i++)
        {
            if ((rawPoints[i] - cleanPoints.Last()).sqrMagnitude > POINT_MERGE_DISTANCE_SQR)
            {
                cleanPoints.Add(rawPoints[i]);
            }
        }

        // Перевіряємо, чи остання точка близька до першої (замкнена фігура)
        if (cleanPoints.Count > 1 && (cleanPoints.First() - cleanPoints.Last()).sqrMagnitude < POINT_MERGE_DISTANCE_SQR)
        {
            cleanPoints.RemoveAt(cleanPoints.Count - 1); // Видаляємо дублікат останньої
        }
        Debug.Log($"Cleaned points: {cleanPoints.Count}");
        return cleanPoints;
    }

    /// <summary>
    /// Обчислює центроїд списку точок.
    /// </summary>
    private static Vector3 CalculateCentroid(List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;
        if (points.Count == 0) return centroid;
        foreach (var p in points) centroid += p;
        return centroid / points.Count;
    }

    /// <summary>
    /// Перевіряє, чи фігура схожа на коло.
    /// </summary>
    private static bool CheckCircle(List<Vector3> points, float tolerance, out Vector3 center, out float radius)
    {
        center = CalculateCentroid(points);
        radius = 0f;
        if (points.Count < 10) return false; // Замало точок для кола

        float avgRadius = 0f;
        foreach (var p in points) avgRadius += Vector3.Distance(p, center);
        avgRadius /= points.Count;

        // Перевіряємо, чи всі точки лежать приблизно на цій відстані
        foreach (var p in points)
        {
            if (Mathf.Abs(Vector3.Distance(p, center) - avgRadius) > tolerance * 2) // Допуск для кола
            {
                return false;
            }
        }
        radius = avgRadius;
        return true;
    }

    /// <summary>
    /// Перевіряє, чи фігура схожа на квадрат (дуже спрощено).
    /// </summary>
    private static bool CheckSquare(List<Vector3> points, float tolerance, out Vector3 center, out float size)
    {
        center = CalculateCentroid(points);
        size = 0f;
        if (points.Count < 4 || points.Count > 5) return false; // Очікуємо 4 кути

        // Знаходимо AABB (рамку)
        float minX = points.Min(p => p.x); float maxX = points.Max(p => p.x);
        float minY = points.Min(p => p.y); float maxY = points.Max(p => p.y);

        float width = maxX - minX;
        float height = maxY - minY;

        // Перевіряємо, чи ширина ~ висоті
        if (Mathf.Abs(width - height) > tolerance * 2) return false;
        // Перевіряємо, чи центр ~ AABB центру
        if (Vector3.Distance(center, new Vector3((minX + maxX) / 2, (minY + maxY) / 2, center.z)) > tolerance) return false;

        size = (width + height) / 2; // Середній розмір
        return true;
    }

    /// <summary>
    /// Перевіряє, чи фігура схожа на п'ятикутник (дуже спрощено).
    /// </summary>
    private static bool CheckPentagon(List<Vector3> points, float tolerance, out Vector3 center, out float radius)
    {
        center = CalculateCentroid(points);
        radius = 0f;
        if (points.Count < 5 || points.Count > 6) return false; // Очікуємо 5 кутів

        // Як і для кола, перевіряємо середній радіус, але з більшим допуском,
        // бо кути можуть бути не ідеальними.
        float avgRadius = 0f;
        foreach (var p in points) avgRadius += Vector3.Distance(p, center);
        avgRadius /= points.Count;

        // Тут потрібна складніша перевірка на 5 кутів, але для простоти
        // обмежимось перевіркою середнього радіусу.
        foreach (var p in points)
        {
            if (Mathf.Abs(Vector3.Distance(p, center) - avgRadius) > tolerance * 3)
            {
                return false;
            }
        }
        radius = avgRadius;
        return true;
    }


    /// <summary>
    /// Перевіряє, чи відповідає фігура користувача цільовим даним.
    /// </summary>
    public static bool CheckMatch(LineRenderer userFigure, LockStageData targetData)
    {
        if (userFigure == null || targetData == null) return false;

        List<Vector3> points = GetCleanPoints(userFigure);
        if (points.Count < 3) return false; // Замало точок для будь-якої фігури

        Vector3 drawnCenter = Vector3.zero;
        float drawnSize = 0f;
        bool shapeMatch = false;

        switch (targetData.ExpectedShape)
        {
            case LockShapeType.Circle:
                shapeMatch = CheckCircle(points, targetData.MatchTolerance, out drawnCenter, out drawnSize);
                break;
            case LockShapeType.Square:
                shapeMatch = CheckSquare(points, targetData.MatchTolerance, out drawnCenter, out drawnSize);
                break;
            case LockShapeType.Pentagon:
                shapeMatch = CheckPentagon(points, targetData.MatchTolerance, out drawnCenter, out drawnSize);
                break;
        }

        if (!shapeMatch)
        {
            Debug.Log($"Невдача: Форма не схожа на {targetData.ExpectedShape}.");
            return false;
        }

        // Перевірка розміру
        if (Mathf.Abs(drawnSize - targetData.TargetSize) > targetData.MatchTolerance)
        {
            Debug.Log($"Невдача: Розмір {drawnSize}, очікувався ~{targetData.TargetSize}.");
            return false;
        }

        // Перевірка центру
        if (Vector3.Distance(drawnCenter, targetData.TargetCenter) > targetData.MatchTolerance)
        {
            Debug.Log($"Невдача: Центр {drawnCenter}, очікувався ~{targetData.TargetCenter}.");
            return false;
        }

        Debug.Log("Успіх! Фігура підійшла.");
        return true;
    }
}