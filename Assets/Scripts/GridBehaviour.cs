using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GridBehaviour : MonoBehaviour
{
    public int X = 5;
    public int Y = 5;
    public float cellSize = 1f;

    public Dictionary<Vector2Int, Cell> Cells => _cells;
    private Dictionary<Vector2Int, Cell> _cells = new Dictionary<Vector2Int, Cell>();

    public Cell GetCell(Vector2Int point)
    {
        if (point.x < 0 || point.x >= X || point.y < 0 || point.y >= Y)
            throw new System.ArgumentOutOfRangeException("point", $"Point {point} is out of grid bounds");

        if (!_cells.TryGetValue(point, out var result))
        {
            result = new Cell();
            _cells[point] = result;
        }
        return result;
    }

    public Vector2 GetCellWorldPosition(Vector2Int point)
    {
        // Проверим что точка внутри сетки (если нужно, иначе уберите)
        if (point.x < 0 || point.x >= X || point.y < 0 || point.y >= Y)
            throw new System.ArgumentOutOfRangeException(nameof(point), $"Point {point} is out of grid bounds.");

        // Получим позицию центра клетки в локальных координатах
        Vector2 localPos = new Vector2(
            (point.x + 0.5f) * cellSize,
            (point.y + 0.5f) * cellSize
        );

        // Если нужно мировое положение — прибавим позицию объекта, иначе верните localPos
        return (Vector2)transform.position + localPos;
    }


    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z));
                                                                                                      

            // Переводим world позицию мыши в локальные координаты относительно начала сетки
            Vector2 local = (Vector2)mouseWorldPos - (Vector2)transform.position;

            // Теперь определяем индекс клетки в сетке
            int cellX = Mathf.FloorToInt(local.x / cellSize);
            int cellY = Mathf.FloorToInt(local.y / cellSize);

            Vector2Int cellIndex = new Vector2Int(cellX, cellY);

            // Проверяем попали ли мы в сетку
            if (cellX >= 0 && cellX < X && cellY >= 0 && cellY < Y)
            {
                Cell cell = GetCell(cellIndex);
                Debug.Log($"Выбрана клетка {cellIndex}");
                // Можно визуализировать выбор или что-то сделать с cell
            }
            else
            {
                Debug.Log("Клик вне сетки");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int x = 0; x <= X; x++)
        {
            // Вертикальные линии
            Vector2 start = (Vector2)transform.position + new Vector2(x * cellSize, 0);
            Vector2 end = (Vector2)transform.position + new Vector2(x * cellSize, Y * cellSize);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= Y; y++)
        {
            // Горизонтальные линии
            Vector2 start = (Vector2)transform.position + new Vector2(0, y * cellSize);
            Vector2 end = (Vector2)transform.position + new Vector2(X * cellSize, y * cellSize);
            Gizmos.DrawLine(start, end);
        }
    }
}
