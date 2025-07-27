using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GridBehaviour))]
public class GridManager : MonoBehaviour
{
    public GridBehaviour Grid => gridBehaviour;
    private GridBehaviour gridBehaviour;

    private UniTaskCoroutine _monitorClicksCorutine;

    private async UniTask MonitorClicksTask(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }


    public void MoveCellTo(Vector2Int from, Vector2Int to)
    {
        if (!gridBehaviour.IsCellFreeOnLayer(to))
        {
            Debug.LogWarning($"Целевая ячейка {to} уже занята. Перемещение отменено.");
            return;
        }

        if (!gridBehaviour.TryGetGridObject(from, out var gridObject))
        {
            Debug.LogWarning($"В ячейке {from} нет объекта для перемещения.");
            return;
        }

        // === Логика обновления состояния сетки ===
        gridBehaviour.MoveGridObjectToCell(gridObject, to);

        gridObject.SetGridPosition(to);

        // === Логика обновления объекта на сцене ===
        var worldPosition = gridBehaviour.GetCellCenterWorld(to);
        gridObject.SetPosition(worldPosition);
    }


    private void Awake()
    {
        _monitorClicksCorutine = new(MonitorClicksTask);
    }

    void Start()
    {
        gridBehaviour = GetComponent<GridBehaviour>();
    }

    private void OnEnable()
    {
        _monitorClicksCorutine.Run();
    }

    private void OnDisable()
    {
        _monitorClicksCorutine.Stop();
    }

    private void OnDestroy()
    {
        _monitorClicksCorutine.Stop();
        _monitorClicksCorutine.Dispose();
    }
}