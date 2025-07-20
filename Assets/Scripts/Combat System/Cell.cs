using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cell
{
    public Vector2Int Point;
    public IList<string> Tags;
    public bool Occupied;
    public Unit Unit;

    public Cell(Vector2Int point, IList<string> tags, bool occupied)
    {
        Point = point;
        Tags = tags;
        Occupied = occupied;
    }
}

