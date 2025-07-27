using Cysharp.Threading.Tasks;
using EditorExtention;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class GridObjectBehaviour : MonoBehaviour
{
    public Vector2Int GridPosition;

    public int Layer = 0;

    [Header("Events")]
    public UnityEvent<Vector2Int> OnGridPositionChanged;

    public void SetGridPosition(Vector2Int position)
    {
        if(GridPosition == position) return;

        GridPosition = position;
        OnGridPositionChanged?.Invoke(position);
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }
}
