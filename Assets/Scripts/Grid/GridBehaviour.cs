using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using System.Linq;
using System;

public class GridBehaviour : MonoBehaviour
{
    public Tilemap tilemap;

    public Vector2 MousePosition;
    public Vector2Int MousePositionOnGrid;

    [Header("Tilemap Mouse Events")]
    public UnityEvent<Vector2Int> OnCellMouseEnter;
    public UnityEvent<Vector2Int> OnCellMouseExit;
    public UnityEvent<Vector2Int> OnCellMouseDown;

    public Dictionary<Vector2Int, Cell> Cells => _cells;
    private Dictionary<Vector2Int, Cell> _cells = new Dictionary<Vector2Int, Cell>();

    public IEnumerable<int> Layers => gridObjectMap.Keys.Select(item => item.Item2);

    private Dictionary<(Vector2Int, int), GridObjectBehaviour> gridObjectMap;

    private UniTaskCoroutine _mouseMonitoringCorutine;

    public void MoveGridObjectToCell(GridObjectBehaviour obj, Vector2Int targetCell)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        int layer = obj.Layer;
        Vector2Int oldCell = obj.GridPosition; // Предполагаем, что у объекта есть инфа о текущей клетке

        // Удалить из старой позиции
        gridObjectMap.Remove((oldCell, layer));

        // Добавить в новую позицию
        gridObjectMap[(targetCell, layer)] = obj;

        // Обновить данные внутри объекта (если нужно)
        obj.GridPosition = targetCell;
    }

    public IEnumerable<GridObjectBehaviour> GetGridObjectsFromChildren()
    {
        // Найти прямого ребенка "Objects"
        var objectsChild = transform.Find("Objects");
        if (objectsChild == null)
        {
            Debug.LogWarning("Child with name Objects not found");
            yield break;
        }

        // Вернуть все GridObjectBehaviour среди всех потомков "Objects"
        foreach (var gridObj in objectsChild.GetComponentsInChildren<GridObjectBehaviour>(true))
        {
            yield return gridObj;
        }
    }

    public Dictionary<(Vector2Int, int), GridObjectBehaviour> CreateGridObjectMap()
    {
        var dict = new Dictionary<(Vector2Int, int), GridObjectBehaviour>();

        foreach (var gridObj in GetGridObjectsFromChildren())
        {
            var key = (gridObj.GridPosition, gridObj.Layer);
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, gridObj);
            }
            else
            {
                //Debug.LogError($"Grid allready exist point: {gridObj.GridPosition} on layer: {gridObj.Layer}. Object: {gridObj.name}");
            }
        }

        return dict;
    }

    public bool TryGetGridObject(Vector2Int cell, out GridObjectBehaviour obj, int layer = 0)
    {
        if (gridObjectMap.TryGetValue((cell, layer), out var value))
        {
            obj = value;
            return true;
        }
        obj = null;
        return false;
    }

    public bool HasGridObject(Vector2Int gridPos, int layer = 0)
    {
        return gridObjectMap.ContainsKey((gridPos, layer));
    }

    public bool IsCellFreeOnLayer(Vector2Int cell, int layer = 0)
    {
        return !gridObjectMap.ContainsKey((cell, layer));
    }

    public Vector2 GetCellCenterWorld(Vector2Int cell)
    {
        return tilemap.GetCellCenterWorld((Vector3Int)cell);
    }

    public Vector2Int WorldToCell(Vector2 pos)
    {
        return (Vector2Int)tilemap.WorldToCell(pos);
    }

    private bool IsCellInsideTilemap(Vector2Int cell)
    {
        // tilemap.cellBounds - это BoundsInt, который можно безопасно использовать для проверки.
        return tilemap.cellBounds.Contains((Vector3Int)cell);
    }

    // TODO: удалить в будущих итерациях и перенести в другое место.
    public async UniTask MouseMonitoringTask(CancellationToken token)
    {
        Vector2Int? prevCell = null;

        while (!token.IsCancellationRequested)
        {
            // Координаты мыши
            var mouseScreen = Mouse.current.position.ReadValue();
            float z = -Camera.main.transform.position.z;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, z));
            Vector2Int curCell = (Vector2Int)tilemap.WorldToCell(mouseWorld);

            bool insideTilemap = IsCellInsideTilemap(curCell);

            // Обработка событий hover/exit/enter
            if (insideTilemap)
            {
                if (prevCell == null)
                {
                    OnCellMouseEnter?.Invoke(curCell);
                }
                else if (prevCell != curCell)
                {
                    OnCellMouseExit?.Invoke(prevCell.Value);
                    OnCellMouseEnter?.Invoke(curCell);
                }
                prevCell = curCell;
            }
            else if (prevCell != null)
            {
                OnCellMouseExit?.Invoke(prevCell.Value);
                prevCell = null;
            }

            // Обработка клика
            if (insideTilemap && Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnCellMouseDown?.Invoke(curCell);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        // Финальное событие выхода
        if (prevCell != null)
        {
            OnCellMouseExit?.Invoke(prevCell.Value);
        }
    }

    private void Awake()
    {
        _mouseMonitoringCorutine = new UniTaskCoroutine(MouseMonitoringTask);
        gridObjectMap = CreateGridObjectMap();
    }

    private void OnEnable()
    {
        _mouseMonitoringCorutine.Run();
    }

    private void OnDisable()
    {
        _mouseMonitoringCorutine.Stop();
    }

    private void OnDestroy()
    {
        _mouseMonitoringCorutine.Stop();
        _mouseMonitoringCorutine.Dispose();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(tilemap.GetCellCenterWorld((Vector3Int)MousePositionOnGrid), 0.5f);
    }


}
