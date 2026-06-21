using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldGenerate
{
    [CreateAssetMenu(fileName = "DualGridTileSet", menuName = "World/Dual Grid Tile Set")]
    public class DualGridTileSetSettings : ScriptableObject
    {
        public List<DualGridTileSetData> tileSets;
        public TileBase waterTile;
    }
}