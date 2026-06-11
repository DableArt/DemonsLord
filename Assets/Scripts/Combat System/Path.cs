using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Path
{
    public List<Vector2Int> nodes;
    public bool IsValid => nodes != null && nodes.Count > 0;
    public int Length => nodes?.Count ?? 0;
    public Vector2Int Start => nodes != null && nodes.Count > 0 ? nodes[0] : Vector2Int.zero;
    public Vector2Int End => nodes != null && nodes.Count > 0 ? nodes[nodes.Count - 1] : Vector2Int.zero;

    public Path(List<Vector2Int> pathNodes)
    {
        nodes = pathNodes;
    }

    public Vector2Int this[int index] => nodes != null && index >= 0 && index < nodes.Count ? nodes[index] : Vector2Int.zero;
}
