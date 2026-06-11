using UnityEngine;

public struct UnitMoveContext
{
    public Unit unit;
    public Vector2Int from;
    public Vector2Int to;

    public UnitMoveContext(Unit unit, Vector2Int from, Vector2Int to)
    {
        this.unit = unit;
        this.from = from;
        this.to = to;
    }
}
