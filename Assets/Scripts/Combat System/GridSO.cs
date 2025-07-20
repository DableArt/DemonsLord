using UnityEngine;

[CreateAssetMenu(fileName = "GridSO", menuName = "SO/Grid")]
public class GridSO : ScriptableObject
{
    public int width = 5;
    public int height = 5;
    public Cell[] Cells;
}

