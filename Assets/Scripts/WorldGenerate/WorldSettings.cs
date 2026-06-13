using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "World/WorldSettings")]
public class WorldSettings : ScriptableObject
{
    [Header("Chunk")]
    public int chunkSize = 32;         // 32x32 тайлов
    public int pixelsPerUnit = 16;     // пока только для справки (не используется)

    [Header("Streaming")]
    public int loadRadiusChunks = 2;   // радиус загрузки чанков вокруг камеры
    public int unloadRadiusChunks = 4; // радиус выгрузки

    [Header("Noise")]
    public int seed = 12345;
    public float noiseScale = 0.06f;
    [Range(0f, 1f)] public float waterThreshold = 0.45f;

    // -----------------------------------------------------------------------
    // Tiles — Ground
    // -----------------------------------------------------------------------
    [Header("Tiles — Ground")]
    [Tooltip("Тайлы обычной земли (трава и т.п.). groundTiles[0] используется по умолчанию. " +
             "Сюда можно добавить несколько вариантов для будущей рандомизации или RuleTile.")]
    public TileBase[] groundTiles;

    [Tooltip("Тайл берега/перехода (трава→камни) — отображается на земляных клетках, " +
             "граничащих с водой. Если не задан, используется groundTiles[0]. " +
             "Совместим с RuleTile для автоматического соединения береговой линии.")]
    public TileBase shoreTile;

    // -----------------------------------------------------------------------
    // Tiles — Water
    // -----------------------------------------------------------------------
    [Header("Tiles — Water")]
    [Tooltip("Тайлы воды. waterTiles[0] используется по умолчанию. " +
             "Назначьте сюда AnimatedTile для покадровой анимации воды (4 кадра). " +
             "Подробная инструкция: Assets/Scripts/WorldGenerate/ANIMATED_WATER_SETUP.md. " +
             "Архитектура совместима с будущей заменой на shader-подход.")]
    public TileBase[] waterTiles;

    [Header("NPC Spawning")]
    public GameObject[] npcPrefabs;
    [Min(0)] public int npcMinCount = 3;
    [Min(0)] public int npcMaxCount = 10;

    [Header("Trees")]
    public GameObject treePrefab;
    [Range(0f, 1f)] public float treeSpawnChance = 0.05f;

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>Возвращает базовый тайл земли (первый из массива) или null.</summary>
    public TileBase GetGroundTile() =>
        (groundTiles != null && groundTiles.Length > 0) ? groundTiles[0] : null;

    /// <summary>
    /// Возвращает береговой/переходный тайл.
    /// Если <see cref="shoreTile"/> не задан — возвращает базовый тайл земли.
    /// </summary>
    public TileBase GetShoreTile() =>
        (shoreTile != null) ? shoreTile : GetGroundTile();

    /// <summary>
    /// Возвращает основной тайл воды (первый из массива) или null.
    /// Поддерживает AnimatedTile — просто назначьте анимированный тайл как waterTiles[0].
    /// </summary>
    public TileBase GetWaterTile() =>
        (waterTiles != null && waterTiles.Length > 0) ? waterTiles[0] : null;
}
