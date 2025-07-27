using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class GridObjectMoverDebug : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.3f, 0.40f);
    [SerializeField] private Color selectColor = new Color(0.6f, 1f, 0.6f, 0.45f);

    public GridManager GridManager => _gridManager ??= GetComponent<GridManager>();
    private GridManager _gridManager;

    private Vector2Int? hoveredCell = null;
    private Vector2Int? selectedCell = null;

    // Вызвать при наведении мыши на новую клетку
    public void OnCellHoverEnter(Vector2Int cell)
    {
        if (hoveredCell != cell)
        {
            hoveredCell = cell;
            // Если нужно - добавить вашу логику реакции на вход
            // Debug.Log($"Hover enter {cell}");
        }
    }

    // Вызвать при выходе мыши из клетки (или когда перестали на неё указывать)
    public void OnCellHoverExit(Vector2Int cell)
    {
    }

    // Вызвать после клика по клетке
    public void HandleSelectCell(Vector2Int cell)
    {
        var grid = GridManager.Grid;
        if (grid == null) return;

        // Если уже выбрана стартовая клетка — пробуем переместить в целевую клетку
        if (selectedCell.HasValue)
        {
            var from = selectedCell.Value;
            var to = cell;

            if (from != to && grid.IsCellFreeOnLayer(to))
            {
                GridManager.MoveCellTo(from, to);
            }

            selectedCell = null;
        }
        else
        {
            // Если клик по клетке с объектом — выделяем её
            if (grid.TryGetGridObject(cell, out var obj))
            {
                selectedCell = cell;
            }
        }
    }

    private void OnDrawGizmos()
    {
        var grid = GridManager?.Grid;
        if (grid == null) return;

        var cellSize = grid.tilemap.cellSize;

        if (hoveredCell.HasValue)
        {
            Gizmos.color = highlightColor;
            DrawCellRect(grid, hoveredCell.Value, cellSize);
        }

        if (selectedCell.HasValue)
        {
            Gizmos.color = selectColor;
            DrawCellRect(grid, selectedCell.Value, cellSize);
        }
    }

    private void DrawCellRect(GridBehaviour grid, Vector2Int cell, Vector3 cellSize)
    {
        Vector3 pos = grid.GetCellCenterWorld(cell);
        Gizmos.DrawCube(pos, cellSize * 0.95f);
    }
}
