using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cell
{
    public IList<string> Tags;
    public bool Occupied;
    public Unit Unit;
    public Cell()
    {

    }

    public Cell(IList<string> tags, bool occupied)
    {
        Tags = tags;
        Occupied = occupied;
    }
}

