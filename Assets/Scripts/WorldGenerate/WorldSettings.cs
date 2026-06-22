using UnityEngine;

namespace WorldGenerate
{
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

        [Header("Dual-grid")]
        [Tooltip("Ширина береговой линии")]
        public int shoreWidth = 2;
        public DualGridTileSetSettings dualGridTileSets;

        [Header("NPC Spawning")]
        public GameObject[] npcPrefabs;
        [Min(0)] public int npcMinCount = 3;
        [Min(0)] public int npcMaxCount = 10;

        [Header("Trees")]
        public GameObject treePrefab;
        [Range(0f, 1f)] public float treeSpawnChance = 0.05f;
    }
}
