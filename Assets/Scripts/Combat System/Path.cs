using System.Collections.Generic;
using UnityEngine;

public struct Path
{
    public readonly List<Vector2Int> Points;

    public Path(IEnumerable<Vector2Int> points)
    {
        Points = points != null ? new List<Vector2Int>(points) : new List<Vector2Int>();
    }

    public bool IsValid => Points != null && Points.Count > 0;

    public int Length => Points?.Count ?? 0;

    public Vector2Int Start => (Points != null && Points.Count > 0) ? Points[0] : default;
    public Vector2Int End => (Points != null && Points.Count > 0) ? Points[Points.Count - 1] : default;

    public Vector2Int this[int idx] => Points[idx];
}
