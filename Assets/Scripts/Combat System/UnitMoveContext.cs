using UnityEngine;

public readonly struct UnitMoveContext
{
    public readonly Unit Unit;
    public readonly Vector2Int From;
    public readonly Vector2Int To;

    public UnitMoveContext(Unit unit, Vector2Int from, Vector2Int to)
    {
        Unit = unit;
        From = from;
        To = to;
    }
}

