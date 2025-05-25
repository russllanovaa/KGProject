using UnityEngine;

public class UnityCanvasDrawingEngine : IDrawingEngine
{
    private PointData? _currentPoint = null;
    private MatrixData _currentTransform = MatrixData.Identity;
    private Color _currentColor = Color.black;

    private GameObject _lineDrawerPrefab; 
    private Transform _lineDrawersParent;  

    private LineDrawer _currentActiveLineDrawer;

    private float _scaleFactor = 0.1f;
    private MatrixData _baseScaleMatrix;
    private Vector3 _offset = new Vector3(0, 0, 0);

    public UnityCanvasDrawingEngine(GameObject lineDrawerPrefab, Transform parentForLineDrawers = null)
    {
        if (lineDrawerPrefab == null)
        {
            Debug.LogError("LineDrawer prefab не може бути null! Будь ласка, призначте Prefab.");
            return;
        }
        _lineDrawerPrefab = lineDrawerPrefab;
        _lineDrawersParent = parentForLineDrawers;

        _baseScaleMatrix = new MatrixData(
            _scaleFactor, 0,
            0, _scaleFactor,
            0, 0// _offset.x, _offset.y
        );
        _currentTransform = _baseScaleMatrix; 

    }

    public void SetColorRGB(float r, float g, float b)
    {
        _currentColor = new Color(r, g, b);

    }

    public void SetColorCMY(float c, float m, float y)
    {
        float red = 1 - c;
        float green = 1 - m;
        float blue = 1 - y;
        SetColorRGB(red, green, blue);
    }

    public void SetColorHSV(float h, float s, float v)
    {
        _currentColor = Color.HSVToRGB(h, s, v);
    }

    public void SetColorLAB(float l, float a, float b)
    {
        Debug.LogWarning("LAB to RGB не реалізовано. Використовується сірий.");
        _currentColor = Color.gray;
    }

    public void StartDraw()
    {

        if (_lineDrawerPrefab == null)
        {
            Debug.LogError("LineDrawer prefab не призначено. Неможливо створити новий LineDrawer.");
            return;
        }
        GameObject newLineDrawerGO = Object.Instantiate(_lineDrawerPrefab);

        if (_lineDrawersParent != null)
        {
            newLineDrawerGO.transform.SetParent(_lineDrawersParent);
            newLineDrawerGO.transform.localPosition = Vector3.zero;
            newLineDrawerGO.transform.localRotation = Quaternion.identity;
            newLineDrawerGO.transform.localScale = Vector3.one;
        }

        newLineDrawerGO.name = $"LinePath_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        _currentActiveLineDrawer = newLineDrawerGO.GetComponent<LineDrawer>();
        if (_currentActiveLineDrawer == null)
        {
            Debug.LogError("Створений LineDrawer prefab не містить компонента LineDrawer!");
            Object.Destroy(newLineDrawerGO); 
            return;
        }

        _currentActiveLineDrawer.color = _currentColor;

        _currentPoint = null;
    }

    public void DrawTo(PointData newPoint)
    {
        if (_currentActiveLineDrawer == null)
        {
            Debug.Log("DrawTo викликано перед StartDraw або після помилки в StartDraw. Немає активного LineDrawer.");
            return;
        }
        PointData transformedNewPoint = ApplyCurrentTransform(newPoint);
        Vector3 newLocalPoint = new Vector3(transformedNewPoint.X, transformedNewPoint.Y, 0);

        if (_currentPoint.HasValue)
        {
            PointData transformedCurrent = ApplyCurrentTransform(_currentPoint.Value);
            Vector3 startLocalPoint = new Vector3(transformedCurrent.X, transformedCurrent.Y, 0);
            _currentActiveLineDrawer.AddLine(startLocalPoint, newLocalPoint);
        }
        _currentPoint = newPoint;
    }

    public void SetCurrentTransformMatrix(MatrixData matrix)
    {

        _currentTransform = matrix;
    }

    public PointData ApplyCurrentTransform(PointData point)
    {

        return _currentTransform.Transform(point);
    }
    public LineDrawer GetLastActiveLineDrawer()
    {
        return _currentActiveLineDrawer;
    }

}