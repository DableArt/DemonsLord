using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Grid
{
    public int width = 12;
    public int height = 8;
    public List<Cell> OccuptedCell => Cells.Where((h) => h.Occupied).ToList();
    public Cell[] Cells;

    public Grid(int width, int height, Cell[] cells)
    {
        this.width = width;
        this.height = height;
        Cells = cells;
    }
}

