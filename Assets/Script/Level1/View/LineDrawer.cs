using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    private List<Vector3> points = new List<Vector3>();
    private LineRenderer lineRenderer;

    public Color color = Color.black;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.positionCount = 0;
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader == null)
        {
            Debug.LogError("Sprites/Default shader not found!");
        }
        else
        {
            lineRenderer.material = new Material(spriteShader);
        }

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.numCapVertices = 10;
        lineRenderer.numCornerVertices = 10;
        lineRenderer.sortingOrder = 15; // Вище за LockHoleView
    }

    public void Clear()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
    }

    public void AddLine(Vector3 start, Vector3 end)
    {
        points.Add(start);
        points.Add(end);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
    public LineRenderer GetLineRenderer()
    {
        return lineRenderer;
    }

}

