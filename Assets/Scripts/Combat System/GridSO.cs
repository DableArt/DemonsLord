using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridSO", menuName = "SO/Grid")]
public class GridSO : ScriptableObject
{
    public CellWraper[] Cells;

    [Serializable]
    public class CellWraper
    {
        public Vector2Int point;
        public Cell Cell;
    }

    private void OnValidate()
    {
        // валидация
    }
}

