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
        // ������������� ������ �� seed, ����� ��� ��� �����������������
        var rnd = new System.Random(settings.seed);
        seedOffsetX = rnd.Next(0, 100000);
        seedOffsetY = rnd.Next(0, 100000);
    }

    /// <summary>
    /// Returns true when the world-space position lies on a Ground tile.
    /// </summary>
    public bool IsGroundTile(Vector3 worldPos)
    {
        int s = settings.chunkSize;
        int chunkX = Mathf.FloorToInt(worldPos.x / s);
        int chunkY = Mathf.FloorToInt(worldPos.y / s);

        int localX = Mathf.FloorToInt(worldPos.x) - chunkX * s;
        int localY = Mathf.FloorToInt(worldPos.y) - chunkY * s;

        localX = Mathf.Clamp(localX, 0, s - 1);
        localY = Mathf.Clamp(localY, 0, s - 1);

        float nx = (chunkX * s + localX + seedOffsetX) * settings.noiseScale;
        float ny = (chunkY * s + localY + seedOffsetY) * settings.noiseScale;
        float n = Mathf.PerlinNoise(nx, ny);

        return n >= settings.waterThreshold; // Ground
    }

    public TileType[,] GenerateChunkData(Vector2Int chunkCoord)
    {
        int s = settings.chunkSize;
        var data = new TileType[s, s];

        // ������� ������� ����� = ���������� ����� * ������ + ��������� ����������
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
