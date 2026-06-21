using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldGenerate
{
    [Serializable]
    public struct DualGridTileSetData
    {
        [Header("Terrain Type")]
        public TerrainType TerrainType;
            
        [Header("Four-Side Connection")]
        public TileBase FSC;
            
        [Header("Convex Corners")]
        public TileBase TopRightConvC;
        public TileBase TopLeftConvC;
        public TileBase BotRightConvC;
        public TileBase BotLeftConvC;
            
        [Header("Concave Corners")]
        public TileBase TopRightConcC;
        public TileBase TopLeftConcC;
        public TileBase BotRightConcC;
        public TileBase BotLeftConcC;
            
        [Header("Two-Sided direct connections")]
        public TileBase TopTSDC;
        public TileBase BotTSDC;
        public TileBase RightTSDC;
        public TileBase LeftTSDC;
            
        [Header("Two-Sided corner connections")]
        public TileBase TopLeftBotRightTSCC;
        public TileBase TopRightBotLeftTSCC;
    }
}