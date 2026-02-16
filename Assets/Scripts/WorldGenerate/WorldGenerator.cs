using UnityEngine;

public enum TileType { Ground, Water }
public class WorldGenerator
{
    private readonly WorldSettings settings;
    private readonly int seedOffsetX;
    private readonly int seedOffsetY;

    public WorldGenerator(WorldSettings settings)
    {
        this.settings = settings;
        // фиксированный оффсет от seed, чтобы мир был детерминированный
        var rnd = new System.Random(settings.seed);
        seedOffsetX = rnd.Next(0, 100000);
        seedOffsetY = rnd.Next(0, 100000);
    }

    public TileType[,] GenerateChunkData(Vector2Int chunkCoord)
    {
        int s = settings.chunkSize;
        var data = new TileType[s, s];

        // мировая позиция тайла = координата чанка * размер + локальная координата
        int baseX = chunkCoord.x * s;
        int baseY = chunkCoord.y * s;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float nx = (baseX + x + seedOffsetX) * settings.noiseScale;
                float ny = (baseY + y + seedOffsetY) * settings.noiseScale;

                float n = Mathf.PerlinNoise(nx, ny); // 0..1
                data[x, y] = (n < settings.waterThreshold) ? TileType.Water : TileType.Ground;
            }
        }

        return data;
    }
}
