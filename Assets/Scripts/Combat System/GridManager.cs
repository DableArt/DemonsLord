using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 12;
    public int height = 8;
    private bool[,] occupied;

    void Awake()
    {
        occupied = new bool[width, height];
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return true; // Вне поля считаем занятым
        return occupied[cell.x, cell.y];
    }

    public void SetCellOccupied(Vector2Int cell, bool value)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return;
        occupied[cell.x, cell.y] = value;
    }

    public bool[,] GetOccupiedGrid()
    {
        return occupied;
    }
} 