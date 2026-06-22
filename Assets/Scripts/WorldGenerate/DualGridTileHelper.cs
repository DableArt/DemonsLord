using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace WorldGenerate
{
    public class DualGridTileHelper
    {
        private TileBase waterTile;
        private Dictionary<TerrainType, DualGridTileSetData> dualGridTileSetsDictionary;
        
        public TileBase WaterCollisionTile { get; private set; }

        public void Init(DualGridTileSetSettings settings)
        {
            waterTile = settings.waterTile;
            WaterCollisionTile = settings.waterColisionTile;
            
            dualGridTileSetsDictionary = new Dictionary<TerrainType, DualGridTileSetData>();

            foreach (var tileSet in settings.tileSets)
                dualGridTileSetsDictionary.Add(tileSet.TerrainType, tileSet);
        }

        public Dictionary<TerrainType, TileBase> GetEntries(TerrainType bl, TerrainType br, TerrainType tl, TerrainType tr)
        {
            var result = new Dictionary<TerrainType, TileBase>();

            // 1. Вода всегда обычный тайл
            if (bl is TerrainType.Water || br is TerrainType.Water || tl is TerrainType.Water || tr is TerrainType.Water)
                result[TerrainType.Water] = waterTile;

            // 2. Определяем тип с наивысшим sorting order (Water=0, Shore=1, Ground=2)
            var corners = new[]{ bl, br, tl, tr };
            TerrainType highestSortOrderTerrainType = TerrainType.Water;

            foreach (var next in corners)
            {
                if (next > highestSortOrderTerrainType)
                    highestSortOrderTerrainType = next;
            }

            // 3. Для highest — подбор тайла по паттерну 2x2
            if (highestSortOrderTerrainType != TerrainType.Water)
            {
                TileBase tile = SelectByPattern(highestSortOrderTerrainType, corners);
                if (tile != null)
                    result[highestSortOrderTerrainType] = tile;
            }

            // 4. Для lower non-Water — FSC
            foreach (var t in corners)
            {
                if (t == TerrainType.Water || t == highestSortOrderTerrainType || result.ContainsKey(t))
                    continue;

                result[t] = SelectByPattern(t, null);
            }

            return result;
        }

        private TileBase SelectByPattern(TerrainType type, TerrainType[] cornerNeighbours)
        {
            // c = [bl, br, tl, tr]
            int countOfType = 0;

            if (cornerNeighbours != null)
                foreach (var cornerNeighbour in cornerNeighbours)
                {
                    if (cornerNeighbour == type)
                        countOfType++;
                }
            else
                return dualGridTileSetsDictionary[type].FSC;
            
            switch (countOfType)
            {
                case 4: return dualGridTileSetsDictionary[type].FSC;

                case 3: // concave — 1 угол ДРУГОГО типа
                    if (cornerNeighbours[0] != type) return dualGridTileSetsDictionary[type].BotLeftConcC;   // bl другой → выемка в tr
                    if (cornerNeighbours[1] != type) return dualGridTileSetsDictionary[type].BotRightConcC;    // br другой → выемка в tl
                    if (cornerNeighbours[2] != type) return dualGridTileSetsDictionary[type].TopLeftConcC;   // tl другой → выемка в br
                    if (cornerNeighbours[3] != type) return dualGridTileSetsDictionary[type].TopRightConcC;    // tr другой → выемка в bl
                    break;

                case 2:
                    // adjacent — TSDC
                    if (cornerNeighbours[0] == type && cornerNeighbours[1] == type) return dualGridTileSetsDictionary[type].TopTSDC;   // нижний ряд
                    if (cornerNeighbours[2] == type && cornerNeighbours[3] == type) return dualGridTileSetsDictionary[type].BotTSDC;   // верхний ряд
                    if (cornerNeighbours[0] == type && cornerNeighbours[2] == type) return dualGridTileSetsDictionary[type].RightTSDC;  // левая колонка
                    if (cornerNeighbours[1] == type && cornerNeighbours[3] == type) return dualGridTileSetsDictionary[type].LeftTSDC; // правая колонка
                    // diagonal — TSCC
                    if (cornerNeighbours[0] == type && cornerNeighbours[3] == type) return dualGridTileSetsDictionary[type].TopLeftBotRightTSCC;   // bl + tr
                    if (cornerNeighbours[1] == type && cornerNeighbours[2] == type) return dualGridTileSetsDictionary[type].TopRightBotLeftTSCC;   // br + tl
                    break;

                case 1: // convex — 1 угол ЭТОГО типа
                    if (cornerNeighbours[0] == type) return dualGridTileSetsDictionary[type].TopRightConvC;
                    if (cornerNeighbours[1] == type) return dualGridTileSetsDictionary[type].TopLeftConvC;
                    if (cornerNeighbours[2] == type) return dualGridTileSetsDictionary[type].BotRightConvC;
                    if (cornerNeighbours[3] == type) return dualGridTileSetsDictionary[type].BotLeftConvC;
                    break;
            }

            return dualGridTileSetsDictionary[type].FSC; // fallback
        }
    }
}