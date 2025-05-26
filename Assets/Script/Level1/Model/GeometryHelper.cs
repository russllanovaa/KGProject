using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GeometryHelper
{
    private const float POINT_MERGE_DISTANCE_SQR = 0.1f * 0.1f; // ³������ ��� ������ �����

    /// <summary>
    /// ������ "������" ������ ����� � LineRenderer, ��������� ��������.
    /// </summary>
    private static List<Vector3> GetCleanPoints(LineRenderer lr)
    {
        List<Vector3> cleanPoints = new List<Vector3>();
        if (lr.positionCount == 0) return cleanPoints;

        Vector3[] rawPoints = new Vector3[lr.positionCount];
        lr.GetPositions(rawPoints);

        // ������ ����� �����
        cleanPoints.Add(rawPoints[0]);

        // ������ �������, ���� ���� �� ����� ������ �� ����������
        for (int i = 1; i < rawPoints.Length; i++)
        {
            if ((rawPoints[i] - cleanPoints.Last()).sqrMagnitude > POINT_MERGE_DISTANCE_SQR)
            {
                cleanPoints.Add(rawPoints[i]);
            }
        }

        // ����������, �� ������� ����� ������� �� ����� (�������� ������)
        if (cleanPoints.Count > 1 && (cleanPoints.First() - cleanPoints.Last()).sqrMagnitude < POINT_MERGE_DISTANCE_SQR)
        {
            cleanPoints.RemoveAt(cleanPoints.Count - 1); // ��������� ������� ��������
        }

        return cleanPoints;
    }

    /// <summary>
    /// �������� ������� ������ �����.
    /// </summary>
    private static Vector3 CalculateCentroid(List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;
        if (points.Count == 0) return centroid;
        foreach (var p in points) centroid += p;
        return centroid / points.Count;
    }

    /// <summary>
    /// ��������, �� ������ ����� �� ����.
    /// </summary>
    private static bool CheckCircle(List<Vector3> points, float tolerance, out Vector3 center, out float radius)
    {
        center = CalculateCentroid(points);
        radius = 0f;
        if (points.Count < 10) return false; // ������ ����� ��� ����

        float avgRadius = 0f;
        foreach (var p in points) avgRadius += Vector3.Distance(p, center);
        avgRadius /= points.Count;

        // ����������, �� �� ����� ������ ��������� �� ��� ������
        foreach (var p in points)
        {
            if (Mathf.Abs(Vector3.Distance(p, center) - avgRadius) > tolerance * 2) // ������ ��� ����
            {
                return false;
            }
        }
        radius = avgRadius;
        return true;
    }

    /// <summary>
    /// ��������, �� ������ ����� �� ������� (���� ��������).
    /// </summary>
    private static bool CheckSquare(List<Vector3> points, float tolerance, out Vector3 center, out float size)
    {
        center = CalculateCentroid(points);
        size = 0f;
        if (points.Count < 4 || points.Count > 5) return false; // ������� 4 ����

        // ��������� AABB (�����)
        float minX = points.Min(p => p.x); float maxX = points.Max(p => p.x);
        float minY = points.Min(p => p.y); float maxY = points.Max(p => p.y);

        float width = maxX - minX;
        float height = maxY - minY;

        // ����������, �� ������ ~ �����
        if (Mathf.Abs(width - height) > tolerance * 2) return false;
        // ����������, �� ����� ~ AABB ������
        if (Vector3.Distance(center, new Vector3((minX + maxX) / 2, (minY + maxY) / 2, center.z)) > tolerance) return false;

        size = (width + height) / 2; // ������� �����
        return true;
    }

    /// <summary>
    /// ��������, �� ������ ����� �� �'��������� (���� ��������).
    /// </summary>
    private static bool CheckPentagon(List<Vector3> points, float tolerance, out Vector3 center, out float radius)
    {
        center = CalculateCentroid(points);
        radius = 0f;
        if (points.Count < 5 || points.Count > 6) return false; // ������� 5 ����

        // �� � ��� ����, ���������� ������� �����, ��� � ������ ��������,
        // �� ���� ������ ���� �� ����������.
        float avgRadius = 0f;
        foreach (var p in points) avgRadius += Vector3.Distance(p, center);
        avgRadius /= points.Count;

        // ��� ������� �������� �������� �� 5 ����, ��� ��� ��������
        // ���������� ��������� ���������� ������.
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
    /// ��������, �� ������� ������ ����������� �������� �����.
    /// </summary>
    public static bool CheckMatch(LineRenderer userFigure, LockStageData targetData)
    {
        if (userFigure == null || targetData == null) return false;

        List<Vector3> points = GetCleanPoints(userFigure);
        if (points.Count < 3) return false; // ������ ����� ��� ����-��� ������

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
            Debug.Log($"Failure: Shape doesn't resemble{targetData.ExpectedShape}.");
            return false;
        }

        // �������� ������
        if (Mathf.Abs(drawnSize - targetData.TargetSize) > targetData.MatchTolerance)
        {
            Debug.Log($"Failure: Wrong size.");
            return false;
        }

        // �������� ������
        if (Vector3.Distance(drawnCenter, targetData.TargetCenter) > targetData.MatchTolerance)
        {
            Debug.Log($"Failure: Wrong center");
            return false;
        }

        Debug.Log("Success");
        return true;
    }
}