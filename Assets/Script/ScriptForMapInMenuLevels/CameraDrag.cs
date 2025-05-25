using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class CameraDrag : MonoBehaviour
{
    private Camera _mainCamera;
    private Vector3 _origin;
    private Vector3 _difference;
    private bool _isDragging;

    [SerializeField] private Tilemap groundTilemap;

    private float minY, maxY;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (groundTilemap == null)
        {
            Debug.LogError("Ground tilemap не призначено.");
            return;
        }

        // Оновлюємо межі, стискаючи Tilemap
        groundTilemap.CompressBounds();

        // Отримаємо світові межі тайлмапа
        BoundsInt tilemapBounds = groundTilemap.cellBounds;
        Vector3 min = groundTilemap.CellToWorld(new Vector3Int(tilemapBounds.xMin, tilemapBounds.yMin, 0));
        Vector3 max = groundTilemap.CellToWorld(new Vector3Int(tilemapBounds.xMax, tilemapBounds.yMax, 0));

        float camHalfHeight = _mainCamera.orthographicSize;

        // Ураховуємо половину висоти камери, щоб не було пустоти
        minY = min.y + camHalfHeight;
        maxY = max.y - camHalfHeight;
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _origin = GetMouseWorldPosition;
        _isDragging = ctx.started || ctx.performed;
    }

    private void LateUpdate()
    {
        if (!_isDragging || groundTilemap == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        _difference = GetMouseWorldPosition - transform.position;
        Vector3 newPos = _origin - _difference;

        // Обмежуємо тільки по Y
        float clampedY = Mathf.Clamp(newPos.y, minY, maxY);

        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }

    private Vector3 GetMouseWorldPosition
    {
        get
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            return _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_mainCamera.transform.position.z));
        }
    }
}
