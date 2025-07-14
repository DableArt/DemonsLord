using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; // Required for List

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 128;        // ������ �����
    public int height = 128;       // ������ �����
    public float scale = 20f;       // ������� ����
    public float offset_x = 0f;     // �������� ���� �� X
    public float offset_y = 0f;     // �������� ���� �� Y
    public int depth = 3;
    public bool useSmoothTransitions = true; // ������������ ������� �������� ����� �������

    [Header("Ground Tile Settings")]
    public List<TileBase> groundTiles; // ������ ������ �����
    [Range(0f, 1f)]
    public List<float> groundTileProbabilities; // ����������� ������ ������� ����� �����

    [Header("Water Tile Settings")]
    public List<TileBase> waterTiles; // ������ ������ ����
    [Range(0f, 1f)]
    public List<float> waterTileProbabilities; // ����������� ������ ������� ����� ����

    [Header("Water Level")]
    public float waterLevel = 0.5f;  // ������� ���� (0-1)

    [Header("Transition Tiles")]
    public TileBase groundWaterTransition_N; // ������� �����-���� (�����)
    public TileBase groundWaterTransition_E; // ������� �����-���� (������)
    public TileBase groundWaterTransition_S; // ������� �����-���� (��)
    public TileBase groundWaterTransition_W; // ������� �����-���� (�����)
    public TileBase groundWaterTransition_NE; // ������� �����-���� (������-������)
    public TileBase groundWaterTransition_SE; // ������� �����-���� (���-������)
    public TileBase groundWaterTransition_SW; // ������� �����-���� (���-�����)
    public TileBase groundWaterTransition_NW; // ������� �����-���� (������-�����)

    [Header("Tilemap References")]
    public Tilemap tilemap;         // ������ �� Tilemap

    void Start()
    {
        offset_x = Random.Range(0f, 99999f);
        offset_y = Random.Range(0f, 99999f);

        // �������� ������������ ������������
        if (groundTiles.Count != groundTileProbabilities.Count || waterTiles.Count != waterTileProbabilities.Count)
        {
            Debug.LogError("���������� ������ � ������������ �� ���������!");
            return;
        }
        GenerateMap();
    }

    void GenerateMap()
    {
        // ������� �����
        tilemap.ClearAllTiles();

        // ���������� ����� �� ������ ����
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = CalculateNoiseValue(x, y);

                // ����������, ����� ���� ���������� (���� ��� �����)
                TileBase tile = noiseValue < waterLevel ? GetRandomTile(waterTiles, waterTileProbabilities) : GetRandomTile(groundTiles, groundTileProbabilities);

                // ������������� ���� �� Tilemap
                tilemap.SetTile(new Vector3Int(x - width / 2, y - height / 2, depth), tile); // ���������� �����
            }
        }

        if (useSmoothTransitions)
        {
            ApplySmoothTransitions();
        }
    }

    float CalculateNoiseValue(int x, int y)
    {
        float xCoord = (float)x / width * scale + offset_x;
        float yCoord = (float)y / height * scale + offset_y;

        // ���������� Perlin Noise ��� ��������� ���������
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    // ������� ��� ��������� ���������� ����� �� ������ � ������ ������������
    TileBase GetRandomTile(List<TileBase> tiles, List<float> probabilities)
    {
        if (tiles.Count == 0) return null; // ���� ��� ������, ���������� null

        float totalProbability = 0f;
        foreach (float prob in probabilities)
        {
            totalProbability += prob;
        }
        if (totalProbability <= 0f) return tiles[Random.Range(0, tiles.Count)]; // ���� ��� ����������� ����� 0

        float randomValue = Random.Range(0f, totalProbability); // �������� �� totalProbability

        float cumulativeProbability = 0f;
        for (int i = 0; i < tiles.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return tiles[i];
            }
        }

        return tiles[tiles.Count - 1]; // ���������� ��������� ����, ���� ���-�� ����� �� ���
    }

    // ��������� ������� �������� ����� ������� ����� � ����
    void ApplySmoothTransitions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int currentPos = new Vector3Int(x - width / 2, y - height / 2, depth);
                TileBase currentTile = tilemap.GetTile(currentPos);

                bool isGround = false;
                foreach (TileBase tile in groundTiles)
                {
                    if (currentTile == tile)
                    {
                        isGround = true;
                        break;
                    }
                }

                if (isGround) // �������� ������ ��� �����
                {
                    TileBase transitionTile = GetTransitionTile(currentPos);
                    if (transitionTile != null)
                    {
                        tilemap.SetTile(currentPos, transitionTile);
                    }
                }
            }
        }
    }

    // ����������, ����� ���� �������� ������������
    TileBase GetTransitionTile(Vector3Int position)
    {
        bool n = IsWater(position + new Vector3Int(0, 1, 0));
        bool e = IsWater(position + new Vector3Int(1, 0, 0));
        bool s = IsWater(position + new Vector3Int(0, -1, 0));
        bool w = IsWater(position + new Vector3Int(-1, 0, 0));

        if (n && !e && !s && !w) return groundWaterTransition_N;
        if (!n && e && !s && !w) return groundWaterTransition_E;
        if (!n && !e && s && !w) return groundWaterTransition_S;
        if (!n && !e && !s && w) return groundWaterTransition_W;

        if (n && e && !s && !w) return groundWaterTransition_NE;
        if (!n && e && s && !w) return groundWaterTransition_SE;
        if (!n && !e && s && w) return groundWaterTransition_SW;
        if (n && !e && !s && w) return groundWaterTransition_NW;

        return null;
    }

    // ���������, �������� �� ���� �����
    bool IsWater(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        if (tile == null) return false; // ��������� �������� �� null

        foreach (TileBase waterTile in waterTiles)
        {
            if (tile == waterTile) return true;
        }
        return false;
    }

    // (�����������) ������� ��� ������������� �����
    public void RegenerateMap()
    {
        GenerateMap();
    }

}