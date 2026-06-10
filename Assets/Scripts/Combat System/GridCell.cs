using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridCell
{
    public TerrainType terrain;
    public int height;
    public bool occupied;
    public Unit unit;

    public GridCell()
    {
        terrain = TerrainType.Normal;
        height = 0;
        occupied = false;
    }

    public GridCell(TerrainType terrain, int height, bool occupied)
    {
        this.terrain = terrain;
        this.height = height;
        this.occupied = occupied;
    }

    public bool IsPassable
    {
        get
        {
            if (occupied) return false;
            return terrain switch
            {
                TerrainType.Obstacle => false,
                TerrainType.Lava => false,
                TerrainType.Trap => true,
                _ => true,
            };
        }
    }

    public int MovementCost
    {
        get
        {
            return terrain switch
            {
                TerrainType.Normal => 1,
                TerrainType.Forest => 2,
                TerrainType.Water => 3,
                TerrainType.Mountain => 3,
                TerrainType.Swamp => 3,
                TerrainType.Sand => 2,
                TerrainType.Rubble => 2,
                TerrainType.Ice => 2,
                TerrainType.MagicField => 1,
                TerrainType.Trap => 2,
                _ => 1,
            };
        }
    }
}
