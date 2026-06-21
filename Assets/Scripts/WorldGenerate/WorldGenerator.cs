using UnityEngine;

namespace WorldGenerate
{
    public enum TerrainType
    {
        Water,
        Shore,
        Ground
    }

    public class WorldGenerator
    {
        private readonly WorldSettings settings;
        private readonly int seedOffsetX;
        private readonly int seedOffsetY;

        private Vector2Int cachedChunkCoord = new Vector2Int(int.MaxValue, int.MaxValue);
        private TerrainType[,] cachedChunkData;

        public WorldGenerator(WorldSettings settings)
        {
            this.settings = settings;
            // ������������� ������ �� seed, ����� ��� ��� �����������������
            var rnd = new System.Random(settings.seed);
            seedOffsetX = rnd.Next(0, 100000);
            seedOffsetY = rnd.Next(0, 100000);
        }

        /// <summary>
        /// Возвращает TerrainType в произвольной точке мира.
        /// Последовательно генерирует чанк на лету, не кэширует.
        /// Используется для спавна NPC и запросов вне чанковой системы.
        /// </summary>
        public TerrainType GetTerrainType(Vector3 worldPos)
        {
            int s = settings.chunkSize;
            int chunkX = Mathf.FloorToInt(worldPos.x / s);
            int chunkY = Mathf.FloorToInt(worldPos.y / s);

            int localX = Mathf.FloorToInt(worldPos.x) - chunkX * s;
            int localY = Mathf.FloorToInt(worldPos.y) - chunkY * s;

            // Кэшируем последний сгенерированный чанк
            var cacheCoord = new Vector2Int(chunkX, chunkY);
            if (cachedChunkCoord != cacheCoord)
            {
                cachedChunkData = GenerateChunkData(cacheCoord);
                cachedChunkCoord = cacheCoord;
            }

            localX = Mathf.Clamp(localX, 0, s - 1);
            localY = Mathf.Clamp(localY, 0, s - 1);

            return cachedChunkData[localX, localY];
        }

        /// <summary>
        /// true если клетка пригодна для ходьбы (не вода).
        /// </summary>
        public bool IsWalkableTile(Vector3 worldPos) =>
            GetTerrainType(worldPos) != TerrainType.Water;

        public TerrainType[,] GenerateChunkData(Vector2Int chunkCoord)
        {
            int s = settings.chunkSize;
            int w = settings.shoreWidth;
            int ext = s + 2 * w;

            // Pass 1: шум Перлина → Water / Ground
            var raw = new TerrainType[ext, ext];

            int baseX = chunkCoord.x * s - w;
            int baseY = chunkCoord.y * s - w;

            for (int y = 0; y < ext; y++)
            {
                for (int x = 0; x < ext; x++)
                {
                    float nx = (baseX + x + seedOffsetX) * settings.noiseScale;
                    float ny = (baseY + y + seedOffsetY) * settings.noiseScale;

                    float n = Mathf.PerlinNoise(nx, ny); // 0..1
                    raw[x, y] = (n < settings.waterThreshold) ? TerrainType.Water : TerrainType.Ground;
                }
            }

            // Pass 2: дистанция до воды → Shore / Ground
            var data = new TerrainType[s, s];
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    int rx = x + w;
                    int ry = y + w;

                    if (raw[rx, ry] == TerrainType.Water)
                        data[x, y] = TerrainType.Water;
                    else
                    {
                        int dist = MinWaterDistanceRaw(raw, rx, ry, ext, settings.shoreWidth);
                        data[x, y] = dist <= settings.shoreWidth ? TerrainType.Shore : TerrainType.Ground;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Возвращает минимальное Chebyshev расстояние от (x, y) до воды.
        /// Если вода не найдена в пределах maxDist, возвращает maxDist + 1.
        /// </summary>
        private static int MinWaterDistanceRaw(TerrainType[,] data, int x, int y, int ext, int maxDist)
        {
            for (int d = 1; d <= maxDist; d++)
            {
                // Верхняя и нижняя строки квадрата на дистанции d
                for (int dx = -d; dx <= d; dx++)
                {
                    if (IsWaterRaw(data, x + dx, y - d, ext)) return d;
                    if (IsWaterRaw(data, x + dx, y + d, ext)) return d;
                }

                // Левая и правая колонки (без углов — уже проверены выше)
                for (int dy = -d + 1; dy <= d - 1; dy++)
                {
                    if (IsWaterRaw(data, x - d, y + dy, ext)) return d;
                    if (IsWaterRaw(data, x + d, y + dy, ext)) return d;
                }
            }

            return maxDist + 1;
        }
        
        private static bool IsWaterRaw(TerrainType[,] raw, int x, int y, int ext)
        {
            if (x < 0 || x >= ext || y < 0 || y >= ext)
                return false;
            return raw[x, y] == TerrainType.Water;
        }
    }
}