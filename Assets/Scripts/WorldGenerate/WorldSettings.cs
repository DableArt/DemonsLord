using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "World/WorldSettings")]
public class WorldSettings : ScriptableObject
{
    [Header("Chunk")]
    public int chunkSize = 32;         // 32x32 тайла
    public int pixelsPerUnit = 16;     // если нужно дл€ расчЄтов (не об€зательно)

    [Header("Streaming")]
    public int loadRadiusChunks = 2;   // сколько чанков вокруг камеры держим
    public int unloadRadiusChunks = 4; // дальше Ч выгружаем

    [Header("Noise")]
    public int seed = 12345;
    public float noiseScale = 0.06f;
    [Range(0f, 1f)] public float waterThreshold = 0.45f;

    [Header("Tiles")]
    public TileBase groundTile;
    public TileBase waterTile;
}
